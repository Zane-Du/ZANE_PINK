$viewsDir = "C:\Users\34163\Desktop\ZANE2027\ZANE\ZANE\Views"
Get-ChildItem $viewsDir -Filter "*.xaml" | ForEach-Object {
    $content = [System.IO.File]::ReadAllText($_.FullName, [System.Text.Encoding]::UTF8)
    $newContent = $content -replace '\{StaticResource ', '{DynamicResource '
    if ($content -ne $newContent) {
        [System.IO.File]::WriteAllText($_.FullName, $newContent, [System.Text.Encoding]::UTF8)
        Write-Host "Fixed: $($_.Name)"
    } else {
        Write-Host "No change: $($_.Name)"
    }
}
