using System;
using System.Windows.Forms;
using BookStoreManagement.Helpers;
using BookStoreManagement.Services;

namespace BookStoreManagement.Forms.Admin
{
    public class InventoryManagementForm : AdminFormBase
    {
        private readonly StoreBookInventoryService _inventoryService = new StoreBookInventoryService();
        private readonly InventoryService _historyService = new InventoryService();
        private readonly StoreService _storeService = new StoreService();
        private DataGridView dgvInventory = null!, dgvHistory = null!;
        private TextBox txtSearch = null!;
        private ComboBox cboStore = null!;

        public InventoryManagementForm(){ Text="Quản lý tồn kho"; Width=1200; Height=720; InitializeComponent(); Load+=(s,e)=>{LoadStores(); LoadInventory(); LoadHistory();}; }
        private void InitializeComponent(){ var top=new Panel{Dock=DockStyle.Top,Height=80}; cboStore=CreateComboBox(top,"Cửa hàng",20,20); txtSearch=CreateTextBox(top,"Tìm kiếm",390,20,260); CreateButton(top,"Tìm",780,18,BtnSearch_Click,80); CreateButton(top,"Tải lại",870,18,(s,e)=>{LoadInventory();LoadHistory();},90); var split=new SplitContainer{Dock=DockStyle.Fill,Orientation=Orientation.Horizontal,SplitterDistance=320}; dgvInventory=CreateGrid(); dgvHistory=CreateGrid(); split.Panel1.Controls.Add(dgvInventory); split.Panel2.Controls.Add(dgvHistory); Controls.Add(split); Controls.Add(top); }
        private int? SelectedStoreId(){ if(cboStore.SelectedValue==null || cboStore.SelectedValue.ToString()=="0") return null; return Convert.ToInt32(cboStore.SelectedValue); }
        private void LoadStores(){try{var stores=_storeService.GetActive(); stores.Insert(0,new BookStoreManagement.Models.Store{Id=0,StoreName="Tất cả"}); cboStore.DataSource=stores; cboStore.DisplayMember="StoreName"; cboStore.ValueMember="Id";}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void LoadInventory(){try{int? storeId=SelectedStoreId(); if(CurrentSession.IsStaff && CurrentSession.StoreId.HasValue) storeId=CurrentSession.StoreId; dgvInventory.DataSource=storeId.HasValue ? _inventoryService.GetByStoreId(storeId.Value) : _inventoryService.GetAll();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void LoadHistory(){try{int? storeId=SelectedStoreId(); if(CurrentSession.IsStaff && CurrentSession.StoreId.HasValue) storeId=CurrentSession.StoreId; dgvHistory.DataSource=storeId.HasValue ? _historyService.GetHistoryByStoreId(storeId.Value) : _historyService.GetHistory();}catch(Exception ex){MessageBox.Show(ex.Message);}}
        private void BtnSearch_Click(object? sender,EventArgs e){try{dgvInventory.DataSource=_inventoryService.Search(txtSearch.Text,SelectedStoreId());}catch(Exception ex){MessageBox.Show(ex.Message);}}
    }
}
