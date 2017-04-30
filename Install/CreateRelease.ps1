cd "$PSScriptRoot" 

$releaseFile = "$PSScriptRoot\builds\currentrelease\MarkdownMonsterSetup.exe"


$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($releaseFile).FileVersion
$version = $version.Trim().Trim(".0")
"Writing Version File for: " + $version

$finalFile = "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\v1.3\MarkdownMonsterSetup-${version}.exe"
copy $releaseFile $finalFile
cd "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases"

git add -f "v1.3/MarkdownMonsterSetup-${version}.exe"
git commit -m "$version"
git push origin master

cd "$PSScriptRoot" 