using System;
using System.Collections.Generic;
using Godot;
using Utils;

public abstract partial class AutomationAgent<DerivedAgent> : Node where DerivedAgent : Node
{
	public delegate void PropertyChange();
	public PropertyChange PropertyChanged;
    private static Dictionary<Type, Node> Singletons;

    public override void _Ready()
    {
        base._Ready();
        Singletons[this.GetType()] = this;
    }

    public static DerivedAgent Singleton()
    {
        Type derived_class = typeof(DerivedAgent);
        if (!Singletons.ContainsKey(derived_class)) return null;
        return (DerivedAgent)Singletons[derived_class];
    }

    public abstract TRectContext[] DebugProcess(ImageTexture texture, Rect2 boundsRect);
}