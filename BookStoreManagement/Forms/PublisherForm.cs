using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Repositories;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public partial class PublisherForm : Form
    {
        private PublisherRepository _publisherRepository;
        private Publisher? _currentPublisher;
        public bool IsDataSaved { get; private set; }

        private Label lblTitle, lblId, lblName, lblPhone, lblEmail, lblAddress, lblStatus;
        private TextBox txtId, txtName, txtPhone, txtEmail, txtAddress;
        private CheckBox chkIsActive;
        private Button btnSave, btnCancel;

        public PublisherForm(int? publisherId = null)
        {
            InitializeComponentLayout();
            _publisherRepository = new PublisherRepository();
            ApplyTheme();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;

            if (publisherId.HasValue)
            {
                lblTitle.Text = "Edit Publisher";
                LoadPublisherData(publisherId.Value);
            }
            else
            {
                lblTitle.Text = "Add New Publisher";
                _currentPublisher = new Publisher();
                chkIsActive.Checked = true;
                txtId.Text = "Auto-generate";
            }
        }

        private void InitializeComponentLayout()
        {
            this.Text = "Update Publisher";
            this.Size = new Size(500, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblTitle = new Label { Location = new Point(24, 24), AutoSize = true, Font = new Font("Segoe UI", 16, FontStyle.Bold) };
            this.Controls.Add(lblTitle);

            int startY = 80;

            lblId = new Label { Text = "Publisher ID", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtId = new TextBox { Location = new Point(24, startY + 25), Width = 436, Font = new Font("Segoe UI", 10), ReadOnly = true, Enabled = false };
            this.Controls.AddRange(new Control[] { lblId, txtId });
            startY += 70;

            lblName = new Label { Text = "Publisher name (*)", Location = new Point(24, startY), AutoSize = true, Font = new Font("Segoe UI", 10) };
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
            chkIsActive = new CheckBox { Text = "Cooperating", Location = new Point(24, startY + 25), AutoSize = true, Font = new Font("Segoe UI", 10) };
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

        private async void LoadPublisherData(int id)
        {
            try
            {
                _currentPublisher = await _publisherRepository.GetByIdAsync(id);
                if (_currentPublisher != null)
                {
                    txtId.Text = $"NXB{_currentPublisher.Id:D3}";
                    txtName.Text = _currentPublisher.PublisherName;
                    txtPhone.Text = _currentPublisher.Phone;
                    txtEmail.Text = _currentPublisher.Email;
                    txtAddress.Text = _currentPublisher.Address;
                    chkIsActive.Checked = _currentPublisher.IsActive;
                }
                else
                {
                    MessageBox.Show("Publisher data not found!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show("Please enter publisher name!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            if (await _publisherRepository.IsNameExistsAsync(name, _currentPublisher?.Id > 0 ? _currentPublisher.Id : null))
            {
                MessageBox.Show("Publisher name already exists!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            try
            {
                _currentPublisher.PublisherName = name;
                _currentPublisher.Phone = txtPhone.Text.Trim();
                _currentPublisher.Email = txtEmail.Text.Trim();
                _currentPublisher.Address = txtAddress.Text.Trim();
                _currentPublisher.IsActive = chkIsActive.Checked;

                if (_currentPublisher.Id == 0)
                {
                    await _publisherRepository.AddAsync(_currentPublisher);
                }
                else
                {
                    await _publisherRepository.UpdateAsync(_currentPublisher);
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

