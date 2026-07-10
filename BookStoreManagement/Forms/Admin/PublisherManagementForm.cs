using System;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms.Admin
{
    public class PublisherManagementForm : AdminFormBase
    {
        private readonly PublisherService _service = new PublisherService();
        private DataGridView dgv = null!;
        private TextBox txtName = null!, txtPhone = null!, txtEmail = null!, txtAddress = null!, txtSearch = null!;
        private CheckBox chkActive = null!;
        private int selectedId;

        public PublisherManagementForm(){ Text="Quản lý nhà xuất bản"; Width=1000; Height=620; InitializeComponent(); Load += (s,e)=>LoadData(); }
        private void InitializeComponent(){ var top=new Panel{Dock=DockStyle.Top,Height=180}; dgv=CreateGrid(); txtName=CreateTextBox(top,"Tên NXB",20,20); txtPhone=CreateTextBox(top,"SĐT",20,55); txtEmail=CreateTextBox(top,"Email",20,90); txtAddress=CreateTextBox(top,"Địa chỉ",380,55,350); chkActive=CreateCheckBox(top,"Đang hoạt động",145,125); txtSearch=CreateTextBox(top,"Tìm kiếm",380,20,260); CreateButton(top,"Thêm",20,140,BtnAdd_Click); CreateButton(top,"Sửa",130,140,BtnUpdate_Click); CreateButton(top,"Khóa/Mở",240,140,BtnToggle_Click); CreateButton(top,"Tìm",765,18,BtnSearch_Click,80); CreateButton(top,"Tải lại",765,55,(s,e)=>LoadData(),80); dgv.CellClick+=Dgv_CellClick; Controls.Add(dgv); Controls.Add(top);}        
        private void LoadData(){try{dgv.DataSource=_service.GetAll();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void Dgv_CellClick(object? sender, DataGridViewCellEventArgs e){ if(e.RowIndex<0)return; selectedId=GetSelectedId(dgv); txtName.Text=dgv.CurrentRow!.Cells["PublisherName"].Value?.ToString(); txtPhone.Text=dgv.CurrentRow.Cells["Phone"].Value?.ToString(); txtEmail.Text=dgv.CurrentRow.Cells["Email"].Value?.ToString(); txtAddress.Text=dgv.CurrentRow.Cells["Address"].Value?.ToString(); chkActive.Checked=Convert.ToBoolean(dgv.CurrentRow.Cells["IsActive"].Value); }
        private Publisher Build()=>new Publisher{Id=selectedId,PublisherName=txtName.Text,Phone=txtPhone.Text,Email=txtEmail.Text,Address=txtAddress.Text,IsActive=chkActive.Checked};
        private void BtnAdd_Click(object? s,EventArgs e){try{var item=Build(); item.Id=0; _service.Add(item); MessageBox.Show("Thêm thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnUpdate_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn NXB.");return;} _service.Update(Build()); MessageBox.Show("Cập nhật thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnToggle_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn NXB.");return;} _service.SetActive(selectedId,!chkActive.Checked); MessageBox.Show("Cập nhật trạng thái thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnSearch_Click(object? s,EventArgs e){try{dgv.DataSource=_service.Search(txtSearch.Text);}catch(Exception ex){MessageBox.Show(ex.Message);}}
    }
}
