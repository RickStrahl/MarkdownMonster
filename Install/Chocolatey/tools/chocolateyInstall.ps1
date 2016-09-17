$packageName = 'markdownmonster'
$fileType = 'exe'
$url = 'https://github.com/RickStrahl/MarkdownMonster/raw/master/Install/Builds/PreRelease/MarkdownMonsterSetup-0.48.exe'
$silentArgs = '/q'
$validExitCodes = @(0)

Install-ChocolateyPackage "$packageName" "$fileType" "$silentArgs" "$url"  -validExitCodes -checksum='BC06913C55F15D184669F21019864F9321419F483F1ACE57E0553858D25DC70A' -checksumType='sha256' $validExitCodes
