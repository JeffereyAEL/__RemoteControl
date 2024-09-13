using Godot;

public enum EAspect
{
    Portrait,
    Square,
    Landscape
}

public static class Utils
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
}