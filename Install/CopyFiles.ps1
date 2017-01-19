Set-ExecutionPolicy Bypass -Scope CurrentUser

$cur="$PSScriptRoot"
$source="$PSScriptRoot\..\MarkdownMonster"
$target="$PSScriptRoot\Distribution"

remove-item -recurse -force ${target}

robocopy ${source}\bin\Release ${target} /MIR

copy ${cur}\mm.exe ${target}\mm.exe

del ${target}\*.vshost.*
del ${target}\*.pdb
del ${target}\*.xml

del ${target}\addins\*.pdb
del ${target}\addins\*.xml