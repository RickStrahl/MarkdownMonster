$root = "$PSScriptRoot" 
cd $root
cd "Builds\CurrentRelease"

& .\MarkdownMonsterSetup.exe /silent

cd $root