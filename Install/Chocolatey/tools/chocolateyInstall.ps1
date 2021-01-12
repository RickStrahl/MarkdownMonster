$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.25/MarkdownMonsterSetup-1.25.16.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "73E844FEAA0F10BD905B28989BF365C2E1434A751160D1D6E991A9AF15814502" -checksumType "sha256"
