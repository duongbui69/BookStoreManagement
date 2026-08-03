import re
file_path = 'c:/Users/ADMIN/Desktop/projects/university/BookStoreManagement/BookStoreManagement/UserControls/ReportsControl.cs'
with open(file_path, 'r', encoding='utf-8') as f:
    text = f.read()

text = text.replace(
    'CreateMetricCard("Tổng Doanh Thu", "1,245,000,000 đ", "+12.5% so với tháng trước", true);',
    'CreateMetricCard("Tổng Doanh Thu", "1,245,000,000 đ", "", true);'
)
text = text.replace(
    'CreateMetricCard("Tổng Đơn Hàng", "4,521", "-2.1% so với tháng trước", false);',
    'CreateMetricCard("Tổng Đơn Hàng", "4,521", "", false);'
)
text = text.replace(
    'CreateMetricCard("Sản phẩm sắp hết hàng", "34", "Cần nhập hàng khẩn cấp", false, true);',
    'CreateMetricCard("Sản phẩm sắp hết hàng", "34", "", false, true);'
)

old_update = '''            string revGrowthSign = _stats.RevenueGrowth >= 0 ? "+" : "";
            UpdateMetricCard(cardRevenue, 
                $"{_stats.TotalRevenue:N0} đ", 
                $"{revGrowthSign}{_stats.RevenueGrowth:N1}% so với tháng trước", 
                _stats.RevenueGrowth >= 0);

            string ordGrowthSign = _stats.OrdersGrowth >= 0 ? "+" : "";
            UpdateMetricCard(cardOrders, 
                $"{_stats.TotalOrders:N0}", 
                $"{ordGrowthSign}{_stats.OrdersGrowth:N1}% so với tháng trước", 
                _stats.OrdersGrowth >= 0);

            UpdateMetricCard(cardLowStock, 
                $"{_stats.LowStockCount}", 
                null, 
                false, true);'''

new_update = '''            UpdateMetricCard(cardRevenue, 
                $"{_stats.TotalRevenue:N0} đ", 
                "", 
                _stats.RevenueGrowth >= 0);

            UpdateMetricCard(cardOrders, 
                $"{_stats.TotalOrders:N0}", 
                "", 
                _stats.OrdersGrowth >= 0);

            UpdateMetricCard(cardLowStock, 
                $"{_stats.LowStockCount}", 
                "", 
                false, true);'''
text = text.replace(old_update, new_update)

text = text.replace(
'''            var pnlBadge = new Guna2Panel { Name = "BadgePanel", BorderRadius = 4, AutoSize = true, Location = new Point(76, 70), BackColor = Color.Transparent };
            var lblBadge = new Label { Name = "BadgeLabel", Text = subText, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Padding = new Padding(2), BackColor = Color.Transparent };
            pnlBadge.Controls.Add(lblBadge);''',
'''            var pnlBadge = new Guna2Panel { Name = "BadgePanel", BorderRadius = 4, AutoSize = true, Location = new Point(76, 70), BackColor = Color.Transparent, Visible = false };
            var lblBadge = new Label { Name = "BadgeLabel", Text = subText, Font = new Font("Segoe UI", 9F, FontStyle.Bold), AutoSize = true, Padding = new Padding(2), BackColor = Color.Transparent, Visible = false };
            pnlBadge.Controls.Add(lblBadge);'''
)

with open(file_path, 'w', encoding='utf-8') as f:
    f.write(text)
