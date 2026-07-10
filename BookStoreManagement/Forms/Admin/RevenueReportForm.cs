using System;
using System.Windows.Forms;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms.Admin
{
    public class RevenueReportForm : AdminFormBase
    {
        private readonly ReportService _reportService = new ReportService();
        private readonly ExcelExportService _excelService = new ExcelExportService();
        private DataGridView dgv = null!;
        private Label lblToday = null!, lblMonth = null!, lblOrders = null!, lblBooks = null!, lblUsers = null!, lblCustomers = null!, lblStores = null!;

        public RevenueReportForm(){ Text="Báo cáo doanh thu"; Width=1100; Height=700; InitializeComponent(); Load+=(s,e)=>LoadReport(); }
        private void InitializeComponent(){ var top=new Panel{Dock=DockStyle.Top,Height=130}; lblToday=MakeLabel(top,"Doanh thu hôm nay: ",20,20); lblMonth=MakeLabel(top,"Doanh thu tháng: ",20,50); lblOrders=MakeLabel(top,"Tổng hóa đơn: ",20,80); lblBooks=MakeLabel(top,"Sách: ",350,20); lblUsers=MakeLabel(top,"Nhân viên: ",350,50); lblCustomers=MakeLabel(top,"Khách hàng: ",350,80); lblStores=MakeLabel(top,"Cửa hàng: ",650,20); CreateButton(top,"Sách bán chạy",650,55,(s,e)=>LoadTopBooks(),120); CreateButton(top,"Doanh thu ngày",780,55,(s,e)=>LoadDailyRevenue(),130); CreateButton(top,"Xuất Excel",920,55,BtnExport_Click,100); dgv=CreateGrid(); Controls.Add(dgv); Controls.Add(top); }
        private Label MakeLabel(Control p,string text,int l,int t){var label=new Label{Text=text,Left=l,Top=t,Width=300,Height=22}; p.Controls.Add(label); return label;}
        private void LoadReport(){try{lblToday.Text="Doanh thu hôm nay: "+_reportService.GetTodayRevenue().ToString("N0"); lblMonth.Text="Doanh thu tháng: "+_reportService.GetMonthRevenue().ToString("N0"); lblOrders.Text="Tổng hóa đơn: "+_reportService.GetTotalOrders(); lblBooks.Text="Sách: "+_reportService.GetTotalBooks(); lblUsers.Text="Nhân viên: "+_reportService.GetTotalUsers(); lblCustomers.Text="Khách hàng: "+_reportService.GetTotalCustomers(); lblStores.Text="Cửa hàng: "+_reportService.GetTotalStores(); LoadTopBooks();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void LoadTopBooks(){try{dgv.DataSource=_reportService.GetTopSellingBooks(10);}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void LoadDailyRevenue(){try{dgv.DataSource=_reportService.GetDailyRevenueByStore();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnExport_Click(object? sender,EventArgs e){try{using var sfd=new SaveFileDialog{Filter="Excel File|*.xlsx",FileName="BaoCao.xlsx"}; if(sfd.ShowDialog()==DialogResult.OK){_excelService.ExportDataGridView(dgv,sfd.FileName,"BaoCao"); MessageBox.Show("Xuất Excel thành công.");}}catch(Exception ex){MessageBox.Show(ex.Message);}}
    }
}
