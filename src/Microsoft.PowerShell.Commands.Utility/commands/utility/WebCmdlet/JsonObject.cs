// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.PowerShell.Commands
{
    /// <summary>
    /// JsonObject class.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Preferring Json over JSON")]
    public static class JsonObject
    {
        #region HelperTypes

        /// <summary>
        /// Context for convert-to-json operation.
        /// </summary>
        public readonly struct ConvertToJsonContext
        {
            /// <summary>
            /// Gets the maximum depth for walking the object graph.
            /// </summary>
            public readonly int MaxDepth;

            /// <summary>
            /// Gets the cancellation token.
            /// </summary>
            public readonly CancellationToken CancellationToken;

            /// <summary>
            /// Gets the StringEscapeHandling setting.
            /// </summary>
            public readonly StringEscapeHandling StringEscapeHandling;

            /// <summary>
            /// Gets the EnumsAsStrings setting.
            /// </summary>
            public readonly bool EnumsAsStrings;

            /// <summary>
            /// Gets the CompressOutput setting.
            /// </summary>
            public readonly bool CompressOutput;

            /// <summary>
            /// Gets the target cmdlet that is doing the convert-to-json operation.
            /// </summary>
            public readonly PSCmdlet Cmdlet;

            /// <summary>
            /// Initializes a new instance of the <see cref="ConvertToJsonContext"/> struct.
            /// </summary>
            /// <param name="maxDepth">The maximum depth to visit the object.</param>
            /// <param name="enumsAsStrings">Indicates whether to use enum names for the JSON conversion.</param>
            /// <param name="compressOutput">Indicates whether to get the compressed output.</param>
            public ConvertToJsonContext(int maxDepth, bool enumsAsStrings, bool compressOutput)
                : this(maxDepth, enumsAsStrings, compressOutput, CancellationToken.None, StringEscapeHandling.Default, targetCmdlet: null)
            {
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="ConvertToJsonContext"/> struct.
            /// </summary>
            /// <param name="maxDepth">The maximum depth to visit the object.</param>
            /// <param name="enumsAsStrings">Indicates whether to use enum names for the JSON conversion.</param>
            /// <param name="compressOutput">Indicates whether to get the compressed output.</param>
            /// <param name="cancellationToken">Specifies the cancellation token for cancelling the operation.</param>
            /// <param name="stringEscapeHandling">Specifies how strings are escaped when writing JSON text.</param>
            /// <param name="targetCmdlet">Specifies the cmdlet that is calling this method.</param>
            public ConvertToJsonContext(
                int maxDepth,
                bool enumsAsStrings,
                bool compressOutput,
                CancellationToken cancellationToken,
                StringEscapeHandling stringEscapeHandling,
                PSCmdlet targetCmdlet)
            {
                this.MaxDepth = maxDepth;
                this.CancellationToken = cancellationToken;
                this.StringEscapeHandling = stringEscapeHandling;
                this.EnumsAsStrings = enumsAsStrings;
                this.CompressOutput = compressOutput;
                this.Cmdlet = targetCmdlet;
            }
        }

        private class DuplicateMemberHashSet : HashSet<string>
        {
            public DuplicateMemberHashSet(int capacity)
                : base(capacity, StringComparer.OrdinalIgnoreCase)
            {
            }
        }

        #endregion HelperTypes

        #region ConvertFromJson

        /// <summary>
        /// Convert a Json string back to an object of type PSObject.
        /// </summary>
        /// <param name="input">The json text to convert.</param>
        /// <param name="error">An error record if the conversion failed.</param>
        /// <returns>A PSObject.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Preferring Json over JSON")]
        public static object ConvertFromJson(string input, out ErrorRecord error)
        {
            return ConvertFromJson(input, returnHashtable: false, out error);
        }

        /// <summary>
        /// Convert a Json string back to an object of type <see cref="System.Management.Automation.PSObject"/> or
        /// <see cref="System.Collections.Hashtable"/> depending on parameter <paramref name="returnHashtable"/>.
        /// </summary>
        /// <param name="input">The json text to convert.</param>
        /// <param name="returnHashtable">True if the result should be returned as a <see cref="System.Collections.Hashtable"/>
        /// instead of a <see cref="System.Management.Automation.PSObject"/></param>
        /// <param name="error">An error record if the conversion failed.</param>
        /// <returns>A <see cref="System.Management.Automation.PSObject"/> or a <see cref="System.Collections.Hashtable"/>
        /// if the <paramref name="returnHashtable"/> parameter is true.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Preferring Json over JSON")]
        public static object ConvertFromJson(string input, bool returnHashtable, out ErrorRecord error)
        {
            return ConvertFromJson(input, returnHashtable, maxDepth: 1024, out error);
        }

        /// <summary>
        /// Convert a JSON string back to an object of type <see cref="System.Management.Automation.PSObject"/> or
        /// <see cref="System.Collections.Hashtable"/> depending on parameter <paramref name="returnHashtable"/>.
        /// </summary>
        /// <param name="input">The JSON text to convert.</param>
        /// <param name="returnHashtable">True if the result should be returned as a <see cref="System.Collections.Hashtable"/>
        /// instead of a <see cref="System.Management.Automation.PSObject"/>.</param>
        /// <param name="maxDepth">The max depth allowed when deserializing the json input. Set to null for no maximum.</param>
        /// <param name="error">An error record if the conversion failed.</param>
        /// <returns>A <see cref="System.Management.Automation.PSObject"/> or a <see cref="System.Collections.Hashtable"/>
        /// if the <paramref name="returnHashtable"/> parameter is true.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Preferring Json over JSON")]
        public static object ConvertFromJson(string input, bool returnHashtable, int? maxDepth, out ErrorRecord error)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            error = null;
            try
            {
                // JsonConvert.DeserializeObject does not throw an exception when an invalid Json array is passed.
                // This issue is being tracked by https://github.com/JamesNK/Newtonsoft.Json/issues/1930.
                // To work around this, we need to identify when input is a Json array, and then try to parse it via JArray.Parse().

                // If input starts with '[' (ignoring white spaces).
                if (Regex.Match(input, @"^\s*\[").Success)
                {
                    // JArray.Parse() will throw a JsonException if the array is invalid.
                    // This will be caught by the catch block below, and then throw an
                    // ArgumentException - this is done to have same behavior as the JavaScriptSerializer.
                    JArray.Parse(input);

                    // Please note that if the Json array is valid, we don't do anything,
                    // we just continue the deserialization.
                }

                var obj = JsonConvert.DeserializeObject(
                    input,
                    new JsonSerializerSettings
                    {
                        // This TypeNameHandling setting is required to be secure.
                        TypeNameHandling = TypeNameHandling.None,
                        MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
                        MaxDepth = maxDepth
                    });

                switch (obj)
                {
                    case JObject dictionary:
                        // JObject is a IDictionary
                        return returnHashtable
                                   ? PopulateHashTableFromJDictionary(dictionary, out error)
                                   : PopulateFromJDictionary(dictionary, new DuplicateMemberHashSet(dictionary.Count), out error);
                    case JArray list:
                        return returnHashtable
                                   ? PopulateHashTableFromJArray(list, out error)
                                   : PopulateFromJArray(list, out error);
                    default:
                        return obj;
                }
            }
            catch (Newtonsoft.Json.JsonException je)
            {
                var msg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.JsonDeserializationFailed, je.Message);

                // the same as JavaScriptSerializer does
                throw new ArgumentException(msg, je);
            }
        }

        // This function is a clone of PopulateFromDictionary using JObject as an input.
        private static PSObject PopulateFromJDictionary(JObject entries, DuplicateMemberHashSet memberHashTracker, out ErrorRecord error)
        {
            error = null;
            var result = new PSObject(entries.Count);
            foreach (var entry in entries)
            {
                if (string.IsNullOrEmpty(entry.Key))
                {
                    var errorMsg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.EmptyKeyInJsonString);
                    error = new ErrorRecord(
                        new InvalidOperationException(errorMsg),
                        "EmptyKeyInJsonString",
                        ErrorCategory.InvalidOperation,
                        null);
                    return null;
                }

                // Case sensitive duplicates should normally not occur since JsonConvert.DeserializeObject
                // does not throw when encountering duplicates and just uses the last entry.
                if (memberHashTracker.TryGetValue(entry.Key, out var maybePropertyName)
                    && string.Compare(entry.Key, maybePropertyName, StringComparison.Ordinal) == 0)
                {
                    var errorMsg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.DuplicateKeysInJsonString, entry.Key);
                    error = new ErrorRecord(
                        new InvalidOperationException(errorMsg),
                        "DuplicateKeysInJsonString",
                        ErrorCategory.InvalidOperation,
                        null);
                    return null;
                }

                // Compare case insensitive to tell the user to use the -AsHashTable option instead.
                // This is because PSObject cannot have keys with different casing.
                if (memberHashTracker.TryGetValue(entry.Key, out var propertyName))
                {
                    var errorMsg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.KeysWithDifferentCasingInJsonString, propertyName, entry.Key);
                    error = new ErrorRecord(
                        new InvalidOperationException(errorMsg),
                        "KeysWithDifferentCasingInJsonString",
                        ErrorCategory.InvalidOperation,
                        null);
                    return null;
                }

                // Array
                switch (entry.Value)
                {
                    case JArray list:
                        {
                            var listResult = PopulateFromJArray(list, out error);
                            if (error != null)
                            {
                                return null;
                            }

                            result.Properties.Add(new PSNoteProperty(entry.Key, listResult));
                            break;
                        }
                    case JObject dic:
                        {
                            // Dictionary
                            var dicResult = PopulateFromJDictionary(dic, new DuplicateMemberHashSet(dic.Count), out error);
                            if (error != null)
                            {
                                return null;
                            }

                            result.Properties.Add(new PSNoteProperty(entry.Key, dicResult));
                            break;
                        }
                    case JValue value:
                        {
                            result.Properties.Add(new PSNoteProperty(entry.Key, value.Value));
                            break;
                        }
                }

                memberHashTracker.Add(entry.Key);
            }

            return result;
        }

        // This function is a clone of PopulateFromList using JArray as input.
        private static ICollection<object> PopulateFromJArray(JArray list, out ErrorRecord error)
        {
            error = null;
            var result = new object[list.Count];

            for (var index = 0; index < list.Count; index++)
            {
                var element = list[index];
                switch (element)
                {
                    case JArray subList:
                        {
                            // Array
                            var listResult = PopulateFromJArray(subList, out error);
                            if (error != null)
                            {
                                return null;
                            }

                            result[index] = listResult;
                            break;
                        }
                    case JObject dic:
                        {
                            // Dictionary
                            var dicResult = PopulateFromJDictionary(dic, new DuplicateMemberHashSet(dic.Count), out error);
                            if (error != null)
                            {
                                return null;
                            }

                            result[index] = dicResult;
                            break;
                        }
                    case JValue value:
                        {
                            result[index] = value.Value;
                            break;
                        }
                }
            }

            return result;
        }

        // This function is a clone of PopulateFromDictionary using JObject as an input.
        private static Hashtable PopulateHashTableFromJDictionary(JObject entries, out ErrorRecord error)
        {
            error = null;
            Hashtable result = new Hashtable(entries.Count);
            foreach (var entry in entries)
            {
                // Case sensitive duplicates should normally not occur since JsonConvert.DeserializeObject
                // does not throw when encountering duplicates and just uses the last entry.
                if (result.ContainsKey(entry.Key))
                {
                    string errorMsg = string.Format(CultureInfo.CurrentCulture, WebCmdletStrings.DuplicateKeysInJsonString, entry.Key);
                    error = new ErrorRecord(
                        new InvalidOperationException(errorMsg),
                        "DuplicateKeysInJsonString",
                        ErrorCategory.InvalidOperation,
                        null);
                    return null;
                }

                switch (entry.Value)
                {
                    case JArray list:
                        {
                            // Array
                            var listResult = PopulateHashTableFromJArray(list, out error);
                            if (error != null)
                            {
                                return null;
                            }

                            result.Add(entry.Key, listResult);
                            break;
                        }
                    case JObject dic:
                        {
                            // Dictionary
                            var dicResult = PopulateHashTableFromJDictionary(dic, out error);
                            if (error != null)
                            {
                                return null;
                            }

                            result.Add(entry.Key, dicResult);
                            break;
                        }
                    case JValue value:
                        {
                            result.Add(entry.Key, value.Value);
                            break;
                        }
                }
            }

            return result;
        }

        // This function is a clone of PopulateFromList using JArray as input.
        private static ICollection<object> PopulateHashTableFromJArray(JArray list, out ErrorRecord error)
        {
            error = null;
            var result = new object[list.Count];

            for (var index = 0; index < list.Count; index++)
            {
                var element = list[index];

                switch (element)
                {
                    case JArray array:
                        {
                            // Array
                            var listResult = PopulateHashTableFromJArray(array, out error);
                            if (error != null)
                            {
                                return null;
                            }

                            result[index] = listResult;
                            break;
                        }
                    case JObject dic:
                        {
                            // Dictionary
                            var dicResult = PopulateHashTableFromJDictionary(dic, out error);
                            if (error != null)
                            {
                                return null;
                            }

                            result[index] = dicResult;
                            break;
                        }
                    case JValue value:
                        {
                            result[index] = value.Value;
                            break;
                        }
                }
            }

            return result;
        }

        #endregion ConvertFromJson

        #region ConvertToJson

            // The implementation is the same as JavaScriptEncoder.Default but can be customized.
            // Default JavaScriptEncoder always escape HTML and follow codepoints (the comment come from .Net Core):
            // 1. Forbid codepoints which aren't mapped to characters or which are otherwise always disallowed
            //    (includes categories Cc, Cs, Co, Cn, Zs [except U+0020 SPACE], Zl, Zp)
            // 2. '\' (U+005C REVERSE SOLIDUS) must always be escaped in Javascript / ECMAScript / JSON.
            //    '/' (U+002F SOLIDUS) is not Javascript / ECMAScript / JSON-sensitive so doesn't need to be escaped.
            // 3. '`' (U+0060 GRAVE ACCENT) is ECMAScript-sensitive (see ECMA-262).
        private static JavaScriptEncoder s_escapeNonAsciiEncoder = InitEscapeNonAsciiEncoder();
        private static JavaScriptEncoder InitEscapeNonAsciiEncoder()
        {
            var textEncoderSettings = new TextEncoderSettings(UnicodeRanges.BasicLatin);

            return JavaScriptEncoder.Create(textEncoderSettings);
        }

        /// <summary>
        /// Convert an object to JSON string.
        /// </summary>
        public static string ConvertToJson(object objectToProcess, in ConvertToJsonContext context)
        {
            try
            {
                var options = new JsonSerializerOptions()
                {
                    WriteIndented = !context.CompressOutput,
                    MaxDepth = context.MaxDepth,
                    IgnoreNullValues = false,
                    Encoder = context.StringEscapeHandling switch
                    {
                        StringEscapeHandling.Default => JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        StringEscapeHandling.EscapeHtml => JavaScriptEncoder.Default,
                        StringEscapeHandling.EscapeNonAscii => s_escapeNonAsciiEncoder,
                        _ => JavaScriptEncoder.Default
                    }
                };

                if (context.EnumsAsStrings)
                {
                    options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                }

                options.Converters.Add(new JsonStringEnumConverter64());
                options.Converters.Add(new JsonConverterPSObject());
                options.Converters.Add(new JsonConverterNullString());
                options.Converters.Add(new JsonConverterDBNull());

                //if (preprocessedObject == null)
                //{
                //    return System.Text.Json.JsonSerializer.Serialize(preprocessedObject, null, options);
                //}

                return System.Text.Json.JsonSerializer.Serialize(objectToProcess, objectToProcess?.GetType(), options);
                //return System.Text.Json.JsonSerializer.Serialize(preprocessedObject, preprocessedObject.GetType(), options);

            }
            catch (OperationCanceledException)
            {
                return null;
            }
        }

        private sealed class JsonConverterDBNull : System.Text.Json.Serialization.JsonConverter<DBNull>
        {
            public override DBNull Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, DBNull _, JsonSerializerOptions options)
            {
                writer.WriteNullValue();
                return;
            }
        }

        private sealed class JsonConverterNullString : System.Text.Json.Serialization.JsonConverter<NullString>
        {
            public override NullString Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, NullString _, JsonSerializerOptions options)
            {
                writer.WriteNullValue();
                return;
            }
        }

        /// <summary>
        /// Converter to convert enums to and from strings with a workaround for long- and ulong-based enums.
        /// </summary>
        /// <remarks>
        /// Win8:378368 Enums based on System.Int64 or System.UInt64 are not JSON-serializable
        /// because JavaScript does not support the necessary precision.
        /// </remarks>
        private class JsonStringEnumConverter64 : JsonConverterFactory
        {
            /// <summary>
            /// Initialize an instance of the <see cref="JsonStringEnumConverter64"/> with the
            /// default naming policy and allows integer values.
            /// </summary>
            public JsonStringEnumConverter64()
            {
                // An empty constructor is needed for construction via attributes
            }

            /// <inheritdoc />
            public override bool CanConvert(Type typeToConvert)
            {
                if (!typeToConvert.IsEnum)
                {
                    return false;
                }

                var underlyingType = Enum.GetUnderlyingType(typeToConvert);
                return (underlyingType == typeof(long) || underlyingType == typeof(ulong));
            }

            /// <inheritdoc />
            public override System.Text.Json.Serialization.JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                var a = new JsonStringEnumConverter(namingPolicy: null, allowIntegerValues: false);
                return a.CreateConverter(typeToConvert, options);
            }
        }

        internal sealed class JsonConverterPSObject : System.Text.Json.Serialization.JsonConverter<PSObject>
        {
            public override PSObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
    /*
                string uriString = reader.GetString();
                if (Uri.TryCreate(uriString, UriKind.RelativeOrAbsolute, out Uri value))
                {
                    return value;
                }

                ThrowHelper.ThrowJsonException();
                return null;
    */
                throw new NotImplementedException();
            }

            public override void Write(Utf8JsonWriter writer, PSObject pso, JsonSerializerOptions options)
            {
                // context.CancellationToken.ThrowIfCancellationRequested();

                if (LanguagePrimitives.IsNull(pso))
                {
                    writer.WriteNullValue();
                    return;
                }

                //IDictionary dictionary = null;
                //AppendPsProperties<System.Text.Json.Serialization.JsonIgnoreAttribute>(pso, dictionary, true);

                var obj = pso.BaseObject;

                bool isCustomObj = false;

                if (obj == NullString.Value
                    || obj == DBNull.Value)
                {
                    obj = null;
                }
                else if (obj.GetType().IsPrimitive
                    || obj.GetType().IsEnum
                    || obj is string
                    || obj is char
                    || obj is bool
                    || obj is DateTime
                    || obj is DateTimeOffset
                    || obj is Guid
                    || obj is Uri
                    || obj is double
                    || obj is float
                    || obj is decimal)
                {
                    //dictionary = obj;
                    //System.Text.Json.JsonSerializer.Serialize(writer, obj, obj.GetType(), options);
                }
                else if (obj is Newtonsoft.Json.Linq.JObject jObject)
                {
                    obj = jObject.ToObject<Dictionary<object, object>>();
                    //System.Text.Json.JsonSerializer.Serialize(writer, dict, dict.GetType(), options);
                }
                else
                {
                    var dictionary = obj as IDictionary;
                    if (dictionary != null)
                    {
                        //rv = ProcessDictionary(dict, currentDepth, in context);
                        //System.Text.Json.JsonSerializer.Serialize(writer, dict, dict.GetType(), options);
                        //isDictionary = true;
                        obj = dictionary;
                    }
                    else
                    {
                        IEnumerable enumerable = obj as IEnumerable;
                        if (enumerable != null)
                        {
                            //rv = ProcessEnumerable(enumerable, currentDepth, in context);
                            //System.Text.Json.JsonSerializer.Serialize(writer, enumerable, enumerable.GetType(), options);
                            //obj = rv;
                        }
                        else
                        {
                            // PSCustomObject or C# object
                            obj = new Dictionary<string, object>();
                            // Since the converter is for PSObject only
                            // we already have all properties in the PSObject
                            // so makes no sense to collect the same properties from base object.
                            //
                            //obj = ProcessCustomObject<System.Text.Json.Serialization.JsonIgnoreAttribute>(obj);
                            //System.Text.Json.JsonSerializer.Serialize(writer, obj, obj.GetType(), options);
                            isCustomObj = true;
                        }
                    }
                }

                SerializePsProperties(writer, pso, obj, isCustomObj, options);
                //writer.WriteStringValue(value.OriginalString);
                //System.Text.Json.JsonSerializer.Serialize(writer, obj, obj.GetType(), options);
            }
        }

        private static void SerializePsProperties(Utf8JsonWriter writer, PSObject pso, object obj, bool isCustomObj, JsonSerializerOptions options)
        {
            bool wasDictionary = true;
            IDictionary dict = obj as IDictionary;

            if (dict == null)
            {
                wasDictionary = false;
                dict = new Dictionary<string, object>();
                dict.Add("value", obj);
            }

            AppendPsProperties(pso, dict, isCustomObj);

            if (wasDictionary == false && dict.Count == 1)
            {
                System.Text.Json.JsonSerializer.Serialize(writer, obj, obj?.GetType(), options);
                return;
            }

            System.Text.Json.JsonSerializer.Serialize(writer, dict, options);
        }

        private static void AppendPsProperties(PSObject psObj, IDictionary receiver, bool isCustomObject)
        {
            // serialize only Extended and Adapted properties..
            PSMemberInfoCollection<PSPropertyInfo> srcPropertiesToSearch =
                new PSMemberInfoIntegratingCollection<PSPropertyInfo>(psObj,
                    isCustomObject ? PSObject.GetPropertyCollection(PSMemberViewTypes.Extended | PSMemberViewTypes.Adapted) :
                    PSObject.GetPropertyCollection(PSMemberViewTypes.Extended));

            foreach (PSPropertyInfo prop in srcPropertiesToSearch)
            {
                if (prop is PSProperty psproperty && psproperty.IsDefined(typeof(System.Text.Json.Serialization.JsonIgnoreAttribute)))
                {
                    continue;
                }

                object value = null;
                try
                {
                    value = prop.Value;
                }
                catch (Exception)
                {
                }

                if (!receiver.Contains(prop.Name))
                {
                    receiver[prop.Name] = value;
                }
            }
        }

        private static IDictionary ProcessCustomObject<T>(object o)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            Type t = o.GetType();

            foreach (FieldInfo info in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!info.IsDefined(typeof(T), true))
                {
                    object value;
                    try
                    {
                        value = info.GetValue(o);
                    }
                    catch (Exception)
                    {
                        value = null;
                    }

                    result.Add(info.Name, value);
                }
            }

            foreach (PropertyInfo info2 in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!info2.IsDefined(typeof(T), true))
                {
                    MethodInfo getMethod = info2.GetGetMethod();
                    if ((getMethod != null) && (getMethod.GetParameters().Length <= 0))
                    {
                        object value;
                        try
                        {
                            value = getMethod.Invoke(o, Array.Empty<object>());
                        }
                        catch (Exception)
                        {
                            value = null;
                        }

                        result.Add(info2.Name, value);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Return an alternate representation of the specified object that serializes the same JSON, except
        /// that properties that cannot be evaluated are treated as having the value null.
        /// Primitive types are returned verbatim.  Aggregate types are processed recursively.
        /// </summary>
        /// <param name="obj">The object to be processed.</param>
        /// <param name="currentDepth">The current depth into the object graph.</param>
        /// <param name="context">The context to use for the convert-to-json operation.</param>
        /// <returns>An object suitable for serializing to JSON.</returns>
        private static object ProcessValue(object obj, int currentDepth, in ConvertToJsonContext context)
        {
            context.CancellationToken.ThrowIfCancellationRequested();

            if (LanguagePrimitives.IsNull(obj))
            {
                return null;
            }

            PSObject pso = obj as PSObject;

            if (pso != null)
            {
                obj = pso.BaseObject;
            }

            object rv = obj;
            bool isPurePSObj = false;
            bool isCustomObj = false;

            if (obj == NullString.Value
                || obj == DBNull.Value)
            {
                rv = null;
            }
            else if (obj is string
                    || obj is char
                    || obj is bool
                    || obj is DateTime
                    || obj is DateTimeOffset
                    || obj is Guid
                    || obj is Uri
                    || obj is double
                    || obj is float
                    || obj is decimal)
            {
                rv = obj;
            }
            else if (obj is Newtonsoft.Json.Linq.JObject jObject)
            {
                rv = jObject.ToObject<Dictionary<object, object>>();
            }
            else
            {
                Type t = obj.GetType();

                if (t.IsPrimitive)
                {
                    rv = obj;
                }
                else if (t.IsEnum)
                {
                    // Win8:378368 Enums based on System.Int64 or System.UInt64 are not JSON-serializable
                    // because JavaScript does not support the necessary precision.
                    Type enumUnderlyingType = Enum.GetUnderlyingType(obj.GetType());
                    if (enumUnderlyingType.Equals(typeof(Int64)) || enumUnderlyingType.Equals(typeof(UInt64)))
                    {
                        rv = obj.ToString();
                    }
                    else
                    {
                        rv = obj;
                    }
                }
                else
                {
                    if (currentDepth > context.MaxDepth)
                    {
                        if (pso != null && pso.ImmediateBaseObjectIsEmpty)
                        {
                            // The obj is a pure PSObject, we convert the original PSObject to a string,
                            // instead of its base object in this case
                            rv = LanguagePrimitives.ConvertTo(pso, typeof(string),
                                CultureInfo.InvariantCulture);
                            isPurePSObj = true;
                        }
                        else
                        {
                            rv = LanguagePrimitives.ConvertTo(obj, typeof(string),
                                CultureInfo.InvariantCulture);
                        }
                    }
                    else
                    {
                        IDictionary dict = obj as IDictionary;
                        if (dict != null)
                        {
                            rv = ProcessDictionary(dict, currentDepth, in context);
                        }
                        else
                        {
                            IEnumerable enumerable = obj as IEnumerable;
                            if (enumerable != null)
                            {
                                rv = ProcessEnumerable(enumerable, currentDepth, in context);
                            }
                            else
                            {
                                rv = ProcessCustomObject<Newtonsoft.Json.JsonIgnoreAttribute>(obj, currentDepth, in context);
                                isCustomObj = true;
                            }
                        }
                    }
                }
            }

            rv = AddPsProperties(pso, rv, currentDepth, isPurePSObj, isCustomObj, in context);

            return rv;
        }

        /// <summary>
        /// Add to a base object any properties that might have been added to an object (via PSObject) through the Add-Member cmdlet.
        /// </summary>
        /// <param name="psObj">The containing PSObject, or null if the base object was not contained in a PSObject.</param>
        /// <param name="obj">The base object that might have been decorated with additional properties.</param>
        /// <param name="depth">The current depth into the object graph.</param>
        /// <param name="isPurePSObj">The processed object is a pure PSObject.</param>
        /// <param name="isCustomObj">The processed object is a custom object.</param>
        /// <param name="context">The context for the operation.</param>
        /// <returns>
        /// The original base object if no additional properties had been added,
        /// otherwise a dictionary containing the value of the original base object in the "value" key
        /// as well as the names and values of an additional properties.
        /// </returns>
        private static object AddPsProperties(object psObj, object obj, int depth, bool isPurePSObj, bool isCustomObj, in ConvertToJsonContext context)
        {
            PSObject pso = psObj as PSObject;

            if (pso == null)
            {
                return obj;
            }

            // when isPurePSObj is true, the obj is guaranteed to be a string converted by LanguagePrimitives
            if (isPurePSObj)
            {
                return obj;
            }

            bool wasDictionary = true;
            IDictionary dict = obj as IDictionary;

            if (dict == null)
            {
                wasDictionary = false;
                dict = new Dictionary<string, object>();
                dict.Add("value", obj);
            }

            AppendPsProperties(pso, dict, depth, isCustomObj, in context);

            if (wasDictionary == false && dict.Count == 1)
            {
                return obj;
            }

            return dict;
        }

        /// <summary>
        /// Append to a dictionary any properties that might have been added to an object (via PSObject) through the Add-Member cmdlet.
        /// If the passed in object is a custom object (not a simple object, not a dictionary, not a list, get processed in ProcessCustomObject method),
        /// we also take Adapted properties into account. Otherwise, we only consider the Extended properties.
        /// When the object is a pure PSObject, it also gets processed in "ProcessCustomObject" before reaching this method, so we will
        /// iterate both extended and adapted properties for it. Since it's a pure PSObject, there will be no adapted properties.
        /// </summary>
        /// <param name="psObj">The containing PSObject, or null if the base object was not contained in a PSObject.</param>
        /// <param name="receiver">The dictionary to which any additional properties will be appended.</param>
        /// <param name="depth">The current depth into the object graph.</param>
        /// <param name="isCustomObject">The processed object is a custom object.</param>
        /// <param name="context">The context for the operation.</param>
        private static void AppendPsProperties(PSObject psObj, IDictionary receiver, int depth, bool isCustomObject, in ConvertToJsonContext context)
        {
            // serialize only Extended and Adapted properties..
            PSMemberInfoCollection<PSPropertyInfo> srcPropertiesToSearch =
                new PSMemberInfoIntegratingCollection<PSPropertyInfo>(psObj,
                    isCustomObject ? PSObject.GetPropertyCollection(PSMemberViewTypes.Extended | PSMemberViewTypes.Adapted) :
                    PSObject.GetPropertyCollection(PSMemberViewTypes.Extended));

            foreach (PSPropertyInfo prop in srcPropertiesToSearch)
            {
                object value = null;
                try
                {
                    value = prop.Value;
                }
                catch (Exception)
                {
                }

                if (!receiver.Contains(prop.Name))
                {
                    receiver[prop.Name] = ProcessValue(value, depth + 1, in context);
                }
            }
        }

        /// <summary>
        /// Return an alternate representation of the specified dictionary that serializes the same JSON, except
        /// that any contained properties that cannot be evaluated are treated as having the value null.
        /// </summary>
        private static object ProcessDictionary(IDictionary dict, int depth, in ConvertToJsonContext context)
        {
            Dictionary<string, object> result = new Dictionary<string, object>(dict.Count);

            foreach (DictionaryEntry entry in dict)
            {
                string name = entry.Key as string;
                if (name == null)
                {
                    // use the error string that matches the message from JavaScriptSerializer
                    string errorMsg = string.Format(
                        CultureInfo.CurrentCulture,
                        WebCmdletStrings.NonStringKeyInDictionary,
                        dict.GetType().FullName);

                    var exception = new InvalidOperationException(errorMsg);
                    if (context.Cmdlet != null)
                    {
                        var errorRecord = new ErrorRecord(exception, "NonStringKeyInDictionary", ErrorCategory.InvalidOperation, dict);
                        context.Cmdlet.ThrowTerminatingError(errorRecord);
                    }
                    else
                    {
                        throw exception;
                    }
                }

                result.Add(name, ProcessValue(entry.Value, depth + 1, in context));
            }

            return result;
        }

        /// <summary>
        /// Return an alternate representation of the specified collection that serializes the same JSON, except
        /// that any contained properties that cannot be evaluated are treated as having the value null.
        /// </summary>
        private static object ProcessEnumerable(IEnumerable enumerable, int depth, in ConvertToJsonContext context)
        {
            List<object> result = new List<object>();

            foreach (object o in enumerable)
            {
                result.Add(ProcessValue(o, depth + 1, in context));
            }

            return result;
        }

        /// <summary>
        /// Return an alternate representation of the specified aggregate object that serializes the same JSON, except
        /// that any contained properties that cannot be evaluated are treated as having the value null.
        ///
        /// The result is a dictionary in which all public fields and public gettable properties of the original object
        /// are represented.  If any exception occurs while retrieving the value of a field or property, that entity
        /// is included in the output dictionary with a value of null.
        /// </summary>
        private static object ProcessCustomObject<T>(object o, int depth, in ConvertToJsonContext context)
        {
            Dictionary<string, object> result = new Dictionary<string, object>();
            Type t = o.GetType();

            foreach (FieldInfo info in t.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!info.IsDefined(typeof(T), true))
                {
                    object value;
                    try
                    {
                        value = info.GetValue(o);
                    }
                    catch (Exception)
                    {
                        value = null;
                    }

                    result.Add(info.Name, ProcessValue(value, depth + 1, in context));
                }
            }

            foreach (PropertyInfo info2 in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!info2.IsDefined(typeof(T), true))
                {
                    MethodInfo getMethod = info2.GetGetMethod();
                    if ((getMethod != null) && (getMethod.GetParameters().Length <= 0))
                    {
                        object value;
                        try
                        {
                            value = getMethod.Invoke(o, Array.Empty<object>());
                        }
                        catch (Exception)
                        {
                            value = null;
                        }

                        result.Add(info2.Name, ProcessValue(value, depth + 1, in context));
                    }
                }
            }

            return result;
        }

        #endregion ConvertToJson
    }
}
