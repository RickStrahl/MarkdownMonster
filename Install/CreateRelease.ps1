cd "$PSScriptRoot" 

# Major version
$release = "v1.23" 
$releaseFile = "$PSScriptRoot\builds\currentrelease\MarkdownMonsterSetup.exe"


$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($releaseFile).FileVersion
"Raw version: " + $version
$version = $version.Trim().Replace(".0","") 
"Writing Version File for: " + $version

$finalFile = "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\$release\MarkdownMonsterSetup-${version}.exe"
Copy-Item $releaseFile $finalFile
Copy-Item $releaseFile "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases\CurrentRelease\MarkdownMonsterSetup.exe"
cd "..\..\MarkdownMonsterAddins\MarkdownMonsterReleases"

git add -f "${release}/MarkdownMonsterSetup-${version}.exe"
git commit -m "$version"
git push origin master

cd "$PSScriptRoot" 

$chocoNuspec = ".\chocolatey\markdownmonster.template.nuspec"
$content = Get-Content -Path $chocoNuspec
$content = $content.Replace("{{version}}",$version)
out-file -filepath $chocoNuSpec.Replace(".template","")  -inputobject $content
