using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookStoreManagement.Repositories;
using BookStoreManagement.Services;
using BookStoreManagement.Themes;

namespace BookStoreManagement.Forms
{
    public class NotificationItem
    {
        public string Icon { get; set; } = "";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public DateTime Time { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;
        public Color AccentColor { get; set; } = Color.FromArgb(52, 152, 219);
    }

    public class NotificationPanel : Form
    {
        private Panel pnlContent;
        private Panel pnlHeader;
        private Label lblTitle;
        private Label lblMarkAll;
        private Panel pnlList;
        private Panel pnlFooter;
        private List<NotificationItem> _items = new List<NotificationItem>();
        private Control _owner;

        public NotificationPanel(Control owner)
        {
            _owner = owner;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.Manual;
            Size = new Size(360, 480);
            BackColor = ThemeManager.CardBackground;
            TopMost = true;

            // Shadow effect via region
            this.Paint += DrawShadow;

            InitializeUI();
            LoadNotificationsAsync();

            // Close when lose focus
            this.Deactivate += (s, e) => this.Close();
        }

        private void DrawShadow(object sender, PaintEventArgs e)
        {
            // Draw border
            using (var pen = new Pen(ThemeManager.TextBoxBorder, 1))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            }
        }

        private void InitializeUI()
        {
            // Header
            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 56,
                BackColor = Color.Transparent
            };
            pnlHeader.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(pnlHeader.ClientRectangle,
                    ThemeManager.Sidebar, ThemeManager.CardBackground, LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, pnlHeader.ClientRectangle);
                }
                using (var pen = new Pen(ThemeManager.TextBoxBorder, 1))
                {
                    e.Graphics.DrawLine(pen, 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
                }
            };

            lblTitle = new Label
            {
                Text = "🔔  Thông báo",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = ThemeManager.TextPrimary,
                AutoSize = true,
                Location = new Point(16, 16),
                BackColor = Color.Transparent
            };

