Set-ExecutionPolicy Bypass -Scope CurrentUser

$cur="$PSScriptRoot"
$source="$PSScriptRoot\..\MarkdownMonster"
$target="$PSScriptRoot\Distribution"

remove-item -recurse -force ${target}

robocopy ${source}\bin\Release ${target} /MIR

copy ${cur}\mm.exe ${target}\mm.exe

del ${target}\*.vshost.*
del ${target}\*.xml

ren ${target}\markdownmonster.pdb ${target}\markdownmonster.TPDB
del ${target}\*.pdb
ren ${target}\markdownmonster.TPDB ${target}\markdownmonster.pdb


get-childitem .\distribution\addins\*.pdb -Recurse | Remove-Item
get-childitem .\distribution\addins\*.xml -Recurse | Remove-Item