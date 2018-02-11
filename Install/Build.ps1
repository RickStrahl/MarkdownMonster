cd "$PSScriptRoot" 
& "$PSScriptRoot\CopyFiles.ps1"

& ".\signtool.exe" sign /v /n "West Wind Technologies" /sm /s MY /tr "http://timestamp.digicert.com" /td SHA256 /fd SHA256 ".\Distribution\MarkdownMonster.exe"
& ".\signtool.exe" sign /v /n "West Wind Technologies" /sm /s MY /tr "http://timestamp.digicert.com" /td SHA256 /fd SHA256 ".\Distribution\mm.exe"

"Running Inno Setup..."
& "C:\Program Files (x86)\Inno Setup 5\iscc.exe" "MarkdownMonster.iss" 

"Signing the main EXE..."
& ".\signtool.exe" sign /v /n "West Wind Technologies" /sm  /tr "http://timestamp.digicert.com" /td SHA256 /fd SHA256 ".\Builds\CurrentRelease\MarkdownMonsterSetup.exe"

copy ".\MarkdownMonsterPortable.md" ".\Distribution"

"Zipping up setup file..."
del ".\Builds\CurrentRelease\MarkdownMonsterSetup.zip"
7z a -tzip ".\Builds\CurrentRelease\MarkdownMonsterSetup.zip" ".\Builds\CurrentRelease\MarkdownMonsterSetup.exe"

"Zipping up portable setup file..."
del ".\Builds\CurrentRelease\MarkdownMonsterPortable.zip"
7z a -tzip -r ".\Builds\CurrentRelease\MarkdownMonsterPortable.zip" ".\Distribution\*.*"
7z a -tzip ".\Builds\CurrentRelease\MarkdownMonsterPortable.zip" ".\MarkdownMonsterPortable.md"


$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo("$PSScriptRoot\builds\currentrelease\MarkdownMonsterSetup.exe").FileVersion
$version = $version.Trim()
"Initial Version: " + $version

# Remove 4th version tuple
try{
    $al = New-Object System.Collections.ArrayList( $null )
    $al.AddRange($version.Split("."))
    $al.RemoveAt(3)
    $version = [System.String]::Join(".", $al.ToArray())
}
catch{ }
"Truncated Version: " + $version

"Writing Version File for: " + $version
$versionFilePath = ".\builds\currentrelease\MarkdownMonster_Version_Template.xml"
$versionFile = Get-Content -Path $versionFilePath  

$versionFile = $versionFile.Replace("{{version}}",$version).Replace("{{preview-version}}",$version).Replace("{{date}}",[System.DateTime]::Now.ToString("MMMM d, yyyy"))
$versionFile
""

out-file -filepath $versionFilePath.Replace("_Template","")  -inputobject $versionFile


get-childitem .\builds\CurrentRelease\* -include *.* | foreach-object { "{0}`t{1}`t{2:n0}`t`t{3}" -f $_.Name, $_.LastWriteTime, $_.Length, [System.Diagnostics.FileVersionInfo]::GetVersionInfo($_).FileVersion }
""
"Done..."