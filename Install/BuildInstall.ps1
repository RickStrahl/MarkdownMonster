cd "$PSScriptRoot" 
& "$PSScriptRoot\CopyFiles.ps1"


& "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\signtool.exe" sign /v /n "West Wind Technologies" /sm /s MY /tr "http://timestamp.digicert.com" /td SHA256 /fd SHA256 ".\Distribution\MarkdownMonster.exe"

# Installmate setup
#& "C:\Program Files\InstallMate 9\BinX64\tin.exe" /build:all "West Wind Markdown Monster.im9" | Out-null

"Running Inno Setup..."
& "C:\Program Files (x86)\Inno Setup 5\iscc.exe" "MarkdownMonster.iss" 

& "C:\Program Files (x86)\Microsoft SDKs\Windows\v7.1A\Bin\signtool.exe" sign /v /n "West Wind Technologies" /sm  /tr "http://timestamp.digicert.com" /td SHA256 /fd SHA256 ".\Builds\CurrentRelease\MarkdownMonsterSetup.exe"


"Zipping up setup file..."
7z a -tzip ".\Builds\CurrentRelease\MarkdownMonsterSetup.zip" ".\Builds\CurrentRelease\MarkdownMonsterSetup.exe"

"Done!"