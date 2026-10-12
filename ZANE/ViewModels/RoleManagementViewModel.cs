using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using System.Collections.ObjectModel;
using ZANE.Models.Auth;
using ZANE.Services;

namespace ZANE.ViewModels;

public partial class RoleManagementViewModel : ObservableObject
{
    private readonly RoleConfigService _roleConfig;
    private readonly AuthService _authService;

    // ── 角色列表 ──
    [ObservableProperty]
    private ObservableCollection<RoleModel> _roles = new();

    [ObservableProperty]
    private RoleModel? _selectedRole;

    // ── 权限面板 ──
    [ObservableProperty]
    private ObservableCollection<PermissionItem> _basicPermissions = new();

    [ObservableProperty]
    private ObservableCollection<PermissionItem> _runPermissions = new();

    [ObservableProperty]
    private ObservableCollection<PermissionItem> _functionPermissions = new();

    [ObservableProperty]
    private ObservableCollection<PermissionItem> _advancedPermissions = new();

    [ObservableProperty]
    private ObservableCollection<PermissionItem> _menuPermissions = new();

    // ── 编辑角色（新建/编辑弹窗） ──
    [ObservableProperty]
    private string _editRoleName = string.Empty;

    [ObservableProperty]
    private bool _isCreatePopupOpen;

    private RoleModel? _editingRole; // 编辑中的角色

    // 已在使用的角色位掩码
    private ulong _usedLevels;

    // 原始保存的配置
    private RoleConfigModel _savedConfig = new();

    public RoleManagementViewModel()
    {
        _roleConfig = new RoleConfigService();
        _authService = new AuthService(); // 共享单例

        LoadConfig();
    }

    private void LoadConfig()
    {
        _savedConfig = _roleConfig.Load();
        Roles = new ObservableCollection<RoleModel>(_savedConfig.Roles);

        // 计算已使用的 level
        _usedLevels = 0;
        foreach (var r in Roles)
            _usedLevels |= r.Level;

        // 按分类筛选权限项
        RefreshPermissionPanels();

        // 默认选中第一个
        if (Roles.Count > 0)
            SelectedRole = Roles[0];
    }

    private void RefreshPermissionPanels()
    {
        BasicPermissions = new ObservableCollection<PermissionItem>(
            _savedConfig.PermissionItems.Where(p => p.Category == "Device"));
        RunPermissions = new ObservableCollection<PermissionItem>(
            _savedConfig.PermissionItems.Where(p => p.Category == "Run"));
        FunctionPermissions = new ObservableCollection<PermissionItem>(
            _savedConfig.PermissionItems.Where(p => p.Category == "Function"));
        AdvancedPermissions = new ObservableCollection<PermissionItem>(
            _savedConfig.PermissionItems.Where(p => p.Category == "Advanced"));
    }

    // ═══════════════════════════════════════
    //  角色切换 → 更新权限勾选状态
    // ═══════════════════════════════════════

    partial void OnSelectedRoleChanged(RoleModel? value)
    {
        if (value == null) return;
        ApplyRoleToPermissions(value);
    }

    private void ApplyRoleToPermissions(RoleModel role)
    {
        foreach (var item in _savedConfig.PermissionItems)
        {
            item.IsSelecked = item.HasPermission(role);
            item.NotifySelection(); // 手动通知 UI 更新
        }

        // 刷新面板以更新勾选状态
        RefreshPermissionPanels();
    }

    // ═══════════════════════════════════════
    //  切换权限勾选
    // ═══════════════════════════════════════

    [RelayCommand]
    private void TogglePermission(PermissionItem? item)
    {
        if (item == null || SelectedRole == null) return;

        item.SetPermission(SelectedRole, !item.IsSelecked);
        item.IsSelecked = item.HasPermission(SelectedRole);

        // 强制刷新列表（CommunityToolkit 无法自动检测集合内对象属性变更）
        RefreshPermissionPanels();
    }

    /// <summary>
    /// 全选/取消全选
    /// </summary>
    [RelayCommand]
    private void SelectAllInCategory(string? category)
    {
        if (string.IsNullOrEmpty(category) || SelectedRole == null) return;

        var items = _savedConfig.PermissionItems.Where(p => p.Category == category).ToList();
        var allSelected = items.All(i => i.IsSelecked);
        var newState = !allSelected;

        foreach (var item in items)
        {
            item.SetPermission(SelectedRole, newState);
            item.IsSelecked = newState;
        }

        RefreshPermissionPanels();
    }

