#pragma warning disable CA1416 // Validate platform compatibility
#if GODOT_WINDOWS

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Godot;

public partial class GDICapture
{
    public ImageTexture Capture(IntPtr winHandle, Rect2I winRect)
    {
        IntPtr win_dc = GetWindowDC(winHandle);
        IntPtr mem_dc = CreateCompatibleDC(win_dc);
        IntPtr new_hbitmap = CreateCompatibleBitmap(win_dc, winRect.Size.X, winRect.Size.Y);
        IntPtr old_hbitmap = SelectObject(mem_dc, new_hbitmap);

        PrintWindow(winHandle, mem_dc, 0);

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
            var h_result = ReleaseDC(winHandle, win_dc);
            if (h_result != 1) throw new Exception($"ReleaseDC returned an non-one HRESULT: {h_result}");

            image = Godot.Image.CreateFromData(winRect.Size.X, winRect.Size.Y, false, Godot.Image.Format.Rgba8, color_data);
        }

        return ImageTexture.CreateFromImage(image);
    }
}
#endif
#pragma warning restore CA1416 // Validate platform compatibility