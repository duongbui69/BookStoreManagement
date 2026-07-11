using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public partial class BookDetailsForm : Form
    {
        private readonly BookViewModel _bookVm;
        private readonly string? _description;

        public BookDetailsForm(BookViewModel bookVm, string? description)
        {
            _bookVm = bookVm;
            _description = description;
            InitializeComponent();
            ApplyTheme();
            LoadData();
        }
        
        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            lblAuthor.ForeColor = ThemeManager.TextSecondary;
            lblCategory.ForeColor = ThemeManager.TextSecondary;
            lblPublisher.ForeColor = ThemeManager.TextSecondary;
            lblISBN.ForeColor = ThemeManager.TextSecondary;
            lblPrice.ForeColor = ThemeManager.TextPrimary;
            lblStock.ForeColor = ThemeManager.TextSecondary;
            
            txtDescription.FillColor = ThemeManager.TextBoxBackground;
            txtDescription.ForeColor = ThemeManager.TextPrimary;
            txtDescription.BorderColor = ThemeManager.TextBoxBorder;
            
            btnClose.FillColor = ThemeManager.ButtonFill;
            btnClose.ForeColor = ThemeManager.ButtonText;
            
            // Allow clicking to drag the window (simple implementation)
            this.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { Win32.ReleaseCapture(); Win32.SendMessage(Handle, Win32.WM_NCLBUTTONDOWN, Win32.HT_CAPTION, 0); } };
            lblTitle.MouseDown += (s, e) => { if (e.Button == MouseButtons.Left) { Win32.ReleaseCapture(); Win32.SendMessage(Handle, Win32.WM_NCLBUTTONDOWN, Win32.HT_CAPTION, 0); } };
        }

        private void LoadData()
        {
            lblTitle.Text = _bookVm.Title;
            lblAuthor.Text = $"Author: {_bookVm.AuthorName}";
            lblCategory.Text = $"Category: {_bookVm.CategoryName}";
            lblPublisher.Text = $"Publisher: {_bookVm.PublisherName}";
            lblISBN.Text = $"ISBN: {_bookVm.ISBN}";
            lblPrice.Text = $"Price: {_bookVm.SellingPrice:N0} VND";
            lblStock.Text = $"Stock: {_bookVm.Quantity} (Min: {_bookVm.MinStock})";
            txtDescription.Text = _description ?? "No description available.";

            try
            {
                if (!string.IsNullOrEmpty(_bookVm.ImagePath))
                {
                    string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Covers", _bookVm.ImagePath);
                    if (File.Exists(imagePath))
                    {
                        picCover.Image = Image.FromFile(imagePath);
                    }
                }
            }
            catch { }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
    
    // Helper to allow borderless form dragging
    public static class Win32
    {
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
    }
}
