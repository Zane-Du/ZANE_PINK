using System.IO;
using Newtonsoft.Json;
using ZANE.Models.Mes;

namespace ZANE.Services;

/// <summary>
/// 配置持久化服务 - 将 MES 参数配置存储为 JSON 文件
/// </summary>
public class ConfigService
{
    private readonly string _configFilePath;

    private static readonly JsonSerializerSettings JsonSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
    };

    public ConfigService()
    {
        var appData = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Configs");
        Directory.CreateDirectory(appData);
        _configFilePath = Path.Combine(appData, "MesParameterConfig.json");
    }

    /// <summary>
    /// 加载 MES 参数配置
    /// </summary>
    public MesParameterConfigModel Load()
    {
        try
        {
            if (!File.Exists(_configFilePath))
            {
                var defaults = CreateDefaultConfig();
                Save(defaults);
                return defaults;
            }

            var json = File.ReadAllText(_configFilePath);
            var config = JsonConvert.DeserializeObject<MesParameterConfigModel>(json, JsonSettings);
            return config ?? CreateDefaultConfig();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ConfigService] 加载配置异常: {ex.Message}");
            return CreateDefaultConfig();
        }
    }

    /// <summary>
    /// 保存 MES 参数配置
    /// </summary>
    public void Save(MesParameterConfigModel config)
    {
        try
        {
            var json = JsonConvert.SerializeObject(config, JsonSettings);
            File.WriteAllText(_configFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[ConfigService] 保存配置异常: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 创建默认配置（含默认 MES 接口列表）
    /// </summary>
    private static MesParameterConfigModel CreateDefaultConfig()
    {
        var config = new MesParameterConfigModel
        {
            MqttServiceInfo = new MqttServiceInfoModel
            {
                BrokerAddress = "10.0.0.2",
                Port = 1883,
                Topic = "/aiipc/MES",
                Interval = 10000,
            },
            MesInterfaceInfo = new MesInterfaceInfoModel
            {
                BaseAddress = "http://20.239.138.231",
                BaseAddress2 = "",
                LocalMesIP = "",
                Timeout = 5000,
                MesParameterItems = new System.Collections.ObjectModel.ObservableCollection<MesInterfacceInfoModel>
                {
                    new() { InterfaceName = "登陆",          BserUrlIndex = 1, Url = "/api/login" },
                    new() { InterfaceName = "原材料上料",     BserUrlIndex = 1, Url = "/api/material/load" },
                    new() { InterfaceName = "原材料卸料",     BserUrlIndex = 1, Url = "/api/material/unload" },
                    new() { InterfaceName = "产品进站检查",   BserUrlIndex = 1, Url = "/api/product/checkin" },
                    new() { InterfaceName = "产品过站检查",   BserUrlIndex = 1, Url = "/api/product/checkout" },
                    new() { InterfaceName = "设备防错点检",   BserUrlIndex = 1, Url = "/api/device/check" },
                    new() { InterfaceName = "跨工序参数获取", BserUrlIndex = 1, Url = "/api/process/param" },
                    new() { InterfaceName = "人员权限校验",   BserUrlIndex = 1, Url = "/api/user/auth" },
                    new() { InterfaceName = "IPQC首巡检",     BserUrlIndex = 1, Url = "/api/ipqc/check" },
                    new() { InterfaceName = "生产参数下发",   BserUrlIndex = 1, Url = "/api/production/param" },
                    new() { InterfaceName = "获取注液补偿值", BserUrlIndex = 1, Url = "/api/injection/compensation" },
                    new() { InterfaceName = "刷卡",           BserUrlIndex = 1, Url = "/api/card/swipe" },
                    new() { InterfaceName = "报警数据上传",   BserUrlIndex = 1, Url = "/api/alarm/upload" },
                    new() { InterfaceName = "设备状态上传",   BserUrlIndex = 1, Url = "/api/device/status" },
                    new() { InterfaceName = "能源数据上传",   BserUrlIndex = 1, Url = "/api/energy/upload" },
                },
            },
        };
        return config;
    }
}
