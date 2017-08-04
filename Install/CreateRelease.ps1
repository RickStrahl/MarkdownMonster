cd "$PSScriptRoot" 

$releaseFile = "$PSScriptRoot\builds\currentrelease\MarkdownMonsterSetup.exe"


$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($releaseFile).FileVersion
"Raw version: " + $version
$version = $version.Trim().Replace(".0","") 
"Writing Version File for: " + $version

$finalFile = "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\v1.5\MarkdownMonsterSetup-${version}.exe"
copy $releaseFile $finalFile
copy $releaseFile "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\CurrentRelease\MarkdownMonsterSetup.exe"
cd "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases"

git add -f "v1.5/MarkdownMonsterSetup-${version}.exe"
git commit -m "$version"
git push origin master

cd "$PSScriptRoot" 