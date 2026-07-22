import os
import re

files = [
    r"Forms\CloseShiftDialog.cs",
    r"UserControls\DashboardControl.cs",
    r"UserControls\ExportReceiptControl.cs",
    r"UserControls\ExportReceiptEditForm.cs",
    r"UserControls\OrdersControl.cs",
    r"UserControls\PurchaseReceiptControl.cs",
    r"UserControls\PurchaseReceiptEditForm.cs",
    r"UserControls\StaffMyInvoicesControl.cs",
    r"UserControls\StaffMyShiftsControl.cs",
    r"UserControls\StaffReturnControl.cs"
]

for f in files:
    if os.path.exists(f):
        with open(f, 'r', encoding='utf-8') as file:
            content = file.read()
            
        content = content.replace('" đ"', '" ₫"')
        content = content.replace(' đ<', ' ₫<')
        
        if "DashboardControl.cs" in f:
            content = content.replace('$"${currentStats.TotalRevenue:N0}"', '$"{currentStats.TotalRevenue:N0} ₫"')
            content = content.replace('$"${currentStats.NetProfit:N0}"', '$"{currentStats.NetProfit:N0} ₫"')
            
        if "OrdersControl.cs" in f:
            content = re.sub(r'ord\.TotalAmount\.ToString\("N0"\)(?! \+ " ₫")', r'ord.TotalAmount.ToString("N0") + " ₫"', content)
            
        if "PurchaseReceiptControl.cs" in f:
            content = re.sub(r'stats\.TotalValue\.ToString\("N0"\)(?! \+ " ₫")', r'stats.TotalValue.ToString("N0") + " ₫"', content)
            content = re.sub(r'item\.TotalAmount\.ToString\("N0"\)(?! \+ " ₫")', r'item.TotalAmount.ToString("N0") + " ₫"', content)
            
        if "PurchaseReceiptEditForm.cs" in f:
            content = re.sub(r'item\.ImportPrice\.ToString\("N0"\)(?! \+ " ₫")', r'item.ImportPrice.ToString("N0") + " ₫"', content)
            content = re.sub(r'item\.LineTotal\.ToString\("N0"\)(?! \+ " ₫")', r'item.LineTotal.ToString("N0") + " ₫"', content)
            
        if "StaffMyShiftsControl.cs" in f:
            content = re.sub(r'item\.Revenue\.ToString\("N0"\)(?! \+ " ₫")', r'item.Revenue.ToString("N0") + " ₫"', content)
            
        if "StaffReturnControl.cs" in f:
            content = re.sub(r'detail\.UnitPrice\.ToString\("N0"\)(?! \+ " ₫")', r'detail.UnitPrice.ToString("N0") + " ₫"', content)
            content = re.sub(r'refundAmt\.ToString\("N0"\)(?! \+ " ₫")', r'refundAmt.ToString("N0") + " ₫"', content)
            
        with open(f, 'w', encoding='utf-8') as file:
            file.write(content)
            
print("Done")
