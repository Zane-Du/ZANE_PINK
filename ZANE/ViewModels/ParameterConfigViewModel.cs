using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using ZANE.Models.Parameters;
using ZANE.Services;

namespace ZANE.ViewModels;

public partial class ParameterConfigViewModel : ObservableObject
{
    private readonly PanelBuilder _panelBuilder;
    private readonly RecipeConfigService _recipeService;

    // ── 配方管理 ──
    [ObservableProperty]
    private ObservableCollection<string> _recipeList = new();

    [ObservableProperty]
    private string _selectedRecipe = string.Empty;

    [ObservableProperty]
    private string _newRecipeName = string.Empty;

    [ObservableProperty]
    private bool _isNewRecipePopupOpen;

    // ── 当前配方副本 ──
    private ParameterRecipeModel _currentRecipeCopy = new();
    private ParameterRecipeModel _savedRecipe = new();

    // ── 动态面板 ──
    [ObservableProperty]
    private StackPanel? _deviceParameterPanel;

    [ObservableProperty]
    private StackPanel? _runParameterPanel;

    [ObservableProperty]
    private StackPanel? _functionEnablePanel;

    [ObservableProperty]
    private StackPanel? _advancedConfigPanel;

    // ── 分类元数据缓存 ──
    private readonly Dictionary<string, List<ControlDefinition>> _categoryDefinitions = new();
    private static readonly Dictionary<string, string> CategoryNames = new()
    {
        ["Device"] = "基本参数",
        ["Run"] = "运行参数",
        ["Function"] = "功能启用",
        ["Advanced"] = "高级设置",
    };

    public ParameterConfigViewModel()
    {
        _panelBuilder = new PanelBuilder(permissionChecker: (propName) => {
            var auth = AppServiceProvider.AuthService;
            if (!auth.IsLoggedIn) return false;
            return auth.HasPropertyPermission(propName);
        });
        _recipeService = new RecipeConfigService();
        // 登录状态变化时刷新面板权限
        AppServiceProvider.AuthService.AuthStateChanged += OnAuthChanged;

        // 扫描参数模型，按分类缓存元数据
        ScanAllDefinitions();

        // 加载配方管理器
        LoadRecipeManager();
    }

    /// <summary>
    /// 扫描所有参数模型的 [ParameterDisplay] 特性
    /// </summary>
    private void ScanAllDefinitions()
    {
        var types = new (Type type, string category)[]
        {
            (typeof(DeviceParameterModel), "Device"),
            (typeof(RunParameterModel), "Run"),
            (typeof(FunctionEnableModel), "Function"),
            (typeof(AdvancedConfigModel), "Advanced"),
        };

        foreach (var (type, category) in types)
        {
            _categoryDefinitions[category] = _panelBuilder.ScanDefinitions(type);
        }
    }

    /// <summary>
    /// 加载配方管理器并加载默认配方
    /// </summary>
    private void LoadRecipeManager()
    {
        var manager = _recipeService.LoadRecipeManager();
        RecipeList = manager.Recipes;
        SelectedRecipe = manager.CurrentRecipe;

        // 加载当前配方数据
        _savedRecipe = _recipeService.LoadRecipe(SelectedRecipe);
        DeepCopyToCopy();

        // 构建 UI 面板
        RebuildPanels();
    }

