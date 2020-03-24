#Set-ExecutionPolicy Bypass -Scope CurrentUser

$cur="$PSScriptRoot"

$netfull = $True;    # netfull is full framework - otherwise net core
if($netfull) {
    $source="$PSScriptRoot\..\MarkdownMonster\bin\Release\net472\win-x86"
}
else {
    $source="$PSScriptRoot\..\MarkdownMonster\bin\Release\netcoreapp3.0\win-x86"
}    
$target="$PSScriptRoot\Distribution"
# edpnotify.exe

# delete the Distribution folder
remove-item -recurse -force ${target}

# copy but exclude libGit extra folders
robocopy ${source} ${target} /MIR /XD lib /XD runtimes 

if ($netfull) {
    robocopy ${source}\lib\win32 ${target}\lib\win32 /MIR /XF *.pdb
    robocopy ${source}\lib\win64 ${target}\lib\win64 /MIR /XF *.pdb
}
else {
    robocopy ${source}\runtimes\win-x86 ${target}\runtimes\win-x86 /MIR 
    robocopy ${source}\runtimes\win-x64 ${target}\runtimes\win-x64 /MIR 
    robocopy ${source}\runtimes\win ${target}\runtimes\win /MIR 
    robocopy ${source}\runtimes\win7 ${target}\runtimes\win7 /MIR
}

Copy-Item ${cur}\license.md ${target}\license.md

# Cleanup output
Remove-Item ${target}\*.vshost.*
Remove-Item ${target}\*.user
Remove-Item ${target}\*.dll.config
Remove-Item ${target}\*.xml -Exclude MarkdownMonster.xml
Remove-Item ${target}\registered.key

if ([System.IO.Directory]::Exists($target + "\.vs")) {
    Remove-Item ${target}\.vs -Recurse -Force
}

# Roslyn - remove extra files
# Remove-Item ${target}\Addins\Snippets\roslyn -Recurse -Force

Remove-Item ${target}\roslyn\Microsoft.CodeAnalysis.VisualBasic.dll
Remove-Item ${target}\roslyn\Microsoft.DiaSymReader.Native.amd64.dll
Remove-Item ${target}\roslyn\Microsoft.DiaSymReader.Native.x86.dll

# Ship MM PDB but none of the others
Remove-Item ${target}\*.pdb -Exclude markdownmonster.pdb

# Remove Cecil unused assemblies
Remove-item ${target}\mono.Cecil.*.dll

# Cleanup Addins folder
get-childitem ${target}\Addins\*.pdb -Recurse | Remove-Item
get-childitem ${target}\Addins\*.config -Recurse | Remove-Item
get-childitem ${target}\Addins\*.xml -Recurse | Remove-Item