import re

file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/Themes/ThemeManager.cs'

with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace('"Inter"', '"Segoe UI"')

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
