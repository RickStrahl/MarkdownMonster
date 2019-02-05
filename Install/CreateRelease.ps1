cd "$PSScriptRoot" 

# Major version
$release = "v1.15" 
$releaseFile = "$PSScriptRoot\builds\currentrelease\MarkdownMonsterSetup.exe"




$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($releaseFile).FileVersion
"Raw version: " + $version
$version = $version.Trim().Replace(".0","") 
"Writing Version File for: " + $version

$finalFile = "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\$release\MarkdownMonsterSetup-${version}.exe"
copy $releaseFile $finalFile
copy $releaseFile "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\CurrentRelease\MarkdownMonsterSetup.exe"
cd "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases"

git add -f "${release}/MarkdownMonsterSetup-${version}.exe"
git commit -m "$version"
git push origin master

cd "$PSScriptRoot" 