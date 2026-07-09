using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class InventoryService : ServiceBase
    {
        private readonly InventoryRepository _inventoryRepository;
        private readonly BookRepository _bookRepository;
        private readonly UserRepository _userRepository;

        public InventoryService()
        {
            _inventoryRepository = new InventoryRepository();
            _bookRepository = new BookRepository();
            _userRepository = new UserRepository();
        }

        public List<InventoryHistoryViewModel> GetHistory() => _inventoryRepository.GetHistory();

        public List<InventoryHistoryViewModel> GetHistoryByBookId(int bookId)
        {
            EnsureId(bookId, "Id sách");
            return _inventoryRepository.GetHistoryByBookId(bookId);
        }

        public List<LowStockBookViewModel> GetLowStockBooks() => _inventoryRepository.GetLowStockBooks();

        public bool AdjustStock(int bookId, int userId, int quantityChange, string? note)
        {
            EnsureId(bookId, "Id sách");
            EnsureId(userId, "Id nhân viên");

            if (quantityChange == 0)
            {
                throw new Exception("Số lượng điều chỉnh phải khác 0.");
            }

            if (_bookRepository.GetById(bookId) == null)
            {
                throw new Exception("Sách không tồn tại.");
            }

            if (_userRepository.GetById(userId) == null)
            {
                throw new Exception("Nhân viên không tồn tại.");
            }

            int currentQuantity = _bookRepository.GetCurrentQuantity(bookId);
            if (currentQuantity + quantityChange < 0)
            {
                throw new Exception("Không thể điều chỉnh vì tồn kho sẽ bị âm.");
            }

            return _inventoryRepository.AdjustStock(bookId, userId, quantityChange, NormalizeNullable(note));
        }

        public bool AdjustStockByCurrentUser(int bookId, int quantityChange, string? note)
        {
            if (!CurrentSession.IsLoggedIn)
            {
                throw new Exception("Bạn cần đăng nhập để điều chỉnh kho.");
            }

            return AdjustStock(bookId, CurrentSession.UserId, quantityChange, note);
        }
    }
}
