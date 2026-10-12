namespace ZANE.Models.Mes;

/// <summary>
/// MES 参数配置（根模型，包含 MQTT 和 HTTP 接口配置）
/// </summary>
public class MesParameterConfigModel
{
    public MesInterfaceInfoModel MesInterfaceInfo { get; set; } = new();
    public MqttServiceInfoModel MqttServiceInfo { get; set; } = new();
}
