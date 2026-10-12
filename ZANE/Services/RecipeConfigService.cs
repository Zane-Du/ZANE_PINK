using System.IO;
using Newtonsoft.Json;
using ZANE.Models.Parameters;

namespace ZANE.Services;

/// <summary>
/// 配方配置持久化服务 —— 每个配方存为一个 JSON 文件
/// </summary>
public class RecipeConfigService
{
    private readonly string _basePath;
    private readonly string _managerFilePath;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
    };

    public RecipeConfigService()
    {
        _basePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs", "Recipes");
        Directory.CreateDirectory(_basePath);
        _managerFilePath = Path.Combine(_basePath, "RecipeManager.json");
    }

    /// <summary>获取所有配方文件路径</summary>
    public string GetRecipePath(string recipeName)
    {
        var safeName = SanitizeFileName(recipeName);
        return Path.Combine(_basePath, $"{safeName}.json");
    }

    /// <summary>
    /// 加载配方管理器（配方列表）
    /// </summary>
    public RecipeManagerModel LoadRecipeManager()
    {
        try
        {
            if (!File.Exists(_managerFilePath))
            {
                var defaults = new RecipeManagerModel();
                SaveRecipeManager(defaults);
                return defaults;
            }

            var json = File.ReadAllText(_managerFilePath);
            return JsonConvert.DeserializeObject<RecipeManagerModel>(json, JsonSettings)
                   ?? new RecipeManagerModel();
        }
        catch
        {
            return new RecipeManagerModel();
        }
    }

    /// <summary>
    /// 保存配方管理器
    /// </summary>
    public void SaveRecipeManager(RecipeManagerModel manager)
    {
        var json = JsonConvert.SerializeObject(manager, JsonSettings);
        File.WriteAllText(_managerFilePath, json);
    }

    /// <summary>
    /// 加载指定配方的参数配置
    /// </summary>
    public ParameterRecipeModel LoadRecipe(string recipeName)
    {
        var path = GetRecipePath(recipeName);
        try
        {
            if (!File.Exists(path))
            {
                var defaults = new ParameterRecipeModel { RecipeName = recipeName };
                SaveRecipe(recipeName, defaults);
                return defaults;
            }

            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ParameterRecipeModel>(json, JsonSettings)
                   ?? new ParameterRecipeModel { RecipeName = recipeName };
        }
        catch
        {
            return new ParameterRecipeModel { RecipeName = recipeName };
        }
    }

    /// <summary>
    /// 保存指定配方的参数配置
    /// </summary>
    public void SaveRecipe(string recipeName, ParameterRecipeModel recipe)
    {
        recipe.RecipeName = recipeName;
        var json = JsonConvert.SerializeObject(recipe, JsonSettings);
        File.WriteAllText(GetRecipePath(recipeName), json);
    }

    /// <summary>
    /// 删除指定配方的参数文件
    /// </summary>
    public void DeleteRecipe(string recipeName)
    {
        var path = GetRecipePath(recipeName);
        if (File.Exists(path))
            File.Delete(path);
    }

    /// <summary>确保文件名安全</summary>
    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(name.Select(c => invalid.Contains(c) ? '_' : c).ToArray());
        return string.IsNullOrWhiteSpace(safe) ? "default" : safe;
    }
}
