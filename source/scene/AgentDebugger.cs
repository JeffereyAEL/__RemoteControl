using System;
using System.Collections.Generic;
using Godot;

public partial class AgentDebuger : Node
{
    private struct TProperty
    {
        public StringName Name;
        public Type ValueType;
        public Variant Value;

        // TODO: make a constructor that takes a class (maybe generic/template) and a property stringname and create everything dynamically
    }

    private List<TProperty> Properties;

    public override void _Ready()
    {
        base._Ready();
        CallDeferred(MethodName.DeferredReady);
        // TODO: create a selector for every referenced property
    }

    public void DeferredReady()
    {
        // TODO: set the current properties of the reference agent
    }

    // give selectors a delegate that fires when input is modified that AgentDebuger subscribes to and redirects to the relevant SetProperty with the new data
}