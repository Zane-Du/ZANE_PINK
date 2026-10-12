#r "System.Drawing.Common"

using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

// ====== 配置 ======
string outputDir = @"C:\Users\34163\Desktop\ZANE\ZANE";
string icoPath = Path.Combine(outputDir, "ZANE.ico");
string png256Path = Path.Combine(outputDir, "zane_logo_256.png");

// === 颜色配置（与DarkTheme/LightTheme中的LogoBackgroundBrush一致）===
// 粉色：#FFE91E63 (Light) / #FFFF4081 (Dark) → 取中间值
Color pinkColor = Color.FromArgb(255, 0xE9, 0x1E, 0x63); // #E91E63
Color whiteColor = Color.White;
int cornerRadius = 16; // 36x36中CornerRadius=8，按比例 256中radius≈57，这里用16更适合视觉
// 实际用圆角矩形，不对cornerRadius做比例缩放

Console.WriteLine("=== 生成粉色圆角 Z Logo 图标 ===");
Console.WriteLine($"粉色: #{pinkColor.R:X2}{pinkColor.G:X2}{pinkColor.B:X2}");
Console.WriteLine("字体: Segoe UI Black, 白色");

// 尺寸列表
var sizes = new[] { 256, 128, 96, 64, 48, 32, 24, 16 };

// 存储所有尺寸的PNG数据
var imageDataDict = new Dictionary<int, byte[]>();

foreach (var size in sizes)
{
    int canvasSize = Math.Max(size, 32); // 最小32以保证圆角可见
    // 实际上直接按size画
    using (Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
    using (Graphics g = Graphics.FromImage(bmp))
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        g.Clear(Color.Transparent);

        // 绘制圆角粉色背景
        // 圆角半径：根据尺寸适当缩放
        int radius = Math.Max(2, size / 5); // 16px 左右比例
        int borderThickness = Math.Max(1, size / 64);
        
        using (GraphicsPath path = RoundedRect(new Rectangle(borderThickness, borderThickness, 
                                             size - borderThickness * 2, size - borderThickness * 2), radius))
        using (SolidBrush pinkBrush = new SolidBrush(pinkColor))
        {
            g.FillPath(pinkBrush, path);
        }

        // 绘制白色 Z 字
        // 根据不同尺寸调整字体大小
        int fontSize = Math.Max(8, size / 5 * 3); // 约 60% 的尺寸
        if (size <= 24) fontSize = size / 2; // 小图标字体再小一点
        if (size <= 16) fontSize = size / 2 + 1;
        
        using (Font font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
        using (SolidBrush whiteBrush = new SolidBrush(whiteColor))
        {
            // 测量文字尺寸
            string text = "Z";
            SizeF textSize = g.MeasureString(text, font);
            
            // 居中绘制
            float x = (size - textSize.Width) / 2f;
            float y = (size - textSize.Height) / 2f;
            
            g.DrawString(text, font, whiteBrush, x, y);
        }

        // 保存到内存流（PNG格式）
        using (MemoryStream ms = new MemoryStream())
        {
            bmp.Save(ms, ImageFormat.Png);
            imageDataDict[size] = ms.ToArray();
        }

        // 如果是256尺寸，也输出PNG预览
        if (size == 256)
        {
            bmp.Save(png256Path, ImageFormat.Png);
            Console.WriteLine($"已生成预览PNG: {png256Path}");
        }
    }
}

// 写入 ICO 文件（嵌入PNG格式图标）
using (FileStream fs = new FileStream(icoPath, FileMode.Create))
using (BinaryWriter w = new BinaryWriter(fs))
{
    w.Write((short)0); // reserved
    w.Write((short)1); // ICO type
    w.Write((short)sizes.Length); // image count
    
    long dataOffset = 6 + sizes.Length * 16;
    
    // Write directory entries
    foreach (var size in sizes)
    {
        byte[] data = imageDataDict[size];
        byte sizeByte = size >= 256 ? (byte)0 : (byte)size;
        
        w.Write(sizeByte); // width
        w.Write(sizeByte); // height
        w.Write((byte)0); // color palette
        w.Write((byte)0); // reserved
        w.Write((short)1); // color planes
        w.Write((short)32); // bits per pixel
        w.Write((int)data.Length); // image size
        w.Write((int)dataOffset); // image offset
        
        dataOffset += data.Length;
    }
    
    // Write image data
    foreach (var size in sizes)
    {
        w.Write(imageDataDict[size]);
    }
}

Console.WriteLine($"\n✅ 已生成 ICO: {icoPath}");
Console.WriteLine($"包含 {sizes.Length} 个尺寸: {string.Join(", ", sizes)}");
Console.WriteLine("请在 csproj 中更新 ApplicationIcon 和项目属性中的图标。");

// ====== Helper方法 ======
static GraphicsPath RoundedRect(Rectangle rect, int radius)
{
    GraphicsPath path = new GraphicsPath();
    int r = Math.Min(radius, Math.Min(rect.Width, rect.Height) / 2);
    
    path.AddArc(rect.X, rect.Y, r * 2, r * 2, 180, 90);
    path.AddArc(rect.Right - r * 2, rect.Y, r * 2, r * 2, 270, 90);
    path.AddArc(rect.Right - r * 2, rect.Bottom - r * 2, r * 2, r * 2, 0, 90);
    path.AddArc(rect.X, rect.Bottom - r * 2, r * 2, r * 2, 90, 90);
    path.CloseFigure();
    
    return path;
}
