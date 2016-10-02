
& "$PSScriptRoot\CopyFiles.ps1"

# Installmate setup
# & "C:\Program Files\InstallMate 9\BinX64\tin.exe" /build:all "West Wind Markdown Monster.im9" | Out-null

"Running Inno Setup..."
& "C:\Program Files (x86)\Inno Setup 5\iscc.exe" "MarkdownMonster.iss" 

"Zipping up setup file..."
7z a -tzip "$PSScriptRoot\Builds\CurrentRelease\MarkdownMonsterSetup.zip" ".\Builds\CurrentRelease\MarkdownMonsterSetup.exe"

"Done!"