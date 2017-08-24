#Set-ExecutionPolicy Bypass -Scope CurrentUser

$cur="$PSScriptRoot"
$source="$PSScriptRoot\..\MarkdownMonster"
$target="$PSScriptRoot\Distribution"

remove-item -recurse -force ${target}

robocopy ${source}\bin\Release ${target} /MIR

Copy-Item ${cur}\mm.exe ${target}\mm.exe

Remove-Item ${target}\*.vshost.*
Remove-Item ${target}\*.xml

Rename-Item ${target}\markdownmonster.pdb ${target}\markdownmonster.TPDB
Remove-Item ${target}\*.pdb
Rename-Item ${target}\markdownmonster.TPDB ${target}\markdownmonster.pdb

get-childitem .\distribution\addins\*.pdb -Recurse | Remove-Item
get-childitem .\distribution\addins\*.config -Recurse | Remove-Item
get-childitem .\distribution\addins\*.xml -Recurse | Remove-Item