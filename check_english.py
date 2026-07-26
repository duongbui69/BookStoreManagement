import os
import re

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'
strings = set()

patterns = [
    r'(?:\bText\s*=\s*|\bPlaceholderText\s*=\s*|\bHeaderText\s*=\s*|\bToolTipText\s*=\s*|\.Show\s*\(\s*)\"([^\"]+)\"',
    r'(?:\bTitle\s*=\s*|\bMessage\s*=\s*)\"([^\"]+)\"'
]

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir or 'Properties' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                    
                    for pattern in patterns:
                        matches = re.findall(pattern, content)
                        for match in matches:
                            if len(match) > 0 and not match.startswith('@'):
                                if "SELECT " not in match.upper() and "INSERT " not in match.upper():
                                    strings.add(match)
                                    
                    ms_matches = re.findall(r'MessageBox\.Show\s*\(\s*\"([^\"]+)\"\s*,\s*\"([^\"]+)\"', content)
                    for m in ms_matches:
                        strings.add(m[0])
                        strings.add(m[1])
            except:
                pass

def is_vietnamese(text):
    vi_chars = set("àáạảãâầấậẩẫăằắặẳẵèéẹẻẽêềếệểễìíịỉĩòóọỏõôồốộổỗơờớợởỡùúụủũưừứựửữỳýỵỷỹđÀÁẠẢÃÂẦẤẬẨẪĂẰẮẶẲẴÈÉẸẺẼÊỀẾỆỂỄÌÍỊỈĨÒÓỌỎÕÔỒỐỘỔỖƠỜỚỢỞỠÙÚỤỦŨƯỪỨỰỬỮỲÝỴỶỸĐ")
    for char in text:
        if char in vi_chars:
            return True
    return False

english_strings = []
for s in sorted(list(strings)):
    # Ignore purely numeric or symbol strings
    if any(c.isalpha() for c in s) and not is_vietnamese(s):
        english_strings.append(s)

with open('leftover_english.txt', 'w', encoding='utf-8') as f:
    for e in english_strings:
        f.write(e + '\n')
        
print(f"Found {len(english_strings)} English strings remaining.")
