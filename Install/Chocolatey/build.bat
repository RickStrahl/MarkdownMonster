cd %~dp0
del *.nupkg
call choco pack
call choco install "MarkdownMonster" -fdv  -s "C:\projects2010\MarkdownMonster\Install\Chocolatey"
