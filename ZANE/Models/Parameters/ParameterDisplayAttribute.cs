namespace ZANE.Models.Parameters;

/// <summary>
/// 参数显示特性 —— 标注在模型类的属性上，PanelBuilder 通过反射读取来动态生成 UI
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ParameterDisplayAttribute : Attribute
{
    /// <summary>显示名称</summary>
    public string Name { get; }
    /// <summary>所属分类: Device|Run|Function|Advanced</summary>
    public string Category { get; }
    /// <summary>描述/工具提示</summary>
    public string? Description { get; set; }
    /// <summary>单位后缀</summary>
    public string? Unit { get; set; }
    /// <summary>分组排序</summary>
    public int Order { get; set; } = 999;

    public ParameterDisplayAttribute(string name, string category)
    {
        Name = name;
        Category = category;
    }
}

/// <summary>
/// 可供 PanelBuilder 使用的参数元数据
/// </summary>
public class ControlDefinition
{
    public string PropertyName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Unit { get; set; }
    public int Order { get; set; } = 999;
    public Type PropertyType { get; set; } = typeof(string);
    public object? DefaultValue { get; set; }
    public bool IsEnum => PropertyType.IsEnum;
    public bool IsBool => PropertyType == typeof(bool);
}
