using System;
using System.Collections.Generic;
using System.Text;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class InvoiceTextService
    {
        public string BuildInvoiceText(List<SalesOrderDetailFullViewModel> details)
        {
            if (details == null || details.Count == 0)
            {
                throw new Exception("Không có dữ liệu hóa đơn.");
            }

            SalesOrderDetailFullViewModel first = details[0];
            var builder = new StringBuilder();

            builder.AppendLine("========================================");
            builder.AppendLine("          HÓA ĐƠN BÁN SÁCH");
            builder.AppendLine("========================================");
            builder.AppendLine($"Mã hóa đơn: {first.OrderCode}");
            builder.AppendLine($"Ngày bán: {first.OrderDate:dd/MM/yyyy HH:mm}");
            builder.AppendLine($"Nhân viên: {first.StaffName}");
            builder.AppendLine($"Khách hàng: {first.CustomerName ?? "Khách lẻ"}");
            builder.AppendLine($"Thanh toán: {first.PaymentMethod}");
            builder.AppendLine("----------------------------------------");

            foreach (SalesOrderDetailFullViewModel item in details)
            {
                builder.AppendLine(item.Title);
                builder.AppendLine($"SL: {item.Quantity} x {item.UnitPrice:N0} - Giảm: {item.DiscountAmount:N0}");
                builder.AppendLine($"Thành tiền: {item.LineTotal:N0}");
                builder.AppendLine("----------------------------------------");
            }

            builder.AppendLine($"TỔNG TIỀN: {first.TotalAmount:N0} VNĐ");
            builder.AppendLine("========================================");
            builder.AppendLine("Cảm ơn quý khách!");

            return builder.ToString();
        }
    }
}
