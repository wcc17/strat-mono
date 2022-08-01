using System;
using System.Collections.Generic;
using System.Text;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StratMono.Scenes
{
    class BoundedMovingCamera : Camera, IUpdatable
    {
        private readonly int _cameraMoveSpeed = 10;

        private readonly Rectangle _levelBounds;
        private VirtualIntegerAxis _cameraMovementXAxisInput;
        private VirtualIntegerAxis _cameraMovementYAxisInput;
        public Vector2 _cameraMovementDirection = new Vector2(0, 0);

        public BoundedMovingCamera(Rectangle levelBounds) : base()
        {
            _levelBounds = levelBounds;

            // horizontal input from dpad, left stick or keyboard left/right
            _cameraMovementXAxisInput = new VirtualIntegerAxis();
            _cameraMovementXAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
            _cameraMovementXAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _cameraMovementXAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A, Keys.D));

            // vertical input from dpad, left stick or keyboard up/down
            _cameraMovementYAxisInput = new VirtualIntegerAxis();
            _cameraMovementYAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            _cameraMovementYAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _cameraMovementYAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.W, Keys.S));
        }

        public void Update()
        {
            updatePosition();
            adjustPositionForBounds();
        }

        private void updatePosition()
        {
            _cameraMovementDirection.X = _cameraMovementXAxisInput.Value;
            _cameraMovementDirection.Y = _cameraMovementYAxisInput.Value;

            if (_cameraMovementDirection.X > 0)
            {
                Position = new Vector2(Position.X + _cameraMoveSpeed, Position.Y);
            }

            if (_cameraMovementDirection.X < 0)
            {
                Position = new Vector2(Position.X - _cameraMoveSpeed, Position.Y);
            }

            if (_cameraMovementDirection.Y > 0)
            {
                Position = new Vector2(Position.X, Position.Y + _cameraMoveSpeed);
            }

            if (_cameraMovementDirection.Y < 0)
            {
                Position = new Vector2(Position.X, Position.Y - _cameraMoveSpeed);
            }
        }

        private void adjustPositionForBounds()
        {
            var bounds = Bounds;
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
