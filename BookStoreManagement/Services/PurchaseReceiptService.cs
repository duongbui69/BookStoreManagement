using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class PurchaseReceiptService : ServiceBase
    {
        private readonly PurchaseReceiptRepository _purchaseReceiptRepository;
        private readonly SupplierRepository _supplierRepository;
        private readonly UserRepository _userRepository;
        private readonly BookRepository _bookRepository;

        public PurchaseReceiptService()
        {
            _purchaseReceiptRepository = new PurchaseReceiptRepository();
            _supplierRepository = new SupplierRepository();
            _userRepository = new UserRepository();
            _bookRepository = new BookRepository();
        }

        public int CreateReceipt(int supplierId, int userId, string? note, List<PurchaseReceiptDetail> details)
        {
            EnsureId(supplierId, "Nhà cung cấp");
            EnsureId(userId, "Nhân viên nhập hàng");

            Supplier? supplier = _supplierRepository.GetById(supplierId);
            if (supplier == null || !supplier.IsActive)
            {
                throw new Exception("Nhà cung cấp không tồn tại hoặc đã ngừng hoạt động.");
            }

            if (_userRepository.GetById(userId) == null)
            {
                throw new Exception("Nhân viên nhập hàng không tồn tại.");
            }

            ValidateDetails(details);

            PurchaseReceipt receipt = new PurchaseReceipt
            {
                ReceiptCode = GenerateUniqueReceiptCode(),
                SupplierId = supplierId,
                UserId = userId,
                Note = NormalizeNullable(note)
            };

            return _purchaseReceiptRepository.CreateReceipt(receipt, details);
        }

        public int CreateCurrentUserReceipt(int supplierId, string? note, List<PurchaseReceiptDetail> details)
        {
            if (!CurrentSession.IsLoggedIn)
            {
                throw new Exception("Bạn cần đăng nhập để tạo phiếu nhập.");
            }

            return CreateReceipt(supplierId, CurrentSession.UserId, note, details);
        }

        public List<PurchaseReceiptListViewModel> GetAll() => _purchaseReceiptRepository.GetAll();

        public PurchaseReceipt GetById(int id)
        {
            EnsureId(id, "Id phiếu nhập");
            return _purchaseReceiptRepository.GetById(id) ?? throw new Exception("Không tìm thấy phiếu nhập.");
        }

        public List<PurchaseReceiptListViewModel> Search(string keyword)
        {
            keyword = Normalize(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? GetAll() : _purchaseReceiptRepository.Search(keyword);
        }

        public List<PurchaseReceiptListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date)
            {
                throw new Exception("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            return _purchaseReceiptRepository.GetByDateRange(fromDate.Date, toDate.Date);
        }

        public string GenerateUniqueReceiptCode()
        {
            string code;
            int retry = 0;

            do
            {
                code = _purchaseReceiptRepository.GenerateReceiptCode();
                if (retry > 0)
                {
                    code += retry.ToString("00");
                }

                retry++;
            }
            while (_purchaseReceiptRepository.IsReceiptCodeExists(code));

            return code;
        }

        private void ValidateDetails(List<PurchaseReceiptDetail> details)
        {
            if (details == null || details.Count == 0)
            {
                throw new Exception("Phiếu nhập phải có ít nhất một sách.");
            }

            foreach (PurchaseReceiptDetail detail in details)
            {
                EnsureId(detail.BookId, "Sách");
                EnsurePositive(detail.Quantity, "Số lượng nhập");
                EnsurePositive(detail.ImportPrice, "Giá nhập");

                Book? book = _bookRepository.GetById(detail.BookId);
                if (book == null || !book.IsActive)
                {
                    throw new Exception("Sách không tồn tại hoặc đã ngừng hoạt động.");
                }
            }
        }
    }
}
