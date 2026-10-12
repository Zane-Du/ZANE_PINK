namespace ZANE.Models.Parameters;

/// <summary>
/// 运行参数
/// </summary>
public class RunParameterModel
{
    [ParameterDisplay("扫码重试次数", "Run", Order = 1)]
    public int RetryCountScanCode { get; set; } = 2;

    [ParameterDisplay("注液标准(g)", "Run", Order = 2, Unit = "g")]
    public float InjectionStandard { get; set; }

    [ParameterDisplay("注液上限(g)", "Run", Order = 3, Unit = "g")]
    public float InjectionUpper { get; set; }

    [ParameterDisplay("注液下限(g)", "Run", Order = 4, Unit = "g")]
    public float InjectionLower { get; set; }

    [ParameterDisplay("前称重上限(g)", "Run", Order = 5, Unit = "g")]
    public float BeforeWeightUpper { get; set; }

    [ParameterDisplay("前称重下限(g)", "Run", Order = 6, Unit = "g")]
    public float BeforeWeightLower { get; set; }

    [ParameterDisplay("后称重上限(g)", "Run", Order = 7, Unit = "g")]
    public float AfterWeightUpper { get; set; }

    [ParameterDisplay("后称重下限(g)", "Run", Order = 8, Unit = "g")]
    public float AfterWeightLower { get; set; }

    [ParameterDisplay("电压上限(V)", "Run", Order = 9, Unit = "V")]
    public float VoltageUpper { get; set; } = 6;

    [ParameterDisplay("电压下限(V)", "Run", Order = 10, Unit = "V")]
    public float VoltageLower { get; set; } = 2;

    [ParameterDisplay("温度上限(℃)", "Run", Order = 11, Unit = "℃")]
    public float TemperatureUpper { get; set; } = 100;

    [ParameterDisplay("温度下限(℃)", "Run", Order = 12, Unit = "℃")]
    public float TemperatureLower { get; set; } = 0;

    [ParameterDisplay("称重超时(ms)", "Run", Order = 13, Unit = "ms")]
    public int WeighingTimeout { get; set; } = 1000;

    [ParameterDisplay("MES心跳间隔(ms)", "Run", Order = 14, Unit = "ms")]
    public int MesHeartbeatInterval { get; set; } = 3000;

    [ParameterDisplay("PLC心跳间隔(ms)", "Run", Order = 15, Unit = "ms")]
    public int PLCHeartbeatInterval { get; set; } = 1000;

    [ParameterDisplay("短路测试上限1(MΩ)", "Run", Order = 16, Unit = "MΩ")]
    public float SpotCheckResistanceUpper1 { get; set; }

    [ParameterDisplay("短路测试下限1(MΩ)", "Run", Order = 17, Unit = "MΩ")]
    public float SpotCheckResistanceLower1 { get; set; }

    [ParameterDisplay("短路测试上限2(MΩ)", "Run", Order = 18, Unit = "MΩ")]
    public float SpotCheckResistanceUpper2 { get; set; }

    [ParameterDisplay("短路测试下限3(MΩ)", "Run", Order = 19, Unit = "MΩ")]
    public float SpotCheckResistanceLower3 { get; set; }

    [ParameterDisplay("短路测试下限4(MΩ)", "Run", Order = 17, Unit = "MΩ")]
    public float SpotCheckResistanceLower4 { get; set; }

    [ParameterDisplay("短路测试上限5(MΩ)", "Run", Order = 18, Unit = "MΩ")]
    public float SpotCheckResistanceUpper5 { get; set; }

    [ParameterDisplay("短路测试下限6(MΩ)", "Run", Order = 19, Unit = "MΩ")]
    public float SpotCheckResistanceLower6 { get; set; }
}
