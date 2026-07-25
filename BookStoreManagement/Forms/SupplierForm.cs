using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public partial class SupplierForm : Form
    {
        private SupplierRepository _supplierRepository;
        private Supplier? _currentSupplier;
        public bool IsDataSaved { get; private set; }

        private Label lblTitle, lblId, lblName, lblPhone, lblEmail, lblAddress, lblStatus;
        private TextBox txtId, txtName, txtPhone, txtEmail, txtAddress;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public SupplierForm(int? supplierId = null)
        {
            InitializeComponentLayout();
            _supplierRepository = new SupplierRepository();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

            if (supplierId.HasValue)
            {
                lblTitle.Text = "Edit Supplier";
                LoadSupplierData(supplierId.Value);
            }
            else
            {
                lblTitle.Text = "Add New Supplier";
                _currentSupplier = new Supplier();
                chkIsActive.Checked = true;
                txtId.Text = "Auto-generate";
            }
        }

        private void InitializeComponentLayout()
        {
            this.Text = "Update Supplier";
            this.Size = new Size(500, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label { Location = new Point(24, 24), AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
            this.Controls.Add(lblTitle);

            int startY = 80;

            lblId = new Label { Text = "Supplier ID", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtId = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10), ReadOnly = true, Enabled = false };
            this.Controls.AddRange(new Control[] { lblId, txtId });
            startY += 70;

            lblName = new Label { Text = "Supplier name (*)", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtName = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblName, txtName });
            startY += 70;

            lblPhone = new Label { Text = "Phone number", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtPhone = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblPhone, txtPhone });
            startY += 70;

            lblEmail = new Label { Text = "Email", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtEmail = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblEmail, txtEmail });
            startY += 70;

            lblAddress = new Label { Text = "Address", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtAddress = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblAddress, txtAddress });
            startY += 70;

            lblStatus = new Label { Text = "Status", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            chkIsActive = new CheckBox { Text = "Trading", Location = new Point(24, startY + 25), AutoSize = true, Font = new Font("Segoe UI", 10) };
            this.Controls.AddRange(new Control[] { lblStatus, chkIsActive });

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 36),
                Location = new Point(this.Width - 250, this.Height - 80),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            btnSave = new Button
            {
                Text = "Save changes",
                Size = new Size(120, 36),
                Location = new Point(this.Width - 140, this.Height - 80),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnSave.Click += BtnSave_Click;

            this.Controls.AddRange(new Control[] { btnSave, btnCancel });
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.CardBackground;
            lblTitle.ForeColor = ThemeManager.TextPrimary;
            
            Label[] labels = { lblId, lblName, lblPhone, lblEmail, lblAddress, lblStatus };
            foreach (var lbl in labels)
            {
                lbl.ForeColor = ThemeManager.TextSecondary;
            }

            TextBox[] textBoxes = { txtId, txtName, txtPhone, txtEmail, txtAddress };
            foreach (var txt in textBoxes)
            {
                txt.BackColor = ThemeManager.TextBoxBackground;
                txt.ForeColor = ThemeManager.TextPrimary;
                txt.BorderStyle = BorderStyle.FixedSingle;
            }

            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;
            btnCancel.BackColor = ThemeManager.CardBackground;

            btnSave.BackColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
            btnSave.FlatAppearance.BorderSize = 0;
            
            chkIsActive.ForeColor = ThemeManager.TextPrimary;
        }

        private async void LoadSupplierData(int id)
        {
            try
            {
                _currentSupplier = await _supplierRepository.GetByIdAsync(id);
                if (_currentSupplier != null)
                {
                    txtId.Text = $"NCC{_currentSupplier.Id:D3}";
                    txtName.Text = _currentSupplier.SupplierName;
                    txtPhone.Text = _currentSupplier.Phone;
                    txtEmail.Text = _currentSupplier.Email;
                    txtAddress.Text = _currentSupplier.Address;
                    chkIsActive.Checked = _currentSupplier.IsActive;
                }
                else
                {
                    MessageBox.Show("Supplier data not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Please enter supplier name!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (await _supplierRepository.IsNameExistsAsync(name, _currentSupplier?.Id > 0 ? _currentSupplier.Id : null))
            {
                MessageBox.Show("Supplier name already exists!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                _currentSupplier.SupplierName = name;
                _currentSupplier.Phone = txtPhone.Text.Trim();
                _currentSupplier.Email = txtEmail.Text.Trim();
                _currentSupplier.Address = txtAddress.Text.Trim();
                _currentSupplier.IsActive = chkIsActive.Checked;

                if (_currentSupplier.Id == 0)
                {
                    await _supplierRepository.AddAsync(_currentSupplier);
                }
                else
                {
                    await _supplierRepository.UpdateAsync(_currentSupplier);
                }

                IsDataSaved = true;
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

