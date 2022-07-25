using System;
using System.Numerics;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace StratMono.Components
{
    public class AiMovement : Component, IUpdatable
    {
        public Vector2 MoveDirection = new Vector2(0, 1);

        public void Update()
        {
            //MoveDirection.X = _xAxisInput.Value;
            //MoveDirection.Y = _yAxisInput.Value;
        }
    }
}