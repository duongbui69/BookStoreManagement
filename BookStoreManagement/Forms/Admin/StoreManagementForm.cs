using System;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms.Admin
{
    public class StoreManagementForm : AdminFormBase
    {
        private readonly StoreService _service = new StoreService();
        private DataGridView dgv = null!;
        private TextBox txtCode = null!, txtName = null!, txtAddress = null!, txtPhone = null!, txtSearch = null!;
        private CheckBox chkActive = null!;
        private int selectedId;

        public StoreManagementForm(){ Text="Quản lý cửa hàng"; Width=1000; Height=620; InitializeComponent(); Load+=(s,e)=>LoadData(); }
        private void InitializeComponent(){ var top=new Panel{Dock=DockStyle.Top,Height=180}; dgv=CreateGrid(); txtCode=CreateTextBox(top,"Mã cửa hàng",20,20); txtName=CreateTextBox(top,"Tên cửa hàng",20,55); txtAddress=CreateTextBox(top,"Địa chỉ",20,90,520); txtPhone=CreateTextBox(top,"SĐT",400,20); chkActive=CreateCheckBox(top,"Đang hoạt động",145,125); txtSearch=CreateTextBox(top,"Tìm kiếm",400,55,260); CreateButton(top,"Thêm",20,140,BtnAdd_Click); CreateButton(top,"Sửa",130,140,BtnUpdate_Click); CreateButton(top,"Khóa/Mở",240,140,BtnToggle_Click); CreateButton(top,"Tìm",785,53,BtnSearch_Click,80); CreateButton(top,"Tải lại",785,90,(s,e)=>LoadData(),80); dgv.CellClick+=Dgv_CellClick; Controls.Add(dgv); Controls.Add(top); }
        private void LoadData(){try{dgv.DataSource=_service.GetAll();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void Dgv_CellClick(object? sender,DataGridViewCellEventArgs e){if(e.RowIndex<0)return; selectedId=GetSelectedId(dgv); txtCode.Text=dgv.CurrentRow!.Cells["StoreCode"].Value?.ToString(); txtName.Text=dgv.CurrentRow.Cells["StoreName"].Value?.ToString(); txtAddress.Text=dgv.CurrentRow.Cells["Address"].Value?.ToString(); txtPhone.Text=dgv.CurrentRow.Cells["Phone"].Value?.ToString(); chkActive.Checked=Convert.ToBoolean(dgv.CurrentRow.Cells["IsActive"].Value);}
        private Store Build()=>new Store{Id=selectedId,StoreCode=txtCode.Text,StoreName=txtName.Text,Address=txtAddress.Text,Phone=txtPhone.Text,IsActive=chkActive.Checked};
        private void BtnAdd_Click(object? s,EventArgs e){try{var item=Build(); item.Id=0; _service.Add(item); MessageBox.Show("Thêm cửa hàng thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnUpdate_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn cửa hàng.");return;} _service.Update(Build()); MessageBox.Show("Cập nhật thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnToggle_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn cửa hàng.");return;} _service.SetActive(selectedId,!chkActive.Checked); MessageBox.Show("Cập nhật trạng thái thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnSearch_Click(object? s,EventArgs e){try{dgv.DataSource=_service.Search(txtSearch.Text);}catch(Exception ex){MessageBox.Show(ex.Message);}}
    }
}
