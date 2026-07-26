using System;
using System.Drawing;
using System.Windows.Forms;
using BookStoreManagement.Themes;

namespace BookStoreManagement.UserControls
{
    public class PageChangedEventArgs : EventArgs
    {
        public int NewPage { get; set; }
    }

    public partial class PaginationControl : UserControl
    {
        public event EventHandler<PageChangedEventArgs> PageChanged;

        private int _totalRecords = 0;
        private int _pageSize = 10;
        private int _currentPage = 1;
        private int _totalPages = 1;

        private Label lblInfo;
        private FlowLayoutPanel flpButtons;

        public PaginationControl()
        {
            InitializeUI();
            ThemeManager.ThemeChanged += ThemeManager_ThemeChanged;
            this.Resize += PaginationControl_Resize;
        }

        private void InitializeUI()
        {
            this.Height = 50;
            this.BackColor = Color.Transparent;

            lblInfo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Location = new Point(10, 15)
            };

            flpButtons = new FlowLayoutPanel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Location = new Point(this.Width - 200, 10)
            };

            this.Controls.Add(lblInfo);
            this.Controls.Add(flpButtons);
            ApplyTheme();
        }

        private void PaginationControl_Resize(object sender, EventArgs e)
        {
            flpButtons.Left = this.Width - flpButtons.Width - 10;
        }

        public void UpdatePagination(int totalRecords, int currentPage, int pageSize)
        {
            _totalRecords = totalRecords;
            _pageSize = pageSize;
            _currentPage = currentPage;
            _totalPages = (int)Math.Ceiling((double)_totalRecords / _pageSize);
            if (_totalPages == 0) _totalPages = 1;
            if (_currentPage > _totalPages) _currentPage = _totalPages;

            int startRec = (_currentPage - 1) * _pageSize + 1;
            int endRec = Math.Min(_currentPage * _pageSize, _totalRecords);
            if (_totalRecords == 0) { startRec = 0; endRec = 0; }

            lblInfo.Text = $"Trang {_currentPage} / {_totalPages}";

            BuildButtons();
            PaginationControl_Resize(this, EventArgs.Empty);
        }

        private void BuildButtons()
        {
            flpButtons.Controls.Clear();

            AddButton("<<", 1, _currentPage > 1);
            AddButton("<", _currentPage - 1, _currentPage > 1);

            int startPage = Math.Max(1, _currentPage - 1);
            int endPage = Math.Min(_totalPages, _currentPage + 1);

            if (_currentPage == 1 && _totalPages >= 3) endPage = 3;
            if (_currentPage == _totalPages && _totalPages >= 3) startPage = _totalPages - 2;

            for (int i = startPage; i <= endPage; i++)
            {
                AddButton(i.ToString(), i, true, i == _currentPage);
            }

            AddButton(">", _currentPage + 1, _currentPage < _totalPages);
            AddButton(">>", _totalPages, _currentPage < _totalPages);
        }

        private void AddButton(string text, int targetPage, bool enabled, bool isCurrent = false)
        {
            var btn = new Button
            {
                Text = text,
                Width = 35,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor = enabled ? Cursors.Hand : Cursors.Default,
                Margin = new Padding(2)
            };
            btn.FlatAppearance.BorderSize = 1;

            if (isCurrent)
            {
                btn.BackColor = ThemeManager.ButtonFill;
                btn.ForeColor = ThemeManager.ButtonText;
                btn.FlatAppearance.BorderColor = ThemeManager.ButtonFill;
            }
            else
            {
                btn.BackColor = ThemeManager.CardBackground;
                btn.ForeColor = enabled ? ThemeManager.TextPrimary : ThemeManager.TextSecondary;
                btn.FlatAppearance.BorderColor = ThemeManager.TextBoxBorder;
            }

            if (!enabled || isCurrent)
            {
                btn.FlatAppearance.MouseOverBackColor = btn.BackColor;
                btn.FlatAppearance.MouseDownBackColor = btn.BackColor;
            }

            if (enabled)
            {
                btn.Click += (s, e) =>
                {
                    if (!isCurrent)
                        PageChanged?.Invoke(this, new PageChangedEventArgs { NewPage = targetPage });
                };
            }

            flpButtons.Controls.Add(btn);
        }

        private void ThemeManager_ThemeChanged(object sender, EventArgs e)
        {
            ApplyTheme();
            BuildButtons();
        }

        private void ApplyTheme()
        {
            lblInfo.ForeColor = ThemeManager.TextSecondary;
        }
    
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                BookStoreManagement.Themes.ThemeManager.ThemeChanged -= ThemeManager_ThemeChanged;
            }
            base.Dispose(disposing);
        }
}
}
