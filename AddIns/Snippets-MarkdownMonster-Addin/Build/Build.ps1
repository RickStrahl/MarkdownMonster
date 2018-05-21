Set-Location "$PSScriptRoot" 

$src = "$env:appdata\Markdown Monster\Addins\Snippets"
"Copying from: $src"

"Cleaning up build files..."
Remove-Item addin.zip

remove-item -recurse -force .\Distribution
md .\Distribution

"Copying files..."
Copy-Item "$src\*.dll" .\Distribution
Copy-Item "$src\version.json" .\Distribution
Copy-Item "$src\version.json" .\

"Zipping up setup file..."
.\7z a -tzip  addin.zip .\Distribution\*.*

remove-item .\Distribution -recurse

Get-ChildItem