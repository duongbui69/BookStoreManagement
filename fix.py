import re

with open('BookStoreManagement/Repositories/PurchaseReceiptRepository.cs', 'r', encoding='utf-8') as f:
    c = f.read()

# Fix 1: Add TotalAmount to sync create
c = re.sub(r'INSERT INTO PurchaseReceipts \(ReceiptCode, StoreId, SupplierId, UserId, Note\)\s*OUTPUT INSERTED\.Id\s*VALUES \(@ReceiptCode, @StoreId, @SupplierId, @UserId, @Note\);', 
    'INSERT INTO PurchaseReceipts (ReceiptCode, StoreId, SupplierId, UserId, Note, TotalAmount)\n                    OUTPUT INSERTED.Id\n                    VALUES (@ReceiptCode, @StoreId, @SupplierId, @UserId, @Note, @TotalAmount);', 
    c)

# Fix 2: Add AddParameter for TotalAmount
c = c.replace('AddParameter(receiptCommand, "@Note", receipt.Note);', 
    'AddParameter(receiptCommand, "@Note", receipt.Note);\n                AddParameter(receiptCommand, "@TotalAmount", receipt.TotalAmount);')

# Fix 3: Remove SellingPrice parsing
c = c.replace('SellingPrice = GetNullableDecimal(reader, "SellingPrice"),', '')

# Fix 4: Remove SellingPrice from SELECT
c = c.replace('SELECT Id, PurchaseReceiptId, BookId, Quantity, ImportPrice, SellingPrice, LineTotal', 
    'SELECT Id, PurchaseReceiptId, BookId, Quantity, ImportPrice, LineTotal')

with open('BookStoreManagement/Repositories/PurchaseReceiptRepository.cs', 'w', encoding='utf-8') as f:
    f.write(c)
