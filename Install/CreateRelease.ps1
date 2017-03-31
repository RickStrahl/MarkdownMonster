cd "$PSScriptRoot" 

$releaseFile = "$PSScriptRoot\builds\currentrelease\MarkdownMonsterSetup.exe"


$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($releaseFile).FileVersion
$version = $version.Trim().Trim(".0")
"Writing Version File for: " + $version

copy $releaseFile "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\v1.2\MarkdownMonsterSetup-${version}.exe"
cd "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases"

git commit -m "$version"
# git push origin master

cd "$PSScriptRoot" 