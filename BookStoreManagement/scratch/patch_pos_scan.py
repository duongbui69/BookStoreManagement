import re
import os

file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/POSControl.cs'

with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

match = re.search(r'var btnScan = CreateOutlineButton\([^,]+, 545, 15\);', text)
if match:
    idx = match.end()
    insertion = """
            btnScan.Click += (s, e) => {
                using (var scanForm = new BookStoreManagement.Forms.ScanBarcodeForm())
                {
                    if (scanForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        string code = scanForm.ScannedCode.ToLower();
                        var book = _allInventory.FirstOrDefault(b => b.BookCode.ToLower() == code || (b.ISBN != null && b.ISBN.ToLower() == code));
                        if (book != null)
                        {
                            AddToCart(book);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Không tìm thấy sách với mã: " + code, "Lỗi", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                        }
                    }
                }
            };
"""
    text = text[:idx] + insertion + text[idx:]
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(text)
    print('Patched POSControl.cs successfully.')
else:
    print('Not found')
