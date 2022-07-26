using System;
using Microsoft.Xna.Framework;
using Nez;

namespace StratMono.Components
{
    public class MouseMovement : Component, IUpdatable
    {
        public Vector2 MoveDirection = new Vector2(0, 0);

        public void Update()
        {
            var cameraPosition = Entity.Scene.Camera.Position;

            Entity.Position = new Vector2(
                Input.MousePosition.X + (cameraPosition.X - Screen.Width / 2), 
                Input.MousePosition.Y + (cameraPosition.Y - Screen.Height / 2));

            //TODO
            //could also potentially use this same pattern in Grid to ensure that all entities are snapped to grid
                //however, this could cause problems when entities are moving from one grid tile to another
        }
    }
}