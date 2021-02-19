$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.26/MarkdownMonsterSetup-1.26.3.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "28BB3F1F43A845FC50CDE794C53480C5FC31F4325EDF096D73C06799D38E0427" -checksumType "sha256"
