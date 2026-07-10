using System;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms.Admin
{
    public class UserManagementForm : AdminFormBase
    {
        private readonly UserService _service = new UserService();
        private readonly StoreService _storeService = new StoreService();
        private readonly RoleService _roleService = new RoleService();
        private DataGridView dgv = null!;
        private TextBox txtUserCode = null!, txtUsername = null!, txtFullName = null!, txtIdentity = null!, txtPhone = null!, txtEmail = null!, txtAddress = null!, txtPassword = null!, txtConfirm = null!, txtSearch = null!;
        private ComboBox cboStore = null!, cboRole = null!;
        private CheckBox chkActive = null!;
        private int selectedId;

        public UserManagementForm(){ Text="Quản lý tài khoản / nhân viên"; Width=1200; Height=700; InitializeComponent(); Load+=(s,e)=>{LoadCombos(); LoadData();}; }
        private void InitializeComponent(){ var top=new Panel{Dock=DockStyle.Top,Height=235}; dgv=CreateGrid(); txtUserCode=CreateTextBox(top,"Mã NV",20,20); txtUsername=CreateTextBox(top,"Username",20,55); txtFullName=CreateTextBox(top,"Họ tên",20,90); txtIdentity=CreateTextBox(top,"CCCD/CMND",20,125); txtPhone=CreateTextBox(top,"SĐT",380,20); txtEmail=CreateTextBox(top,"Email",380,55); txtAddress=CreateTextBox(top,"Địa chỉ",380,90); cboStore=CreateComboBox(top,"Cửa hàng",740,20); cboRole=CreateComboBox(top,"Vai trò",740,55); txtPassword=CreateTextBox(top,"Mật khẩu",740,90); txtConfirm=CreateTextBox(top,"Xác nhận",740,125); chkActive=CreateCheckBox(top,"Đang hoạt động",145,160); txtSearch=CreateTextBox(top,"Tìm kiếm",380,125); txtPassword.UseSystemPasswordChar=true; txtConfirm.UseSystemPasswordChar=true; CreateButton(top,"Thêm",20,195,BtnAdd_Click); CreateButton(top,"Sửa",130,195,BtnUpdate_Click); CreateButton(top,"Khóa/Mở",240,195,BtnToggle_Click); CreateButton(top,"Reset MK",350,195,BtnResetPassword_Click); CreateButton(top,"Tìm",725,123,BtnSearch_Click,80); CreateButton(top,"Tải lại",815,123,(s,e)=>LoadData(),80); dgv.CellClick+=Dgv_CellClick; Controls.Add(dgv); Controls.Add(top); }
        private void LoadCombos(){try{cboStore.DataSource=_storeService.GetActive(); cboStore.DisplayMember="StoreName"; cboStore.ValueMember="Id"; cboRole.DataSource=_roleService.GetAll(); cboRole.DisplayMember="RoleName"; cboRole.ValueMember="Id";}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void LoadData(){try{dgv.DataSource=_service.GetAll();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private int? SelectedStoreId()=>cboStore.SelectedValue==null?null:Convert.ToInt32(cboStore.SelectedValue);
        private int SelectedRoleId()=>cboRole.SelectedValue==null?0:Convert.ToInt32(cboRole.SelectedValue);
        private void Dgv_CellClick(object? sender,DataGridViewCellEventArgs e){if(e.RowIndex<0)return; selectedId=GetSelectedId(dgv); txtUserCode.Text=dgv.CurrentRow!.Cells["UserCode"].Value?.ToString(); txtUsername.Text=dgv.CurrentRow.Cells["Username"].Value?.ToString(); txtFullName.Text=dgv.CurrentRow.Cells["FullName"].Value?.ToString(); txtIdentity.Text=dgv.CurrentRow.Cells["IdentityNumber"].Value?.ToString(); txtPhone.Text=dgv.CurrentRow.Cells["Phone"].Value?.ToString(); txtEmail.Text=dgv.CurrentRow.Cells["Email"].Value?.ToString(); txtAddress.Text=dgv.CurrentRow.Cells["Address"].Value?.ToString(); chkActive.Checked=Convert.ToBoolean(dgv.CurrentRow.Cells["IsActive"].Value); if(dgv.CurrentRow.Cells["StoreId"].Value!=null) cboStore.SelectedValue=dgv.CurrentRow.Cells["StoreId"].Value; cboRole.SelectedValue=dgv.CurrentRow.Cells["RoleId"].Value; }
        private User Build()=>new User{Id=selectedId,UserCode=txtUserCode.Text,StoreId=SelectedStoreId(),RoleId=SelectedRoleId(),IdentityNumber=txtIdentity.Text,Username=txtUsername.Text,FullName=txtFullName.Text,Phone=txtPhone.Text,Email=txtEmail.Text,Address=txtAddress.Text,IsActive=chkActive.Checked};
        private void BtnAdd_Click(object? s,EventArgs e){try{var item=Build(); item.Id=0; _service.Add(item,txtPassword.Text,txtConfirm.Text); MessageBox.Show("Thêm tài khoản thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnUpdate_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn tài khoản.");return;} _service.Update(Build()); MessageBox.Show("Cập nhật thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnToggle_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn tài khoản.");return;} _service.SetActive(selectedId,!chkActive.Checked); MessageBox.Show("Cập nhật trạng thái thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnResetPassword_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn tài khoản.");return;} _service.ResetPassword(selectedId,txtPassword.Text,txtConfirm.Text); MessageBox.Show("Reset mật khẩu thành công.");}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnSearch_Click(object? s,EventArgs e){try{dgv.DataSource=_service.Search(txtSearch.Text);}catch(Exception ex){MessageBox.Show(ex.Message);}}
    }
}
