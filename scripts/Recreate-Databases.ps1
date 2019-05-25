#Requires -RunAsAdministrator

$instanceName = "welcome-to-the-state-machine"

$serverName = "(localdb)\" + $instanceName
sqlcmd -S $serverName -i ".\Teardown-Databases.sql"
sqlcmd -S $serverName -i ".\Setup-Databases.sql"