#Set-ExecutionPolicy Bypass -Scope CurrentUser

$cur="$PSScriptRoot"
$source="$PSScriptRoot\..\MarkdownMonster"
$target="$PSScriptRoot\Distribution"

# delete the Distribution folder
remove-item -recurse -force ${target}

# copy but exclude libGit extra folders
robocopy ${source}\bin\Release\net462 ${target} /MIR /XD lib /XF git2*.pdb

robocopy ${source}\bin\Release\net462\lib\win32 ${target}\lib\win32 /MIR /XF git2*.pdb

Copy-Item ${cur}\mm.exe ${target}\mm.exe
Copy-Item ${cur}\license.md ${target}\license.md

# Cleanup output
Remove-Item ${target}\*.vshost.*
Remove-Item ${target}\*.xml
Remove-Item ${target}\*.user
Remove-Item ${target}\*.dll.config
Remove-Item ${target}\.vs -Recurse -Force


# Roslyn - remove extra files
Remove-Item ${target}\Addins\Snippets\roslyn -Recurse -Force
Remove-Item ${target}\roslyn\Microsoft.CodeAnalysis.VisualBasic.dll
Remove-Item ${target}\roslyn\Microsoft.DiaSymReader.Native.amd64.dll
Remove-Item ${target}\roslyn\Microsoft.DiaSymReader.Native.x86.dll

# Want to ship MM PDB but not any others
Remove-Item ${target}\*.pdb -Exclude markdownmonster.pdb

# Cleanup Addins folder
get-childitem ${target}\Addins\*.pdb -Recurse | Remove-Item
get-childitem ${target}\Addins\*.config -Recurse | Remove-Item
get-childitem ${target}\Addins\*.xml -Recurse | Remove-Item