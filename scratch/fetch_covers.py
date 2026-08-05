import sys
import os
import urllib.request
import urllib.parse
import json
import pyodbc
import time
import shutil

# Enable utf8
sys.stdout.reconfigure(encoding='utf-8')

# Connection string
conn_str = 'Driver={ODBC Driver 17 for SQL Server};Server=(localdb)\\MSSQLLocalDB;Database=BookStoreDB;Trusted_Connection=yes;'

def get_cover_url(title):
    try:
        url = "https://www.googleapis.com/books/v1/volumes?q=" + urllib.parse.quote(title)
        req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
        with urllib.request.urlopen(req) as response:
            data = json.loads(response.read().decode('utf-8'))
            if 'items' in data and len(data['items']) > 0:
                for item in data['items']:
                    if 'volumeInfo' in item and 'imageLinks' in item['volumeInfo']:
                        thumbnail = item['volumeInfo']['imageLinks'].get('thumbnail')
                        if thumbnail:
                            # Use https
                            return thumbnail.replace('http:', 'https:')
    except Exception as e:
        print(f"Error fetching {title}: {e}")
    return None

def main():
    try:
        conn = pyodbc.connect(conn_str)
    except Exception as e:
        print("Cannot connect to db:", e)
        return

    cursor = conn.cursor()
    cursor.execute("SELECT Id, Title, ImagePath FROM Books")
    books = cursor.fetchall()

    cover_dir_src = os.path.join('BookStoreManagement', 'Covers')
    cover_dir_bin = os.path.join('BookStoreManagement', 'bin', 'Debug', 'net10.0-windows', 'Covers')
    
    os.makedirs(cover_dir_src, exist_ok=True)
    os.makedirs(cover_dir_bin, exist_ok=True)

    print(f"Total books: {len(books)}")
    
    # We will fetch a subset to avoid hitting rate limits too hard, maybe first 50 unique books
    # But wait, there are 500 books. Google API limit is around 1000 per day. We can fetch all or just top ones.
    # Actually, a lot of manga titles might not be found easily, or maybe they are.
    
    updated = 0
    for book in books:
        book_id = book[0]
        title = book[1]
        image_path = book[2]

        print(f"Processing ID {book_id}: {title}")
        
        # If it doesn't have an image path, we assign one
        if not image_path:
            image_path = f"book_{book_id}.jpg"
            cursor.execute("UPDATE Books SET ImagePath = ? WHERE Id = ?", (image_path, book_id))
            conn.commit()

        target_file_src = os.path.join(cover_dir_src, image_path)
        target_file_bin = os.path.join(cover_dir_bin, image_path)

        # Only download if we haven't successfully downloaded a unique one, but wait, the existing ones are dummy.
        # How to know if it's dummy? Dummy sizes are in the dictionary we found. But to be safe, we just overwrite.
        # But wait, it's 500 requests, might take a few minutes. Let's do it!
        url = get_cover_url(title)
        if url:
            try:
                req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
                with urllib.request.urlopen(req) as response:
                    img_data = response.read()
                    with open(target_file_src, 'wb') as f:
                        f.write(img_data)
                    with open(target_file_bin, 'wb') as f:
                        f.write(img_data)
                    print(f"  -> Downloaded cover.")
                    updated += 1
            except Exception as e:
                print(f"  -> Failed to download: {e}")
        else:
            print(f"  -> No cover found.")
            
        time.sleep(0.5) # rate limit

    print(f"Done. Updated {updated} covers.")

if __name__ == '__main__':
    main()
