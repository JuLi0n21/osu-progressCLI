Set WshShell = CreateObject("WScript.Shell" ) 
WshShell.Run "taskkill /IM osu!progressCLI.exe /F"
Set WshShell = Nothing 