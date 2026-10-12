using System.Collections.ObjectModel;

namespace ZANE.Models.Mes;

/// <summary>
/// MES 接口配置模型（HTTP 接口集合）
/// </summary>
public class MesInterfaceInfoModel
{
    public string BaseAddress { get; set; } = "http://20.239.138.231";
    public string BaseAddress2 { get; set; } = string.Empty;
    public string LocalMesIP { get; set; } = string.Empty;
    public int Timeout { get; set; } = 5000;
    public ObservableCollection<MesInterfacceInfoModel> MesParameterItems { get; set; } = new();
}
