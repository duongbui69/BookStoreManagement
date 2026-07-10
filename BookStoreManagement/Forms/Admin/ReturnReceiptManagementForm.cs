using System;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms.Admin
{
    public class ReturnReceiptManagementForm : AdminFormBase
    {
        private readonly ReturnReceiptService _service = new ReturnReceiptService();
        private readonly ExcelExportService _excelService = new ExcelExportService();
        private DataGridView dgv = null!, dgvDetails = null!;
        private TextBox txtSearch = null!;
        private int selectedId;
        public ReturnReceiptManagementForm(){ Text="Quản lý trả hàng"; Width=1200; Height=720; InitializeComponent(); Load+=(s,e)=>LoadData(); }
        private void InitializeComponent(){ var top=new Panel{Dock=DockStyle.Top,Height=80}; txtSearch=CreateTextBox(top,"Tìm kiếm",20,20,260); CreateButton(top,"Tìm",410,18,BtnSearch_Click,80); CreateButton(top,"Chi tiết",500,18,BtnDetails_Click,90); CreateButton(top,"Xuất Excel",600,18,BtnExport_Click,100); CreateButton(top,"Tải lại",710,18,(s,e)=>LoadData(),90); var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,SplitterDistance=350}; dgv=CreateGrid(); dgvDetails=CreateGrid(); dgv.CellClick+=(s,e)=>{if(e.RowIndex>=0)selectedId=GetSelectedId(dgv);}; split.Panel1.Controls.Add(dgv); split.Panel2.Controls.Add(dgvDetails); Controls.Add(split); Controls.Add(top); }
        private void LoadData(){try{dgv.DataSource=CurrentSession.IsAdmin?_service.GetAll():_service.GetVisibleReturns();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnSearch_Click(object? sender,EventArgs e){try{dgv.DataSource=_service.Search(txtSearch.Text);}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnDetails_Click(object? sender,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn phiếu trả.");return;} dgvDetails.DataSource=_service.GetDetails(selectedId);}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnExport_Click(object? sender,EventArgs e){try{using var sfd=new SaveFileDialog{Filter="Excel File|*.xlsx",FileName="TraHang.xlsx"}; if(sfd.ShowDialog()==DialogResult.OK){_excelService.ExportDataGridView(dgv,sfd.FileName,"TraHang"); MessageBox.Show("Xuất Excel thành công.");}}catch(Exception ex){MessageBox.Show(ex.Message);}}
    }
}
