using System;
using System.IO;

namespace BookStoreManagement.Services
{
    public class FileStorageService
    {
        private readonly string _bookImageFolder;

        public FileStorageService()
        {
            _bookImageFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "images", "books");
            Directory.CreateDirectory(_bookImageFolder);
        }

        public string SaveBookImage(string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath)) return string.Empty;
            if (!File.Exists(sourceFilePath)) throw new FileNotFoundException("Không tìm thấy file ảnh.");

            string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".bmp" && extension != ".gif")
            {
                throw new Exception("Chỉ cho phép upload ảnh jpg, jpeg, png, bmp hoặc gif.");
            }

            string fileName = Guid.NewGuid().ToString("N") + extension;
            string destinationPath = Path.Combine(_bookImageFolder, fileName);
            File.Copy(sourceFilePath, destinationPath, true);
            return Path.Combine("assets", "images", "books", fileName);
        }
    }
}
