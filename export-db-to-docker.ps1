# Script export/sync database tu SQL Server local len Docker container circleapp-db
$LocalInstance = "DESKTOP-GE26PFI\SQLEXPRESS"
$SourceDb = "CircleAppDbCopy"
$TargetDb = "CircleAppDb"
$TempBakHost = "$PSScriptRoot\temp_circleapp.bak"
$ContainerName = "circleapp-db"
$SaPassword = "YourStrong@Password123"

# Doc password tu .env neu co
if (Test-Path "$PSScriptRoot\.env") {
    Get-Content "$PSScriptRoot\.env" | ForEach-Object {
        if ($_ -match "^MSSQL_SA_PASSWORD=(.*)$") {
            $SaPassword = $matches[1].Trim()
        }
    }
}

Write-Host "1. Dang backup database $SourceDb tu $LocalInstance..." -ForegroundColor Cyan
sqlcmd -S $LocalInstance -E -Q "BACKUP DATABASE [$SourceDb] TO DISK = '$TempBakHost' WITH FORMAT;"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Loi khi backup database local!" -ForegroundColor Red
    exit 1
}

Write-Host "2. Dang copy file backup vao container $ContainerName..." -ForegroundColor Cyan
docker cp $TempBakHost "${ContainerName}:/var/opt/mssql/backup.bak"

Write-Host "3. Dang restore vao database $TargetDb tren container..." -ForegroundColor Cyan
$restoreCmd = "ALTER DATABASE [$TargetDb] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; RESTORE DATABASE [$TargetDb] FROM DISK = '/var/opt/mssql/backup.bak' WITH REPLACE, MOVE 'CircleAppDb' TO '/var/opt/mssql/data/CircleAppDb.mdf', MOVE 'CircleAppDb_log' TO '/var/opt/mssql/data/CircleAppDb_log.ldf'; ALTER DATABASE [$TargetDb] SET MULTI_USER;"
docker exec $ContainerName /opt/mssql-tools18/bin/sqlcmd -S 127.0.0.1 -U sa -P $SaPassword -C -Q $restoreCmd

Write-Host "4. Dang restart web container de cap nhat connection pool..." -ForegroundColor Cyan
docker restart circleapp-web

Write-Host "5. Don dep file backup tam..." -ForegroundColor Cyan
Remove-Item -Path $TempBakHost -Force -ErrorAction SilentlyContinue
docker exec $ContainerName rm -f /var/opt/mssql/backup.bak

Write-Host "==> XONG! Toan bo data tu $SourceDb da duoc sync thanh cong len Docker container." -ForegroundColor Green
