using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;

namespace ZANE.Services;

/// <summary>
/// MQTT 通信帮助类
/// </summary>
public class MqttHelper : IDisposable
{
    private IMqttClient? _mqttClient;

    /// <summary>
    /// 发送 MQTT 消息
    /// </summary>
    public async Task SendMessageAsync(string brokerAddress, int port, string topic, string payload)
    {
        try
        {
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(brokerAddress, port)
                .WithCleanSession()
                .WithTimeout(TimeSpan.FromSeconds(5))
                .Build();

            await _mqttClient.ConnectAsync(options);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(Encoding.UTF8.GetBytes(payload))
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .WithRetainFlag(false)
                .Build();

            await _mqttClient.PublishAsync(message);

            await _mqttClient.DisconnectAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MqttHelper] 发送消息异常: {ex.Message}");
            throw;
        }
        finally
        {
            _mqttClient?.Dispose();
            _mqttClient = null;
        }
    }

    public void Dispose()
    {
        _mqttClient?.Dispose();
        _mqttClient = null;
    }
}
