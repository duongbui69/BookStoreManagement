using System;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms.Admin
{
    public class SalesOrderManagementForm : AdminFormBase
    {
        private readonly SalesOrderService _service = new SalesOrderService();
        private readonly ExcelExportService _excelService = new ExcelExportService();
        private DataGridView dgvOrders = null!, dgvDetails = null!;
        private TextBox txtSearch = null!;
        private int selectedId;

        public SalesOrderManagementForm(){ Text="Quản lý hóa đơn bán"; Width=1200; Height=720; InitializeComponent(); Load+=(s,e)=>LoadData(); }
        private void InitializeComponent(){ var top=new Panel{Dock=DockStyle.Top,Height=80}; txtSearch=CreateTextBox(top,"Tìm kiếm",20,20,260); CreateButton(top,"Tìm",410,18,BtnSearch_Click,80); CreateButton(top,"Chi tiết",500,18,BtnDetails_Click,90); CreateButton(top,"Hủy đơn",600,18,BtnCancel_Click,90); CreateButton(top,"Xuất Excel",700,18,BtnExport_Click,100); CreateButton(top,"Tải lại",810,18,(s,e)=>LoadData(),90); var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,SplitterDistance=350}; dgvOrders=CreateGrid(); dgvDetails=CreateGrid(); dgvOrders.CellClick+=(s,e)=>{if(e.RowIndex>=0)selectedId=GetSelectedId(dgvOrders);}; split.Panel1.Controls.Add(dgvOrders); split.Panel2.Controls.Add(dgvDetails); Controls.Add(split); Controls.Add(top); }
        private void LoadData(){try{dgvOrders.DataSource=CurrentSession.IsAdmin?_service.GetAll():_service.GetMyOrders();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnSearch_Click(object? sender,EventArgs e){try{dgvOrders.DataSource=_service.Search(txtSearch.Text);}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnDetails_Click(object? sender,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn hóa đơn.");return;} dgvDetails.DataSource=_service.GetDetails(selectedId);}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnCancel_Click(object? sender,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn hóa đơn.");return;} if(MessageBox.Show("Hủy hóa đơn này?","Xác nhận",MessageBoxButtons.YesNo)==DialogResult.Yes){_service.CancelOrder(selectedId); MessageBox.Show("Đã hủy hóa đơn."); LoadData();}}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnExport_Click(object? sender,EventArgs e){try{using var sfd=new SaveFileDialog{Filter="Excel File|*.xlsx",FileName="HoaDonBan.xlsx"}; if(sfd.ShowDialog()==DialogResult.OK){_excelService.ExportDataGridView(dgvOrders,sfd.FileName,"HoaDon"); MessageBox.Show("Xuất Excel thành công.");}}catch(Exception ex){MessageBox.Show(ex.Message);}}
    }
}
