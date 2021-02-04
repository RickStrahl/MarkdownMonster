$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.26/MarkdownMonsterSetup-1.26.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "7C3C13106A64C4D7C3DA1A3F44E21ECAB0B3C283257566ED0685107B93B1D239" -checksumType "sha256"
