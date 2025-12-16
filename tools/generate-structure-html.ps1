<#
PowerShell script to generate an HTML file representing the project folder/file structure.
Generates a nested ordered list with anchors so that PDF converters will produce bookmarks.
Usage:
  .\generate-structure-html.ps1 -RootPath . -OutputFile ..\docs\project-structure.htm
#>
param(
    [Parameter(Mandatory=$false)]
    [string]$RootPath = ".",

    [Parameter(Mandatory=$false)]
    [string]$OutputFile = "..\docs\project-structure.htm",

    [Parameter(Mandatory=$false)]
    [string[]]$Exclude = @("bin","obj",".git"),

    [Parameter(Mandatory=$false)]
    [int]$MaxDepth = 10
)

function Write-Node([System.IO.DirectoryInfo]$dir, [int]$level) {
    if ($level -gt $MaxDepth) { return }
    $id = [System.Uri]::EscapeDataString($dir.FullName)
    Add-Content -Path $global:out -Value "<li><a id='dir-$id' href='#dir-$id'>$($dir.Name)/</a>"
    Add-Content -Path $global:out -Value "<ol>"
    foreach ($sub in $dir.GetDirectories() | Sort-Object Name) {
        if ($Exclude -contains $sub.Name) { continue }
        Write-Node $sub ($level + 1)
    }
    foreach ($file in $dir.GetFiles() | Sort-Object Name) {
        if ($Exclude -contains $file.Name) { continue }
        $fid = [System.Uri]::EscapeDataString($file.FullName)
        Add-Content -Path $global:out -Value "<li><a id='file-$fid' href='#file-$fid'>$($file.Name)</a></li>"
    }
    Add-Content -Path $global:out -Value "</ol></li>"
}

# Resolve root
$rootItem = Get-Item -Path $RootPath -ErrorAction Stop
if (-not ($rootItem -is [System.IO.DirectoryInfo])) {
    $root = Get-Item -Path $RootPath | Get-Item
} else {
    $root = $rootItem
}

# Ensure output directory exists and compute full path
$outputFullPath = [System.IO.Path]::GetFullPath($OutputFile)
$outputDir = Split-Path -Path $outputFullPath -Parent
if (-not (Test-Path -Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
}
$global:out = $outputFullPath

# Write header
"<!doctype html>" | Out-File -FilePath $global:out -Encoding utf8
"<html><head><meta charset='utf-8'><title>Project Structure</title></head><body>" | Out-File -FilePath $global:out -Append -Encoding utf8
"<h1>Project Structure: $($root.FullName)</h1>" | Out-File -FilePath $global:out -Append -Encoding utf8
"<ol>" | Out-File -FilePath $global:out -Append -Encoding utf8
Write-Node $root 1
"</ol></body></html>" | Out-File -FilePath $global:out -Append -Encoding utf8

Write-Host "Generated $global:out"