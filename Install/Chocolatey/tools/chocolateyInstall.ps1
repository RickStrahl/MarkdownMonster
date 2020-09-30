$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.24/MarkdownMonsterSetup-1.24.9.exe'

$silentArgs = '/VERYSILENT'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes  $validExitCodes  -checksum "C32AD352E6BC3A4394A4298718E3338E4B2896E504FE8BB444FAFD5BA8468158" -checksumType "sha256"
