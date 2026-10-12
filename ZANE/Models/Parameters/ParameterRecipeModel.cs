using System.Collections.ObjectModel;

namespace ZANE.Models.Parameters;

/// <summary>
/// 配方配置 —— 参数配置的根模型
/// </summary>
public class ParameterRecipeModel
{
    /// <summary>配方名称</summary>
    public string RecipeName { get; set; } = "默认配方";

    /// <summary>基本（设备）参数</summary>
    public DeviceParameterModel DeviceParameter { get; set; } = new();

    /// <summary>运行参数</summary>
    public RunParameterModel RunParameter { get; set; } = new();

    /// <summary>功能启用开关</summary>
    public FunctionEnableModel FunctionEnable { get; set; } = new();

    /// <summary>高级设置</summary>
    public AdvancedConfigModel AdvancedConfig { get; set; } = new();
}

/// <summary>
/// 配方管理模型 —— 管理所有配方名称
/// </summary>
public class RecipeManagerModel
{
    /// <summary>所有配方名称列表</summary>
    public ObservableCollection<string> Recipes { get; set; } = new() { "默认配方" };

    /// <summary>当前配方名称</summary>
    public string CurrentRecipe { get; set; } = "默认配方";
}
