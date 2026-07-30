using Godot;
using System;

namespace ZNet.Source.Interface;

public partial class CanvasItemSwitcher : Node
{
    [Export] Godot.Collections.Array<CanvasItem> _items = new();
    [Export] Godot.Collections.Dictionary<Button, CanvasItem> _binds = new();

    public override void _Ready()
    {
        if (_items.Count > 0)
            Switch(_items[0]);

        foreach (var pair in _binds)
        {
            pair.Key.Pressed += () => Switch(pair.Value);
        }

    }

    public void Register(CanvasItem item)
    {
        if (_items.Contains(item))
            throw new ArgumentException("Item already registered");

        _items.Add(item);
    }

    public void Unregister(CanvasItem item)
    {
        if (!_items.Contains(item))
            throw new ArgumentException($"No item {item} found in registry");

        _items.Remove(item);
    }

    public void Switch(CanvasItem item)
    {
        if (!_items.Contains(item))
            throw new ArgumentException($"No item {item} found in registry");

        foreach (CanvasItem child in _items)
            child.Visible = child == item;
    }

    public void SwitchByName(string name)
    {
        foreach (CanvasItem item in _items)
        {
            if (item.Name == name)
            {
                Switch(item);
            }
        }
    }
}
