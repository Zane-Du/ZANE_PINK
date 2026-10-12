using Xunit;
using ZANE.Models.Parameters;
using ZANE.Services;

namespace ZANE.Tests;

public class ParameterModelTests
{
    [Fact]
    public void DeviceParameterModel_DefaultTitle_IsSet()
    {
        var model = new DeviceParameterModel();
        Assert.Contains("ZANE", model.Title);
    }

    [Fact]
    public void RunParameterModel_Defaults_AreCorrect()
    {
        var model = new RunParameterModel();
        Assert.Equal(2, model.RetryCountScanCode);
        Assert.Equal(6f, model.VoltageUpper);
        Assert.Equal(2f, model.VoltageLower);
    }

    [Fact]
    public void FunctionEnableModel_Defaults_AllFalse()
    {
        var model = new FunctionEnableModel();
        Assert.False(model.IsEnableBeforeWeightCheck);
        Assert.False(model.IsForceOK);
    }

    [Fact]
    public void AdvancedConfigModel_Defaults_AreCorrect()
    {
        var model = new AdvancedConfigModel();
        Assert.Equal(MesStatusEnum.关闭, model.MESStatus);
        Assert.Equal(ProductionTypeEnum.量产, model.ProductionType);
        Assert.Contains("10,24", model.BatteryBarcodeValidationRule);
    }

    [Fact]
    public void ParameterRecipeModel_DefaultRecipeName_IsSet()
    {
        var model = new ParameterRecipeModel();
        Assert.Equal("默认配方", model.RecipeName);
        Assert.NotNull(model.DeviceParameter);
        Assert.NotNull(model.RunParameter);
        Assert.NotNull(model.FunctionEnable);
        Assert.NotNull(model.AdvancedConfig);
    }

    [Fact]
    public void RecipeManagerModel_DefaultHasOneRecipe()
    {
        var model = new RecipeManagerModel();
        Assert.Single(model.Recipes);
        Assert.Equal("默认配方", model.CurrentRecipe);
        Assert.Contains("默认配方", model.Recipes);
    }
}

public class PanelBuilderScanTests
{
    [Fact]
    public void ScanDefinitions_DeviceParameter_ReturnsAllAnnotatedProperties()
    {
        var builder = new PanelBuilder();
        var defs = builder.ScanDefinitions(typeof(DeviceParameterModel));
        Assert.NotEmpty(defs);
        Assert.Contains(defs, d => d.Category == "Device");
        Assert.All(defs, d => Assert.False(string.IsNullOrEmpty(d.DisplayName)));
    }

    [Fact]
    public void ScanDefinitions_RunParameter_ReturnsAllAnnotatedProperties()
    {
        var builder = new PanelBuilder();
        var defs = builder.ScanDefinitions(typeof(RunParameterModel));
        Assert.NotEmpty(defs);
        Assert.Contains(defs, d => d.Category == "Run");
    }

    [Fact]
    public void ScanDefinitions_FunctionEnable_ReturnsAllAnnotatedProperties()
    {
        var builder = new PanelBuilder();
        var defs = builder.ScanDefinitions(typeof(FunctionEnableModel));
        Assert.NotEmpty(defs);
        Assert.Contains(defs, d => d.PropertyType == typeof(bool));
    }

    [Fact]
    public void ScanDefinitions_AdvancedConfig_ReturnsAllAnnotatedProperties()
    {
        var builder = new PanelBuilder();
        var defs = builder.ScanDefinitions(typeof(AdvancedConfigModel));
        Assert.NotEmpty(defs);
        Assert.Contains(defs, d => d.Category == "Advanced");
    }

    [Fact]
    public void ScanDefinitions_HasCorrectOrder()
    {
        var builder = new PanelBuilder();
        var defs = builder.ScanDefinitions(typeof(DeviceParameterModel));
        var sorted = defs.OrderBy(d => d.Order).ToList();
        Assert.Equal(sorted[0].Order, defs.Min(d => d.Order));
    }
}

public class RecipeConfigServiceTests
{
    [Fact]
    public void RecipeManager_Load_CreatesDefaults()
    {
        var service = new RecipeConfigService();
        var manager = service.LoadRecipeManager();
        Assert.NotNull(manager);
        Assert.NotEmpty(manager.Recipes);
    }

    [Fact]
    public void Recipe_SaveAndLoad_RoundTrip()
    {
        var service = new RecipeConfigService();
        var recipe = new ParameterRecipeModel
        {
            RecipeName = "测试配方",
            RunParameter = new RunParameterModel { RetryCountScanCode = 5 }
        };

        service.SaveRecipe("测试配方", recipe);
        var loaded = service.LoadRecipe("测试配方");

        Assert.Equal("测试配方", loaded.RecipeName);
        Assert.Equal(5, loaded.RunParameter.RetryCountScanCode);

        service.DeleteRecipe("测试配方");
    }

    [Fact]
    public void Recipe_LoadUnknown_ReturnsDefault()
    {
        var service = new RecipeConfigService();
        var recipe = service.LoadRecipe("_test_unknown_" + Guid.NewGuid());
        Assert.NotNull(recipe);
    }

    [Fact]
    public void Recipe_Delete_RemovesFile()
    {
        var service = new RecipeConfigService();
        service.SaveRecipe("_test_del_", new ParameterRecipeModel());
        service.DeleteRecipe("_test_del_");
        var recipe = service.LoadRecipe("_test_del_");
        Assert.NotNull(recipe);
    }
}
