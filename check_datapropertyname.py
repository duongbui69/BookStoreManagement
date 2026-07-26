import os, re

for root, dirs, files in os.walk('BookStoreManagement'):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8', errors='ignore') as f:
                for line in f:
                    if re.search(r'DataPropertyName\s*=\s*"[^a-zA-Z0-9"]+', line):
                        print(f'{filepath}: {line.strip()}')
