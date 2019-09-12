#Requires -RunAsAdministrator

$instanceName = "welcome-to-the-state-machine"

sqllocaldb stop $instanceName
sqllocaldb delete $instanceName

$databasesPath = "$ENV:UserProfile\$instanceName-databases"
mkdir -Force $databasesPath
rm -Recurse $databasesPath