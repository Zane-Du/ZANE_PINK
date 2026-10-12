namespace ZANE.Models.Auth;

/// <summary>
/// 角色模型 —— 每个角色对应一个 bit 位权限
/// </summary>
public class RoleModel
{
    /// <summary>权限位标记（如 1<<0, 1<<1, ...）</summary>
    public ulong Level { get; set; } = 1;
    /// <summary>角色名称（如"管理员"、"工艺"、"设备"）</summary>
    public string Name { get; set; } = string.Empty;

    public RoleModel() { }

    public RoleModel(ulong level, string name)
    {
        Level = level;
        Name = name;
    }
}

/// <summary>
/// 权限项 —— 对应参数配置中一个可编辑字段的权限设置
/// </summary>
public partial class PermissionItem : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
{
    public string PropertyName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public ulong EditLevel { get; set; }

    [CommunityToolkit.Mvvm.ComponentModel.ObservableProperty]
    private bool _isSelecked;

    public bool HasPermission(RoleModel role)
    {
        if (role == null) return false;
        return (role.Level & EditLevel) > 0;
    }

    public void SetPermission(RoleModel role, bool grant)
    {
        if (grant)
            EditLevel = EditLevel | role.Level;
        else
            EditLevel = EditLevel & ~role.Level;
    }
}

public class UserModel
{
    public string Account { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string Tel { get; set; } = string.Empty;
    public DateTime RegisterTime { get; set; } = DateTime.Now;
    public DateTime LoginTime { get; set; }
    public DateTime UpdateTime { get; set; }
}

/// <summary>
/// 角色权限配置根模型
/// </summary>
public class RoleConfigModel
{
    public List<RoleModel> Roles { get; set; } = new()
    {
        new(ulong.MaxValue, "管理员"),
        new(1 << 1, "工艺"),
        new(1 << 2, "设备"),
        new(1 << 3, "生产"),
    };

    public List<PermissionItem> PermissionItems { get; set; } = new();
}
