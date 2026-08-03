import re
file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/DashboardControl.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

if 'using BookStoreManagement.Events;' not in text:
    text = text.replace('using System.Windows.Forms;', 'using System.Windows.Forms;\nusing BookStoreManagement.Events;')

text = text.replace(
'''        private async void DashboardControl_Load(object sender, EventArgs e)
        {
            await LoadDataAsync();
        }''',
'''        private async void DashboardControl_Load(object sender, EventArgs e)
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

old_paint = '''        private void PnlLineChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlLineChart.ClientRectangle;

            if (currentStats == null || currentStats.MonthlyRevenue.Count == 0) return;

            var sortedMonths = currentStats.MonthlyRevenue.OrderBy(x => x.Key).ToList();
            if (sortedMonths.Count < 2) return;

            float maxVal = (float)sortedMonths.Max(x => x.Value);
            if (maxVal == 0) maxVal = 1;

            float paddingX = 40f;
            float paddingY = 40f;
            float bottomY = rect.Height - paddingY;
            float chartWidth = rect.Width - (paddingX * 2);
            float chartHeight = rect.Height - paddingY - 20f;

            PointF[] points = new PointF[sortedMonths.Count];
            for (int i = 0; i < sortedMonths.Count; i++)
            {
                float x = paddingX + (i * (chartWidth / (sortedMonths.Count - 1)));
                float y = bottomY - ((float)sortedMonths[i].Value / maxVal * chartHeight);
                points[i] = new PointF(x, y);

                using (var font = new Font("Segoe UI", 8F))
                using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                {
                    var format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    g.TranslateTransform(x, bottomY + 15);
                    g.RotateTransform(-45);
                    g.DrawString($"Tháng {sortedMonths[i].Key:MM/yy}", font, brush, 0, 0, format);
                    g.ResetTransform();
                }
            }

            using (var path = new GraphicsPath())
            {
                path.AddCurve(points);
                path.AddLine(points.Last().X, bottomY, points.First().X, bottomY);
                path.CloseFigure();

                using (var brush = new LinearGradientBrush(new RectangleF(0, 0, rect.Width, rect.Height), Color.FromArgb(100, 41, 128, 185), Color.Transparent, LinearGradientMode.Vertical))
                {
                    g.FillPath(brush, path);
                }
            }

            using (var pen = new Pen(Color.FromArgb(41, 128, 185), 3f))
            {
                g.DrawCurve(pen, points);
            }

            foreach (var pt in points)
            {
                g.FillEllipse(Brushes.White, pt.X - 4, pt.Y - 4, 8, 8);
                using (var pen = new Pen(Color.FromArgb(41, 128, 185), 2f))
                {
                    g.DrawEllipse(pen, pt.X - 4, pt.Y - 4, 8, 8);
                }
            }
        }'''

new_paint = '''        private void PnlLineChart_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = pnlLineChart.ClientRectangle;

            if (currentStats == null || currentStats.MonthlyRevenue.Count == 0) return;

            var sortedMonths = currentStats.MonthlyRevenue.OrderBy(x => x.Key).ToList();
            if (sortedMonths.Count == 0) return;

            float maxVal = (float)sortedMonths.Max(x => x.Value);
            if (maxVal == 0) maxVal = 1;

            float paddingX = 40f;
            float paddingY = 40f;
            float bottomY = rect.Height - paddingY;
            float chartWidth = rect.Width - (paddingX * 2);
            float chartHeight = rect.Height - paddingY - 20f;
            
            float barWidth = (chartWidth / sortedMonths.Count) * 0.6f;
            float stepX = chartWidth / sortedMonths.Count;

            for (int i = 0; i < sortedMonths.Count; i++)
            {
                float xCenter = paddingX + (i * stepX) + (stepX / 2);
                float barHeight = ((float)sortedMonths[i].Value / maxVal * chartHeight);
                float x = xCenter - (barWidth / 2);
                float y = bottomY - barHeight;

                // Draw Bar
                if (barHeight > 0)
                {
                    RectangleF barRect = new RectangleF(x, y, barWidth, barHeight);
                    using (var brush = new LinearGradientBrush(barRect, Color.FromArgb(41, 128, 185), Color.FromArgb(141, 188, 215), LinearGradientMode.Vertical))
                    {
                        g.FillRectangle(brush, barRect);
                    }
                }

                // Draw Label
                using (var font = new Font("Segoe UI", 8F))
                using (var brush = new SolidBrush(ThemeManager.TextSecondary))
                {
                    var format = new StringFormat();
                    format.Alignment = StringAlignment.Center;
                    g.TranslateTransform(xCenter, bottomY + 15);
                    g.RotateTransform(-45);
                    g.DrawString($"Tháng {sortedMonths[i].Key:MM/yy}", font, brush, 0, 0, format);
                    g.ResetTransform();
                }
            }
        }'''
text = text.replace(old_paint, new_paint)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
