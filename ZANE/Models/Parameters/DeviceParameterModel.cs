namespace ZANE.Models.Parameters;

/// <summary>
/// 基本（设备）参数
/// </summary>
public class DeviceParameterModel
{
    [ParameterDisplay("软件标题", "Device", Order = 1)]
    public string Title { get; set; } = "ZANE 控制系统";

    [ParameterDisplay("设备编码", "Device", Order = 2)]
    public string DeviceCode { get; set; } = string.Empty;

    [ParameterDisplay("站点编码", "Device", Order = 3)]
    public string SiteCode { get; set; } = string.Empty;

    [ParameterDisplay("车间", "Device", Order = 4)]
    public string WorkShop { get; set; } = string.Empty;

    [ParameterDisplay("产线编号", "Device", Order = 5)]
    public string LineName { get; set; } = string.Empty;

    [ParameterDisplay("工序编号", "Device", Order = 6)]
    public string ProcessOperationName { get; set; } = string.Empty;

    [ParameterDisplay("工作令", "Device", Order = 7)]
    public string WorkOrderNo { get; set; } = string.Empty;

    [ParameterDisplay("电解液批次", "Device", Order = 8)]
    public string ElectrolyteLotCode { get; set; } = string.Empty;

    [ParameterDisplay("胶钉批次", "Device", Order = 9)]
    public string GlueNailCode { get; set; } = string.Empty;

    [ParameterDisplay("MES登录Id", "Device", Order = 10)]
    public string SessionId { get; set; } = string.Empty;
}
