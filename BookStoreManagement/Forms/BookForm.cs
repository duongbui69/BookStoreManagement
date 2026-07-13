using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public class BookForm : Form
    {
        private CatalogService _catalogService;
        private CategoryService _categoryService;
        private AuthorService _authorService;
        private PublisherService _publisherService;

        private TextBox txtBookCode, txtISBN, txtTitle, txtPublishYear, txtPageCount, txtSellingPrice, txtQuantity, txtMinStock, txtDescription;
        private ComboBox cbCategory, cbAuthor, cbPublisher;
        private CheckBox chkIsActive;
        private PictureBox pbImage;
        private Button btnBrowseImg, btnSave, btnCancel;

        private string _imagePath = "";
        public Book? BookModel { get; private set; }

        public BookForm(Book? bookToEdit = null)
        {
            _catalogService = new CatalogService();
            _categoryService = new CategoryService();
            _authorService = new AuthorService();
            _publisherService = new PublisherService();
            
            BookModel = bookToEdit;
            InitializeComponent();
            LoadDropdowns();
            
            if (BookModel != null) BindData();
            ApplyTheme();
        }

        private void InitializeComponent()
        {
            this.Text = BookModel == null ? "Add New Book" : "Edit Book";
            this.Size = new Size(800, 650);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

            Label lblTitle = new Label { Text = this.Text, Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblTitle);

            // Left Col
            int y = 70;
            txtBookCode = CreateInput("Book Code", 20, ref y);
            txtISBN = CreateInput("ISBN", 20, ref y);
            txtTitle = CreateInput("Title", 20, ref y);
            txtPublishYear = CreateInput("Publish Year", 20, ref y);
            txtPageCount = CreateInput("Page Count", 20, ref y);
            
            Label lblCat = new Label { Text = "Category", Location = new Point(20, y), AutoSize = true };
            cbCategory = new ComboBox { Location = new Point(20, y + 20), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            this.Controls.AddRange(new Control[] { lblCat, cbCategory });
            y += 60;

            Label lblAuth = new Label { Text = "Author", Location = new Point(20, y), AutoSize = true };
            cbAuthor = new ComboBox { Location = new Point(20, y + 20), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            this.Controls.AddRange(new Control[] { lblAuth, cbAuthor });
            y += 60;

            // Right Col
            int yRight = 70;
            Label lblPub = new Label { Text = "Publisher", Location = new Point(350, yRight), AutoSize = true };
            cbPublisher = new ComboBox { Location = new Point(350, yRight + 20), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10F) };
            this.Controls.AddRange(new Control[] { lblPub, cbPublisher });
            yRight += 60;

            txtSellingPrice = CreateInput("Selling Price", 350, ref yRight);
            txtQuantity = CreateInput("Initial Quantity (Global)", 350, ref yRight);
            txtMinStock = CreateInput("Min Stock", 350, ref yRight);
            txtDescription = CreateInput("Description", 350, ref yRight);

            chkIsActive = new CheckBox { Text = "Is Active", Location = new Point(350, yRight), AutoSize = true, Checked = true };
            this.Controls.Add(chkIsActive);
            yRight += 40;

            // Image Upload
            pbImage = new PictureBox { Location = new Point(670, 70), Width = 100, Height = 130, BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            btnBrowseImg = new Button { Text = "Browse...", Location = new Point(670, 210), Width = 100, Height = 30, FlatStyle = FlatStyle.Flat };
            btnBrowseImg.Click += BtnBrowseImg_Click;
            this.Controls.AddRange(new Control[] { pbImage, btnBrowseImg });

            // Buttons
            btnSave = new Button { Text = "Save", Location = new Point(280, 550), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            btnCancel = new Button { Text = "Cancel", Location = new Point(400, 550), Width = 100, Height = 40, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private TextBox CreateInput(string label, int x, ref int y)
        {
            Label lbl = new Label { Text = label, Location = new Point(x, y), AutoSize = true };
            TextBox txt = new TextBox { Location = new Point(x, y + 20), Width = 300, Font = new Font("Segoe UI", 10F) };
            this.Controls.AddRange(new Control[] { lbl, txt });
            y += 60;
            return txt;
        }

        private void LoadDropdowns()
        {
            try {
                cbCategory.DataSource = _categoryService.Search("");
                cbCategory.DisplayMember = "CategoryName"; cbCategory.ValueMember = "Id";
                
                var authors = _authorService.Search("");
                authors.Insert(0, new Author { Id = 0, AuthorName = "--- Select ---" });
                cbAuthor.DataSource = authors;
                cbAuthor.DisplayMember = "AuthorName"; cbAuthor.ValueMember = "Id";

                var publishers = _publisherService.Search("");
                publishers.Insert(0, new Publisher { Id = 0, PublisherName = "--- Select ---" });
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
                pbImage.Image = Image.FromFile(_imagePath);
            }
        }

        private void BtnBrowseImg_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                if (ofd.ShowDialog() == DialogResult.OK) {
                    pbImage.Image = Image.FromFile(ofd.FileName);
                    
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
            try {
                if (BookModel == null) BookModel = new Book();
                
                BookModel.BookCode = txtBookCode.Text.Trim();
                BookModel.ISBN = txtISBN.Text.Trim();
                BookModel.Title = txtTitle.Text.Trim();
                if (int.TryParse(txtPublishYear.Text, out int y)) BookModel.PublishYear = y;
                if (int.TryParse(txtPageCount.Text, out int pc)) BookModel.PageCount = pc;
                if (decimal.TryParse(txtSellingPrice.Text, out decimal sp)) BookModel.SellingPrice = sp;
                if (int.TryParse(txtQuantity.Text, out int q)) BookModel.Quantity = q;
                if (int.TryParse(txtMinStock.Text, out int ms)) BookModel.MinStock = ms;
                BookModel.Description = txtDescription.Text.Trim();
                BookModel.ImagePath = _imagePath;
                BookModel.IsActive = chkIsActive.Checked;
                
                if (cbCategory.SelectedValue != null) BookModel.CategoryId = (int)cbCategory.SelectedValue;
                
                int authId = (int)cbCategory.SelectedValue;
                BookModel.AuthorId = authId > 0 ? authId : (int?)null;
                
                int pubId = (int)cbPublisher.SelectedValue;
                BookModel.PublisherId = pubId > 0 ? pubId : (int?)null;

                BookService _bookService = new BookService(); // Missing instance
                if (BookModel.Id == 0) {
                    _bookService.Add(BookModel);
                    MessageBox.Show("Book added.");
                } else {
                    _bookService.Update(BookModel);
                    MessageBox.Show("Book updated.");
                }
                this.DialogResult = DialogResult.OK;
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            foreach (Control c in this.Controls) {
                if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
                if (c is TextBox t) { t.BackColor = ThemeManager.TextBoxBackground; t.ForeColor = ThemeManager.TextPrimary; }
                if (c is CheckBox cb) cb.ForeColor = ThemeManager.TextPrimary;
            }
            btnBrowseImg.BackColor = ThemeManager.HoverColor; btnBrowseImg.ForeColor = ThemeManager.TextPrimary; btnBrowseImg.FlatAppearance.BorderSize=0;
            btnSave.BackColor = ThemeManager.ButtonFill; btnSave.ForeColor = ThemeManager.ButtonText; btnSave.FlatAppearance.BorderSize = 0;
            btnCancel.BackColor = ThemeManager.HoverColor; btnCancel.ForeColor = ThemeManager.TextPrimary; btnCancel.FlatAppearance.BorderSize = 0;
        }
    }
}


