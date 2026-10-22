# Stops all running java.exe and adb.exe processes.
foreach ($name in 'java', 'adb') {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    if ($procs) {
        $procs | Stop-Process -Force
    } else {
        Write-Host "process `"$name.exe`" not running."
    }
}
