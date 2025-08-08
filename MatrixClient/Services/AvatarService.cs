using System;
using System.IO;
using Avalonia.Media.Imaging;
using SkiaSharp;

public static class AvatarService
{
  public static Bitmap GetOrCreateAvatar(string jid)
  {
    string initial = GetInitial(jid);
    string path = GetAvatarPath(jid);

    if (!File.Exists(path))
    {
      var bitmap = GenerateAvatar(initial, 128);
      using var stream = File.OpenWrite(path);
      bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
    }

    using var fileStream = File.OpenRead(path);
    return new Bitmap(fileStream);
  }

  private static string GetInitial(string jid)
  {
    var local = jid.Split('@')[0];
    return string.IsNullOrEmpty(local) ? "?" : local.Substring(0, 1).ToUpper();
  }

  private static string GetAvatarPath(string jid)
  {
    var name = jid.Replace("@", "").Replace(".", "");
    var fileName = $"{name}.png";
    var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyApp", "Avatars");
    Directory.CreateDirectory(folder);
    return Path.Combine(folder, fileName);
  }

  private static SKBitmap GenerateAvatar(string initial, int size)
  {
    var bitmap = new SKBitmap(size, size);
    using var canvas = new SKCanvas(bitmap);
    canvas.Clear(SKColors.Transparent);

    var circlePaint = new SKPaint
    {
      Color = SKColors.SteelBlue,
      IsAntialias = true
    };
    canvas.DrawCircle(size / 2, size / 2, size / 2, circlePaint);
    var textPaint = new SKPaint
    {
      Color = SKColors.White,
      IsAntialias = true,
      TextSize = size / 2,
      TextAlign = SKTextAlign.Center // Alignment is set on SKPaint
    };

// Measure text bounds using SKPaint
    SKRect bounds = new SKRect();
    textPaint.MeasureText(initial, ref bounds);

// Calculate position
    float x = size / 2;
    float y = size / 2 - bounds.MidY;

// Draw text using SKPaint only
    canvas.DrawText(initial, x, y, textPaint);

    //
    // var textPaint = new SKPaint
    // {
    //   Color = SKColors.White,
    //   IsAntialias = true,     
    // };
    // var font = new SKFont(SKTypeface.Default, size / 2);
    // var align = SKTextAlign.Center;
    //
    //
    // font.MeasureText(initial, out SKRect bounds, textPaint);
    //
    // float x = size / 2;
    // float y = size / 2 - bounds.MidY;
    //
    // canvas.DrawText(initial, x, y, align, font, textPaint);

    return bitmap;
  }
}
