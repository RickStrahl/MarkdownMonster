$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.26/MarkdownMonsterSetup-1.26.2.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "DB8A0E8AFF9DC148206BE2C524F99C183B9EC967EE3D3C3DFF2504B7D9C483EA" -checksumType "sha256"
