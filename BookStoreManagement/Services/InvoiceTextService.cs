using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BookStoreManagement.ViewModels;

namespace BookStoreManagement.Services
{
    public class InvoiceTextService : ServiceBase
    {
        public string BuildSalesInvoiceText(List<SalesOrderDetailFullViewModel> details)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(details != null && details.Count > 0, "Không có dữ liệu hóa đơn.");

            var first = details[0];
            decimal total = details.Max(x => x.TotalAmount);

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("              HÓA ĐƠN BÁN SÁCH");
            sb.AppendLine("========================================");
            sb.AppendLine($"Mã hóa đơn : {first.OrderCode}");
            sb.AppendLine($"Cửa hàng   : {first.StoreName}");
            sb.AppendLine($"Ngày bán   : {first.OrderDate:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Nhân viên  : {first.StaffName}");
            sb.AppendLine($"Khách hàng : {first.CustomerName}");
            sb.AppendLine("----------------------------------------");

            foreach (var item in details)
            {
                sb.AppendLine($"{item.Title}");
                sb.AppendLine($"  SL: {item.Quantity}  ĐG: {item.UnitPrice:N0}  Giảm: {item.DiscountAmount:N0}  TT: {item.LineTotal:N0}");
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Tổng tiền  : {total:N0} VNĐ");
            sb.AppendLine($"Thanh toán : {first.PaymentMethod}");
            sb.AppendLine("========================================");
            sb.AppendLine("Cảm ơn quý khách!");
            return sb.ToString();
        }

        public string BuildReturnReceiptText(List<ReturnReceiptDetailFullViewModel> details)
        {
            PermissionService.RequireStaffOrAdmin();
            Require(details != null && details.Count > 0, "Không có dữ liệu phiếu trả.");

            var first = details[0];
            decimal total = details.Sum(x => x.RefundAmount);

            var sb = new StringBuilder();
            sb.AppendLine("========================================");
            sb.AppendLine("              BIÊN LAI TRẢ SÁCH");
            sb.AppendLine("========================================");
            sb.AppendLine($"Mã phiếu trả: {first.ReturnCode}");
            sb.AppendLine($"Cửa hàng    : {first.StoreName}");
            sb.AppendLine($"Ngày trả    : {first.ReturnDate:dd/MM/yyyy HH:mm}");
            sb.AppendLine($"Nhân viên   : {first.StaffName}");
            sb.AppendLine($"Khách hàng  : {first.CustomerName}");
            sb.AppendLine("----------------------------------------");

            foreach (var item in details)
            {
                sb.AppendLine($"{item.Title}");
                sb.AppendLine($"  SL trả: {item.Quantity}  ĐG: {item.UnitPrice:N0}  Hoàn: {item.RefundAmount:N0}");
                if (!string.IsNullOrWhiteSpace(item.ReturnReason)) sb.AppendLine($"  Lý do: {item.ReturnReason}");
            }

            sb.AppendLine("----------------------------------------");
            sb.AppendLine($"Tổng hoàn   : {total:N0} VNĐ");
            sb.AppendLine("========================================");
            return sb.ToString();
        }
    }
}
