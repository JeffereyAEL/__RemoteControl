using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Utils;

public partial class SlimeCastleMining : AutomationAgent<SlimeCastleMining>
{
	protected enum EProperty
	{
		None,
		ComparisonColor,
		PixelChecks,
		PixelCheckMargin,

	}

	protected Godot.Color ExpectedColor = Color.Color8(31, 15, 25, 255);
	protected int ExpectedColorDiffMargin = 30;
	protected int PixelChecks = 5;
	protected int PixelChecksMargin = 3;

	private const double _Width = 769d;
	private const double _Height = 1400d;
	private const double _RelOfsX = 85d / _Width;
	private readonly TVector2D _BlockRelSize = new(100d / _Width, 100d / _Height);
	private readonly Godot.Color _Warm = Color.Color8(255, 0, 0, 255);
	

	public void SetComparisonColor(Color c)
	{
		ExpectedColor = c;
		PropertyChanged?.Invoke();
	}

	public void SetExpectedColorMargin(int m)
	{
		ExpectedColorDiffMargin = m;
		PropertyChanged?.Invoke();
	}

	public void SetPixelChecks(int c)
	{
		PixelChecks = c;
		PropertyChanged?.Invoke();
	}

	public void SetPixelCheckMargin(int m)
	{
		PixelChecksMargin = m;
		PropertyChanged?.Invoke();
	}
    protected static TVector2D LerpSpace(TVector2D spaceA, TVector2D spaceB, TVector2D posA)
	{
		return new TVector2D{
			X = posA.X / spaceA.X * spaceB.X,
			Y = posA.Y / spaceA.Y * spaceB.Y
		};
	} 
	
	protected static int Diff(Color a, Color b)
	{

		return Mathf.Abs(a.R8 - b.R8) + Mathf.Abs(a.G8 - b.G8) + Mathf.Abs(a.B8 - b.B8);
	}

	public override TRectContext[] DebugProcess(ImageTexture texture, Rect2 boundsRect)
	{
		var debug_rects = new List<TRectContext>();
		var tex_img = texture.GetImage();
		var img_size = (TVector2D)tex_img.GetSize();
		var img_block_size = new TVector2D(_BlockRelSize.X * img_size.X, _BlockRelSize.Y * img_size.Y);
		double img_ofs_x = _RelOfsX * img_size.X;

		// global space declarations
		var bounds_pos = (TVector2D)boundsRect.Position;
		var bounds_size = (TVector2D)boundsRect.Size;
		var bounds_block_size = new TVector2D(_BlockRelSize.X * bounds_size.X, _BlockRelSize.Y * bounds_size.Y);
		double bounds_ofs_x = _RelOfsX * bounds_size.X;

		TVector2D pos = new TVector2D(bounds_ofs_x, 300d) + bounds_pos;
		TVector2D size = new(bounds_size.X - 2d * bounds_ofs_x, bounds_size.Y - 300d);
        var mining_area = new TRectContext(new(pos.ToInt(), size.ToInt()), "MiningArea", $"{LerpSpace(bounds_size, img_size, pos-bounds_pos):0.0}");
		mining_area.DrawColor = Color.Color8(255, 135, 135, 255);
        debug_rects.Add(mining_area);

		Color p; int closeness;
		TVector2D block_pos, pixel_pos;

		// color matching needs to be done in image space
		for (double left = img_ofs_x; left < img_size.X - img_ofs_x; left += img_block_size.X)
		{
			int y_iter = 1;
			// for (double top = img_size.Y - img_block_size.Y; top > img_size.Y / 3.5d; top -= img_block_size.Y, ++y_iter)
			for (double top = img_size.Y / 3.5d; top < img_size.Y; top += 1d)
			{
				closeness = PixelChecks;
				block_pos = new(left, top);
				for (int i = 0; i < PixelChecks; ++i)
				{
					pixel_pos = block_pos;
					pixel_pos.X += (i + 1d) / (PixelChecks + 2d) * img_block_size.X;
					p = tex_img.GetPixel(pixel_pos.IntX, pixel_pos.IntY);
					if (Diff(p, ExpectedColor) > ExpectedColorDiffMargin)
					{
						// for debugging rects we need to use global space
						--closeness;
						TRectContext error = new();
						error.Rect.Position = (bounds_pos + LerpSpace(img_size, bounds_size, pixel_pos)).ToInt();
						error.Rect.Size = new Vector2I(1, 1);
						error.DrawColor = Color.Color8(255, 35, 35, 255);
						error.DrawFill = false;
						debug_rects.Add(error);
					}
				}
				if (closeness < PixelChecksMargin) break;

				// for debugging rects we need to use global space
				TRectContext c = new();
				c.Rect.Position = (bounds_pos + LerpSpace(img_size, bounds_size, block_pos)).ToInt();
				c.Rect.Size = bounds_block_size.ToInt();
				c.DrawFill = false;
				c.DrawColor = Color.Color8(25, 255, 25, 255);
				c.AddLabel($"Block#{y_iter:0}", $"{block_pos:0}");
				debug_rects.Add(c);
			}
		}

		return debug_rects.ToArray();
	}
}
