import os
import re
import json

vietnamese_chars = re.compile(r'[àáảãạăằắẳẵặâầấẩẫậèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđÀÁẢÃẠĂẰẮẲẴẶÂẦẤẨẪẬÈÉẺẼẸÊỀẾỂỄỆÌÍỈĨỊÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢÙÚỦŨỤƯỪỨỬỮỰỲÝỶỸỴĐ]')

strings = set()
root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                    # Find all strings inside double quotes
                    matches = re.findall(r'\"([^\"]*)\"', content)
                    for match in matches:
                        if vietnamese_chars.search(match):
                            strings.add(match)
            except Exception as e:
                print(f"Error reading {filepath}: {e}")

output = list(strings)
output.sort()

# Also try to find property assignments in Designer files for Texts like Text = "..."
with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/vi_strings.json', 'w', encoding='utf-8') as f:
    json.dump(output, f, ensure_ascii=False, indent=2)

print(f'Extracted {len(output)} strings.')
