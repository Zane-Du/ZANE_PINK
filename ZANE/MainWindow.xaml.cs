using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using ZANE.Logging;
using ZANE.Logging.Enums;
using ZANE.Models;
using ZANE.ViewModels;
using System;

namespace ZANE;

public partial class MainWindow : Window
{
    private bool _isDraggingSplitter;
    private Rect _normalRect;

    public MainWindow()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            var handle = new WindowInteropHelper(this).Handle;
            var source = HwndSource.FromHwnd(handle);
            source?.AddHook(WindowProc);

            // 启动时记录当前非最大化位置
            _normalRect = new Rect(Left, Top, Width, Height);

            // 启动后立即最大化
            _isFakeMaximized = true;
            WindowState = WindowState.Maximized;
            MaximizeIcon.Text = "❐";

            // 注册右下角弹窗提示
            Log4NetHelper.PromptAction += ShowNotification;
        };
    }

    /// <summary>
    /// Win32 WndProc — 拦截 WM_NCCALCSIZE，最大化时告诉系统不要非客户区（去掉透明边框）
    /// </summary>
    private IntPtr WindowProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_NCCALCSIZE = 0x0083;
        const int WM_GETMINMAXINFO = 0x0024;

        if (msg == WM_NCCALCSIZE && wParam != IntPtr.Zero && _isFakeMaximized)
        {
            // wParam != 0 时 lParam 指向 NCCALCSIZE_PARAMS
            // 直接返回 0 表示 "客户区 = 整个窗口"，砍掉所有非客户区
            handled = true;
            return IntPtr.Zero;
        }

        if (msg == WM_GETMINMAXINFO && _isFakeMaximized)
        {
            // 防止 WPF 在最大化时调整大小（补全工作区）
            var workArea = SystemParameters.WorkArea;
            var monitor = System.Windows.Forms.Screen.FromHandle(hwnd).WorkingArea;

            // 告诉系统最大化尺寸 = 工作区尺寸，不要留边框位置
            var mmi = (MINMAXINFO)Marshal.PtrToStructure(lParam, typeof(MINMAXINFO))!;
            mmi.ptMaxPosition.x = monitor.Left;
            mmi.ptMaxPosition.y = monitor.Top;
            mmi.ptMaxSize.x = monitor.Width;
            mmi.ptMaxSize.y = monitor.Height;
            Marshal.StructureToPtr(mmi, lParam, false);

            handled = true;
            return IntPtr.Zero;
        }

        return IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    private void NavItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is Border border && border.DataContext is NavigationItem navItem)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.NavigateToCommand.Execute(navItem);
            }
        }
    }

    // ====== 工具栏拖拽窗口 ======

    private void Toolbar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
        {
            DragMove();
        }
    }

    private void TopMenuButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.DataContext is TopMenuItem menuItem)
        {
            if (menuItem.SubItems == null || menuItem.SubItems.Count == 0)
                return;

            ShowDropdownMenu(button, menuItem.SubItems);
        }
    }

    /// <summary>
    /// 用 Popup + ListBox 实现自定义下拉菜单，完全脱离 MenuItem
    /// </summary>
    private void ShowDropdownMenu(Button anchor, List<TopMenuSubItem> items)
    {
        // 创建 Popup
        var popup = CreateDropdownPopup(anchor);

        // 创建 ListBox
        var listBox = new ListBox();
        listBox.Background = Brushes.Transparent;
        listBox.BorderThickness = new Thickness(0);
        listBox.Padding = new Thickness(0);
        listBox.Margin = new Thickness(0);
        listBox.MinWidth = 200;
        listBox.MaxHeight = 400;
        listBox.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        listBox.SelectionMode = SelectionMode.Single;
        listBox.Focusable = true;

        // 每项模板
        listBox.ItemTemplate = CreateDropdownItemTemplate();
        listBox.ItemsSource = items;

        // 点击项
        listBox.SelectionChanged += (s, args) =>
        {
            if (listBox.SelectedItem is TopMenuSubItem selected)
            {
                popup.IsOpen = false;
                selected.Command?.Invoke();
            }
        };

        // 失去焦点时关闭
        void onLostFocus(object? s, EventArgs args)
        {
            popup.IsOpen = false;
        }
        listBox.LostFocus += onLostFocus;

        // 点外面也关闭
        popup.Closed += (s, args) =>
        {
            listBox.LostFocus -= onLostFocus;
        };

        popup.Child = new Border
        {
            Background = TryFindResource("ContentBackgroundBrush") as Brush
                         ?? new SolidColorBrush(Color.FromRgb(30, 30, 30)),
            BorderBrush = TryFindResource("BorderBrush") as Brush
                          ?? new SolidColorBrush(Color.FromRgb(60, 60, 60)),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(4),
            Child = listBox,
            Effect = new System.Windows.Media.Effects.DropShadowEffect
            {
                BlurRadius = 12,
                Color = Colors.Black,
                Opacity = 0.3,
                Direction = 270
            }
        };

        popup.IsOpen = true;

        // 聚焦 ListBox 让键盘导航生效
        listBox.Focus();
    }

    /// <summary>
    /// 创建 Popup，定位到按钮下方
    /// </summary>
    private static Popup CreateDropdownPopup(Button anchor)
    {
        var popup = new Popup();
        popup.PlacementTarget = anchor;
        popup.Placement = PlacementMode.Bottom;
        popup.HorizontalOffset = 0;
        popup.VerticalOffset = 4;
        popup.StaysOpen = false;        // 点击外面自动关闭
        popup.PopupAnimation = PopupAnimation.Fade;
        popup.AllowsTransparency = true;
        return popup;
    }

    /// <summary>
    /// 下拉项的 DataTemplate（纯代码构建）
    /// </summary>
    private static DataTemplate CreateDropdownItemTemplate()
    {
        var template = new DataTemplate(typeof(TopMenuSubItem));

        // 最外层 Border — 圆角、悬停背景
        var borderFactory = new FrameworkElementFactory(typeof(Border));
        borderFactory.SetValue(Border.CursorProperty, Cursors.Hand);
        borderFactory.SetValue(Border.PaddingProperty, new Thickness(8, 6, 12, 6));
        borderFactory.SetValue(Border.MarginProperty, new Thickness(0));
        borderFactory.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));

        // 内部 Grid — 图标 + 文字
        var gridFactory = new FrameworkElementFactory(typeof(Grid));

        var iconText = new FrameworkElementFactory(typeof(TextBlock));
        iconText.SetValue(TextBlock.TextProperty, new Binding("Icon"));
        iconText.SetValue(TextBlock.FontSizeProperty, 15.0);
        iconText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        iconText.SetValue(TextBlock.WidthProperty, 22.0);
        iconText.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        var labelText = new FrameworkElementFactory(typeof(TextBlock));
        labelText.SetValue(TextBlock.TextProperty, new Binding("Text"));
        labelText.SetValue(TextBlock.FontSizeProperty, 13.0);
        labelText.SetValue(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center);
        labelText.SetValue(TextBlock.MarginProperty, new Thickness(8, 0, 0, 0));
        labelText.SetResourceReference(TextBlock.ForegroundProperty, "TextPrimaryBrush");

        // 用 DockPanel 比 Grid 简单
        var dockFactory = new FrameworkElementFactory(typeof(DockPanel));
        dockFactory.AppendChild(iconText);
        dockFactory.AppendChild(labelText);

        gridFactory.AppendChild(dockFactory);
        borderFactory.AppendChild(gridFactory);
        template.VisualTree = borderFactory;

        // 悬停高亮
        var trigger = new Trigger();
        trigger.Property = Border.IsMouseOverProperty;
        trigger.Value = true;
        trigger.Setters.Add(new Setter(Border.BackgroundProperty, new DynamicResourceExtension("NavHoverBackgroundBrush")));
        template.Triggers.Add(trigger);

        return template;
    }

    private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    // ====== Win32 helpers ======
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private static readonly IntPtr HWND_TOP = IntPtr.Zero;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_SHOWWINDOW = 0x0040;
    private const int SW_SHOWMAXIMIZED = 3;
    private const int SW_RESTORE = 9;

    private bool _isFakeMaximized;

    private void MaximizeBtn_Click(object sender, RoutedEventArgs e)
    {
        var handle = new WindowInteropHelper(this).Handle;

        if (_isFakeMaximized)
        {
            // 还原到之前记录的位置
            _isFakeMaximized = false;
            WindowState = WindowState.Normal;
            if (!_normalRect.IsEmpty && _normalRect.Width > 0 && _normalRect.Height > 0)
            {
                Left = _normalRect.Left;
                Top = _normalRect.Top;
                Width = _normalRect.Width;
                Height = _normalRect.Height;
            }
            MaximizeIcon.Text = "☐";
        }
        else
        {
            if (WindowState == WindowState.Normal)
            {
                _normalRect = new Rect(Left, Top, Width, Height);
            }

            _isFakeMaximized = true;
            var workArea = SystemParameters.WorkArea;

            // 让 WPF 走正常最大化流程，WM_GETMINMAXINFO / WM_NCCALCSIZE 会修正尺寸和边框
            WindowState = WindowState.Maximized;

            MaximizeIcon.Text = "❐";
        }
    }

    protected override void OnStateChanged(EventArgs e)
    {
        base.OnStateChanged(e);

        if (WindowState == WindowState.Maximized)
        {
            if (!_isFakeMaximized)
            {
                // 记录双击标题栏之类的系统最大化场景
                _isFakeMaximized = true;
                if (!_normalRect.IsEmpty && _normalRect.Width > 0 && _normalRect.Height > 0)
                {
                    // 用当前正常状态下的位置覆盖记录
                    _normalRect = new Rect(Left, Top, Width, Height);
                }
            }
            MaximizeIcon.Text = "❐";
        }
        else if (WindowState == WindowState.Normal && !_isFakeMaximized)
        {
            MaximizeIcon.Text = "☐";
        }
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
    }

    // ====== 导航栏分隔条拖拽 ======

    private void Splitter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isDraggingSplitter = true;
        SplitterBorder.CaptureMouse();
        Cursor = Cursors.SizeWE;
    }

    private void Splitter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isDraggingSplitter) return;
        _isDraggingSplitter = false;
        SplitterBorder.ReleaseMouseCapture();
        Cursor = null;
    }

    private void Splitter_MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDraggingSplitter) return;
        if (DataContext is not MainViewModel vm) return;

        var newWidth = e.GetPosition(this).X;
        newWidth = Math.Max(150, Math.Min(500, newWidth));

        vm.SidebarWidth = new GridLength(newWidth);
    }

    // ====== 右下角通知弹窗（队列式，多条堆叠，各自独立计时） ======

    /// <summary>
    /// 通知数据模型
    /// </summary>
    private class NotificationItem
    {
        public Border? Border { get; set; }
        public TextBlock? Icon { get; set; }
        public TextBlock? Time { get; set; }
        public TextBlock? Text { get; set; }
    }

    /// <summary>
    /// 接收 Log4NetHelper.PromptAction 回调，显示右下角通知
    /// </summary>
    private void ShowNotification(string message, Log4NetLevelEnum level)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => ShowNotification(message, level));
            return;
        }

        try
        {
            // 构建新的通知项 UI
            var border = new Border
            {
                Width = 380,
                Padding = new Thickness(14, 10, 14, 10),
                Margin = new Thickness(0, 0, 0, 8),
                CornerRadius = new CornerRadius(8),
                BorderThickness = new Thickness(1),
                Opacity = 0,
                Background = (Brush)FindResource("ContentBackgroundBrush"),
                BorderBrush = (Brush)FindResource("BorderBrush"),
            };

            var icon = new Border
            {
                Width = 24,
                Height = 24,
                CornerRadius = new CornerRadius(4),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0),
                Child = new TextBlock
                {
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };

            var iconText = (TextBlock)icon.Child;

            var timeText = new TextBlock
            {
                FontSize = 11,
                Foreground = (Brush)FindResource("TextSecondaryBrush"),
                Margin = new Thickness(0, 0, 0, 4),
                Text = DateTime.Now.ToString("HH:mm:ss"),
            };

            var textBlock = new TextBlock
            {
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                Foreground = (Brush)FindResource("TextPrimaryBrush"),
                VerticalAlignment = VerticalAlignment.Center,
                MaxHeight = 60,
                Text = message,
            };

            switch (level)
            {
                case Log4NetLevelEnum.错误:
                    iconText.Text = "✕";
                    iconText.Foreground = Brushes.White;
                    icon.Background = new SolidColorBrush(Color.FromRgb(231, 76, 60));
                    break;
                case Log4NetLevelEnum.警告:
                    iconText.Text = "⚠";
                    iconText.Foreground = Brushes.White;
                    icon.Background = new SolidColorBrush(Color.FromRgb(243, 156, 18));
                    break;
                case Log4NetLevelEnum.成功:
                    iconText.Text = "✓";
                    iconText.Foreground = Brushes.White;
                    icon.Background = new SolidColorBrush(Color.FromRgb(39, 174, 96));
                    break;
                default:
                    iconText.Text = "ℹ";
                    iconText.Foreground = Brushes.White;
                    icon.Background = new SolidColorBrush(Color.FromRgb(149, 165, 166));
                    break;
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            Grid.SetColumn(icon, 0);
            Grid.SetRowSpan(icon, 2);
            Grid.SetRowSpan(icon, 2);
            Grid.SetColumn(timeText, 1);
            Grid.SetRow(timeText, 0);
            Grid.SetColumn(textBlock, 1);
            Grid.SetRow(textBlock, 1);

            grid.Children.Add(icon);
            grid.Children.Add(timeText);
            grid.Children.Add(textBlock);
            border.Child = grid;

            // 加入容器（插到最前面，新通知在上方）
            NotificationContainer.Items.Insert(0, border);

            // 限制最大显示数量
            while (NotificationContainer.Items.Count > 12)
            {
                var last = NotificationContainer.Items[^1] as Border;
                NotificationContainer.Items.RemoveAt(NotificationContainer.Items.Count - 1);
                last?.BeginAnimation(OpacityProperty, null);
            }

            // 淡入动画
            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(300)));
            fadeIn.FillBehavior = FillBehavior.HoldEnd;
            EventHandler fadeInHandler = null!;
            fadeInHandler = (s, a) =>
            {
                if (s is DoubleAnimation anim)
                    anim.Completed -= fadeInHandler;

                // 停留 3 秒后淡出
                var timer = new System.Windows.Threading.DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(3),
                    Tag = border,
                };
                EventHandler tickHandler = null!;
                tickHandler = (t, args) =>
                {
                    var tmr = (System.Windows.Threading.DispatcherTimer)t;
                    tmr.Stop();
                    tmr.Tick -= tickHandler;

                    var target = tmr.Tag as Border;
                    if (target == null || !NotificationContainer.Items.Contains(target))
                        return;

                    var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(500)));
                    fadeOut.FillBehavior = FillBehavior.HoldEnd;
                    EventHandler fadeOutHandler = null!;
                    fadeOutHandler = (o, ea) =>
                    {
                        NotificationContainer.Items.Remove(target);
                        if (o is DoubleAnimation da)
                            da.Completed -= fadeOutHandler;
                    };
                    fadeOut.Completed += fadeOutHandler;
                    target.BeginAnimation(OpacityProperty, fadeOut);
                };
                timer.Tick += tickHandler;
                timer.Start();
            };
            fadeIn.Completed += fadeInHandler;
            border.BeginAnimation(OpacityProperty, fadeIn);
        }
        catch
        {
            // 静默处理
        }
    }
}
