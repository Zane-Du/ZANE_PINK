using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace ZANE.Services;

public class MesService : IDisposable
{
    private readonly HttpClient _httpClient;

    public MesService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<string> SendRequestAsync(string url, string jsonContent, int timeoutMs = 5000)
    {
        try
        {
            _httpClient.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            try
            {
                var obj = JsonConvert.DeserializeObject(result);
                if (obj != null)
                    result = JsonConvert.SerializeObject(obj, Formatting.Indented);
            }
            catch { }

            return result;
        }
        catch (TaskCanceledException)
        {
            return "{\"error\": \"请求超时\"}";
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"{ex.Message}\"}}";
        }
    }

    public string GetTestRequestJson(string interfaceName, string testUser = "tlszx055")
    {
        var json = interfaceName switch
        {
            "登陆" => "{\"UserName\":\"TEST\",\"password\":\"Aa@12345\",\"type\":\"常规登陆\"}",
            "原材料上料" or "原材料卸料" => "{\"materialCode\":\"testMaterialCode\",\"materialLotCode\":\"testMaterialLotCode\",\"qty\":1,\"type\":1}",
            "产品进站检查" => "{\"barcode\":\"00PCTEST202602120001\"}",
            "产品过站检查" => "{\"battery\":{\"barcode\":\"00PCTEST202602120001\"}}",
            "设备防错点检" => "{\"code\":\"testBarcode\",\"name\":\"testSN\",\"value\":\"3000\",\"result\":\"0\",\"unit\":\"g\"}",
            "跨工序参数获取" => "{\"barcode\":\"testBarcode\"}",
            "人员权限校验" => "{\"UserName\":\"" + testUser + "\"}",
            "IPQC首巡检" => "{}",
            "生产参数下发" => "{}",
            "获取注液补偿值" => "{\"battery\":{\"barcode\":\"TestCode\"}}",
            "刷卡" => "{\"UserName\":\"" + testUser + "\"}",
            "报警数据上传" => "{\"alarmCode\":\"ALM001\",\"message\":\"Test Alarm\"}",
            "设备状态上传" => "{\"deviceId\":\"DEV001\",\"status\":\"running\"}",
            "能源数据上传" => "{\"power\":100.5,\"voltage\":380.0}",
            _ => "{}"
        };

        try
        {
            var obj = JsonConvert.DeserializeObject(json);
            if (obj != null)
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }
        catch { }

        return json;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
}
