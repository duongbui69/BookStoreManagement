import sys
import re

file_path = 'BookStoreManagement/UserControls/POSControl.cs'
with open(file_path, 'r', encoding='utf-8-sig') as f:
    content = f.read()

# Replace the entire CreateProductCard function
pattern = r'private Control CreateProductCard\(StoreBookInventoryViewModel item\)\s*\{.*?return pnl;\s*\}'

new_func = '''private Control CreateProductCard(StoreBookInventoryViewModel item)
        {
            var pnl = new Guna.UI2.WinForms.Guna2Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10),
                BorderRadius = 12,
                FillColor = ThemeManager.CardBackground,
                BorderColor = ThemeManager.CardBorder,
                BorderThickness = 1,
                Cursor = Cursors.Hand
            };

            var pic = new PictureBox
            {
                Dock = DockStyle.Top,
                Height = 160,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0)
            };

            bool hasImage = false;
            if (!string.IsNullOrEmpty(item.ImagePath))
            {
                string fullPath = System.IO.Path.Combine(Application.StartupPath, "Covers", item.ImagePath);
                if (System.IO.File.Exists(fullPath))
                {
                    pic.Image = Image.FromFile(fullPath);
                    hasImage = true;
                }
            }

            if (!hasImage)
            {
                pic.Paint += (s, e) => {
                    var g = e.Graphics;
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    string initials = item.Title.Length >= 2 ? item.Title.Substring(0, 2).ToUpper() : item.Title.ToUpper();
                    using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                    using (var font = new Font("Segoe UI", 36, FontStyle.Bold))
                    {
                        var size = g.MeasureString(initials, font);
                        g.DrawString(initials, font, brush, (pic.Width - size.Width) / 2, (pic.Height - size.Height) / 2);
                    }
                };
            }

            var lblTitle = new Label
            {
                Text = item.Title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 50,
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                ForeColor = ThemeManager.TextPrimary,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(10, 10, 10, 0)
            };

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 0, 10, 10)
            };

            var lblPrice = new Label
            {
                Text = $"{item.SellingPrice:N0} đ",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = ThemeManager.ButtonFill,
                Dock = DockStyle.Left,
                AutoSize = true,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.BottomLeft
            };

            var lblStock = new Label
            {
                Text = $"Tồn: {item.Quantity}",
                Font = new Font("Segoe UI", 9F),
                Dock = DockStyle.Right,
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = item.Quantity > 0 ? ThemeManager.TextSecondary : Color.Red,
                TextAlign = ContentAlignment.BottomRight,
                Padding = new Padding(0, 4, 0, 0)
            };

            pnlBottom.Controls.Add(lblStock);
            pnlBottom.Controls.Add(lblPrice);

            pnl.Controls.Add(pnlBottom);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(pic);
            
            // To ensure correct Z-order and docking for Top items:
            lblTitle.BringToFront();
            pic.SendToBack();

            pnl.Click += (s, e) => AddToCart(item);
            pic.Click += (s, e) => AddToCart(item);
            lblTitle.Click += (s, e) => AddToCart(item);
            pnlBottom.Click += (s, e) => AddToCart(item);
            lblPrice.Click += (s, e) => AddToCart(item);
            lblStock.Click += (s, e) => AddToCart(item);

            return pnl;
        }'''

content = re.sub(pattern, new_func, content, flags=re.DOTALL)
with open(file_path, 'w', encoding='utf-8-sig') as f:
    f.write(content)
print('Product card updated')