    /// <summary>
    /// 深拷贝当前配方到副本
    /// </summary>
    private void DeepCopyToCopy()
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(_savedRecipe);
        _currentRecipeCopy = Newtonsoft.Json.JsonConvert.DeserializeObject<ParameterRecipeModel>(json)
                              ?? new ParameterRecipeModel { RecipeName = SelectedRecipe };
    }

    /// <summary>
    /// 深拷贝副本到已保存
    /// </summary>
    private void CopyToSaved()
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(_currentRecipeCopy);
        _savedRecipe = Newtonsoft.Json.JsonConvert.DeserializeObject<ParameterRecipeModel>(json)
                       ?? new ParameterRecipeModel { RecipeName = SelectedRecipe };
    }

    /// <summary>
    /// 重新生成所有 UI 面板
    /// </summary>
    private void OnAuthChanged()
    {
        // 登录/注销后刷新面板，更新控件的 IsEnabled 状态
        RebuildPanels();
    }

    private void RebuildPanels()
    {
        DeviceParameterPanel = _panelBuilder.BuildPanel(
            _currentRecipeCopy.DeviceParameter,
            _categoryDefinitions["Device"]);

        RunParameterPanel = _panelBuilder.BuildPanel(
            _currentRecipeCopy.RunParameter,
            _categoryDefinitions["Run"]);

        FunctionEnablePanel = _panelBuilder.BuildPanel(
            _currentRecipeCopy.FunctionEnable,
            _categoryDefinitions["Function"]);

        AdvancedConfigPanel = _panelBuilder.BuildPanel(
            _currentRecipeCopy.AdvancedConfig,
            _categoryDefinitions["Advanced"]);
    }

    // ═══════════════════════════════════════════
    //  配方切换
    // ═══════════════════════════════════════════

    partial void OnSelectedRecipeChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        // 加载该配方数据
        _savedRecipe = _recipeService.LoadRecipe(value);
        DeepCopyToCopy();
        RebuildPanels();

        // 更新配方管理器的当前配方
        var manager = _recipeService.LoadRecipeManager();
        manager.CurrentRecipe = value;
        _recipeService.SaveRecipeManager(manager);
    }

    // ═══════════════════════════════════════════
    //  保存配置
    // ═══════════════════════════════════════════

    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            CopyToSaved();
            _savedRecipe.RecipeName = SelectedRecipe;
            _recipeService.SaveRecipe(SelectedRecipe, _savedRecipe);
            Growl.Success($"配方「{SelectedRecipe}」保存成功！");
        }
        catch (Exception ex)
        {
            Growl.Error($"保存失败：{ex.Message}");
        }
    }

    // ═══════════════════════════════════════════
    //  新建配方
    // ═══════════════════════════════════════════

    [RelayCommand]
    private void OpenNewRecipePopup()
    {
        NewRecipeName = string.Empty;
        IsNewRecipePopupOpen = true;
    }

    [RelayCommand]
    private void CreateRecipe()
    {
        if (string.IsNullOrWhiteSpace(NewRecipeName))
        {
            Growl.Warning("请输入配方名称！");
            return;
        }

        if (RecipeList.Contains(NewRecipeName))
        {
            Growl.Warning("配方名称已存在，请重新输入！");
            return;
        }

        // 添加到配方列表
        RecipeList.Add(NewRecipeName);

        // 新建默认配方数据
        var newRecipe = new ParameterRecipeModel { RecipeName = NewRecipeName };
        _recipeService.SaveRecipe(NewRecipeName, newRecipe);

        // 更新配方管理器
        var manager = _recipeService.LoadRecipeManager();
        manager.Recipes = RecipeList;
        manager.CurrentRecipe = NewRecipeName;
        _recipeService.SaveRecipeManager(manager);

        // 切换到新配方
        SelectedRecipe = NewRecipeName;
        IsNewRecipePopupOpen = false;

        Growl.Success($"已创建配方「{NewRecipeName}」");
    }

    // ═══════════════════════════════════════════
    //  删除配方
    // ═══════════════════════════════════════════

    [RelayCommand]
    private void DeleteRecipe()
    {
        if (RecipeList.Count < 2)
        {
            Growl.Warning("至少保留一个配方！");
            return;
        }

        // 删除文件
        _recipeService.DeleteRecipe(SelectedRecipe);

        // 从列表中移除
        RecipeList.Remove(SelectedRecipe);

        // 更新配方管理器
        var manager = _recipeService.LoadRecipeManager();
        manager.Recipes = RecipeList;
        manager.CurrentRecipe = RecipeList[0];
        _recipeService.SaveRecipeManager(manager);

        // 切换到第一个配方
        SelectedRecipe = RecipeList[0];
        Growl.Success($"已删除配方");
    }
}
