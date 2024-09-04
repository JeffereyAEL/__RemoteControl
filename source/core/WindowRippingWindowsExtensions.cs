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
	// DEBUG
	private const string DebugOutputDir = "assets\\debug\\outputs\\" ;

	// Class
	private string WinTitle;

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
	
	public WindowRippingWindowsExtensions()
	{
		WinTitle = null;
	}


	public WindowRippingWindowsExtensions(string winTitle)
	{
		WinTitle = winTitle;
	}	

	public bool IsValid()
	{
		return WinTitle != null;
	}

	public static WindowRippingWindowsExtensions Factory(string winTitle)
	{
		return new WindowRippingWindowsExtensions(winTitle);
	}

	public Godot.ImageTexture CaptureWindow()
	{
		IntPtr win_handle = FindWindow(null, WinTitle);
		if (win_handle == IntPtr.Zero)
			throw new ArgumentException($"Unable to find window with name \"{WinTitle}\"");

		RECT win_rect = new RECT();
		if (!GetWindowRect(win_handle, out win_rect)) 
			throw new Exception($"Unable to find window with handle \"{win_handle}\"");

		int win_width = win_rect.Right - win_rect.Left;
		int win_height = win_rect.Bottom - win_rect.Top;
		
		IntPtr win_dc = GetWindowDC(win_handle);
		IntPtr mem_dc = CreateCompatibleDC(win_dc);
		IntPtr new_hbitmap = CreateCompatibleBitmap(win_dc, win_width, win_height);
		IntPtr old_hbitmap = SelectObject(mem_dc, new_hbitmap);

		PrintWindow(win_handle, mem_dc, 0);

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
			var h_result = ReleaseDC(win_handle, win_dc);
			if (h_result != 1) throw new Exception($"ReleaseDC returned an non-one HRESULT: {h_result}");

			image = Godot.Image.CreateFromData(win_width, win_height, false, Godot.Image.Format.Rgba8, color_data);
		}

		return ImageTexture.CreateFromImage(image);
	}

#if GODOT_DEBUG
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