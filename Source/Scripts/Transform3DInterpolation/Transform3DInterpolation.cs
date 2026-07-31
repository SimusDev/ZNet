using Godot;
using System;

namespace Scripts
{
    [GlobalClass]
    public partial class Transform3DInterpolation : Node
    {
        [Export] private Node3D target;
        [Export] private float _interpolationSpeed = 40.0f;


    }   
}
