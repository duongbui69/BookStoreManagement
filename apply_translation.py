import os
import json

with open('c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/translate_dict.json', 'r', encoding='utf-8') as f:
    translate_dict = json.load(f)

# Sort dictionary by length of keys descending to prevent partial replacements
# e.g., replacing "Thêm mới" before "Thêm mới cửa hàng" would break the latter
sorted_keys = sorted(translate_dict.keys(), key=len, reverse=True)

root_dir = r'c:\Users\ADMIN\Desktop\projects\university\BookStoreManagement\BookStoreManagement'

changed_files_count = 0

for subdir, dirs, files in os.walk(root_dir):
    if 'obj' in subdir or 'bin' in subdir or 'Migrations' in subdir:
        continue
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(subdir, file)
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                original_content = content
                for k in sorted_keys:
                    v = translate_dict[k]
                    # Replace only exact double-quoted strings to avoid breaking code logic or SQL
                    content = content.replace(f'"{k}"', f'"{v}"')
                
                if content != original_content:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    changed_files_count += 1
                    
            except Exception as e:
                print(f"Error processing {filepath}: {e}")

print(f"Translation applied to {changed_files_count} files.")
