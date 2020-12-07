$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.25/MarkdownMonsterSetup-1.25.8.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "CEC1547363F615EF839B0774F080708097C7CA8C1B34E2A549629FE51F155C1B" -checksumType "sha256"
