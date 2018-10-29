$CaseFoldingTxt = './CaseFolding.txt'
function Load-CaseFoldingTxt
{
    Invoke-WebRequest -Uri http://www.unicode.org/Public/UCD/latest/ucd/CaseFolding.txt -OutFile $CaseFoldingTxt
}

function ConvertFromUtf32
{
    param([int32] $utf32)

    $utf32 = $utf32 - 0x10000
    $address0 = [int]($utf32 / 0x400) + 0xd800
    $address1 = $utf32 % 0x400 + 0xdc00
    [System.String]::Format('0x{0:X4}{1:X4}', $address0, $address1)
}
function Start-Gen
{
    $CaseFoldingTxtHeader = Get-Content -LiteralPath $CaseFoldingTxt -TotalCount 6 | ForEach-Object { "// $_" }

    $lines = Get-Content -LiteralPath $CaseFoldingTxt | Where-Object { !$_.StartsWith("#") -and $_ -ne "" }

    $SimpleCaseFoldingTableBMPaneIn = @()
    $SimpleCaseFoldingTableBMPaneOut = @()
    $lines | ForEach-Object {
        $blocks = $_ -split "; "
        if ($blocks[1] -eq "C" -or $blocks[1] -eq "S") {
            $SimpleCaseFoldingTableBMPaneIn += "0x" + $blocks[0] + ""
            $SimpleCaseFoldingTableBMPaneOut += "0x" + $blocks[2] + ""
            #Write-Host "$blocks[0] ($SimpleCaseFoldingTablePane01In) - $blocks[2] ($(ConvertFromUtf32 ("0x"+$blocks[2])))"
            #Sleep 5
        }
    }

    $fileDate = [System.Datetime]::Now.ToString("yyyyMMddmmss")

    $template = @"
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// The file automatically generated at $fileDate
// Source file is loaded from file http://www.unicode.org/Public/UCD/latest/ucd/CaseFolding.txt

$CaseFoldingTxtHeader

using System.Collections.Generic;

namespace System.Management.Automation.Unicode
{
    /// <summary>
    /// Simple case folding methods.
    /// </summary>
    internal static partial class SimpleCaseFolding
    {
        /// <summary>
        /// Lookup a char in the 's_simpleCaseFoldingTableSMPaneIn' table. Get a index. Use the index to lookup target char in 's_simpleCaseFoldingTableInOut'
        /// </summary>
        private static readonly List<Int32> s_simpleCaseFoldingTableSMPaneIn = new List<Int32>()
        {
            $($SimpleCaseFoldingTableBMPaneIn -join ",`n            ")
        };

        private static readonly List<Int32> s_simpleCaseFoldingTableSMPaneOut = new List<Int32>()
        {
            $($SimpleCaseFoldingTableBMPaneOut -join ",`n            ")
        };

    }
}
"@

    Set-Content -LiteralPath ".\CaseFolding-$fileDate.gen.cs" -Value $template
}
