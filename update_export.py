import re

files = [
    ('BookStoreManagement/UserControls/AuthorControl.cs', 'btnExport', 'dgvData', 'DanhSachTacGia.xlsx', 'Tác Giả'),
    ('BookStoreManagement/UserControls/PublisherControl.cs', 'btnExport', 'dgvData', 'DanhSachNhaXuatBan.xlsx', 'Nhà Xuất Bản'),
    ('BookStoreManagement/UserControls/SupplierControl.cs', 'btnExport', 'dgvData', 'DanhSachNhaCungCap.xlsx', 'Nhà Cung Cấp'),
    ('BookStoreManagement/UserControls/ReportsControl.cs', 'btnExportExcel', 'dgvTopSelling', 'BaoCaoDoanhThu.xlsx', 'Báo Cáo')
]

for path, btn_name, dgv_name, file_name, sheet_name in files:
    with open(path, 'r', encoding='utf-8-sig', errors='ignore') as f:
        content = f.read()
    
    # 1. Replace the click handler
    pattern = rf'{btn_name}\.Click\s*\+=\s*\(s,\s*e\)\s*=>\s*MessageBox\.Show\([^)]+\);'
    replacement = f'{btn_name}.Click += BtnExport_Click;'
    
    if not re.search(pattern, content):
        print(f'Pattern not found in {path}')
        continue
        
    content = re.sub(pattern, replacement, content, count=1)
    
    # 2. Add the BtnExport_Click method at the end of the class.
    # Find the last two closing braces
    method_code = f'''
        private void BtnExport_Click(object? sender, EventArgs e)
        {{
            try
            {{
                using (SaveFileDialog sfd = new SaveFileDialog() {{ Filter = "Excel Workbook|*.xlsx", FileName = "{file_name}" }})
                {{
                    if (sfd.ShowDialog() == DialogResult.OK)
                    {{
                        var excelService = new BookStoreManagement.Services.ExcelExportService();
                        excelService.ExportDataGridView({dgv_name}, sfd.FileName, "{sheet_name}");
                        MessageBox.Show("Xuất file Excel thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }}
                }}
            }}
            catch (Exception ex)
            {{
                MessageBox.Show("Lỗi khi xuất Excel: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }}
        }}
    }}
}}
'''
    content = re.sub(r'(\s*\}\s*\})[\s]*$', method_code, content)
    
    with open(path, 'w', encoding='utf-8-sig') as f:
        f.write(content)
        
    print(f'Successfully updated {path}')
