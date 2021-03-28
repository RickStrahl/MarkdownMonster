$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.26/MarkdownMonsterSetup-1.26.5.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "4C576F2AB5824E84E31C3693025ACD3C741C1F6BF3FF793C9B0948263B780EF9" -checksumType "sha256"
