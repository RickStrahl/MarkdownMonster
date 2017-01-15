# Script builds a Chocolatey Package and tests it locally
# 
#  Assumes: Uses latest release out of Pre-release folder
#           Release has been checked in to GitHub Repo
#   Builds: ChocolateyInstall.ps1 file with download URL and sha256 embedded

cd "$PSScriptRoot" 

remove-item ".\tools" -recurse -force

$sourceFolder = "..\Distribution" 
$file = "$sourceFolder\MarkdownMonster.exe"
write-host $file

$sha = get-filehash -path "$file" -Algorithm SHA256  | select -ExpandProperty "Hash"
write-host $sha

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("..\Builds\CurrentRelease\MarkdownMonsterSetup.exe").FileVersion
write-host $version

robocopy $sourceFolder .\tools /MIR


#empty install file - we just have content no code
$filetext = ""
out-file -filepath .\tools\chocolateyinstall.ps1 -inputobject $filetext

# uninstall script
copy chocolateyuninstall.ps1 .\tools

$filetext = @"
MarkdownMonster.exe
Sha256: $sha
"@

out-file -filepath .\tools\verify.txt -inputobject $filetext

del *.nupkg

# Create .nupkg from .nuspec
choco pack

#choco uninstall "MarkdownMonster.Portable"

choco install "MarkdownMonster.Portable" -fdv -y  -s ".\" 
