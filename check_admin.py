import os

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'
found = False

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir or 'Properties' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
                if 'SYSTEM ADMINISTRATOR' in content.upper():
                    print(f"Found in {file}")
                    found = True
if not found:
    print("Not found (replaced successfully).")
