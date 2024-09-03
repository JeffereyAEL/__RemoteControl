#if GODOT_WINDOWS
#pragma warning disable CA1416 // Validate platform compatibility

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Godot;


public partial class WindowRippingWindowsExtensions : GodotObject
{
	private int Width;
	private int Height;
	private IntPtr WinHandle;

	private CompressedTexture2D DebugImage;

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll")]
	private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

	[StructLayout(LayoutKind.Sequential)]
	private struct RECT
	{
		public int Left;
		public int Top;
		public int Right;
		public int Bottom;
	}

	[DllImport("user32.dll")]
	private static extern IntPtr GetWindowDC(IntPtr hWnd);

	[DllImport("user32.dll")]
	private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

	[DllImport("gdi32.dll")]
	private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

	[DllImport("gdi32.dll")]
	private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

	[DllImport("gdi32.dll")]
	private static extern bool BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);

	[DllImport("gdi32.dll")]
	private static extern bool DeleteObject(IntPtr hObject);

	private const int SRCCOPY = 0x00CC0020;
	
	public WindowRippingWindowsExtensions()
	{
		WinHandle = IntPtr.Zero;
		Width = 0;
		Height = 0;
	}

	public WindowRippingWindowsExtensions(string winTitle)
	{
		Godot.GD.Print("In WindowRipping.WindowsExtensions Constructor");
		WinHandle = FindWindow(null, winTitle);
		if (WinHandle == IntPtr.Zero)
			throw new ArgumentException($"Unable to find window with name \"{winTitle}\"");

		RECT win_rect = new RECT();
		if (!GetWindowRect(WinHandle, out win_rect)) 
			throw new Exception($"Unable to find window with handle \"{WinHandle}\"");
		else
		{
			Width = win_rect.Right - win_rect.Left;
			Height = win_rect.Bottom - win_rect.Top;
		}
		Godot.GD.Print($"window size: {Width}, {Height}");
		DebugImage = GD.Load<CompressedTexture2D>("res://assets/debug/Hell.png");
		SaveBytesColorData(TextureToArray(DebugImage),  "WorkingImage.txt");
	}	

	public bool IsValid()
	{
		return WinHandle != IntPtr.Zero;
	}

	public static WindowRippingWindowsExtensions Factory(string winTitle)
	{
		return new WindowRippingWindowsExtensions(winTitle);
	}

    public Godot.ImageTexture CaptureWindow()
	{
		if (WinHandle == IntPtr.Zero) throw new NullReferenceException("WRWE class was not intialized properly, WinHandle was IntPtr.Zero");

		IntPtr win_dc = GetWindowDC(WinHandle);
		IntPtr mem_dc = CreateCompatibleDC(win_dc);
		IntPtr new_hbitmap = CreateCompatibleBitmap(win_dc, Width, Height);
		IntPtr old_hbitmap = SelectObject(mem_dc, new_hbitmap);

		BitBlt(mem_dc, 0, 0, Width, Height, win_dc, Width, Height, SRCCOPY);

		SelectObject(mem_dc, old_hbitmap);

        // System.Drawing.Bitmap bitmap = System.Drawing.Bitmap.FromHbitmap(new_hbitmap);
		using (System.Drawing.Bitmap bitmap = System.Drawing.Image.FromHbitmap(new_hbitmap))
		{
			BitmapData bitmap_data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);
			GD.Print($"Format: {bitmap.PixelFormat}");
			GD.Print($"bitmap.size: {bitmap.Width}, {bitmap.Height} | window");

			// Calculate the size of the raw pixel data
			int stride = bitmap_data.Stride;
			var row_num_bytes = Mathf.Abs(stride);
			
			int image_num_bytes = row_num_bytes * bitmap.Height;
			GD.Print($"stride = {stride} bytes");
			SaveIntPtrColorData(bitmap_data.Scan0, image_num_bytes, row_num_bytes, "bitmap_data.txt");
			GD.Print($"Picture Data = {Mathf.Abs(image_num_bytes)} bytes");

			// Create a byte array to hold the raw pixel data
			byte[] color_data = new byte[image_num_bytes];
			SaveBytesColorData(color_data,  "init_pixel_data.txt");

			if (stride > 0)
				// Copy the pixel data from the bitmap's data array to the byte array
				Marshal.Copy(bitmap_data.Scan0, color_data, 0, color_data.Length);
			else
			{
				for (int strides = 0; strides < bitmap.Height; ++strides)
				{
					int dest_ofs = strides * row_num_bytes;
					IntPtr source_start = bitmap_data.Scan0 - row_num_bytes * strides;
					Marshal.Copy(source_start, color_data, dest_ofs, row_num_bytes);
					for (int pixel_ofs = 0; pixel_ofs < row_num_bytes / 4; ++pixel_ofs)
					{
						color_data[dest_ofs + 3 + 4 * pixel_ofs] = 255;
					}
				}
			}

			// Unlock the bitmap
			bitmap.UnlockBits(bitmap_data);
			
			SaveBytesColorData(color_data,  "blited_pixel_data.txt");
			Godot.Image image = Godot.Image.CreateFromData(Width, Height, false, Godot.Image.Format.Rgba8, color_data);
			GD.Print($"image Size = {Width * Height * 4}");

			DeleteObject(new_hbitmap);
			DeleteObject(mem_dc);
			ReleaseDC(WinHandle, win_dc);

			return ImageTexture.CreateFromImage(image);
		}
	}

	private byte[] TextureToArray(CompressedTexture2D compressedTexture)
    {
        if (compressedTexture == null)
        {
            GD.PrintErr("CompressedTexture2D is null.");
            return null;
        }

        // Get the texture data
        return DebugImage.GetImage().GetData();
    }

	private void SaveBytesColorData(byte[] data, string fileName)
	{
		using (StreamWriter writer = new StreamWriter("assets//debug//"+fileName))
		{
			int length = data.Length;
			for (int i = 0; i < length; i += 4)
			{
				writer.WriteLine("// top-left -> bottom-right pixel color data");
				// Determine the end index for the current line
				int end = Math.Min(i + 4, length);
				
				// Write the bytes as integers
				for (int j = i; j < end; j++)
				{
					writer.Write((int)data[j] + " ");
				}

				// Ensure the line ends
				writer.WriteLine();
			}
		}
	}

	private void SaveIntPtrColorData(IntPtr data, int dataLen, int stride, string fileName)
	{
		using (StreamWriter writer = new StreamWriter("assets//debug//"+fileName))
		{
			writer.WriteLine("// top-left -> bottom-right pixel color data");
			int length = dataLen;
			for (int row = 0; row < length / stride; row += 4)
			{
				for (int pixel = 0; pixel < stride / 4; ++pixel)
				{
					// Write the bytes as integers
					for (int comp = 0; comp < 4; ++comp)
					{
						writer.Write(Marshal.ReadByte(data, comp) + " ");
					}

					// Ensure the line ends
					writer.WriteLine();
				}
			}
		}
	}
}

#pragma warning restore CA1416 // Validate platform compatibility
#endif