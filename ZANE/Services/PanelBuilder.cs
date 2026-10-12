using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HC = HandyControl.Controls;
using HDC = HandyControl.Data;

namespace ZANE.Services;

public class PanelBuilder
{
    private readonly Func<string, bool>? _permissionChecker;

    /// <param name="permissionChecker">
    /// 可选参数：属性名 → bool，返回 true 表示有编辑权限。
    /// 传 null 表示全部可编辑。
    /// </param>
    public PanelBuilder(Func<string, bool>? permissionChecker = null)
    {
        _permissionChecker = permissionChecker;
    }

    public StackPanel BuildPanel(object modelInstance, IEnumerable<Models.Parameters.ControlDefinition> definitions)
    {
        var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 0) };

        foreach (var def in definitions.OrderBy(d => d.Order))
        {
            var control = CreateControl(modelInstance, def);
            if (control != null)
                panel.Children.Add(control);
        }

        return panel;
    }

    /// <summary>
    /// 检查当前是否有编辑权限
    /// </summary>
    private bool HasEditPermission(string propertyName)
    {
        if (_permissionChecker == null) return true; // 无检查器 = 全部可编辑
        return _permissionChecker(propertyName);
    }

    public List<Models.Parameters.ControlDefinition> ScanDefinitions(Type modelType)
    {
        var list = new List<Models.Parameters.ControlDefinition>();
        var props = modelType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            var attr = prop.GetCustomAttribute<Models.Parameters.ParameterDisplayAttribute>();
            if (attr == null) continue;

            list.Add(new Models.Parameters.ControlDefinition
            {
                PropertyName = prop.Name,
                DisplayName = attr.Name,
                Category = attr.Category,
                Description = attr.Description,
                Unit = attr.Unit,
                Order = attr.Order,
                PropertyType = prop.PropertyType,
            });
        }

        return list;
    }

    private FrameworkElement? CreateControl(object modelInstance, Models.Parameters.ControlDefinition def)
    {
        var prop = modelInstance.GetType().GetProperty(def.PropertyName);
        if (prop == null) return null;

        bool canEdit = HasEditPermission(def.PropertyName);

        if (def.IsBool)
            return CreateCheckBox(modelInstance, prop, def, canEdit);

        if (def.IsEnum)
            return CreateComboBox(modelInstance, prop, def, canEdit);

        return CreateTextBox(modelInstance, prop, def, canEdit);
    }

    private FrameworkElement CreateTextBox(object modelInstance, PropertyInfo prop, Models.Parameters.ControlDefinition def, bool canEdit)
    {
        var textBox = new HC.TextBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 4, 5, 4),
            Height = 32,
            IsEnabled = canEdit,
        };

        HC.TitleElement.SetTitle(textBox, def.DisplayName);
        HC.TitleElement.SetTitlePlacement(textBox, HDC.TitlePlacementType.Left);
        HC.TitleElement.SetTitleWidth(textBox, new GridLength(140));

        var binding = new Binding(def.PropertyName)
        {
            Source = modelInstance,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.LostFocus,
        };
        textBox.SetBinding(HC.TextBox.TextProperty, binding);

        return textBox;
    }

    private FrameworkElement CreateCheckBox(object modelInstance, PropertyInfo prop, Models.Parameters.ControlDefinition def, bool canEdit)
    {
        var checkBox = new System.Windows.Controls.CheckBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 8, 5, 8),
            Content = def.DisplayName,
            IsEnabled = canEdit,
        };

        var binding = new Binding(def.PropertyName)
        {
            Source = modelInstance,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        };
        checkBox.SetBinding(System.Windows.Controls.CheckBox.IsCheckedProperty, binding);

        return checkBox;
    }

    private FrameworkElement CreateComboBox(object modelInstance, PropertyInfo prop, Models.Parameters.ControlDefinition def, bool canEdit)
    {
        var comboBox = new HC.ComboBox
        {
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 4, 5, 4),
            Height = 32,
            IsEnabled = canEdit,
        };

        HC.TitleElement.SetTitle(comboBox, def.DisplayName);
        HC.TitleElement.SetTitlePlacement(comboBox, HDC.TitlePlacementType.Left);
        HC.TitleElement.SetTitleWidth(comboBox, new GridLength(140));

        var enumValues = Enum.GetValues(def.PropertyType);
        foreach (var val in enumValues)
            comboBox.Items.Add(val);

        var binding = new Binding(def.PropertyName)
        {
            Source = modelInstance,
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
        };
        comboBox.SetBinding(System.Windows.Controls.ComboBox.SelectedItemProperty, binding);

        return comboBox;
    }
}
