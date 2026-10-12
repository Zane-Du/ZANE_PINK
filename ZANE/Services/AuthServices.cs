using Newtonsoft.Json;
using ZANE.Models.Auth;
using System.IO;
using ZANE.Models.Parameters;

namespace ZANE.Services;

public class RoleConfigService
{
    private readonly string _filePath;
    private readonly PanelBuilder _panelBuilder;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
    };

    public RoleConfigService()
    {
        var appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
        Directory.CreateDirectory(appData);
        _filePath = Path.Combine(appData, "RoleConfig.json");
        _panelBuilder = new PanelBuilder();
    }

    public RoleConfigModel Load()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                var defaults = CreateDefault();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(_filePath);
            var config = JsonConvert.DeserializeObject<RoleConfigModel>(json, JsonSettings);
            if (config == null) return CreateDefault();

            // 去重角色
            config.Roles = config.Roles
                .GroupBy(r => r.Level)
                .Select(g => g.First())
                .ToList();

            SyncPermissionItems(config);
            return config;
        }
        catch
        {
            return CreateDefault();
        }
    }

    public void Save(RoleConfigModel config)
    {
        var json = JsonConvert.SerializeObject(config, JsonSettings);
        File.WriteAllText(_filePath, json);
    }

    private void SyncPermissionItems(RoleConfigModel config)
    {
        // 1) 从参数模型同步
        var currentDefs = new List<ControlDefinition>();
        foreach (var (type, _) in GetModelTypes())
            currentDefs.AddRange(_panelBuilder.ScanDefinitions(type));

        config.PermissionItems.RemoveAll(p =>
            !currentDefs.Any(d => d.PropertyName == p.PropertyName));

        foreach (var def in currentDefs)
        {
            if (!config.PermissionItems.Any(p => p.PropertyName == def.PropertyName))
            {
                config.PermissionItems.Add(new PermissionItem
                {
                    PropertyName = def.PropertyName,
                    DisplayName = def.DisplayName,
                    Category = def.Category,
                    EditLevel = (ulong)1 << 62,
                });
            }
        }

        // 2) 同步页面菜单权限
        var menuItems = new (string name, string display)[]
        {
            ("MainPage", "主页面"), ("LogAnalysis", "日志分析"),
            ("ParameterConfig", "参数配置"), ("ChartDisplay", "图表显示"),
            ("ControlPanel", "控制面板"), ("MESTest", "MES测试"),
            ("CameraCalib", "相机标定"), ("ImageDisplay", "图像显示"),
            ("MotionControl", "运动控制"), ("AspNet", "ASP.NET"),
            ("ProductionHistory", "生产历史"), ("DeviceConfig", "设备配置"),
            ("SignalConfig", "信号配置"), ("UserManagement", "角色管理"),
            ("TestPage1", "用户管理"),
        };

        config.PermissionItems.RemoveAll(p =>
            p.Category == "Menu" && !menuItems.Any(m => m.name == p.PropertyName));

        foreach (var (name, display) in menuItems)
        {
            if (!config.PermissionItems.Any(p => p.PropertyName == name && p.Category == "Menu"))
            {
                config.PermissionItems.Add(new PermissionItem
                {
                    PropertyName = name,
                    DisplayName = display,
                    Category = "Menu",
                    EditLevel = (ulong)1 << 62,
                });
            }
        }

        config.PermissionItems = config.PermissionItems
            .OrderBy(p => p.Category).ThenBy(p => p.DisplayName).ToList();
    }

    private static List<(Type type, string category)> GetModelTypes()
    {
        return new()
        {
            (typeof(DeviceParameterModel), "Device"),
            (typeof(RunParameterModel), "Run"),
            (typeof(FunctionEnableModel), "Function"),
            (typeof(AdvancedConfigModel), "Advanced"),
        };
    }

    private RoleConfigModel CreateDefault()
    {
        var config = new RoleConfigModel();
        SyncPermissionItems(config);
        return config;
    }
}

public class AuthService
{
    private readonly string _usersFilePath;
    private readonly RoleConfigService _roleConfigService;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
    };

    public UserModel? CurrentUser { get; private set; }
    public bool IsLoggedIn => CurrentUser != null;
    public RoleModel? CurrentRole { get; private set; }
    public event Action? AuthStateChanged;

    public AuthService()
    {
        var appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
        Directory.CreateDirectory(appData);
        _usersFilePath = Path.Combine(appData, "Users.json");
        _roleConfigService = new RoleConfigService();
    }

    public bool Login(string account, string password)
    {
        var users = LoadUsers();
        var user = users.FirstOrDefault(u => u.Account == account && u.Password == password);
        if (user == null) return false;

        CurrentUser = user;
        var roleConfig = _roleConfigService.Load();
        CurrentRole = roleConfig.Roles.FirstOrDefault(r => r.Name == user.RoleName);
        AuthStateChanged?.Invoke();
        return true;
    }

    public void Logout()
    {
        CurrentUser = null;
        CurrentRole = null;
        AuthStateChanged?.Invoke();
    }

    public List<UserModel> LoadUsers()
    {
        try
        {
            if (!File.Exists(_usersFilePath))
            {
                var defaults = GetDefaultUsers();
                SaveUsers(defaults);
                return defaults;
            }
            var json = File.ReadAllText(_usersFilePath);
            return JsonConvert.DeserializeObject<List<UserModel>>(json, JsonSettings) ?? GetDefaultUsers();
        }
        catch { return GetDefaultUsers(); }
    }

    public void SaveUsers(List<UserModel> users)
    {
        var json = JsonConvert.SerializeObject(users, JsonSettings);
        File.WriteAllText(_usersFilePath, json);
    }

    public void AddUser(UserModel user)
    {
        var users = LoadUsers();
        users.Add(user);
        SaveUsers(users);
    }

    public void DeleteUser(string account)
    {
        var users = LoadUsers();
        users.RemoveAll(u => u.Account == account);
        SaveUsers(users);
    }

    public void UpdateUser(UserModel updatedUser)
    {
        var users = LoadUsers();
        var idx = users.FindIndex(u => u.Account == updatedUser.Account);
        if (idx >= 0)
        {
            updatedUser.UpdateTime = DateTime.Now;
            users[idx] = updatedUser;
            SaveUsers(users);
        }
    }

    private static List<UserModel> GetDefaultUsers()
    {
        return new()
        {
            new() { Account = "admin", Name = "管理员", Password = "admin", RoleName = "管理员" },
            new() { Account = "tech", Name = "工艺师", Password = "tech", RoleName = "工艺" },
            new() { Account = "device", Name = "设备员", Password = "device", RoleName = "设备" },
        };
    }

    public bool HasPermission(PermissionItem item)
    {
        if (CurrentRole == null) return false;
        return item.HasPermission(CurrentRole);
    }

    public bool HasPropertyPermission(string propertyName)
    {
        if (CurrentRole == null) return false;
        var config = _roleConfigService.Load();
        var item = config.PermissionItems.FirstOrDefault(p => p.PropertyName == propertyName);
        if (item == null) return false;
        return item.HasPermission(CurrentRole);
    }

    /// <summary>
    /// 检查当前用户是否有某页面(菜单)的访问权限
    /// </summary>
    public bool HasPagePermission(string viewName)
    {
        if (CurrentRole == null) return false;
        var config = _roleConfigService.Load();
        var item = config.PermissionItems.FirstOrDefault(p =>
            p.PropertyName == viewName && p.Category == "Menu");
        if (item == null) return false;
        return item.HasPermission(CurrentRole);
    }
}
