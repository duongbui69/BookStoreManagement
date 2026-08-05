using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;
using Guna.UI2.WinForms;

namespace BookStoreManagement.Forms
{
    public class BookForm : Form
    {
        private BookService _bookService;
        private CategoryService _categoryService;
        private AuthorService _authorService;
        private PublisherService _publisherService;

        private Guna2TextBox txtBookCode, txtISBN, txtTitle, txtPublishYear, txtPageCount, txtSellingPrice, txtQuantity, txtMinStock, txtDescription;
        private Guna2ComboBox cbCategory, cbAuthor, cbPublisher;
        private Guna2CheckBox chkIsActive;
        private Guna2PictureBox pbImage;
        private Guna2Button btnBrowseImg, btnSave, btnCancel;
        private ErrorProvider _errorProvider;

        private string _imagePath = "";
        public Book? BookModel { get; private set; }

        public BookForm(Book? bookToEdit = null)
        {
            _bookService = new BookService();
            _categoryService = new CategoryService();
            _authorService = new AuthorService();
            _publisherService = new PublisherService();
            
            BookModel = bookToEdit;
            _errorProvider = new ErrorProvider { BlinkStyle = ErrorBlinkStyle.NeverBlink };
            InitializeComponent();
            _errorProvider.ContainerControl = this;
            LoadDropdowns();
            
            if (BookModel != null) BindData();
            
            cbCategory.SelectedIndexChanged += (s, e) => {
                if (cbCategory.Focused || cbCategory.ContainsFocus)
                {
                    if (cbAuthor.Items.Count > 0) cbAuthor.SelectedIndex = 0;
                    if (cbPublisher.Items.Count > 0) cbPublisher.SelectedIndex = 0;
                }
            };

            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = BookModel == null ? "Thêm Sách mới" : "Chỉnh sửa Sách";
            this.Size = new Size(860, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ControlBox = false;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 18F, FontStyle.Bold), Location = new Point(24, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            // Left Col (X = 24)
            int yLeft = 70;
            txtBookCode = CreateInput("Mã sách", 24, ref yLeft);
            txtBookCode.MaxLength = 50;

            txtISBN = CreateInput("Mã ISBN", 24, ref yLeft);
            txtISBN.MaxLength = 30;

            txtTitle = CreateInput("Tiêu đề sách", 24, ref yLeft);
            txtTitle.MaxLength = 200;

            txtPublishYear = CreateInput("Năm xuất bản", 24, ref yLeft);
            txtPublishYear.MaxLength = 4;
            ValidationHelper.WireDigitsOnly(txtPublishYear, _errorProvider);

            txtPageCount = CreateInput("Số trang", 24, ref yLeft);
            txtPageCount.MaxLength = 6;
            ValidationHelper.WireDigitsOnly(txtPageCount, _errorProvider);
            
            Label lblCat = new Label { Text = "Danh mục", Location = new Point(24, yLeft), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbCategory = new Guna2ComboBox { Location = new Point(24, yLeft + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblCat, cbCategory });
            yLeft += 75;

            Label lblAuth = new Label { Text = "Tác giả", Location = new Point(24, yLeft), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbAuthor = new Guna2ComboBox { Location = new Point(24, yLeft + 25), Width = 340, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblAuth, cbAuthor });

            // Right Col (X = 400)
            int yRight = 70;
            Label lblPub = new Label { Text = "Nhà xuất bản", Location = new Point(400, yRight), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            cbPublisher = new Guna2ComboBox { Location = new Point(400, yRight + 25), Width = 280, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lblPub, cbPublisher });
            yRight += 75;

            txtSellingPrice = CreateInput("Giá bán (VNĐ)", 400, ref yRight, 280);
            txtSellingPrice.MaxLength = 20;
            ValidationHelper.WireDecimalOnly(txtSellingPrice, _errorProvider);

            txtQuantity = CreateInput("Số lượng tồn kho ban đầu", 400, ref yRight, 280);
            txtQuantity.MaxLength = 10;
            ValidationHelper.WireDigitsOnly(txtQuantity, _errorProvider);

            txtMinStock = CreateInput("Tồn kho tối thiểu", 400, ref yRight, 280);
            txtMinStock.MaxLength = 10;
            ValidationHelper.WireDigitsOnly(txtMinStock, _errorProvider);
            
            Label lblDesc = new Label { Text = "Mô tả / Giới thiệu sách", Location = new Point(400, yRight), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            txtDescription = new Guna2TextBox { Location = new Point(400, yRight + 25), Width = 400, Height = 80, Font = new Font("Segoe UI", 10F), Multiline = true, BorderRadius = 4, MaxLength = 500 };
            this.Controls.AddRange(new Control[] { lblDesc, txtDescription });
            yRight += 115;

            chkIsActive = new Guna2CheckBox { Text = "Đang mở bán (Hoạt động)", Location = new Point(400, yRight), AutoSize = true, Checked = true, Font = new Font("Segoe UI", 10F) };
            this.Controls.Add(chkIsActive);
            
            // Image Upload
            pbImage = new Guna2PictureBox { Location = new Point(700, 70), Width = 100, Height = 130, BorderRadius = 4, SizeMode = PictureBoxSizeMode.Zoom };
            btnBrowseImg = new Guna2Button { Text = "Chọn ảnh", Location = new Point(700, 210), Width = 100, Height = 36, BorderRadius = 4, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnBrowseImg.Click += BtnBrowseImg_Click;
            this.Controls.AddRange(new Control[] { pbImage, btnBrowseImg });

            // Buttons
            btnSave = new Guna2Button { Text = "Lưu thông tin", Location = new Point(270, 580), Width = 140, Height = 45, BorderRadius = 8, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Guna2Button { Text = "Hủy bỏ", Location = new Point(450, 580), Width = 140, Height = 45, BorderRadius = 8, Font = new Font("Segoe UI", 10F, FontStyle.Bold), Cursor = Cursors.Hand, FillColor = Color.Transparent, BorderThickness = 1 };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private Guna2TextBox CreateInput(string label, int x, ref int y, int width = 340)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            Guna2TextBox txt = new Guna2TextBox { Location = new Point(x, y + 25), Width = width, Height = 40, Font = new Font("Segoe UI", 10F), BorderRadius = 4 };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += 75;
            return txt;
        }

        private void LoadDropdowns()
        {
            try {
                cbCategory.DataSource = _categoryService.Search("");
                cbCategory.DisplayMember = "CategoryName"; cbCategory.ValueMember = "Id";
                
                var authors = _authorService.Search("");
                authors.Insert(0, new Author { Id = 0, AuthorName = "--- Chọn Tác Giả ---" });
                cbAuthor.DataSource = authors;
                cbAuthor.DisplayMember = "AuthorName"; cbAuthor.ValueMember = "Id";

                var publishers = _publisherService.Search("");
                publishers.Insert(0, new Publisher { Id = 0, PublisherName = "--- Chọn Nhà Xuất Bản ---" });
                cbPublisher.DataSource = publishers;
                cbPublisher.DisplayMember = "PublisherName"; cbPublisher.ValueMember = "Id";
            } catch {}
        }

        private void BindData()
        {
            if (BookModel == null) return;
            txtBookCode.Text = BookModel.BookCode;
            txtISBN.Text = BookModel.ISBN;
            txtTitle.Text = BookModel.Title;
            txtPublishYear.Text = BookModel.PublishYear?.ToString();
            txtPageCount.Text = BookModel.PageCount?.ToString();
            txtSellingPrice.Text = BookModel.SellingPrice.ToString();
            txtQuantity.Text = BookModel.Quantity.ToString();
            txtQuantity.Enabled = false; // Cannot edit quantity directly after creation
            txtMinStock.Text = BookModel.MinStock.ToString();
            txtDescription.Text = BookModel.Description;
            chkIsActive.Checked = BookModel.IsActive;
            
            cbCategory.SelectedValue = BookModel.CategoryId;
            cbAuthor.SelectedValue = BookModel.AuthorId ?? 0;
            cbPublisher.SelectedValue = BookModel.PublisherId ?? 0;
            
            _imagePath = BookModel.ImagePath;
            if (!string.IsNullOrEmpty(_imagePath) && File.Exists(_imagePath)) {
                using (var fs = new System.IO.FileStream(_imagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                {
                    var temp = Image.FromStream(fs);
                    pbImage.Image = new Bitmap(temp);
                }
            }
        }

        private void BtnBrowseImg_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK) {
                    using (var fs = new System.IO.FileStream(ofd.FileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        var temp = Image.FromStream(fs);
                        pbImage.Image = new Bitmap(temp);
                    }
                    
                    // Copy to Images folder
                    string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                    if (!Directory.Exists(imgDir)) Directory.CreateDirectory(imgDir);
                    
                    string newFileName = Guid.NewGuid().ToString() + Path.GetExtension(ofd.FileName);
                    _imagePath = Path.Combine(imgDir, newFileName);
                    File.Copy(ofd.FileName, _imagePath, true);
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            // --- Validation ---
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                MessageBox.Show("Vui lòng nhập tiêu đề sách.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }

            if (!string.IsNullOrWhiteSpace(txtPublishYear.Text))
            {
                if (!int.TryParse(txtPublishYear.Text, out int yr) || yr < 1000 || yr > DateTime.Now.Year)
                {
                    MessageBox.Show("Năm xuất bản không hợp lệ.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPublishYear.Focus();
                    return;
                }
            }

            if (!decimal.TryParse(txtSellingPrice.Text, out decimal sp) || sp < 0)
            {
                MessageBox.Show("Giá bán không hợp lệ. Vui lòng nhập số dương.", "Lỗi dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSellingPrice.Focus();
                return;
            }
            // --- End Validation ---

            try {
                if (BookModel == null) BookModel = new Book();
                
                BookModel.BookCode = txtBookCode.Text.Trim();
                BookModel.ISBN = txtISBN.Text.Trim();
                BookModel.Title = txtTitle.Text.Trim();
                if (int.TryParse(txtPublishYear.Text, out int y)) BookModel.PublishYear = y;
                if (int.TryParse(txtPageCount.Text, out int pc)) BookModel.PageCount = pc;
                if (decimal.TryParse(txtSellingPrice.Text, out decimal spVal)) BookModel.SellingPrice = spVal;
                if (int.TryParse(txtQuantity.Text, out int q)) BookModel.Quantity = q;
                if (int.TryParse(txtMinStock.Text, out int ms)) BookModel.MinStock = ms;
                BookModel.Description = txtDescription.Text.Trim();
                BookModel.IsActive = chkIsActive.Checked;
                BookModel.CategoryId = (int)cbCategory.SelectedValue;
                
                int authId = (int)cbAuthor.SelectedValue;
                BookModel.AuthorId = authId > 0 ? authId : (int?)null;

                int pubId = (int)cbPublisher.SelectedValue;
                BookModel.PublisherId = pubId > 0 ? pubId : (int?)null;

                BookModel.ImagePath = _imagePath;

                if (BookModel.Id == 0) {
                    _bookService.Add(BookModel);
                    MessageBox.Show("Đã thêm sách thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                } else {
                    _bookService.Update(BookModel);
                    MessageBox.Show("Đã cập nhật sách thành công.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            foreach (Control c in this.Controls) {
                if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
                if (c is Guna2TextBox t) { 
                    t.FillColor = ThemeManager.TextBoxBackground; 
                    t.ForeColor = ThemeManager.TextPrimary; 
                    t.BorderColor = ThemeManager.TextBoxBorder; 
                }
                if (c is Guna2ComboBox cb) {
                    cb.FillColor = ThemeManager.TextBoxBackground;
                    cb.ForeColor = ThemeManager.TextPrimary;
                    cb.BorderColor = ThemeManager.TextBoxBorder;
                }
                if (c is Guna2CheckBox chk) chk.ForeColor = ThemeManager.TextPrimary;
            }
            
            btnBrowseImg.FillColor = ThemeManager.ButtonFill;
            btnBrowseImg.ForeColor = ThemeManager.ButtonText;
            
            btnSave.FillColor = ThemeManager.ButtonFill; 
            btnSave.ForeColor = ThemeManager.ButtonText; 
            
            btnCancel.BorderColor = ThemeManager.TextBoxBorder; 
            btnCancel.ForeColor = ThemeManager.TextPrimary; 
        }
    }
}
