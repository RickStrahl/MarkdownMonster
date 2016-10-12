cd %~dp0
call choco pack
call choco install "MarkdownMonster" -fdv  -s "C:\projects2010\MarkdownMonster\Install\Chocolatey"
