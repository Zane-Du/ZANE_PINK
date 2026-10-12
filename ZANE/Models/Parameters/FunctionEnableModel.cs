namespace ZANE.Models.Parameters;

/// <summary>
/// 功能启用开关
/// </summary>
public class FunctionEnableModel
{
    [ParameterDisplay("启用前称重检测", "Function", Order = 1)]
    public bool IsEnableBeforeWeightCheck { get; set; }

    [ParameterDisplay("启用后称重检测", "Function", Order = 2)]
    public bool IsEnableAfterWeightCheck { get; set; }

    [ParameterDisplay("启用注液量检测", "Function", Order = 3)]
    public bool IsEnableInjectionCheck { get; set; }

    [ParameterDisplay("启用温度检测", "Function", Order = 4)]
    public bool IsEnableTemperatureCheck { get; set; }

    [ParameterDisplay("启用虚拟码", "Function", Order = 5)]
    public bool IsEnablePseudoCode { get; set; }

    [ParameterDisplay("空载模式", "Function", Order = 6)]
    public bool IsEmptyLoadMode { get; set; }

    [ParameterDisplay("强制结果合格", "Function", Order = 7)]
    public bool IsForceOK { get; set; }

    [ParameterDisplay("启用电芯条码校验", "Function", Order = 8)]
    public bool IsEnableCellBarcodeVerification { get; set; }

    [ParameterDisplay("启用刷卡模式", "Function", Order = 9)]
    public bool EnableCardLoginMode { get; set; }

    [ParameterDisplay("复用旧数据", "Function", Order = 10)]
    public bool IsEnableReuseOldData { get; set; }
}
