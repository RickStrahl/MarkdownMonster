# Script builds a Chocolatey Package and tests it locally
# 
#  Assumes: Uses latest release out of Pre-release folder
#           Release has been checked in to GitHub Repo
#   Builds: ChocolateyInstall.ps1 file with download URL and sha256 embedded

cd "$PSScriptRoot" 

copy ..\builds\currentrelease\MarkdownMonsterPortable.zip .\tools

del *.nupkg

# Create .nupkg from .nuspec
choco pack

choco uninstall "MarkdownMonster.Portable"

choco install "MarkdownMonster.Portable" -fdv -y  -s ".\" 