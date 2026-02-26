$files = @(
    "Assets/Font/GenJyuuGothicScore.asset"
    "Assets/Font/GenJyuuGothicXGold.asset"
    "Assets/Font/GenJyuuGothicX-Medium SDF.asset"
    "Assets/Font/Jua-Regular SDF.asset"
    "Assets/Font/Minimoon SDF 1.asset"
    "Assets/Font/SourceHanSans-Medium SDF.asset"
)

foreach ($file in $files) {
    if (Test-Path $file) {
        git update-index --assume-unchanged $file
        Write-Host "OK Ignored: $file"
    } else {
        Write-Host "WARNING Not found: $file"
    }
}

Write-Host "Done."
