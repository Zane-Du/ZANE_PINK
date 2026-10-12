namespace ZANE.Models.Mes;

/// <summary>
/// MQTT 服务配置模型
/// </summary>
public class MqttServiceInfoModel
{
    public string BrokerAddress { get; set; } = "10.0.0.2";
    public int Port { get; set; } = 1883;
    public string Topic { get; set; } = string.Empty;
    public int Interval { get; set; } = 10000;
}
