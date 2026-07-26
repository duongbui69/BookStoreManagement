import re

file_path = 'BookStoreManagement/Repositories/ExportReceiptRepository.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    c = f.read()

# Fix 1: Remove LineTotal from INSERT queries
c = re.sub(r'INSERT INTO ExportReceiptDetails \(ReceiptId, BookId, Quantity, Price, LineTotal\)\s*VALUES \(@ReceiptId, @BookId, @Quantity, @Price, @LineTotal\);', 
    'INSERT INTO ExportReceiptDetails (ReceiptId, BookId, Quantity, Price)\n                    VALUES (@ReceiptId, @BookId, @Quantity, @Price);', 
    c)

# Fix 2: Remove detail.LineTotal from anonymous objects
c = c.replace('detail.Price,\n                        detail.LineTotal\n                    }', 'detail.Price\n                    }')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(c)
