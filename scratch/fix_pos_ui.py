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
                Margin = new Padding(15),
                BorderRadius = 12,
                FillColor = ThemeManager.CardBackground,
                BorderColor = ThemeManager.CardBorder,
                BorderThickness = 1,
                Cursor = Cursors.Hand,
                ShadowDecoration = { Enabled = true, Shadow = new Padding(0, 0, 8, 8), Color = Color.FromArgb(40, 0, 0, 0), BorderRadius = 12 }
            };

            var tlpCard = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1,
                BackColor = Color.Transparent,
                Margin = new Padding(0),
                Padding = new Padding(10)
            };
            tlpCard.RowStyles.Add(new RowStyle(SizeType.Percent, 60F)); // Image
            tlpCard.RowStyles.Add(new RowStyle(SizeType.Percent, 25F)); // Title
            tlpCard.RowStyles.Add(new RowStyle(SizeType.Percent, 15F)); // Price & Stock
            
            pnl.Controls.Add(tlpCard);

            var pic = new PictureBox
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                SizeMode = PictureBoxSizeMode.Zoom,
                Margin = new Padding(0, 0, 0, 5)
            };

            var lblTitle = new Label
            {
                Text = item.Title,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Dock = DockStyle.Fill,
                AutoEllipsis = true,
                BackColor = Color.Transparent,
                ForeColor = ThemeManager.TextPrimary,
                TextAlign = ContentAlignment.TopLeft,
                Padding = new Padding(0, 5, 0, 0)
            };

            var pnlBottom = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(0)
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

            tlpCard.Controls.Add(pic, 0, 0);
            tlpCard.Controls.Add(lblTitle, 0, 1);
            tlpCard.Controls.Add(pnlBottom, 0, 2);

            // Fetch image dynamically
            LoadImageAsync(item, pic);

            pnl.Click += (s, e) => AddToCart(item);
            tlpCard.Click += (s, e) => AddToCart(item);
            pic.Click += (s, e) => AddToCart(item);
            lblTitle.Click += (s, e) => AddToCart(item);
            pnlBottom.Click += (s, e) => AddToCart(item);
            lblPrice.Click += (s, e) => AddToCart(item);
            lblStock.Click += (s, e) => AddToCart(item);

            return pnl;
        }

        private async void LoadImageAsync(StoreBookInventoryViewModel item, PictureBox pic)
        {
            try
            {
                // First try to load local image, BUT ignore dummy files that are exactly 44897, 79801, 50025, 92890, etc. (the dummy seed images)
                // Actually, just check if it's a dummy image by checking if it contains "book_" and we want to replace it.
                // To be safe, we will just use the title to generate a colorful placeholder OR fetch from Google.
                string safeTitle = string.Join("_", item.Title.Split(System.IO.Path.GetInvalidFileNameChars()));
                string localCoverName = safeTitle + ".jpg";
                string fullPath = System.IO.Path.Combine(Application.StartupPath, "Covers", localCoverName);

                if (System.IO.File.Exists(fullPath))
                {
                    pic.Image = Image.FromFile(fullPath);
                    return;
                }
                
                // If not found, paint a temporary placeholder
                pic.Paint += PaintPlaceholder;
                pic.Tag = item.Title; // Store title for paint
                pic.Invalidate();

                // Fetch from Google Books API
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
                    string url = "https://www.googleapis.com/books/v1/volumes?q=intitle:" + Uri.EscapeDataString(item.Title);
                    var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        // Extremely simple JSON parsing to find thumbnail
                        int thumbIndex = json.IndexOf("\\"thumbnail\\": \\"");
                        if (thumbIndex > 0)
                        {
                            int start = thumbIndex + 15;
                            int end = json.IndexOf("\\"", start);
                            string thumbUrl = json.Substring(start, end - start);
                            thumbUrl = thumbUrl.Replace("http:", "https:");
                            
                            var imgData = await client.GetByteArrayAsync(thumbUrl);
                            if (imgData.Length > 0)
                            {
                                System.IO.Directory.CreateDirectory(System.IO.Path.Combine(Application.StartupPath, "Covers"));
                                System.IO.File.WriteAllBytes(fullPath, imgData);
                                
                                // Update PictureBox
                                pic.Paint -= PaintPlaceholder;
                                using (var ms = new System.IO.MemoryStream(imgData))
                                {
                                    pic.Image = Image.FromStream(ms);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
        }

        private void PaintPlaceholder(object sender, PaintEventArgs e)
        {
            var pic = sender as PictureBox;
            if (pic == null || pic.Tag == null) return;
            string title = pic.Tag.ToString();
            
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            
            // Generate a color based on title hash
            int hash = Math.Abs(title.GetHashCode());
            Color[] colors = { Color.FromArgb(239,83,80), Color.FromArgb(171,71,188), Color.FromArgb(66,165,245), Color.FromArgb(102,187,106), Color.FromArgb(255,167,38), Color.FromArgb(141,110,99) };
            Color bgColor = colors[hash % colors.Length];
            
            using (var brush = new SolidBrush(bgColor))
            {
                g.FillRoundedRectangle(brush, new Rectangle(0, 0, pic.Width, pic.Height), 8);
            }
            
            string initials = title.Length >= 2 ? title.Substring(0, 2).ToUpper() : title.ToUpper();
            using (var brush = new SolidBrush(Color.White))
            using (var font = new Font("Segoe UI", 28, FontStyle.Bold))
            {
                var size = g.MeasureString(initials, font);
                g.DrawString(initials, font, brush, (pic.Width - size.Width) / 2, (pic.Height - size.Height) / 2);
            }
        }
'''

content = re.sub(pattern, new_func, content, flags=re.DOTALL)

# Add FillRoundedRectangle extension method to Graphics if not exists
if 'FillRoundedRectangle' not in content:
    extension_code = '''
    public static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(this Graphics graphics, Brush brush, Rectangle bounds, int cornerRadius)
        {
            if (graphics == null) throw new ArgumentNullException(nameof(graphics));
            if (brush == null) throw new ArgumentNullException(nameof(brush));

            using (System.Drawing.Drawing2D.GraphicsPath path = new System.Drawing.Drawing2D.GraphicsPath())
            {
                int d = cornerRadius * 2;
                path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
                path.AddArc(bounds.X + bounds.Width - d, bounds.Y, d, d, 270, 90);
                path.AddArc(bounds.X + bounds.Width - d, bounds.Y + bounds.Height - d, d, d, 0, 90);
                path.AddArc(bounds.X, bounds.Y + bounds.Height - d, d, d, 90, 90);
                path.CloseFigure();
                graphics.FillPath(brush, path);
            }
        }
    }
}'''
    # replace the last closing brace of the namespace
    content = content.rstrip()
    if content.endswith('}'):
        content = content[:-1] + extension_code

with open(file_path, 'w', encoding='utf-8-sig') as f:
    f.write(content)
print('POSControl updated with async images and TableLayoutPanel card')
"
