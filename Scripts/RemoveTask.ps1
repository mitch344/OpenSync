#Run Powershell as Admin
# Be sure set "Set-ExecutionPolicy RemoteSigned" to allow locally created Powershell scripts to run
# Also be sure to Right Click on the script file got to Properties and Unblock

$TaskName = "OpenSync"

if (Get-ScheduledTask | Where-Object {$_.TaskName -like $TaskName}) {
    Unregister-ScheduledTask -TaskName $TaskName -Confirm:$false
    Write-Host "The scheduled task '$TaskName' has been removed."
} else {
    Write-Host "The scheduled task '$TaskName' does not exist."
}
