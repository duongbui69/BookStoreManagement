$files = @(
    "Forms\CloseShiftDialog.cs",
    "UserControls\DashboardControl.cs",
    "UserControls\ExportReceiptControl.cs",
    "UserControls\ExportReceiptEditForm.cs",
    "UserControls\OrdersControl.cs",
    "UserControls\PurchaseReceiptControl.cs",
    "UserControls\PurchaseReceiptEditForm.cs",
    "UserControls\StaffMyInvoicesControl.cs",
    "UserControls\StaffMyShiftsControl.cs",
    "UserControls\StaffReturnControl.cs"
)

foreach ($file in $files) {
    $path = "C:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\$file"
    if (Test-Path $path) {
        $content = Get-Content $path -Raw
        
        # Replace " đ" with " ₫" globally in these files
        $content = $content -replace '" đ"', '" ₫"'
        $content = $content -replace " đ<", " ₫<"
        
        # Specific replacements
        if ($file -match "DashboardControl.cs") {
            $content = $content -replace '\$"\$\{currentStats.TotalRevenue:N0\}"', '$"{currentStats.TotalRevenue:N0} ₫"'
            $content = $content -replace '\$"\$\{currentStats.NetProfit:N0\}"', '$"{currentStats.NetProfit:N0} ₫"'
        }
        
        if ($file -match "OrdersControl.cs") {
            $content = $content -replace 'ord\.TotalAmount\.ToString\("N0"\)(?! \+ " ₫")', 'ord.TotalAmount.ToString("N0") + " ₫"'
        }
        
        if ($file -match "PurchaseReceiptControl.cs") {
            $content = $content -replace 'stats\.TotalValue\.ToString\("N0"\)(?! \+ " ₫")', 'stats.TotalValue.ToString("N0") + " ₫"'
            $content = $content -replace 'item\.TotalAmount\.ToString\("N0"\)(?! \+ " ₫")', 'item.TotalAmount.ToString("N0") + " ₫"'
        }
        
        if ($file -match "PurchaseReceiptEditForm.cs") {
            $content = $content -replace 'item\.ImportPrice\.ToString\("N0"\)(?! \+ " ₫")', 'item.ImportPrice.ToString("N0") + " ₫"'
            $content = $content -replace 'item\.LineTotal\.ToString\("N0"\)(?! \+ " ₫")', 'item.LineTotal.ToString("N0") + " ₫"'
        }
        
        if ($file -match "StaffMyShiftsControl.cs") {
            $content = $content -replace 'item\.Revenue\.ToString\("N0"\)(?! \+ " ₫")', 'item.Revenue.ToString("N0") + " ₫"'
        }
        
        if ($file -match "StaffReturnControl.cs") {
            $content = $content -replace 'detail\.UnitPrice\.ToString\("N0"\)(?! \+ " ₫")', 'detail.UnitPrice.ToString("N0") + " ₫"'
            $content = $content -replace 'refundAmt\.ToString\("N0"\)(?! \+ " ₫")', 'refundAmt.ToString("N0") + " ₫"'
        }
        
        Set-Content -Path $path -Value $content -Encoding UTF8
    }
}
Write-Host "Done"
