$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.24/MarkdownMonsterSetup-1.24.8.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "A8E30948D22448CC53E5F9F469CF26808049CE655DCD8D58E127A292B300ADB2" -checksumType "sha256"
