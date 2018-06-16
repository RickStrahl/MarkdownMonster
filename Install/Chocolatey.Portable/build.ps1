# Script builds a Chocolatey Package and tests it locally
# 
#  Assumes: Uses latest release out of Pre-release folder
#           Release has been checked in to GitHub Repo
#   Builds: ChocolateyInstall.ps1 file with download URL and sha256 embedded

Set-Location "$PSScriptRoot" 

Copy-Item ..\builds\currentrelease\MarkdownMonsterPortable.zip .\tools

$sha = get-filehash -path ".\tools\MarkdownMonsterPortable.zip" -Algorithm SHA256  | select -ExpandProperty "Hash"
write-host $sha

$filetext = @"
`VERIFICATION
`MarkdownMonster.Portable.zip
`SHA256 Checksum Value: $sha
"@
out-file -filepath .\tools\Verification.txt -inputobject $filetext

Remove-Item *.nupkg

# Create .nupkg from .nuspec    
choco pack

choco uninstall "MarkdownMonster.Portable"

choco install "MarkdownMonster.Portable" -fd -y  -s ".\" 