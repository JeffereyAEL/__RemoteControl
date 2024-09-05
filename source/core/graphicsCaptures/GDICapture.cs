#if GODOT_WINDOWS
#pragma warning disable CA1416 // Validate platform compatibility

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Godot;

// TODO: impliment OnWindowSizeChanged so that our fetching programmatially updates our image size
public partial class GDICapture : GraphicsCapture
{
	// DEBUG
	private const string DebugOutputDir = "assets\\debug\\outputs\\" ;

	// Class
	private IntPtr WinHandle;

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

	[DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool PrintWindow(IntPtr hwnd, IntPtr hdcBlt, uint nFlags);

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
	
	public GDICapture()
	{
		WinHandle = IntPtr.Zero;
	}

	protected override void _Factory()
	{
		WinHandle = FindWindow(null, WinTitle);
		if (WinHandle == IntPtr.Zero)
			throw new ArgumentException($"Unable to find window with name \"{WinTitle}\"");

		RECT win_rect = new RECT();
		if (!GetWindowRect(WinHandle, out win_rect)) 
			throw new Exception($"Unable to find window with handle \"{WinHandle}\"");

		WinWidth = win_rect.Right - win_rect.Left;
		WinHeight = win_rect.Bottom - win_rect.Top;
	}

	public static GDICapture Factory(string winTitle)
	{
		return Factory<GDICapture>(winTitle);
	}

	public override Godot.ImageTexture CaptureWindow()
	{
		
		IntPtr win_dc = GetWindowDC(WinHandle);
		IntPtr mem_dc = CreateCompatibleDC(win_dc);
		IntPtr new_hbitmap = CreateCompatibleBitmap(win_dc, WinWidth, WinHeight);
		IntPtr old_hbitmap = SelectObject(mem_dc, new_hbitmap);

		PrintWindow(WinHandle, mem_dc, 0);

		Godot.Image image;
		using (System.Drawing.Bitmap bitmap = System.Drawing.Image.FromHbitmap(new_hbitmap))
		{
			BitmapData bitmap_data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, bitmap.PixelFormat);

			int stride = bitmap_data.Stride;
			var row_num_bytes = Mathf.Abs(stride);
			int image_num_bytes = row_num_bytes * bitmap.Height;
													
			byte[] color_data = new byte[image_num_bytes];
			for (int row = 0; row < bitmap.Height; ++row)
			{
				int dest_ptr = row * row_num_bytes;
				IntPtr src_ptr = bitmap_data.Scan0 - row_num_bytes * row;
				for (int pixel = 0; pixel < row_num_bytes / 4; ++pixel)
				{
					int pixel_ofs = 4*pixel;
					Marshal.Copy(src_ptr+pixel_ofs,   color_data, dest_ptr+pixel_ofs+2, 1);		// B
					Marshal.Copy(src_ptr+pixel_ofs+1, color_data, dest_ptr+pixel_ofs+1, 1); 	// G
					Marshal.Copy(src_ptr+pixel_ofs+2, color_data, dest_ptr+pixel_ofs,   1); 	// R
					color_data[dest_ptr+pixel_ofs+3] = 255;										// A
				}
			}

			bitmap.UnlockBits(bitmap_data);
			SelectObject(mem_dc, old_hbitmap);
			DeleteObject(new_hbitmap);
			DeleteObject(mem_dc);
			var h_result = ReleaseDC(WinHandle, win_dc);
			if (h_result != 1) throw new Exception($"ReleaseDC returned an non-one HRESULT: {h_result}");

			image = Godot.Image.CreateFromData(WinWidth, WinHeight, false, Godot.Image.Format.Rgba8, color_data);
		}

		return ImageTexture.CreateFromImage(image);
	}

    public override void OnWindowSizeChanged()
    {
        throw new NotImplementedException();
    }

#if DEBUG
	private void SaveBitmap(System.Drawing.Bitmap bitmap, string fileName)
	{
		using (StreamWriter writer = new StreamWriter(DebugOutputDir+fileName))
		{
			writer.WriteLine("// bottom-left -> top-right pixel color data");
			for (int y = 0; y < bitmap.Height; ++y)
			{
				for (int x = 0; x < bitmap.Width; ++x)
				{
					var p = bitmap.GetPixel(x, y);

					writer.WriteLine($"Color ({p.A/255.0f}, {p.B/255f}, {p.G/255f}, {p.R/255f})");
				}
			}
		}
	}

		private void SaveImage(Godot.Image image, string fileName)
	{
		using (StreamWriter writer = new StreamWriter(DebugOutputDir+fileName))
		{
			writer.WriteLine("// bottom-left -> top-right pixel color data");
			for (int y = 0; y < image.GetHeight(); ++y)
			{
				for (int x = 0; x < image.GetWidth(); ++x)
				{
					var p = image.GetPixel(x, y);
					writer.WriteLine($"Color ({p.R}, {p.G}, {p.B}, {p.A})");
				}
			}
		}
	}
#endif
}

#pragma warning restore CA1416 // Validate platform compatibility
#endif