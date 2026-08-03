import re
file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/ReportsControl.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

if 'using BookStoreManagement.Events;' not in text:
    text = text.replace('using System.Windows.Forms;', 'using System.Windows.Forms;\nusing BookStoreManagement.Events;')

text = text.replace(
'''        private async void ReportsControl_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }''',
'''        private async void ReportsControl_Load(object sender, EventArgs e)
        {
            GlobalEvents.TransactionCompleted -= OnTransactionCompleted;
            GlobalEvents.TransactionCompleted += OnTransactionCompleted;
            await LoadDataAsync();
        }

        private async void OnTransactionCompleted()
        {
            if (this.IsHandleCreated && !this.IsDisposed)
            {
                this.Invoke((System.Windows.Forms.MethodInvoker)async delegate { await LoadDataAsync(); });
            }
        }'''
)

old_paint = '''        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_stats == null || _stats.MonthlyRevenue.Count == 0) return;

            int paddingX = 60;
            int paddingY = 40;
            int chartWidth = pnlChart.Width - paddingX - 40;
            int chartHeight = pnlChart.Height - paddingY - 80;
            
            decimal maxVal = _stats.MonthlyRevenue.Values.Count > 0 ? _stats.MonthlyRevenue.Values.Max() : 0;
            if (maxVal == 0) maxVal = 100000;
            // Round up to nearest nice number
            decimal steps = maxVal / 5;
            
            using (var font1 = new Font("Segoe UI", 9F))
            using (var font2 = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                using (var penGrid = new Pen(Color.FromArgb(80, ThemeManager.TextBoxBorder), 1))
                {
                    penGrid.DashStyle = DashStyle.Dash;
                    for (int i = 0; i <= 5; i++)
                    {
                        int y = pnlChart.Height - paddingY - (i * chartHeight / 5);
                        g.DrawLine(penGrid, paddingX, y, pnlChart.Width - 40, y);

                        using (var b = new SolidBrush(ThemeManager.TextSecondary))
                        {
                            decimal val = maxVal * i / 5;
                            string valStr = val >= 1000000 ? $"{(val/1000000):N1}M" : $"{(val/1000):N0}k";
                            g.DrawString(valStr, font1, b, 10, y - 8);
                        }
                    }
                }

                int numPoints = _stats.MonthlyRevenue.Count;
                if (numPoints > 1)
                {
                    PointF[] points = new PointF[numPoints];
                    int idx = 0;
                    float stepX = (float)chartWidth / (numPoints - 1);

                    foreach (var kvp in _stats.MonthlyRevenue.OrderBy(k => k.Key))
                    {
                        DateTime m = kvp.Key;
                        decimal rev = kvp.Value;
                        
                        float x = paddingX + idx * stepX;
                        float y = pnlChart.Height - paddingY - (float)(rev / maxVal) * chartHeight;
                        
                        points[idx] = new PointF(x, y);

                        using (var b = new SolidBrush(ThemeManager.TextSecondary))
                        {
                            string lbl = m.ToString("MM/yy");
                            var size = g.MeasureString(lbl, font2);
                            g.DrawString(lbl, font2, b, x - size.Width / 2, pnlChart.Height - paddingY + 10);
                        }
                        idx++;
                    }

                    // Draw line
                    using (var penLine = new Pen(ThemeManager.ButtonFill, 3))
                    {
                        g.DrawLines(penLine, points);
                    }

                    // Draw points
                    foreach (var pt in points)
                    {
                        g.FillEllipse(Brushes.White, pt.X - 4, pt.Y - 4, 8, 8);
                        using (var penLine = new Pen(ThemeManager.ButtonFill, 2))
                        {
                            g.DrawEllipse(penLine, pt.X - 4, pt.Y - 4, 8, 8);
                        }
                    }
                }
            }

            pnlChart.FillColor = ThemeManager.CardBackground;
            pnlChart.BorderColor = ThemeManager.TextBoxBorder;
            if (pnlChart.Controls["ChartTitle"] is Label cTitle) cTitle.ForeColor = ThemeManager.TextPrimary;'''

new_paint = '''        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_stats == null || _stats.MonthlyRevenue.Count == 0) return;

            int paddingX = 60;
            int paddingY = 40;
            int chartWidth = pnlChart.Width - paddingX - 40;
            int chartHeight = pnlChart.Height - paddingY - 80;
            
            decimal maxVal = _stats.MonthlyRevenue.Values.Count > 0 ? _stats.MonthlyRevenue.Values.Max() : 0;
            if (maxVal == 0) maxVal = 100000;
            // Round up to nearest nice number
            decimal steps = maxVal / 5;
            
            using (var font1 = new Font("Segoe UI", 9F))
            using (var font2 = new Font("Segoe UI", 9F, FontStyle.Bold))
            {
                using (var penGrid = new Pen(Color.FromArgb(80, ThemeManager.TextBoxBorder), 1))
                {
                    penGrid.DashStyle = DashStyle.Dash;
                    for (int i = 0; i <= 5; i++)
                    {
                        int y = pnlChart.Height - paddingY - (i * chartHeight / 5);
                        g.DrawLine(penGrid, paddingX, y, pnlChart.Width - 40, y);

                        using (var b = new SolidBrush(ThemeManager.TextSecondary))
                        {
                            decimal val = maxVal * i / 5;
                            string valStr = val >= 1000000 ? $"{(val/1000000):N1}M" : $"{(val/1000):N0}k";
                            g.DrawString(valStr, font1, b, 10, y - 8);
                        }
                    }
                }

                int numPoints = _stats.MonthlyRevenue.Count;
                if (numPoints > 0)
                {
                    int idx = 0;
                    float stepX = (float)chartWidth / numPoints;
                    float barWidth = stepX * 0.6f;

                    foreach (var kvp in _stats.MonthlyRevenue.OrderBy(k => k.Key))
                    {
                        DateTime m = kvp.Key;
                        decimal rev = kvp.Value;
                        
                        float xCenter = paddingX + (idx * stepX) + (stepX / 2);
                        float barHeight = (float)(rev / maxVal) * chartHeight;
                        float x = xCenter - (barWidth / 2);
                        float y = pnlChart.Height - paddingY - barHeight;
                        
                        if (barHeight > 0)
                        {
                            RectangleF barRect = new RectangleF(x, y, barWidth, barHeight);
                            using (var brush = new LinearGradientBrush(barRect, Color.FromArgb(41, 128, 185), Color.FromArgb(141, 188, 215), LinearGradientMode.Vertical))
                            {
                                g.FillRectangle(brush, barRect);
                            }
                        }

                        using (var b = new SolidBrush(ThemeManager.TextSecondary))
                        {
                            string lbl = m.ToString("MM/yy");
                            var size = g.MeasureString(lbl, font2);
                            g.DrawString(lbl, font2, b, xCenter - size.Width / 2, pnlChart.Height - paddingY + 10);
                        }
                        idx++;
                    }
                }
            }

            pnlChart.FillColor = ThemeManager.CardBackground;
            pnlChart.BorderColor = ThemeManager.TextBoxBorder;
            if (pnlChart.Controls["ChartTitle"] is Label cTitle) cTitle.ForeColor = ThemeManager.TextPrimary;'''

text = text.replace(old_paint, new_paint)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
