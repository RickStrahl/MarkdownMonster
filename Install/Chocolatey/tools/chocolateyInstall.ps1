$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.25/MarkdownMonsterSetup-1.25.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "49E61CF85163FAE69DAB2C2F12DD5E8AF7E7AC14DEF33C360F14881C0F025997" -checksumType "sha256"
