$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.24/MarkdownMonsterSetup-1.24.14.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "70824AD9B2828B4258D74D08F1CCE96607A2A25BEEE7FFD2651F42F0206ABD7F" -checksumType "sha256"
