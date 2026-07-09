using System;
using System.Collections.Generic;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class SalesOrderService : ServiceBase
    {
        private readonly SalesOrderRepository _salesOrderRepository;
        private readonly BookRepository _bookRepository;
        private readonly UserRepository _userRepository;
        private readonly CustomerRepository _customerRepository;

        public SalesOrderService()
        {
            _salesOrderRepository = new SalesOrderRepository();
            _bookRepository = new BookRepository();
            _userRepository = new UserRepository();
            _customerRepository = new CustomerRepository();
        }

        public int CreateOrder(int userId, int? customerId, string paymentMethod, string? note, List<SalesOrderDetail> details)
        {
            EnsureId(userId, "Nhân viên lập hóa đơn");
            EnsureRequired(paymentMethod, "Phương thức thanh toán");

            if (_userRepository.GetById(userId) == null)
            {
                throw new Exception("Nhân viên lập hóa đơn không tồn tại.");
            }

            if (customerId.HasValue && _customerRepository.GetById(customerId.Value) == null)
            {
                throw new Exception("Khách hàng không tồn tại.");
            }

            ValidateDetails(details);

            SalesOrder order = new SalesOrder
            {
                OrderCode = GenerateUniqueOrderCode(),
                UserId = userId,
                CustomerId = customerId,
                PaymentMethod = Normalize(paymentMethod),
                OrderStatus = AppConstants.OrderStatuses.Completed,
                Note = NormalizeNullable(note)
            };

            return _salesOrderRepository.CreateOrder(order, details);
        }

        public int CreateCurrentUserOrder(int? customerId, string paymentMethod, string? note, List<SalesOrderDetail> details)
        {
            if (!CurrentSession.IsLoggedIn)
            {
                throw new Exception("Bạn cần đăng nhập để tạo hóa đơn.");
            }

            return CreateOrder(CurrentSession.UserId, customerId, paymentMethod, note, details);
        }

        public List<SalesOrderListViewModel> GetAll()
        {
            return _salesOrderRepository.GetAll();
        }

        public List<SalesOrderListViewModel> GetMyOrders()
        {
            if (!CurrentSession.IsLoggedIn)
            {
                throw new Exception("Bạn chưa đăng nhập.");
            }

            return _salesOrderRepository.GetByStaffId(CurrentSession.UserId);
        }

        public SalesOrder GetById(int id)
        {
            EnsureId(id, "Id hóa đơn");
            return _salesOrderRepository.GetById(id) ?? throw new Exception("Không tìm thấy hóa đơn.");
        }

        public SalesOrder GetByOrderCode(string orderCode)
        {
            orderCode = Normalize(orderCode);
            EnsureRequired(orderCode, "Mã hóa đơn");
            return _salesOrderRepository.GetByOrderCode(orderCode) ?? throw new Exception("Không tìm thấy hóa đơn.");
        }

        public List<SalesOrderListViewModel> GetByStaffId(int staffId)
        {
            EnsureId(staffId, "Id nhân viên");
            return _salesOrderRepository.GetByStaffId(staffId);
        }

        public List<SalesOrderListViewModel> Search(string keyword)
        {
            keyword = Normalize(keyword);
            return string.IsNullOrWhiteSpace(keyword) ? GetAll() : _salesOrderRepository.Search(keyword);
        }

        public List<SalesOrderListViewModel> GetByDateRange(DateTime fromDate, DateTime toDate)
        {
            if (fromDate.Date > toDate.Date)
            {
                throw new Exception("Ngày bắt đầu không được lớn hơn ngày kết thúc.");
            }

            return _salesOrderRepository.GetByDateRange(fromDate.Date, toDate.Date);
        }

        public List<SalesOrderDetailFullViewModel> GetDetails(int salesOrderId)
        {
            EnsureId(salesOrderId, "Id hóa đơn");
            return _salesOrderRepository.GetDetails(salesOrderId);
        }

        public bool CancelOrder(int salesOrderId)
        {
            EnsureId(salesOrderId, "Id hóa đơn");

            SalesOrder order = GetById(salesOrderId);
            if (order.OrderStatus == AppConstants.OrderStatuses.Cancelled)
            {
                throw new Exception("Hóa đơn đã bị hủy trước đó.");
            }

            return _salesOrderRepository.CancelOrder(salesOrderId);
        }

        public string GenerateUniqueOrderCode()
        {
            string code;
            int retry = 0;

            do
            {
                code = _salesOrderRepository.GenerateOrderCode();
                if (retry > 0)
                {
                    code += retry.ToString("00");
                }

                retry++;
            }
            while (_salesOrderRepository.IsOrderCodeExists(code));

            return code;
        }

        private void ValidateDetails(List<SalesOrderDetail> details)
        {
            if (details == null || details.Count == 0)
            {
                throw new Exception("Hóa đơn phải có ít nhất một sách.");
            }

            foreach (SalesOrderDetail detail in details)
            {
                EnsureId(detail.BookId, "Sách");
                EnsurePositive(detail.Quantity, "Số lượng bán");
                EnsurePositive(detail.UnitPrice, "Đơn giá bán");
                EnsureNonNegative(detail.DiscountAmount, "Giảm giá");

                Book? book = _bookRepository.GetById(detail.BookId);
                if (book == null || !book.IsActive)
                {
                    throw new Exception("Sách không tồn tại hoặc đã ngừng hoạt động.");
                }

                if (!_bookRepository.HasEnoughStock(detail.BookId, detail.Quantity))
                {
                    throw new Exception($"Sách '{book.Title}' không đủ tồn kho.");
                }
            }
        }
    }
}
