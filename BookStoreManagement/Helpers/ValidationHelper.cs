using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace BookStoreManagement.Helpers
{
    /// <summary>
    /// Bộ công cụ kiểm tra tính hợp lệ dùng chung cho các form nhập liệu.
    /// Sử dụng ErrorProvider để hiển thị thông báo lỗi bên cạnh ô nhập liệu.
    /// </summary>
    public static class ValidationHelper
    {
        // ─────────────────────────────────────────────────────────────
        // Wire-up methods – gắn TextChanged + ErrorProvider vào control
        // Gọi trong InitializeComponent sau khi tạo control.
        // ─────────────────────────────────────────────────────────────

        /// <summary>Chỉ cho phép chữ cái (Unicode/tiếng Việt), khoảng trắng và dấu gạch ngang.</summary>
        public static void WireTextOnly(Control ctrl, ErrorProvider ep)
        {
            ctrl.TextChanged += (s, e) =>
            {
                bool invalid = !string.IsNullOrEmpty(ctrl.Text) &&
                               ctrl.Text.Any(c => !char.IsControl(c) && !char.IsLetter(c) && c != ' ' && c != '-');
                ep.SetError(ctrl, invalid ? "Trường này chỉ được nhập chữ cái và khoảng trắng" : "");
            };
        }

        /// <summary>Như WireTextOnly nhưng cho phép thêm dấu chấm (dùng cho tên có học vị như Th.S).</summary>
        public static void WireTextWithDot(Control ctrl, ErrorProvider ep)
        {
            ctrl.TextChanged += (s, e) =>
            {
                bool invalid = !string.IsNullOrEmpty(ctrl.Text) &&
                               ctrl.Text.Any(c => !char.IsControl(c) && !char.IsLetter(c) && c != ' ' && c != '-' && c != '.');
                ep.SetError(ctrl, invalid ? "Trường này chỉ được nhập chữ cái, khoảng trắng hoặc dấu chấm" : "");
            };
        }

        /// <summary>Chỉ cho phép chữ số 0-9 (SĐT, CCCD, số nguyên).</summary>
        public static void WireDigitsOnly(Control ctrl, ErrorProvider ep)
        {
            ctrl.TextChanged += (s, e) =>
            {
                bool invalid = !string.IsNullOrEmpty(ctrl.Text) &&
                               ctrl.Text.Any(c => !char.IsDigit(c));
                ep.SetError(ctrl, invalid ? "Trường này chỉ được nhập chữ số (0-9)" : "");
            };
        }

        /// <summary>Chỉ cho phép số thực không âm (giá tiền, lương).</summary>
        public static void WireDecimalOnly(Control ctrl, ErrorProvider ep)
        {
            ctrl.TextChanged += (s, e) =>
            {
                string text = ctrl.Text.Trim();
                if (string.IsNullOrEmpty(text)) { ep.SetError(ctrl, ""); return; }
                bool hasInvalidChars = text.Any(c => !char.IsDigit(c) && c != '.');
                bool multipleDots = text.Count(c => c == '.') > 1;
                ep.SetError(ctrl, (hasInvalidChars || multipleDots)
                    ? "Vui lòng nhập số hợp lệ (VD: 12500 hoặc 12500.5)" : "");
            };
        }

        /// <summary>Kiểm tra định dạng email theo thời gian thực.</summary>
        public static void WireEmailValidation(Control ctrl, ErrorProvider ep)
        {
            ctrl.TextChanged += (s, e) =>
            {
                string text = ctrl.Text.Trim();
                if (string.IsNullOrEmpty(text)) { ep.SetError(ctrl, ""); return; }
                ep.SetError(ctrl, !IsValidEmail(text)
                    ? "Địa chỉ email không hợp lệ (VD: abc@gmail.com)" : "");
            };
        }

        // ─────────────────────────────────────────────────────────────
        // Validation methods – gọi trước khi lưu (BtnSave_Click)
        // ─────────────────────────────────────────────────────────────

        /// <summary>Kiểm tra định dạng email hợp lệ.</summary>
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return true; // email không bắt buộc
            return Regex.IsMatch(email.Trim(),
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        /// <summary>Kiểm tra số điện thoại (10–11 chữ số).</summary>
        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return true; // SĐT không bắt buộc
            return Regex.IsMatch(phone.Trim(), @"^\d{10,11}$");
        }

        /// <summary>Kiểm tra CMND (9 số) hoặc CCCD (12 số).</summary>
        public static bool IsValidIdentity(string identity)
        {
            if (string.IsNullOrWhiteSpace(identity)) return true; // không bắt buộc
            return Regex.IsMatch(identity.Trim(), @"^\d{9}$|^\d{12}$");
        }

        /// <summary>Kiểm tra tên đăng nhập (4-50 ký tự, chỉ chữ/số/gạch dưới/dấu chấm).</summary>
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            return Regex.IsMatch(username.Trim(), @"^[a-zA-Z0-9_.]{4,50}$");
        }

        /// <summary>Kiểm tra mật khẩu (tối thiểu 6 ký tự).</summary>
        public static bool IsValidPassword(string password)
        {
            return !string.IsNullOrWhiteSpace(password) && password.Length >= 6;
        }
    }
}
