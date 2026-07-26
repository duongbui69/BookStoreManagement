import re

# Fix VoucherTypeControl.cs
vc_file = 'BookStoreManagement/UserControls/VoucherTypeControl.cs'
with open(vc_file, 'r', encoding='utf-8') as f:
    vc_content = f.read()

vc_content = vc_content.replace('using BookStoreManagement.Repositories;', 'using BookStoreManagement.Repositories;\nusing BookStoreManagement.Helpers;')
with open(vc_file, 'w', encoding='utf-8') as f:
    f.write(vc_content)

# Fix InventoryRepository.cs
ir_file = 'BookStoreManagement/Repositories/InventoryRepository.cs'
with open(ir_file, 'r', encoding='utf-8') as f:
    ir_content = f.read()

ir_content = ir_content.replace('ExecuteScalar<int>', 'ExecuteScalarInt')
ir_content = ir_content.replace('ExecuteScalar<decimal>', 'ExecuteScalarDecimal')

with open(ir_file, 'w', encoding='utf-8') as f:
    f.write(ir_content)
