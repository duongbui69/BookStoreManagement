using System;
using System.Windows.Forms;
using BookStoreManagement.Models;
using BookStoreManagement.Services;
using BookStoreManagement.Helpers;

namespace BookStoreManagement.Forms.Admin
{
    public class BookManagementForm : AdminFormBase
    {
        private readonly BookService _bookService = new BookService();
        private readonly CategoryService _categoryService = new CategoryService();
        private readonly AuthorService _authorService = new AuthorService();
        private readonly PublisherService _publisherService = new PublisherService();
        private readonly FileStorageService _fileService = new FileStorageService();
        private DataGridView dgv = null!;
        private Panel topPanel = null!;
        private TextBox txtBookCode = null!, txtISBN = null!, txtTitle = null!, txtPublishYear = null!, txtPageCount = null!, txtPrice = null!, txtQuantity = null!, txtMinStock = null!, txtDescription = null!, txtImagePath = null!, txtSearch = null!;
        private ComboBox cboCategory = null!, cboAuthor = null!, cboPublisher = null!;
        private CheckBox chkActive = null!;
        private int selectedId;

        public BookManagementForm(){ Text="Quản lý sách"; Width=1250; Height=740; InitializeComponent(); Load+=(s,e)=>{LoadCombos(); SetupByRole(); LoadData();}; }
        private void InitializeComponent(){ var top=new Panel{Dock=DockStyle.Top,Height=270}; topPanel=top; dgv=CreateGrid(); txtBookCode=CreateTextBox(top,"Mã sách",20,20); txtISBN=CreateTextBox(top,"ISBN",20,55); txtTitle=CreateTextBox(top,"Tên sách",20,90); txtPublishYear=CreateTextBox(top,"Năm XB",20,125); txtPageCount=CreateTextBox(top,"Số trang",20,160); cboCategory=CreateComboBox(top,"Danh mục",380,20); cboAuthor=CreateComboBox(top,"Tác giả",380,55); cboPublisher=CreateComboBox(top,"NXB",380,90); txtPrice=CreateTextBox(top,"Giá bán",380,125); txtQuantity=CreateTextBox(top,"Tổng tồn",380,160); txtMinStock=CreateTextBox(top,"Tồn tối thiểu",740,20); txtDescription=CreateTextBox(top,"Mô tả",740,55,330); txtImagePath=CreateTextBox(top,"Ảnh",740,90,330); chkActive=CreateCheckBox(top,"Đang hoạt động",865,125); txtSearch=CreateTextBox(top,"Tìm kiếm",740,160,260); CreateButton(top,"Chọn ảnh",1080,88,BtnChooseImage_Click,90); CreateButton(top,"Thêm",20,220,BtnAdd_Click); CreateButton(top,"Sửa",130,220,BtnUpdate_Click); CreateButton(top,"Khóa/Mở",240,220,BtnToggle_Click); CreateButton(top,"Tìm",1025,158,BtnSearch_Click,80); CreateButton(top,"Tải lại",1110,158,(s,e)=>LoadData(),80); dgv.CellClick+=Dgv_CellClick; Controls.Add(dgv); Controls.Add(top); }
        private void LoadCombos(){try{cboCategory.DataSource=_categoryService.GetActive(); cboCategory.DisplayMember="CategoryName"; cboCategory.ValueMember="Id"; cboAuthor.DataSource=_authorService.GetActive(); cboAuthor.DisplayMember="AuthorName"; cboAuthor.ValueMember="Id"; cboPublisher.DataSource=_publisherService.GetActive(); cboPublisher.DisplayMember="PublisherName"; cboPublisher.ValueMember="Id";}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void LoadData(){try{dgv.DataSource=CurrentSession.IsAdmin?_bookService.GetAll():_bookService.GetActive();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void SetupByRole(){ if(!CurrentSession.IsAdmin){ foreach(Control c in topPanel.Controls){ if(c is Button b && (b.Text=="Thêm" || b.Text=="Sửa" || b.Text=="Khóa/Mở" || b.Text=="Chọn ảnh")) b.Enabled=false; } } }
        private int SelectedInt(ComboBox cbo)=>cbo.SelectedValue==null?0:Convert.ToInt32(cbo.SelectedValue);
        private int? SelectedNullableInt(ComboBox cbo)=>cbo.SelectedValue==null?null:Convert.ToInt32(cbo.SelectedValue);
        private void Dgv_CellClick(object? sender,DataGridViewCellEventArgs e){if(e.RowIndex<0)return; selectedId=GetSelectedId(dgv); txtBookCode.Text=dgv.CurrentRow!.Cells["BookCode"].Value?.ToString(); txtISBN.Text=dgv.CurrentRow.Cells["ISBN"].Value?.ToString(); txtTitle.Text=dgv.CurrentRow.Cells["Title"].Value?.ToString(); txtPublishYear.Text=dgv.CurrentRow.Cells["PublishYear"].Value?.ToString(); txtPageCount.Text=dgv.CurrentRow.Cells["PageCount"].Value?.ToString(); txtPrice.Text=dgv.CurrentRow.Cells["SellingPrice"].Value?.ToString(); txtQuantity.Text=dgv.CurrentRow.Cells["Quantity"].Value?.ToString(); txtMinStock.Text=dgv.CurrentRow.Cells["MinStock"].Value?.ToString(); txtDescription.Text=dgv.CurrentRow.Cells["Description"].Value?.ToString(); txtImagePath.Text=dgv.CurrentRow.Cells["ImagePath"].Value?.ToString(); chkActive.Checked=Convert.ToBoolean(dgv.CurrentRow.Cells["IsActive"].Value); cboCategory.SelectedValue=dgv.CurrentRow.Cells["CategoryId"].Value; if(dgv.CurrentRow.Cells["AuthorId"].Value!=null)cboAuthor.SelectedValue=dgv.CurrentRow.Cells["AuthorId"].Value; if(dgv.CurrentRow.Cells["PublisherId"].Value!=null)cboPublisher.SelectedValue=dgv.CurrentRow.Cells["PublisherId"].Value;}
        private Book Build()=>new Book{Id=selectedId,BookCode=txtBookCode.Text,ISBN=txtISBN.Text,Title=txtTitle.Text,PublishYear=ToNullableInt(txtPublishYear.Text),PageCount=ToNullableInt(txtPageCount.Text),CategoryId=SelectedInt(cboCategory),AuthorId=SelectedNullableInt(cboAuthor),PublisherId=SelectedNullableInt(cboPublisher),SellingPrice=ToDecimal(txtPrice.Text),Quantity=ToInt(txtQuantity.Text),MinStock=ToInt(txtMinStock.Text),Description=txtDescription.Text,ImagePath=txtImagePath.Text,IsActive=chkActive.Checked};
        private void BtnChooseImage_Click(object? s,EventArgs e){try{using var ofd=new OpenFileDialog{Filter="Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif"}; if(ofd.ShowDialog()==DialogResult.OK){txtImagePath.Text=_fileService.SaveBookImage(ofd.FileName);}}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnAdd_Click(object? s,EventArgs e){try{var item=Build(); item.Id=0; _bookService.Add(item); MessageBox.Show("Thêm sách thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnUpdate_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn sách.");return;} _bookService.Update(Build()); MessageBox.Show("Cập nhật thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnToggle_Click(object? s,EventArgs e){try{if(selectedId<=0){MessageBox.Show("Chọn sách.");return;} _bookService.SetActive(selectedId,!chkActive.Checked); MessageBox.Show("Cập nhật trạng thái thành công."); LoadData();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnSearch_Click(object? s,EventArgs e){try{dgv.DataSource=_bookService.Search(txtSearch.Text);}catch(Exception ex){MessageBox.Show(ex.Message);}}
    }
}