            lblMarkAll = new Label
            {
                Text = "Đánh dấu tất cả đã đọc",
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.ButtonFill,
                AutoSize = true,
                BackColor = Color.Transparent,
                Cursor = Cursors.Hand
            };
            lblMarkAll.Click += (s, e) => MarkAllRead();

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblMarkAll);
            pnlHeader.Resize += (s, e) =>
            {
                lblMarkAll.Location = new Point(pnlHeader.Width - lblMarkAll.Width - 14, 20);
            };

            // List panel
            pnlList = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(0)
            };

            // Footer
            pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 44,
                BackColor = Color.Transparent
            };
            pnlFooter.Paint += (s, e) =>
            {
                using (var pen = new Pen(ThemeManager.TextBoxBorder, 1))
                {
                    e.Graphics.DrawLine(pen, 0, 0, pnlFooter.Width, 0);
                }
            };

            Label lblViewAll = new Label
            {
                Text = "Xem tất cả →",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeManager.ButtonFill,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Cursor = Cursors.Hand,
                BackColor = Color.Transparent
            };
            lblViewAll.Click += (s, e) => this.Close();
            pnlFooter.Controls.Add(lblViewAll);

            this.Controls.Add(pnlList);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);
        }

        private async void LoadNotificationsAsync()
        {
            _items.Clear();

            // 1. Low stock notifications (from DB)
            try
            {
                var dashRepo = new DashboardRepository();
                var stats = await dashRepo.GetStatsAsync();
                foreach (var item in stats.LowStockItems.Take(5))
                {
                    _items.Add(new NotificationItem
                    {
                        Icon = "📚",
                        Title = "Sắp hết hàng",
                        Message = $"{item.Title} — còn {item.CurrentStock}/{item.Threshold} cuốn",
                        Time = DateTime.Now.AddMinutes(-new Random().Next(5, 60)),
                        IsRead = false,
                        AccentColor = item.CurrentStock == 0
                            ? Color.FromArgb(231, 76, 60)
                            : Color.FromArgb(230, 126, 34)
                    });
                }
            }
            catch { }

            // 2. Active shift notification
            try
            {
                var shiftService = new ShiftService();
                var shift = shiftService.GetActiveShift();
                if (shift != null)
                {
                    var elapsed = DateTime.Now - shift.StartTime;
                    _items.Add(new NotificationItem
                    {
                        Icon = "⏱️",
                        Title = "Ca làm việc đang mở",
                        Message = $"Ca \"{shift.ShiftName}\" đã chạy {(int)elapsed.TotalHours}h{elapsed.Minutes:00}p",
                        Time = shift.StartTime,
                        IsRead = true,
                        AccentColor = Color.FromArgb(39, 174, 96)
                    });
                }
            }
            catch { }

            // If no notifications
            if (_items.Count == 0)
            {
                _items.Add(new NotificationItem
                {
                    Icon = "✅",
                    Title = "Không có thông báo",
                    Message = "Mọi thứ đều ổn!",
                    Time = DateTime.Now,
                    IsRead = true,
                    AccentColor = Color.FromArgb(127, 140, 141)
                });
            }

            // Sort: unread first, then by time desc
            _items = _items.OrderBy(x => x.IsRead).ThenByDescending(x => x.Time).ToList();

            RenderItems();
        }

        private void RenderItems()
        {
            if (this.IsDisposed) return;
            if (this.InvokeRequired)
            {
                this.Invoke((Action)RenderItems);
                return;
            }

            pnlList.Controls.Clear();
            int y = 0;
            foreach (var item in _items)
            {
                var row = CreateItemRow(item);
                row.Top = y;
                pnlList.Controls.Add(row);
                y += row.Height;
            }
        }

        private Panel CreateItemRow(NotificationItem item)
        {
            Panel row = new Panel
            {
                Width = pnlList.Width > 0 ? pnlList.Width : 360,
                Height = 72,
                BackColor = item.IsRead ? Color.Transparent : Color.FromArgb(10, ThemeManager.ButtonFill.R, ThemeManager.ButtonFill.G, ThemeManager.ButtonFill.B),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Accent left bar
            Panel accent = new Panel
            {
                Width = 4,
                Height = 72,
                Location = new Point(0, 0),
                BackColor = item.IsRead ? ThemeManager.TextBoxBorder : item.AccentColor
            };
            row.Controls.Add(accent);

            // Icon
            Label icon = new Label
            {
                Text = item.Icon,
                Font = new Font("Segoe UI Emoji", 18F),
                Location = new Point(14, 14),
                Size = new Size(40, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            row.Controls.Add(icon);

            // Title
            Label title = new Label
            {
                Text = item.Title,
                Font = new Font("Segoe UI", 10F, item.IsRead ? FontStyle.Regular : FontStyle.Bold),
                ForeColor = item.IsRead ? ThemeManager.TextSecondary : ThemeManager.TextPrimary,
                Location = new Point(58, 10),
                Size = new Size(240, 20),
                BackColor = Color.Transparent
            };
            row.Controls.Add(title);

            // Message
            Label msg = new Label
            {
                Text = item.Message,
                Font = new Font("Segoe UI", 9F),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(58, 30),
                Size = new Size(240, 30),
                BackColor = Color.Transparent
            };
            row.Controls.Add(msg);

            // Time
            Label time = new Label
            {
                Text = FormatTime(item.Time),
                Font = new Font("Segoe UI", 8F),
                ForeColor = ThemeManager.TextSecondary,
                Location = new Point(row.Width - 80, 10),
                Size = new Size(68, 20),
                TextAlign = ContentAlignment.MiddleRight,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            row.Controls.Add(time);

            // Unread dot
            if (!item.IsRead)
            {
                Panel dot = new Panel
                {
                    Width = 8,
                    Height = 8,
                    BackColor = item.AccentColor,
                    Location = new Point(row.Width - 16, 32),
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };
                // Make it circle
                dot.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, 8, 8, 8, 8));
                row.Controls.Add(dot);
            }

            // Bottom divider
            row.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(30, ThemeManager.TextBoxBorder.R, ThemeManager.TextBoxBorder.G, ThemeManager.TextBoxBorder.B), 1))
                {
                    e.Graphics.DrawLine(pen, 0, row.Height - 1, row.Width, row.Height - 1);
                }
            };

            // Hover effect
            row.MouseEnter += (s, e) => row.BackColor = Color.FromArgb(20, ThemeManager.ButtonFill.R, ThemeManager.ButtonFill.G, ThemeManager.ButtonFill.B);
            row.MouseLeave += (s, e) => row.BackColor = item.IsRead ? Color.Transparent : Color.FromArgb(10, ThemeManager.ButtonFill.R, ThemeManager.ButtonFill.G, ThemeManager.ButtonFill.B);

            // Mark as read on click
            row.Click += (s, e) =>
            {
                item.IsRead = true;
                RenderItems();
            };
            foreach (Control c in row.Controls)
            {
                c.Click += (s, e) =>
                {
                    item.IsRead = true;
                    RenderItems();
                };
                c.MouseEnter += (s, e) => row.BackColor = Color.FromArgb(20, ThemeManager.ButtonFill.R, ThemeManager.ButtonFill.G, ThemeManager.ButtonFill.B);
                c.MouseLeave += (s, e) => row.BackColor = item.IsRead ? Color.Transparent : Color.FromArgb(10, ThemeManager.ButtonFill.R, ThemeManager.ButtonFill.G, ThemeManager.ButtonFill.B);
            }

            return row;
        }

        private void MarkAllRead()
        {
            foreach (var item in _items) item.IsRead = true;
            RenderItems();
        }

        private string FormatTime(DateTime time)
        {
            var diff = DateTime.Now - time;
            if (diff.TotalMinutes < 1) return "Vừa xong";
            if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} phút";
            if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} giờ";
            return time.ToString("dd/MM");
        }

        [System.Runtime.InteropServices.DllImport("Gdi32.dll")]
        private static extern IntPtr CreateRoundRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect, int nWidthEllipse, int nHeightEllipse);

        public int UnreadCount => _items.Count(x => !x.IsRead);

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            // Position below owner button
            if (_owner != null)
            {
                var screenPt = _owner.PointToScreen(new Point(0, _owner.Height));
                // Adjust so panel doesn't go off screen
                int x = screenPt.X - Width + _owner.Width;
                int y = screenPt.Y + 4;
                var screen = Screen.FromPoint(screenPt).WorkingArea;
                if (x + Width > screen.Right) x = screen.Right - Width - 4;
                if (x < screen.Left) x = screen.Left + 4;
                this.Location = new Point(x, y);
            }
        }

        protected override bool ShowWithoutActivation => false;
    }
}
