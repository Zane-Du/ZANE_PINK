using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

// ====== 配置 ======
string outputDir = @"C:\Users\34163\Desktop\ZANE\ZANE";
string icoPath = Path.Combine(outputDir, "ZANE.ico");
string png256Path = Path.Combine(outputDir, "zane_logo_256.png");

// 粉色
Color pinkColor = Color.FromArgb(255, 0xE9, 0x1E, 0x63);
Color whiteColor = Color.White;

Console.WriteLine("=== 生成粉色圆角 Z Logo 图标 ===");
Console.WriteLine($"粉色: #{pinkColor.R:X2}{pinkColor.G:X2}{pinkColor.B:X2}");

var sizes = new[] { 256, 128, 96, 64, 48, 32, 24, 16 };
var imageDataDict = new Dictionary<int, byte[]>();

foreach (var size in sizes)
{
    using (Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
    using (Graphics g = Graphics.FromImage(bmp))
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
        g.Clear(Color.Transparent);

        // 粉色圆角背景
        int radius = Math.Max(2, size / 5);
        int borderMargin = Math.Max(1, size / 64);
        int drawSize = size - borderMargin * 2;
        
        using (GraphicsPath path = RoundedRect(new Rectangle(borderMargin, borderMargin, drawSize, drawSize), radius))
        using (SolidBrush pinkBrush = new SolidBrush(pinkColor))
        {
            g.FillPath(pinkBrush, path);
        }

        // 白色 Z 字
        int fontSize = Math.Max(8, size / 5 * 3);
        if (size <= 24) fontSize = size / 2;
        if (size <= 16) fontSize = size / 2 + 1;
        
        using (Font font = new Font("Segoe UI", fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
        using (SolidBrush whiteBrush = new SolidBrush(whiteColor))
        {
            SizeF textSize = g.MeasureString("Z", font);
            float x = (size - textSize.Width) / 2f;
            float y = (size - textSize.Height) / 2f;
            g.DrawString("Z", font, whiteBrush, x, y);
        }

        using (MemoryStream ms = new MemoryStream())
        {
            bmp.Save(ms, ImageFormat.Png);
            imageDataDict[size] = ms.ToArray();
        }

        if (size == 256)
        {
            bmp.Save(png256Path, ImageFormat.Png);
            Console.WriteLine($"已生成预览PNG: {png256Path}");
        }
    }
}

// 写入 ICO
using (FileStream fs = new FileStream(icoPath, FileMode.Create))
using (BinaryWriter w = new BinaryWriter(fs))
{
    w.Write((short)0);
    w.Write((short)1);
    w.Write((short)sizes.Length);

    long dataOffset = 6 + sizes.Length * 16;

    foreach (var size in sizes)
    {
        byte[] data = imageDataDict[size];
        byte sizeByte = size >= 256 ? (byte)0 : (byte)size;

        w.Write(sizeByte);
        w.Write(sizeByte);
        w.Write((byte)0);
        w.Write((byte)0);
        w.Write((short)1);
        w.Write((short)32);
        w.Write((int)data.Length);
        w.Write((int)dataOffset);

        dataOffset += data.Length;
    }

    foreach (var size in sizes)
    {
        w.Write(imageDataDict[size]);
    }
}

Console.WriteLine($"\n✅ 已生成 ICO: {icoPath}");
Console.WriteLine($"包含 {sizes.Length} 个尺寸: {string.Join(", ", sizes)}");

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
