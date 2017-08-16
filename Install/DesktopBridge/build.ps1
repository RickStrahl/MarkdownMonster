#REM  Run from DesktopConverter Command Line
DesktopAppConverter.exe `
    -Destination C:\Temp\AppxBaseImages\MarkdownMonster `
    -Installer ..\Builds\CurrentRelease\MarkdownMonsterSetup.exe `
    -InstallerArguments "/VERYSILENT" `    
    -PackageName "MarkdownMonster" `
    -Publisher "CN=West Wind Technologies,C=US" `
    -Version "1.5.1.3" `
    -AppExecutable "MarkdownMonster.exe" `
    -PackageDisplayName "Markdown Monster" `
    -PackagePublisherDisplayName "West Wind Technologies" `
    -AppDisplayName "Markdown Monster" `
    -AppDescription "A rich Markdown Editor and Weblog Publisher" `
    -AppFileTypes "'.md', '.markdown', '.mdcrypt'" `
    -MakeAppx -Verbose -sign -verify