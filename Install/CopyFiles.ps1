#Set-ExecutionPolicy Bypass -Scope CurrentUser

$cur="$PSScriptRoot"
$source="$PSScriptRoot\..\MarkdownMonster"
$target="$PSScriptRoot\Distribution"

# delete the Distribution folder
remove-item -recurse -force ${target}

# copy but exclude libGit extra folders
robocopy ${source}\bin\Release ${target} /MIR /XD linux osx /XF git2*.pdb

Copy-Item ${cur}\mm.exe ${target}\mm.exe
Copy-Item ${cur}\license.md ${target}\license.md

Remove-Item ${target}\*.vshost.*
Remove-Item ${target}\*.xml

Remove-Item ${target}\Addins\Snippets\roslyn -Recurse -Force
Remove-Item ${target}\roslyn\Microsoft.CodeAnalysis.VisualBasic.dll
Remove-Item ${target}\roslyn\Microsoft.DiaSymReader.Native.amd64.dll
Remove-Item ${target}\roslyn\Microsoft.DiaSymReader.Native.x86.dll

# Want to ship main PDB but not any others
Rename-Item ${target}\markdownmonster.pdb ${target}\markdownmonster.TPDB
Remove-Item ${target}\*.pdb
Rename-Item ${target}\markdownmonster.TPDB ${target}\markdownmonster.pdb

get-childitem .\distribution\addins\*.pdb -Recurse | Remove-Item
get-childitem .\distribution\addins\*.config -Recurse | Remove-Item
get-childitem .\distribution\addins\*.xml -Recurse | Remove-Item