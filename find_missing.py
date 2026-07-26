import os
import re

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir or 'Properties' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
                matches = re.finditer(r'\"([^\"]*Showing[^\"]*)\"', content, re.IGNORECASE)
                for m in matches:
                    print(f"{file}: {m.group(0)}")
                    
                matches = re.finditer(r'\"([^\"]*management[^\"]*)\"', content, re.IGNORECASE)
                for m in matches:
                    print(f"{file}: {m.group(0)}")
