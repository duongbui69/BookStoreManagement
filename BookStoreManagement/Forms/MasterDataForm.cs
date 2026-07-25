using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public partial class MasterDataForm : Form
    {
        private string dataType;
        private int? recordId;

        private Panel pnlMain;
        private Label lblTitle;
        private Button btnClose;

        private Label lblName;
        private TextBox txtName;

        private Label lblDesc;
        private TextBox txtDesc;

        private Label lblPhone;
        private TextBox txtPhone;

        private Label lblEmail;
        private TextBox txtEmail;

        private Label lblStatus;
        private ComboBox cboStatus;

        private Button btnSave;
        private Button btnCancel;

        private CategoryRepository _categoryRepo;
        private AuthorRepository _authorRepo;
        private PublisherRepository _publisherRepo;

        public MasterDataForm(string dataType, int? id)
        {
            this.dataType = dataType;
            this.recordId = id;

            _categoryRepo = new CategoryRepository();
            _authorRepo = new AuthorRepository();
            _publisherRepo = new PublisherRepository();

            InitializeComponents();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponents()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Size = new Size(500, 450);

            Panel borderPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(2), BackColor = ThemeManager.TextBoxBorder };
            pnlMain = new Panel { Dock = DockStyle.Fill, BackColor = ThemeManager.CardBackground };
            borderPanel.Controls.Add(pnlMain);
            this.Controls.Add(borderPanel);

            lblTitle = new Label
            {
                Text = recordId.HasValue ? "Update data" : "Add new data",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(24, 24)
            };

            btnClose = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 12),
                Size = new Size(32, 32),
                Location = new Point(this.Width - 56, 20),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            pnlMain.Controls.Add(lblTitle);
            pnlMain.Controls.Add(btnClose);

            int yPos = 80;

            lblName = new Label { Text = "Name", AutoSize = true, Location = new Point(24, yPos), Font = new Font("Segoe UI", 10) };
            txtName = new TextBox { Location = new Point(24, yPos + 25), Width = this.Width - 52, Font = new Font("Segoe UI", 10), Padding = new Padding(8) };
            pnlMain.Controls.Add(lblName);
            pnlMain.Controls.Add(txtName);
            yPos += 70;

            if (dataType == "Publisher")
            {
                lblPhone = new Label { Text = "Phone number", AutoSize = true, Location = new Point(24, yPos), Font = new Font("Segoe UI", 10) };
                txtPhone = new TextBox { Location = new Point(24, yPos + 25), Width = this.Width - 52, Font = new Font("Segoe UI", 10), Padding = new Padding(8) };
                pnlMain.Controls.Add(lblPhone);
                pnlMain.Controls.Add(txtPhone);
                yPos += 70;

                lblEmail = new Label { Text = "Email", AutoSize = true, Location = new Point(24, yPos), Font = new Font("Segoe UI", 10) };
                txtEmail = new TextBox { Location = new Point(24, yPos + 25), Width = this.Width - 52, Font = new Font("Segoe UI", 10), Padding = new Padding(8) };
                pnlMain.Controls.Add(lblEmail);
                pnlMain.Controls.Add(txtEmail);
                yPos += 70;
            }
            else
            {
                lblDesc = new Label { Text = "Description", AutoSize = true, Location = new Point(24, yPos), Font = new Font("Segoe UI", 10) };
                txtDesc = new TextBox { Location = new Point(24, yPos + 25), Width = this.Width - 52, Height = 80, Multiline = true, Font = new Font("Segoe UI", 10), Padding = new Padding(8) };
                pnlMain.Controls.Add(lblDesc);
                pnlMain.Controls.Add(txtDesc);
                yPos += 120;
            }

            lblStatus = new Label { Text = "Status", AutoSize = true, Location = new Point(24, yPos), Font = new Font("Segoe UI", 10) };
            cboStatus = new ComboBox { Location = new Point(24, yPos + 25), Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Font = new Font("Segoe UI", 10) };
            cboStatus.Items.Add("Active");
            cboStatus.Items.Add("Locked");
            cboStatus.SelectedIndex = 0;
            pnlMain.Controls.Add(lblStatus);
            pnlMain.Controls.Add(cboStatus);
            yPos += 80;

            this.Height = yPos + 80;

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 36),
                Location = new Point(this.Width - 232, this.Height - 60),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = "Save changes",
                Size = new Size(120, 36),
                Location = new Point(this.Width - 148, this.Height - 60),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Regular)
            };
            btnSave.Click += BtnSave_Click;

            pnlMain.Controls.Add(btnCancel);
            pnlMain.Controls.Add(btnSave);
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.TextBoxBorder;
            pnlMain.BackColor = ThemeManager.CardBackground;

            lblTitle.ForeColor = ThemeManager.TextPrimary;
            btnClose.ForeColor = ThemeManager.TextSecondary;
            btnClose.BackColor = ThemeManager.CardBackground;

            lblName.ForeColor = ThemeManager.TextSecondary;
            txtName.BackColor = ThemeManager.Background;
            txtName.ForeColor = ThemeManager.TextPrimary;

            if (dataType == "Publisher")
            {
                lblPhone.ForeColor = ThemeManager.TextSecondary;
                txtPhone.BackColor = ThemeManager.Background;
                txtPhone.ForeColor = ThemeManager.TextPrimary;

                lblEmail.ForeColor = ThemeManager.TextSecondary;
                txtEmail.BackColor = ThemeManager.Background;
                txtEmail.ForeColor = ThemeManager.TextPrimary;
            }
            else
            {
                lblDesc.ForeColor = ThemeManager.TextSecondary;
                txtDesc.BackColor = ThemeManager.Background;
                txtDesc.ForeColor = ThemeManager.TextPrimary;
            }

            lblStatus.ForeColor = ThemeManager.TextSecondary;
            cboStatus.BackColor = ThemeManager.Background;
            cboStatus.ForeColor = ThemeManager.TextPrimary;

            btnCancel.BackColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;

            btnSave.BackColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
            btnSave.FlatAppearance.BorderSize = 0;
        }

        private async void LoadData()
        {
            if (!recordId.HasValue)
            {
                if (dataType == "Category") lblTitle.Text = "Add New Category";
                else if (dataType == "Author") lblTitle.Text = "Add New Author";
                else if (dataType == "Publisher") lblTitle.Text = "Add New Publisher";
                return;
            }

            if (dataType == "Category")
            {
                lblTitle.Text = "Update Category";
                var category = await _categoryRepo.GetByIdAsync(recordId.Value);
                if (category != null)
                {
                    txtName.Text = category.CategoryName;
                    txtDesc.Text = category.Description;
                    cboStatus.SelectedIndex = category.IsActive ? 0 : 1;
                }
            }
            else if (dataType == "Author")
            {
                lblTitle.Text = "Update Author";
                var author = await _authorRepo.GetByIdAsync(recordId.Value);
                if (author != null)
                {
                    txtName.Text = author.AuthorName;
                    txtDesc.Text = author.Description;
                    cboStatus.SelectedIndex = author.IsActive ? 0 : 1;
                }
            }
            else if (dataType == "Publisher")
            {
                lblTitle.Text = "Update Publisher";
                var publisher = await _publisherRepo.GetByIdAsync(recordId.Value);
                if (publisher != null)
                {
                    txtName.Text = publisher.PublisherName;
                    txtPhone.Text = publisher.Phone;
                    txtEmail.Text = publisher.Email;
                    cboStatus.SelectedIndex = publisher.IsActive ? 0 : 1;
                }
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter name!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isActive = cboStatus.SelectedIndex == 0;

            try
            {
                if (dataType == "Category")
                {
                    if (await _categoryRepo.IsNameExistsAsync(txtName.Text, recordId))
                    {
                        MessageBox.Show("Category name already exists!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var cat = new Category
                    {
                        Id = recordId ?? 0,
                        CategoryName = txtName.Text.Trim(),
                        Description = txtDesc?.Text?.Trim(),
                        IsActive = isActive
                    };

                    if (recordId.HasValue) await _categoryRepo.UpdateAsync(cat);
                    else await _categoryRepo.AddAsync(cat);
                }
                else if (dataType == "Author")
                {
                    if (await _authorRepo.IsNameExistsAsync(txtName.Text, recordId))
                    {
                        MessageBox.Show("Author name already exists!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var author = new Author
                    {
                        Id = recordId ?? 0,
                        AuthorName = txtName.Text.Trim(),
                        Description = txtDesc?.Text?.Trim(),
                        IsActive = isActive
                    };

                    if (recordId.HasValue) await _authorRepo.UpdateAsync(author);
                    else await _authorRepo.AddAsync(author);
                }
                else if (dataType == "Publisher")
                {
                    if (await _publisherRepo.IsNameExistsAsync(txtName.Text, recordId))
                    {
                        MessageBox.Show("Publisher name already exists!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    var publisher = new Publisher
                    {
                        Id = recordId ?? 0,
                        PublisherName = txtName.Text.Trim(),
                        Phone = txtPhone?.Text?.Trim(),
                        Email = txtEmail?.Text?.Trim(),
                        IsActive = isActive
                    };

                    if (recordId.HasValue) await _publisherRepo.UpdateAsync(publisher);
                    else await _publisherRepo.AddAsync(publisher);
                }

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

