using System;
using Nez;
using Nez.Sprites;

namespace StratMono.Components
{
    public class CameraComponent : Camera, IUpdatable
    {
        private MoveComponent _moveComponent;

        public override void OnAddedToEntity()
        {
            _moveComponent = Entity.GetComponent<MoveComponent>();
        }
        
        public void Update()
        {
            if (_moveComponent.MoveDirection.X > 0)
            {
                Position = new Microsoft.Xna.Framework.Vector2(Position.X + (10), Position.Y);
            }

            if (_moveComponent.MoveDirection.X < 0)
            {
                Position = new Microsoft.Xna.Framework.Vector2(Position.X - (10), Position.Y);
            }

            if (_moveComponent.MoveDirection.Y > 0)
            {
                Position = new Microsoft.Xna.Framework.Vector2(Position.X, Position.Y + (10));
            }

            if (_moveComponent.MoveDirection.Y < 0)
            {
                Position = new Microsoft.Xna.Framework.Vector2(Position.X, Position.Y - (10));
            }
        }
    }
}