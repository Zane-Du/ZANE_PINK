#r "System.Drawing.Common"

using System.Drawing;
using System.Drawing.Imaging;

string icoPath = @"C:\Users\34163\Desktop\ZANE\ZANE\ZANE.ico";
string outputPath = @"C:\Users\34163\Desktop\ZANE\ZANE\ZANE_new.ico";

// Load source icon
Icon icon = new Icon(icoPath);
Bitmap bmp = icon.ToBitmap();

Console.WriteLine($"原始尺寸: {bmp.Width}x{bmp.Height}, PixelFormat: {bmp.PixelFormat}");

// Create a new bitmap with transparency
Bitmap result = new Bitmap(bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);

Color targetBg = Color.FromArgb(255, 255, 255, 255); // exact white background
int tolerance = 40;

for (int y = 0; y < bmp.Height; y++)
{
    for (int x = 0; x < bmp.Width; x++)
    {
        Color c = bmp.GetPixel(x, y);
        
        // If the pixel is close to white (background), make it transparent
        int diff = Math.Abs(c.R - 255) + Math.Abs(c.G - 255) + Math.Abs(c.B - 255);
        bool isEdge = diff > 0 && diff < tolerance * 3;
        
        if (diff < 3) // pure/near white → transparent
        {
            result.SetPixel(x, y, Color.FromArgb(0, c.R, c.G, c.B));
        }
        else if (isEdge) // edge pixel: blend transparency
        {
            float factor = Math.Min(1f, diff / (tolerance * 3f));
            int alpha = (int)(255 * factor);
            result.SetPixel(x, y, Color.FromArgb(alpha, c.R, c.G, c.B));
        }
        else
        {
            result.SetPixel(x, y, Color.FromArgb(255, c.R, c.G, c.B));
        }
    }
}

// Save as PNG (for preserving transparency)
string pngPath = @"C:\Users\34163\Desktop\ZANE\ZANE\ZANE_transparent.png";
result.Save(pngPath, ImageFormat.Png);
Console.WriteLine($"已生成透明PNG: {pngPath}");

// Now create multi-resolution ICO from the transparent PNG
// ICO format: we'll embed multiple sizes from the source
var sizes = new[] { 256, 128, 64, 48, 32, 24, 16 };
using (FileStream fs = new FileStream(outputPath, FileMode.Create))
using (BinaryWriter w = new BinaryWriter(fs))
{
    // ICO header
    w.Write((short)0); // reserved
    w.Write((short)1); // ICO type
    w.Write((short)sizes.Length); // image count
    
    long dataOffset = 6 + sizes.Length * 16;
    var imageDataList = new List<byte[]>();
    
    foreach (var size in sizes)
    {
        int s = Math.Min(size, bmp.Width);
        using (Bitmap resized = new Bitmap(s, s))
        using (Graphics g = Graphics.FromImage(resized))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.Clear(Color.Transparent);
            g.DrawImage(result, 0, 0, s, s);
            
            using (MemoryStream ms = new MemoryStream())
            {
                resized.Save(ms, ImageFormat.Png);
                imageDataList.Add(ms.ToArray());
            }
        }
    }
    
    // Write directory entries
    for (int i = 0; i < sizes.Length; i++)
    {
        int size = sizes[i];
        byte[] data = imageDataList[i];
        
        w.Write((byte)(size >= 256 ? (byte)0 : size));
        w.Write((byte)(size >= 256 ? (byte)0 : size));
        w.Write((byte)0); // color palette
        w.Write((byte)0); // reserved
        w.Write((short)1); // color planes
        w.Write((short)32); // bits per pixel
        w.Write((int)data.Length); // image size
        w.Write((int)dataOffset); // image offset
        
        dataOffset += data.Length;
    }
    
    // Write image data
    foreach (var data in imageDataList)
    {
        w.Write(data);
    }
}

Console.WriteLine($"已生成透明ICO: {outputPath}");

// Cleanup
bmp.Dispose();
result.Dispose();
icon.Dispose();
