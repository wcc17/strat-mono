using System;
using Nez;
using Microsoft.Xna.Framework;
using StratMono.Event;

namespace StratMono.Components
{
    public class BoundedMovingCamera : Camera, IUpdatable
    {
        private RectangleF _levelBounds;
        private int _cameraMoveSpeed = 10;

        public BoundedMovingCamera(RectangleF levelBounds)
        {
            _levelBounds = levelBounds;
        }

        public override void OnAddedToEntity()
        {
            Entity.AddComponent(new ControllerMovement());
        }
        
        public void Update()
        {
            handleMovement();
            handleBounds();
        }

        private void handleMovement()
        {
            var controllerMovement = Entity.GetComponent<ControllerMovement>();

            if (controllerMovement.MoveDirection.X > 0)
            {
                Position = new Vector2(Position.X + _cameraMoveSpeed, Position.Y);
            }

            if (controllerMovement.MoveDirection.X < 0)
            {
                Position = new Vector2(Position.X - _cameraMoveSpeed, Position.Y);
            }

            if (controllerMovement.MoveDirection.Y > 0)
            {
                Position = new Vector2(Position.X, Position.Y + _cameraMoveSpeed);
            }

            if (controllerMovement.MoveDirection.Y < 0)
            {
                Position = new Vector2(Position.X, Position.Y - _cameraMoveSpeed);
            }

            var cameraGameEvent = new CameraGameEvent()
            {
                cameraPosition = Position
            };
            GameEventEmitter.Emitter.Emit(GameEventType.CameraPositionChanged, cameraGameEvent);
        }

        private void handleBounds()
        {
            // actual Camera bounds. "_levelBounds" is the entire map we want to keep the rectangle inside. Position is the center of the camera (bounds.width/2, bounds.height,2)
            var bounds = this.Bounds;

            if (bounds.X < _levelBounds.Left)
            {
                Position = new Vector2(bounds.Width / 2, Position.Y);
            }

            if ((bounds.X + bounds.Width) > _levelBounds.Right)
            {
                Position = new Vector2(_levelBounds.Right - (bounds.Width / 2), Position.Y);
            }

            if (bounds.Y < _levelBounds.Top)
            {
                Position = new Vector2(Position.X, bounds.Height / 2);
            }

            if ((bounds.Y + bounds.Height) > _levelBounds.Bottom)
            {
                Position = new Vector2(Position.X, _levelBounds.Bottom - (bounds.Height / 2));
            }
        }
    }
}