using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using ZANE.Models.Mes;
using ZANE.Services;

namespace ZANE.ViewModels;

public partial class MESTestViewModel : ObservableObject
{
    private readonly ConfigService _configService;
    private readonly MqttHelper _mqttHelper;
    private readonly MesService _mesService;

    // ─────────────── 原始配置（已持久化的） ───────────────
    private MesParameterConfigModel _savedConfig = new();

    // ─────────────── 可编辑副本 ───────────────
    [ObservableProperty]
    private MesParameterConfigModel _mesParameterCopy = new();

    // ─────────────── MQTT 测试区 ───────────────
    [ObservableProperty]
    private string _mqttTestInterfaceName = "";

    [ObservableProperty]
    private string _mqttTestUrl = "";

    [ObservableProperty]
    private string _mqttTestSendTxt = "";

    // ─────────────── MES 测试区 ───────────────
    [ObservableProperty]
    private string _testInterfaceName = "";

    [ObservableProperty]
    private string _testUrl = "";

    [ObservableProperty]
    private string _testUser = "tlszx055";

    [ObservableProperty]
    private string _sendTxt = "";

    [ObservableProperty]
    private string _receiveTxt = "";

    [ObservableProperty]
    private bool _isSending;

    // 当前选中的 MES 接口（用于发送测试）
    private MesInterfacceInfoModel? _selectedMesInterface;

    public List<byte> BaseIndexs { get; } = [0, 1, 2];

    public MESTestViewModel()
    {
        _configService = new ConfigService();
        _mqttHelper = new MqttHelper();
        _mesService = new MesService();

        LoadConfig();
    }

    // ═══════════════════════════════════════════
    //  配置加载 / 保存
    // ═══════════════════════════════════════════

    /// <summary>
    /// 加载配置到可编辑副本
    /// </summary>
    private void LoadConfig()
    {
        _savedConfig = _configService.Load();
        DeepCopyToCopy();
    }

    private void DeepCopyToCopy()
    {
        var json = JsonConvert.SerializeObject(_savedConfig);
        MesParameterCopy = JsonConvert.DeserializeObject<MesParameterConfigModel>(json) ?? new MesParameterConfigModel();
    }

    /// <summary>
    /// 保存配置（同步到原始配置并持久化）
    /// </summary>
    [RelayCommand]
    private void SaveConfig()
    {
        try
        {
            // 深拷贝副本到原始
            var json = JsonConvert.SerializeObject(MesParameterCopy);
            _savedConfig = JsonConvert.DeserializeObject<MesParameterConfigModel>(json) ?? new MesParameterConfigModel();
            _configService.Save(_savedConfig);
            Growl.Success("配置保存成功！");
        }
        catch (Exception ex)
        {
            Growl.Error($"保存失败：{ex.Message}");
        }
    }

    // ═══════════════════════════════════════════
    //  MQTT 测试
    // ═══════════════════════════════════════════

    /// <summary>
    /// 点击 MQTT 测试按钮：将当前 MQTT topic 信息填入测试区
    /// </summary>
    [RelayCommand]
    private void MqttTest()
    {
        MqttTestInterfaceName = "MQTT Topic";
        MqttTestUrl = MesParameterCopy.MqttServiceInfo.Topic;
        MqttTestSendTxt = "";
        Growl.Info("已加载 MQTT 配置到测试区，请填写发送内容后点击发送");
    }

    /// <summary>
    /// 发送 MQTT 测试消息
    /// </summary>
    [RelayCommand]
    private async Task SendMqttMessage()
    {
        if (string.IsNullOrWhiteSpace(MqttTestSendTxt))
        {
            Growl.Warning("请输入要发送的消息内容！");
            return;
        }

        IsSending = true;
        try
        {
            var mqtt = MesParameterCopy.MqttServiceInfo;
            await _mqttHelper.SendMessageAsync(mqtt.BrokerAddress, mqtt.Port, mqtt.Topic, MqttTestSendTxt);
            Growl.Success("MQTT 消息发送成功！");
        }
        catch (Exception ex)
        {
            Growl.Error($"MQTT 发送失败：{ex.Message}");
        }
        finally
        {
            IsSending = false;
        }
    }

    // ═══════════════════════════════════════════
    //  MES 接口测试
    // ═══════════════════════════════════════════

    /// <summary>
    /// 点击 DataGrid 中某接口的测试按钮
    /// </summary>
    [RelayCommand]
    private void MesInterfaceTest(MesInterfacceInfoModel? mesInterface)
    {
        if (mesInterface == null) return;

        _selectedMesInterface = mesInterface;
        TestInterfaceName = mesInterface.InterfaceName;

        // 根据 BserUrlIndex 拼接完整 URL
        TestUrl = mesInterface.BserUrlIndex switch
        {
            1 => $"{MesParameterCopy.MesInterfaceInfo.BaseAddress}{mesInterface.Url}",
            2 => $"{MesParameterCopy.MesInterfaceInfo.BaseAddress2}{mesInterface.Url}",
            _ => mesInterface.Url
        };

        // 生成测试请求 JSON
        SendTxt = _mesService.GetTestRequestJson(mesInterface.InterfaceName, TestUser);

        Growl.Info($"已加载接口 [{mesInterface.InterfaceName}] 的测试数据");
    }

    /// <summary>
    /// 发送 MES 请求
    /// </summary>
    [RelayCommand]
    private async Task SendMesRequest()
    {
        if (string.IsNullOrWhiteSpace(SendTxt))
        {
            Growl.Warning("请输入要发送的请求数据！");
            return;
        }

        if (_selectedMesInterface == null)
        {
            Growl.Warning("请先在接口列表中点击某个接口的「测试」按钮！");
            return;
        }

        IsSending = true;
        ReceiveTxt = "";
        try
        {
            var timeout = MesParameterCopy.MesInterfaceInfo.Timeout;


            if (TestInterfaceName == "MQTT Topic")
            {
                // MQTT 发送
                var mqtt = MesParameterCopy.MqttServiceInfo;
                await _mqttHelper.SendMessageAsync(mqtt.BrokerAddress, mqtt.Port, mqtt.Topic, SendTxt);
                ReceiveTxt = "MQTT 消息已发送（无返回数据）";
            }
            else
            {
                // HTTP 发送
                var result = await _mesService.SendRequestAsync(TestUrl, SendTxt, timeout);
                ReceiveTxt = result;
            }

            Growl.Success("请求已发送！");
        }
        catch (Exception ex)
        {
            ReceiveTxt = $"{{\"error\": \"{ex.Message}\"}}";
            Growl.Error($"请求失败：{ex.Message}");
        }
        finally
        {
            IsSending = false;
        }
    }

    /// <summary>
    /// 格式化 JSON 文本（美化）
    /// </summary>
    [RelayCommand]
    private void FormatJson()
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(SendTxt))
            {
                var obj = JsonConvert.DeserializeObject(SendTxt);
                SendTxt = JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
        }
        catch
        {
            Growl.Warning("JSON 格式无效");
        }
    }
}
