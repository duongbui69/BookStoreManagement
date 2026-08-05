import os, re

results = set()

def scan_dir(directory):
    for root, dirs, files in os.walk(directory):
        for file in files:
            if file.endswith('.cs'):
                with open(os.path.join(root, file), 'r', encoding='utf-8') as f:
                    content = f.read()
                    matches = re.finditer(r'MessageBox\.Show\(\s*\"(.*?)\"', content)
                    for m in matches:
                        results.add(f'{file}: {m.group(1)}')

scan_dir('UserControls')
scan_dir('Forms')

with open('msgbox_results.txt', 'w', encoding='utf-8') as f:
    for r in sorted(results):
        f.write(r + '\n')
