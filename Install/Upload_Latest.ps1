# param([string]$uid = "uid", [string]$pwd = "")

Set-ExecutionPolicy Bypass -Scope CurrentUser

# Credential set with:
#  Get-Credential | Export-CliXml  -Path .\FtpCredential.xml
$credential = Import-Clixml -Path .\FtpCredential.xml
if( $null -ne $credential)
{
    $uid = $credential.Username;
    $pwd = $credential.Password;    
}
else {
    # otherwise prompt for password
    $uid= Read-Host -Prompt 'Username' 
    $pwd=Read-Host -Prompt 'Password' -AsSecureString
}
$pwd = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($pwd))

Write-host $uid
write-host $pwd

if(!$pwd) {Exit;}

curl.exe -T ".\Builds\CurrentRelease\MarkdownMonsterSetup.exe"  "ftps://west-wind.com/Westwind_sysroot/Ftp/Files/MarkdownMonsterSetup_Latest.exe" -u ${uid}:${pwd} -k