cd "$PSScriptRoot" 

# Major version
$release = "v1.23" 
$releaseFile = "$PSScriptRoot\builds\currentrelease\MarkdownMonsterSetup.exe"
$versionFile = "$PSScriptRoot\builds\currentrelease\MarkdownMonster_Version.xml"
$portableFile = "$PSScriptRoot\builds\currentrelease\MarkdownMonsterPortable.zip"

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($releaseFile).FileVersion
"Raw version: " + $version
$version = $version.Trim().Replace(".0","") 
"Writing Version File for: " + $version

$finalFile = "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\$release\MarkdownMonsterSetup-${version}.exe"
$finalCurrentReleaseFolder = "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\CurrentRelease"


Copy-Item $releaseFile $finalFile
Copy-Item $versionFile $finalCurrentReleaseFolder\MarkdownMonster_Version.xml
Copy-Item $portableFile $finalCurrentReleaseFolder\MarkdownMonsterPortable.zip

Copy-Item $releaseFile "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\CurrentRelease\MarkdownMonsterSetup.exe"
cd "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases"

git add .
git stage
git commit -m "$version"
git push origin master

cd "$PSScriptRoot" 

$chocoNuspec = ".\chocolatey\markdownmonster.template.nuspec"
$content = Get-Content -Path $chocoNuspec
$content = $content.Replace("{{version}}",$version)
out-file -filepath $chocoNuSpec.Replace(".template","")  -inputobject $content
