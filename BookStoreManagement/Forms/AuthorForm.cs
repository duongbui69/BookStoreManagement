using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public class AuthorForm : Form
    {
        private AuthorService _service;
        private DataGridView dgvList;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnEdit;
        
        // Input fields
        private Panel pnlInput;
        private TextBox txtId;
        private TextBox txtAuthorName;
        private TextBox txtDescription;
        private CheckBox chkIsActive;        private Button btnSave;
        private Button btnCancel;
        private bool isEditMode = false;

        public AuthorForm()
        {
            _service = new AuthorService();
            InitializeComponent();
            ApplyTheme();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "Manage Authors";
            this.Size = new Size(1000, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;

                        Panel pnlTop = new Panel { Dock = DockStyle.Top, Height = 80, BackColor = Color.White };
            Label lblSearch = new Label { Text = "Search Author", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.DimGray, Location = new Point(20, 15), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(20, 40), Width = 350, Font = new Font("Segoe UI", 11F), PlaceholderText = "Search by name or code..." };
            txtSearch.TextChanged += (s, e) => LoadData();
            
            btnAdd = new Button { Text = "+ Add New", Location = new Point(400, 38), Width = 120, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 240, 240), ForeColor = Color.Black };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += (s, e) => ShowInputPanel(false);
            
            btnEdit = new Button { Text = "Edit Selected", Location = new Point(530, 38), Width = 120, Height = 30, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 240, 240), ForeColor = Color.Black };
            btnEdit.FlatAppearance.BorderSize = 0;
            btnEdit.Click += (s, e) => {
                if (dgvList.CurrentRow != null) {
                    ShowInputPanel(true);
                } else {
                    MessageBox.Show("Please select a row to edit.");
                }
            };

            pnlTop.Controls.AddRange(new Control[] { lblSearch, txtSearch, btnAdd, btnEdit });
                        this.Controls.Add(pnlTop);
            Panel pnlDivider = new Panel { Dock = DockStyle.Top, Height = 1, BackColor = Color.FromArgb(230, 230, 230) };
            this.Controls.Add(pnlDivider);

                        dgvList = new DataGridView {
                Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
                EnableHeadersVisualStyles = false,
                RowHeadersVisible = false,
                GridColor = Color.FromArgb(230, 230, 230)
            };
            dgvList.RowTemplate.Height = 60;
            dgvList.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.Black,
                SelectionBackColor = Color.FromArgb(245, 245, 245),
                SelectionForeColor = Color.Black,
                Font = new Font("Segoe UI", 10F),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            dgvList.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = Color.White,
                ForeColor = Color.DimGray,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 15, 0)
            };
            dgvList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvList.ColumnHeadersHeight = 50;
            dgvList.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
            
            dgvList.DataBindingComplete += (s, e) => {
                foreach (DataGridViewColumn col in dgvList.Columns) {
                    col.HeaderText = col.HeaderText.ToUpper();
                }
            };
            this.Controls.Add(dgvList);

            // Input Panel
            pnlInput = new Panel { Dock = DockStyle.Right, Width = 350, Visible = false, BackColor = Color.WhiteSmoke };
            Label lblInputTitle = new Label { Text = "Author Details", Font = new Font("Segoe UI", 12F, FontStyle.Bold), Location = new Point(20, 20), AutoSize = true };
            pnlInput.Controls.Add(lblInputTitle);

            txtId = new TextBox { Visible = false };
            pnlInput.Controls.Add(txtId);

            int y = 60;            Label lblAuthorName = new Label { Text = "AuthorName", Location = new Point(20, y), AutoSize = true };
            txtAuthorName = new TextBox { Location = new Point(20, y + 20), Width = 300, Font = new Font("Segoe UI", 10F) };
            pnlInput.Controls.AddRange(new Control[] { lblAuthorName, txtAuthorName });
            y += 60;            Label lblDescription = new Label { Text = "Description", Location = new Point(20, y), AutoSize = true };
            txtDescription = new TextBox { Location = new Point(20, y + 20), Width = 300, Font = new Font("Segoe UI", 10F) };
            pnlInput.Controls.AddRange(new Control[] { lblDescription, txtDescription });
            y += 60;            chkIsActive = new CheckBox { Text = "Is Active", Location = new Point(20, y), AutoSize = true };
            pnlInput.Controls.Add(chkIsActive);
            y += 40;            btnSave = new Button { Text = "Save", Location = new Point(20, y), Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            btnSave.Click += BtnSave_Click;
            
            btnCancel = new Button { Text = "Cancel", Location = new Point(130, y), Width = 100, Height = 35, FlatStyle = FlatStyle.Flat };
            btnCancel.Click += (s, e) => pnlInput.Visible = false;

            pnlInput.Controls.AddRange(new Control[] { btnSave, btnCancel });
            this.Controls.Add(pnlInput);
            pnlInput.BringToFront(); dgvList.BringToFront();
        }

        private void LoadData()
        {
            var data = _service.Search(txtSearch.Text.Trim());
            dgvList.DataSource = data;
        }

        private void ShowInputPanel(bool isEdit)
        {
            isEditMode = isEdit;
            pnlInput.Visible = true;
            if (!isEdit)
            {
                txtId.Text = "0";
                txtAuthorName.Text = string.Empty;
                txtDescription.Text = string.Empty;
                chkIsActive.Checked = true;            }
            else
            {
                var row = dgvList.CurrentRow;
                txtId.Text = row.Cells["Id"].Value.ToString();
                txtAuthorName.Text = row.Cells["AuthorName"].Value?.ToString();
                txtDescription.Text = row.Cells["Description"].Value?.ToString();
                chkIsActive.Checked = (bool)row.Cells["IsActive"].Value;            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            try
            {
                var model = new Author
                {
                    Id = int.Parse(txtId.Text),
                    AuthorName = txtAuthorName.Text.Trim(),
                    Description = txtDescription.Text.Trim(),
                    IsActive = chkIsActive.Checked,                };

                if (isEditMode)
                {
                    if (_service.Update(model))
                    {
                        MessageBox.Show("Updated successfully!");
                        pnlInput.Visible = false;
                        LoadData();
                    }
                }
                else
                {
                    if (_service.Add(model) > 0)
                    {
                        MessageBox.Show("Added successfully!");
                        pnlInput.Visible = false;
                        LoadData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void ApplyTheme()
        {
            this.BackColor = ThemeManager.Background;
            // dgvList.BackgroundColor = ThemeManager.CardBackground;
            // dgvList.DefaultCellStyle.BackColor = ThemeManager.CardBackground;
            // dgvList.DefaultCellStyle.ForeColor = ThemeManager.TextPrimary;
            pnlInput.BackColor = ThemeManager.Sidebar;
            foreach (Control c in pnlInput.Controls) {
                if (c is Label l) l.ForeColor = ThemeManager.TextPrimary;
                if (c is CheckBox cb) cb.ForeColor = ThemeManager.TextPrimary;
            }
            btnAdd.BackColor = ThemeManager.ButtonFill; btnAdd.ForeColor = ThemeManager.ButtonText; btnAdd.FlatAppearance.BorderSize=0;
            btnEdit.BackColor = ThemeManager.HoverColor; btnEdit.ForeColor = ThemeManager.TextPrimary; btnEdit.FlatAppearance.BorderSize=0;
            btnSave.BackColor = ThemeManager.ButtonFill; btnSave.ForeColor = ThemeManager.ButtonText; btnSave.FlatAppearance.BorderSize=0;
            btnCancel.BackColor = ThemeManager.HoverColor; btnCancel.ForeColor = ThemeManager.TextPrimary; btnCancel.FlatAppearance.BorderSize=0;
        }
    }
}





