using System;
using System.Data.Common;
using System.Text.RegularExpressions;
using Godot;

namespace Utils
{
	public delegate void DebugChanged();

    public enum EAspect
    {
        Portrait = -1,
        Square = 0,
        Landscape = 1
    }

    public struct TRectContext
    {
        public static Color DefaultColor = Color.Color8(45, 255, 45, 255);
        public static bool DefaultDrawFill = false;
        public Rect2I Rect;
        public Color DrawColor;
        public bool DrawFill;
        public string[] Label;

        public TRectContext()
        {
            Rect = new Rect2I();
            DrawColor = new Color();
            DrawFill = true;
            Label = Array.Empty<string>();
        }
        public TRectContext(Rect2I rect, params string[] label)
        {
            Rect = rect;
            DrawColor = DefaultColor;
            DrawFill = DefaultDrawFill;
            Label = label;
        }

        public void AddLabel(params string[] addLabel)
        {
            var new_label = new string[Label.Length+addLabel.Length];
            int idx = 0, _ref;
            for (_ref = 0; _ref < Label.Length; ++idx, ++_ref)
            {
                new_label[idx] = Label[_ref];
            }
            for (_ref = 0; _ref < new_label.Length; ++idx, ++_ref)
            {
                new_label[idx] = addLabel[_ref];
            }
            Label = new_label;
        }
    }

    public struct TVector2D : IFormattable
    {
        public double X;
        public double Y;

        public int IntX {
            get {return Mathf.RoundToInt(X);}
        }
        public int IntY {
            get {return Mathf.RoundToInt(Y);}
        }

        public TVector2D(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Vector2I ToInt()
        {
            return new Vector2I(IntX, IntY);
        }

        public Vector2 ToFloat()
        {
            return new Vector2((float)X, (float)Y);
        }

        public string ToString(string format, IFormatProvider formatProvider)
        {
            Regex match_int_format = new(@"0+([.]0*)?");
            if (!match_int_format.IsMatch(format)) throw new ArgumentException($"\"{format}\" is not a valid format for TVector2D");
            var m = match_int_format.Match(format);
            string x = X.ToString(m.ToString(), formatProvider);
            string y = Y.ToString(m.ToString(), formatProvider);
            return $"({x}, {y})";
        }

        public static explicit operator TVector2D(Godot.Vector2 v)
        {
            return new TVector2D(v.X, v.Y);
        }

        public static explicit operator TVector2D(Godot.Vector2I v)
        {
            return new TVector2D(v.X, v.Y);
        }

        public static TVector2D operator +(TVector2D a, TVector2D b)
        {
            return new(
                a.X + b.X,
                a.Y + b.Y
            );
        }

        public static TVector2D operator -(TVector2D a, TVector2D b)
        {
            return new(
                a.X - b.X,
                a.Y - b.Y
            );
        }

        public static TVector2D operator *(TVector2D a, double b)
        {
            return new(
                a.X * b,
                a.Y * b
            );
        }

        public static TVector2D operator /(TVector2D a, double b)
        {
            return new(
                a.X / b,
                a.Y / b
            );
        }
    }

    public static class Util
    {
        public static EAspect Aspect(Vector2I size)
        {
            float a = size.X / (float)size.Y;
            if (a < 1f) return EAspect.Portrait;
            else if (a > 1f) return EAspect.Landscape;
            else return EAspect.Square;
        }
        public static EAspect Aspect(Vector2 size)
        {
            float a = size.X / size.Y;
            if (a < 1f) return EAspect.Portrait;
            else if (a > 1f) return EAspect.Landscape;
            else return EAspect.Square;
        }

        public static Color LerpColor(Color From, Color To, float Weight)
        {
            return new()
            {
                R = Mathf.Lerp(From.R, To.R, Weight),
                G = Mathf.Lerp(From.G, To.G, Weight),
                B = Mathf.Lerp(From.B, To.B, Weight),
                A = Mathf.Lerp(From.A, To.A, Weight)
            };
        }

        public static Vector2 LerpVector2(Vector2 area, Vector2 relPos)
        {
            return new()
            {
                X = Mathf.Lerp(0f, area.X, relPos.X),
                Y = Mathf.Lerp(0f, area.Y, relPos.Y)
            };
        }

        public static Vector2I LerpVector2I(Vector2I area, Vector2I relPos)
        {
            var result = LerpVector2((Vector2)area, (Vector2)relPos);
            return new()
            {
                X = Mathf.RoundToInt(result.X),
                Y = Mathf.RoundToInt(result.Y)
            };
        }
    }
}