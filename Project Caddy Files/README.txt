**README**
Attached in this file is all of the prompts you will need to get the server loaded, there should be the following:
	- caddyfile (file)
	- caddycmdprompt.txt (used to initiate the final cmd prompts)
	- duckdns.ps1 (powershell prompt used with task)
In order to get it to run properly you will need:
	- caddy w/ duckdns extension downloaded
	- duck dns downloaded
	- create a task to run "duckdns.ps1" every 5 minutes
		- trigger: set to run every 5 minutes
		- action: run powershell.exe with '-File "C:\path\to\duckdns.ps1"'
	- create a caddy directory file on your computer
	- put the caddyfile file in the directory with caddy_windows_amd64_custom.exe
	- run caddycmdprompt.txt (copy and paste into cmd.exe)
	- Ensure ports 80 and 443 are open on your network and firewall
