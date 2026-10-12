namespace ZANE.Models.Parameters;

/// <summary>
/// 高级设置
/// </summary>
public class AdvancedConfigModel
{
    [ParameterDisplay("MES状态", "Advanced", Order = 1)]
    public MesStatusEnum MESStatus { get; set; } = MesStatusEnum.关闭;



    [ParameterDisplay("生产类型", "Advanced", Order = 2)]
    public ProductionTypeEnum ProductionType { get; set; } = ProductionTypeEnum.量产;



    [ParameterDisplay("电芯条码验证规则", "Advanced", Order = 3)]
    public string BatteryBarcodeValidationRule { get; set; } = "^[a-zA-Z0-9]{10,24}$";



    [ParameterDisplay("托盘条码验证规则", "Advanced", Order = 4)]
    public string TrayBarcodeValidationRule { get; set; } = "^[a-zA-Z0-9]{10,24}$";

    [ParameterDisplay("SQL超时打印(秒)", "Advanced", Order = 5, Unit = "s")]
    public float SqlOutTime { get; set; } = 0.6f;

    [ParameterDisplay("日志打印所有SQL", "Advanced", Order = 6)]
    public bool PrintAllSql { get; set; }

    [ParameterDisplay("点检电芯1", "Advanced", Order = 7)]
    public string SpotCode1 { get; set; } = string.Empty;

    [ParameterDisplay("点检电芯2", "Advanced", Order = 8)]
    public string SpotCode2 { get; set; } = string.Empty;

    [ParameterDisplay("点检电芯3", "Advanced", Order = 9)]
    public string SpotCode3 { get; set; } = string.Empty;

    [ParameterDisplay("点检电芯4", "Advanced", Order = 10)]
    public string SpotCode4 { get; set; } = string.Empty;
}

/// <summary>
/// MES 状态枚举
/// </summary>
public enum MesStatusEnum
{
    关闭,
    启用
}

/// <summary>
/// 生产类型枚举
/// </summary>
public enum ProductionTypeEnum
{
    量产,
    试产,
    返工
}
