namespace ZANE.Models.Mes;

/// <summary>
/// MES 接口信息模型（单个接口）
/// </summary>
public class MesInterfacceInfoModel
{
    public string InterfaceName { get; set; } = string.Empty;
    public byte BserUrlIndex { get; set; } = 1;
    public string Url { get; set; } = string.Empty;
}
