using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public class StoreForm : Form
    {
        private Store? _store;
        private StoreRepository _repository;
        private Label lblTitle;
        private TextBox txtStoreCode, txtStoreName, txtAddress, txtPhone, txtManagerName;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public StoreForm(Store? store = null)
        {
            _store = store;
            _repository = new StoreRepository();
            InitializeComponent();
            SetupTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = _store == null ? "Add new store" : "Update store";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label
            {
                Text = this.Text,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 20),
                AutoSize = true
            };
            this.Controls.Add(lblTitle);

            int startY = 70;
            int spacing = 65;

            // Store Code
            this.Controls.Add(new Label { Text = "Store ID / Code *", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9) });
            txtStoreCode = new TextBox { Location = new Point(20, startY + 20), Width = 440, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(txtStoreCode);
            startY += spacing;

            // Store Name
            this.Controls.Add(new Label { Text = "Store Name *", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9) });
            txtStoreName = new TextBox { Location = new Point(20, startY + 20), Width = 440, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(txtStoreName);
            startY += spacing;

            // Address
            this.Controls.Add(new Label { Text = "Address *", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9) });
            txtAddress = new TextBox { Location = new Point(20, startY + 20), Width = 440, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(txtAddress);
            startY += spacing;

            // Phone
            this.Controls.Add(new Label { Text = "Phone Number", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9) });
            txtPhone = new TextBox { Location = new Point(20, startY + 20), Width = 440, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(txtPhone);
            startY += spacing;

            // Manager Name
            this.Controls.Add(new Label { Text = "Manager Name", Location = new Point(20, startY), AutoSize = true, Font = new Font("Segoe UI", 9) });
            txtManagerName = new TextBox { Location = new Point(20, startY + 20), Width = 440, Font = new Font("Segoe UI", 10) };
            this.Controls.Add(txtManagerName);
            startY += spacing;

            // Status
            chkIsActive = new CheckBox { Text = "Active", Location = new Point(20, startY + 20), AutoSize = true, Font = new Font("Segoe UI", 10), Checked = true };
            this.Controls.Add(chkIsActive);
            startY += spacing;

            // Buttons
            btnSave = new Button
            {
                Text = "Save",
                Location = new Point(250, startY + 20),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(360, startY + 20),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
                Cursor = Cursors.Hand
            };
            btnCancel.Click += (s, e) => this.Close();
            this.Controls.Add(btnCancel);
        }

        private void SetupTheme()
        {
            this.BackColor = ThemeManager.Background;
            this.ForeColor = ThemeManager.TextPrimary;

            foreach (Control control in this.Controls)
            {
                if (control is Label lbl)
                {
                    lbl.ForeColor = lbl == lblTitle ? ThemeManager.TextPrimary : ThemeManager.TextSecondary;
                }
                else if (control is TextBox txt)
                {
                    txt.BackColor = ThemeManager.CardBackground;
                    txt.ForeColor = ThemeManager.TextPrimary;
                    txt.BorderStyle = BorderStyle.FixedSingle;
                }
                else if (control is CheckBox chk)
                {
                    chk.ForeColor = ThemeManager.TextPrimary;
                }
            }

            btnSave.BackColor = ThemeManager.ButtonFill;
            btnSave.ForeColor = ThemeManager.ButtonText;
            btnSave.FlatAppearance.BorderSize = 0;

            btnCancel.BackColor = ThemeManager.CardBackground;
            btnCancel.ForeColor = ThemeManager.TextPrimary;
            btnCancel.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;
        }

        private void LoadData()
        {
            if (_store != null)
            {
                txtStoreCode.Text = _store.StoreCode;
                txtStoreName.Text = _store.StoreName;
                txtAddress.Text = _store.Address;
                txtPhone.Text = _store.Phone;
                txtManagerName.Text = _store.ManagerName;
                chkIsActive.Checked = _store.IsActive;

                txtStoreCode.Enabled = false; // Usually don't allow changing the code once created
            }
        }

        private void BtnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStoreCode.Text) || 
                string.IsNullOrWhiteSpace(txtStoreName.Text) || 
                string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please fill required fields (*).", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_store == null)
            {
                if (_repository.IsStoreCodeExists(txtStoreCode.Text))
                {
                    MessageBox.Show("Store code already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var newStore = new Store
                {
                    StoreCode = txtStoreCode.Text,
                    StoreName = txtStoreName.Text,
                    Address = txtAddress.Text,
                    Phone = txtPhone.Text,
                    ManagerName = txtManagerName.Text,
                    IsActive = chkIsActive.Checked
                };
                _repository.Add(newStore);
            }
            else
            {
                _store.StoreName = txtStoreName.Text;
                _store.Address = txtAddress.Text;
                _store.Phone = string.IsNullOrWhiteSpace(txtPhone.Text) ? null : txtPhone.Text;
                _store.ManagerName = string.IsNullOrWhiteSpace(txtManagerName.Text) ? null : txtManagerName.Text;
                _store.IsActive = chkIsActive.Checked;

                _repository.Update(_store);
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
