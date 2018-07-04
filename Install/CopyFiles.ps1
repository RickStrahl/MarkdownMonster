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

# Want to ship main PDB but not any others
Rename-Item ${target}\markdownmonster.pdb ${target}\markdownmonster.TPDB
Rename-Item ${target}\Dragablz.pdb ${target}\Dragablz.tpdb
Remove-Item ${target}\*.pdb
Rename-Item ${target}\markdownmonster.TPDB ${target}\markdownmonster.pdb
Rename-Item ${target}\Dragablz.tpdb ${target}\Dragablz.pdb

get-childitem .\distribution\addins\*.pdb -Recurse | Remove-Item
get-childitem .\distribution\addins\*.config -Recurse | Remove-Item
get-childitem .\distribution\addins\*.xml -Recurse | Remove-Item