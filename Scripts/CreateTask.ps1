#Script Notes:
#Run Powershell as Admin
# Be sure set "Set-ExecutionPolicy RemoteSigned" to allow locally created Powershell scripts to run
# Also be sure to Right Click on the script file got to Properties and Unblock
#Point the file chooser to the OpenSync.exe where you have it installed.

Add-Type -AssemblyName System.Windows.Forms
$FileDialog = New-Object System.Windows.Forms.OpenFileDialog
$FileDialog.filter = "Executable files (*.exe)|*.exe"
$DialogResult = $FileDialog.ShowDialog()

if ($DialogResult -eq [System.Windows.Forms.DialogResult]::OK) {
    $ExePath = $FileDialog.FileName
    $TaskName = "OpenSync"
    $Action = New-ScheduledTaskAction -Execute $ExePath
    $Trigger = New-ScheduledTaskTrigger -AtLogon
    Register-ScheduledTask -Action $Action -Trigger $Trigger -TaskName $TaskName -Description "Syncing Software"
    
    Write-Host "Task has been created to run $ExePath at user logon."
} else {
    Write-Host "No file selected. Exiting script."
}
