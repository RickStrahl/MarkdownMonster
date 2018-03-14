# Script builds a Chocolatey Package and tests it locally
# 
#  Assumes: Uses latest release out of Pre-release folder
#           Release has been checked in to GitHub Repo
#   Builds: ChocolateyInstall.ps1 file with download URL and sha256 embedded

cd "$PSScriptRoot" 

$releasePath = "C:\projects2010\MarkdownMonsterAddins\MarkdownMonsterReleases\v1.9"

#$file = "MarkdownMonsterSetup-0.55.exe"
$file = gci "$releasePath" | sort LastWriteTime | select -last 1 | select -ExpandProperty "Name"
write-host $file

$sha = get-filehash -path "$releasePath\$file" -Algorithm SHA256  | select -ExpandProperty "Hash"
write-host $sha


$filetext = @"
`$packageName = 'markdownmonster'
`$fileType = 'exe'
`$url = 'https://github.com/RickStrahl/MarkdownMonsterReleases/raw/master/v1.9/$file'

`$silentArgs = '/VERYSILENT'
`$validExitCodes = @(0)

Install-ChocolateyPackage "`packageName" "`$fileType" "`$silentArgs" "`$url"  -validExitCodes  `$validExitCodes  -checksum "$sha" -checksumType "sha256"
"@

out-file -filepath .\tools\chocolateyinstall.ps1 -inputobject $filetext

del *.nupkg

# Create .nupkg from .nuspec
choco pack

choco uninstall "MarkdownMonster" -f

choco install "MarkdownMonster" -fd  -y -s ".\"
#choco install "MarkdownMonster" -fdv  -y -s ".\"

# choco push