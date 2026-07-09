using System;
using System.IO;

namespace BookStoreManagement.Services
{
    public class FileStorageService
    {
        private readonly string _bookImageFolder;

        public FileStorageService()
        {
            _bookImageFolder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "assets",
                "images",
                "books"
            );

            Directory.CreateDirectory(_bookImageFolder);
        }

        public string? SaveBookImage(string? sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
            {
                return null;
            }

            if (!File.Exists(sourceFilePath))
            {
                throw new FileNotFoundException("Không tìm thấy file ảnh.", sourceFilePath);
            }

            string extension = Path.GetExtension(sourceFilePath).ToLowerInvariant();
            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png" && extension != ".bmp" && extension != ".gif")
            {
                throw new Exception("File ảnh chỉ được dùng định dạng jpg, jpeg, png, bmp hoặc gif.");
            }

            var fileInfo = new FileInfo(sourceFilePath);
            long maxSize = 5 * 1024 * 1024;
            if (fileInfo.Length > maxSize)
            {
                throw new Exception("Dung lượng ảnh không được vượt quá 5MB.");
            }

            string fileName = Guid.NewGuid().ToString("N") + extension;
            string destinationPath = Path.Combine(_bookImageFolder, fileName);

            File.Copy(sourceFilePath, destinationPath, overwrite: true);

            return Path.Combine("assets", "images", "books", fileName);
        }

        public void DeleteFileIfExists(string? relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return;
            }

            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
    }
}
