using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using System.Collections.ObjectModel;
using ZANE.Models.Auth;
using ZANE.Services;

namespace ZANE.ViewModels;

public partial class UserManagementPageViewModel : ObservableObject
{
    private readonly AuthService _authService;

    // ── 用户列表 ──
    [ObservableProperty]
    private ObservableCollection<UserModel> _users = new();

    [ObservableProperty]
    private UserModel? _selectedUser;

    // ── 编辑弹窗 ──
    [ObservableProperty]
    private bool _isPopupOpen;

    [ObservableProperty]
    private string _editAccount = string.Empty;

    [ObservableProperty]
    private string _editName = string.Empty;

    [ObservableProperty]
    private string _editPassword = string.Empty;

    [ObservableProperty]
    private string _editTel = string.Empty;

    [ObservableProperty]
    private string _editRoleName = string.Empty;

    [ObservableProperty]
    private bool _isEditMode; // true=编辑, false=新建

    [ObservableProperty]
    private string _popupTitle = "新建用户";

    // 可用角色列表
    [ObservableProperty]
    private ObservableCollection<string> _availableRoles = new();

    private bool _isCreating;

    public UserManagementPageViewModel()
    {
        _authService = AppServiceProvider.AuthService;
        LoadUsers();
    }

    private void LoadUsers()
    {
        var users = _authService.LoadUsers();
        Users = new ObservableCollection<UserModel>(users);

        // 加载角色列表
        var roleService = new RoleConfigService();
        var config = roleService.Load();
        AvailableRoles = new ObservableCollection<string>(
            config.Roles.Select(r => r.Name));

        // 默认选中第一行
        if (Users.Count > 0)
            SelectedUser = Users[0];
    }

    // ═══════════════════════════════════════
    //  新建用户
    // ═══════════════════════════════════════

    [RelayCommand]
    private void OpenCreatePopup()
    {
        _isCreating = true;
        IsEditMode = false;
        PopupTitle = "新建用户";
        EditAccount = string.Empty;
        EditName = string.Empty;
        EditPassword = string.Empty;
        EditTel = string.Empty;
        EditRoleName = AvailableRoles.FirstOrDefault() ?? string.Empty;
        IsPopupOpen = true;
    }

    // ═══════════════════════════════════════
    //  编辑用户
    // ═══════════════════════════════════════

    [RelayCommand]
    private void OpenEditPopup()
    {
        if (SelectedUser == null)
        {
            Growl.Warning("请先选择一个用户！");
            return;
        }

        _isCreating = false;
        IsEditMode = true;
        PopupTitle = "编辑用户";
        EditAccount = SelectedUser.Account;
        EditName = SelectedUser.Name;
        EditPassword = SelectedUser.Password;
        EditTel = SelectedUser.Tel;
        EditRoleName = SelectedUser.RoleName;
        IsPopupOpen = true;
    }

    // ═══════════════════════════════════════
    //  保存（新建/更新）
    // ═══════════════════════════════════════

    [RelayCommand]
    private void SaveUser()
    {
        if (string.IsNullOrWhiteSpace(EditAccount))
        {
            Growl.Warning("请输入账号！");
            return;
        }
        if (string.IsNullOrWhiteSpace(EditName))
        {
            Growl.Warning("请输入姓名！");
            return;
        }
        if (string.IsNullOrWhiteSpace(EditPassword))
        {
            Growl.Warning("请输入密码！");
            return;
        }

        var now = DateTime.Now;

        if (_isCreating)
        {
            // 检查账号是否已存在
            if (Users.Any(u => u.Account == EditAccount))
            {
                Growl.Warning($"账号「{EditAccount}」已存在！");
                return;
            }

            var newUser = new UserModel
            {
                Account = EditAccount,
                Name = EditName,
                Password = EditPassword,
                Tel = EditTel,
                RoleName = EditRoleName,
                RegisterTime = now,
                UpdateTime = now,
            };

            _authService.AddUser(newUser);
            Users.Add(newUser);
            Growl.Success($"已创建用户「{EditName}」");
        }
        else
        {
            // 更新
            var updated = new UserModel
            {
                Account = EditAccount,
                Name = EditName,
                Password = EditPassword,
                Tel = EditTel,
                RoleName = EditRoleName,
                RegisterTime = SelectedUser?.RegisterTime ?? now,
                UpdateTime = now,
            };

            _authService.UpdateUser(updated);

            // 刷新列表
            var idx = Users.IndexOf(SelectedUser!);
            if (idx >= 0)
            {
                Users.RemoveAt(idx);
                Users.Insert(idx, updated);
            }
            Growl.Success($"已更新用户「{EditName}」");
        }

        IsPopupOpen = false;
    }

    // ═══════════════════════════════════════
    //  删除用户
    // ═══════════════════════════════════════

    [RelayCommand]
    private void DeleteUser()
    {
        if (SelectedUser == null)
        {
            Growl.Warning("请先选择一个用户！");
            return;
        }

        if (SelectedUser.Account == "admin")
        {
            Growl.Warning("不能删除管理员账户！");
            return;
        }

        if (HandyControl.Controls.MessageBox.Show(
                $"确认删除用户「{SelectedUser.Name}」？",
                "确认删除",
                System.Windows.MessageBoxButton.OKCancel,
                System.Windows.MessageBoxImage.Warning) != System.Windows.MessageBoxResult.OK)
            return;

        _authService.DeleteUser(SelectedUser.Account);
        Users.Remove(SelectedUser);
        SelectedUser = Users.FirstOrDefault();
        Growl.Success("用户已删除");
    }

    // ═══════════════════════════════════════
    //  取消弹窗
    // ═══════════════════════════════════════

    [RelayCommand]
    private void CancelPopup()
    {
        IsPopupOpen = false;
    }
}
