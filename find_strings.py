import os
import re

root = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement\Services'

strings_to_translate = set()
for subdir, dirs, files in os.walk(root):
    for f in files:
        if f.endswith('.cs'):
            with open(os.path.join(subdir, f), 'r', encoding='utf-8') as file:
                content = file.read()
                matches = re.finditer(r'Exception\(\"([^\"]+)\"\)', content)
                for m in matches:
                    strings_to_translate.add(m.group(1))
                matches = re.finditer(r'Require\([^,]+,\s*\"([^\"]+)\"\)', content)
                for m in matches:
                    strings_to_translate.add(m.group(1))
                    
for s in sorted(strings_to_translate):
    print(s)
