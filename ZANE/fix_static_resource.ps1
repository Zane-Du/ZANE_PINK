$viewsDir = "C:\Users\34163\Desktop\ZANE2027\ZANE\ZANE\Views"
$xamlFiles = Get-ChildItem -Path $viewsDir -Filter "*.xaml"

foreach ($file in $xamlFiles) {
    $content = Get-Content -Path $file.FullName -Raw
    
    # Count occurrences before
    $countBefore = [regex]::Matches($content, "{StaticResource ").Count
    
    if ($countBefore -gt 0) {
        $newContent = $content -replace "{StaticResource ", "{DynamicResource "
        Set-Content -Path $file.FullName -Value $newContent -NoNewline
        
        $countAfter = [regex]::Matches($newContent, "{DynamicResource ").Count
        Write-Host "$($file.Name): $countBefore StaticResource -> $countAfter DynamicResource"
    } else {
        Write-Host "$($file.Name): no StaticResource found (skipped)"
    }
}

Write-Host "Done!"
