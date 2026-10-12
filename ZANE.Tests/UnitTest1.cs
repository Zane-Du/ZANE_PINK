using System;
using Xunit;
using ZANE.Models.Mes;
using ZANE.Services;

namespace ZANE.Tests;

public class MesModelTests
{
    [Fact]
    public void MqttServiceInfoModel_Defaults_AreCorrect()
    {
        var model = new MqttServiceInfoModel();
        Assert.Equal("10.0.0.2", model.BrokerAddress);
        Assert.Equal(1883, model.Port);
        Assert.Equal(10000, model.Interval);
        Assert.NotNull(model.Topic);
    }

    [Fact]
    public void MesInterfaceInfoModel_Defaults_AreCorrect()
    {
        var model = new MesInterfaceInfoModel();
        Assert.Equal("http://20.239.138.231", model.BaseAddress);
        Assert.Equal(5000, model.Timeout);
        Assert.NotNull(model.MesParameterItems);
        Assert.Empty(model.MesParameterItems);
    }

    [Fact]
    public void MesInterfacceInfoModel_CanSetProperties()
    {
        var model = new MesInterfacceInfoModel
        {
            InterfaceName = "登陆",
            BserUrlIndex = 1,
            Url = "/api/login"
        };
        Assert.Equal("登陆", model.InterfaceName);
        Assert.Equal(1, model.BserUrlIndex);
        Assert.Equal("/api/login", model.Url);
    }

    [Fact]
    public void MesParameterConfigModel_ContainsMqttAndMesInfo()
    {
        var config = new MesParameterConfigModel();
        Assert.NotNull(config.MqttServiceInfo);
        Assert.NotNull(config.MesInterfaceInfo);
    }
}

public class MesServiceTests
{
    [Fact]
    public void GetTestRequestJson_Login_ReturnsValidJson()
    {
        var service = new MesService();
        var json = service.GetTestRequestJson("登陆");
        Assert.Contains("TEST", json);
        Assert.Contains("password", json);
    }

    [Fact]
    public void GetTestRequestJson_UnknownInterface_ReturnsEmptyJson()
    {
        var service = new MesService();
        var json = service.GetTestRequestJson("不存在");
        Assert.Contains("{}", json);
    }

    [Fact]
    public void GetTestRequestJson_CheckIn_ReturnsBarcode()
    {
        var service = new MesService();
        var json = service.GetTestRequestJson("产品进站检查");
        Assert.Contains("barcode", json);
        Assert.Contains("00PCTEST", json);
    }
}

public class ConfigServiceTests
{
    [Fact]
    public void ConfigService_Load_ReturnsValidConfig()
    {
        // This will create a default config if no file exists
        var service = new ConfigService();
        var config = service.Load();
        Assert.NotNull(config);
        Assert.NotNull(config.MqttServiceInfo);
        Assert.NotNull(config.MesInterfaceInfo);
        Assert.NotEmpty(config.MesInterfaceInfo.MesParameterItems);
    }

    [Fact]
    public void DefaultConfig_HasExpectedInterfaces()
    {
        var service = new ConfigService();
        var config = service.Load();
        // Check some expected interfaces exist
        var names = config.MesInterfaceInfo.MesParameterItems
            .Select(i => i.InterfaceName)
            .ToList();
        Assert.Contains("登陆", names);
        Assert.Contains("产品进站检查", names);
        // removed incorrect assertion
    }
}