    // ═══════════════════════════════════════
    //  保存权限
    // ═══════════════════════════════════════

    [RelayCommand]
    private void SavePermissions()
    {
        _roleConfig.Save(_savedConfig);
        Growl.Success("权限配置保存成功！");
    }

    // ═══════════════════════════════════════
    //  新建角色
    // ═══════════════════════════════════════

    [RelayCommand]
    private void OpenCreatePopup()
    {
        EditRoleName = string.Empty;
        _editingRole = null;
        IsCreatePopupOpen = true;
    }

    [RelayCommand]
    private void CreateRole()
    {
        if (string.IsNullOrWhiteSpace(EditRoleName))
        {
            Growl.Warning("请输入角色名称！");
            return;
        }

        if (Roles.Any(r => r.Name == EditRoleName))
        {
            Growl.Warning($"角色「{EditRoleName}」已存在！");
            return;
        }

        // 找一个空闲的 bit 位
        ulong newLevel = FindFreeLevel();
        if (newLevel == 0)
        {
            Growl.Warning("角色数量已达上限！");
            return;
        }

        var role = new RoleModel(newLevel, EditRoleName);
        Roles.Add(role);
        _usedLevels |= newLevel;
        _savedConfig.Roles.Add(role);

        _roleConfig.Save(_savedConfig);
        SelectedRole = role;
        IsCreatePopupOpen = false;

        Growl.Success($"已创建角色「{EditRoleName}」");
    }

    private ulong FindFreeLevel()
    {
        for (int i = 0; i < 60; i++)
        {
            ulong bit = (ulong)1 << i;
            if ((_usedLevels & bit) == 0)
                return bit;
        }
        return 0;
    }

    // ═══════════════════════════════════════
    //  删除角色
    // ═══════════════════════════════════════

    [RelayCommand]
    private void DeleteRole()
    {
        if (SelectedRole == null) return;

        // 保护默认角色
        if (SelectedRole.Name is "管理员" or "工艺" or "设备" or "生产")
        {
            Growl.Warning("不能删除默认角色！");
            return;
        }

        if (HandyControl.Controls.MessageBox.Show($"确认删除角色「{SelectedRole.Name}」？", "确认", System.Windows.MessageBoxButton.OKCancel, System.Windows.MessageBoxImage.Warning) != System.Windows.MessageBoxResult.OK)
            return;

        _usedLevels &= ~SelectedRole.Level;
        _savedConfig.Roles.RemoveAll(r => r.Level == SelectedRole.Level);
        Roles.Remove(SelectedRole);

        _roleConfig.Save(_savedConfig);

        // 选中第一个
        if (Roles.Count > 0)
            SelectedRole = Roles[0];

        Growl.Success("角色已删除");
    }

    // ═══════════════════════════════════════
    //  编辑角色名称
    // ═══════════════════════════════════════

    [RelayCommand]
    private void OpenEditPopup()
    {
        if (SelectedRole == null) return;
        _editingRole = SelectedRole;
        EditRoleName = SelectedRole.Name;
        IsCreatePopupOpen = true; // 复用来编辑
    }

    [RelayCommand]
    private void SaveRoleName()
    {
        if (_editingRole == null || string.IsNullOrWhiteSpace(EditRoleName)) return;

        if (Roles.Any(r => r.Name == EditRoleName && r.Level != _editingRole.Level))
        {
            Growl.Warning("角色名称已存在！");
            return;
        }

        _editingRole.Name = EditRoleName;
        var saved = _savedConfig.Roles.FirstOrDefault(r => r.Level == _editingRole.Level);
        if (saved != null) saved.Name = EditRoleName;

        _roleConfig.Save(_savedConfig);

        // 刷新列表
        var idx = Roles.IndexOf(_editingRole);
        if (idx >= 0)
        {
            Roles.RemoveAt(idx);
            Roles.Insert(idx, _editingRole);
        }

        IsCreatePopupOpen = false;
        Growl.Success("角色名称已更新");
    }

    [RelayCommand]
    private void CancelPopup()
    {
        IsCreatePopupOpen = false;
    }
}

public static class PermissionItemExtensions
{
    public static void NotifySelection(this PermissionItem item)
    {
    }
}
