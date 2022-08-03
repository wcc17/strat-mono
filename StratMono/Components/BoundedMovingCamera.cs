using System;
using System.Collections.Generic;
using System.Text;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace StratMono.Components
{
    public class BoundedMovingCamera : Camera, IUpdatable
    {
        private readonly int _cameraMoveSpeed = 10;

        private readonly Rectangle _levelBounds;
        private VirtualIntegerAxis _cameraMovementXAxisInput;
        private VirtualIntegerAxis _cameraMovementYAxisInput;
        private Vector2 _cameraMovementDirection = new Vector2(0, 0);

        private readonly Vector2 _noMoveGoal = new Vector2(-1, -1);
        private Vector2 _moveGoal = new Vector2(-1, -1);
        public Vector2 MoveGoal 
        {
            get 
            {
                return _moveGoal;
            }

            set 
            {
                _moveGoal = setMoveGoal(value);
            } 
        }

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

            if (_cameraMovementDirection.X != 0 || _cameraMovementDirection.Y != 0)
            {
                _moveGoal = _noMoveGoal;
                handleInput();
            }

            if (!_moveGoal.Equals(_noMoveGoal))
            {
                handleMoveGoal();
            }
        }

        private void handleInput()
        {

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

        //TODO: should re-use this?
        private void handleMoveGoal()
        {
            var moveGoalSpeed = 25;
            var remainingDistanceX = Math.Abs(_moveGoal.X - Position.X);
            var remainingDistanceY = Math.Abs(_moveGoal.Y - Position.Y);
            var moveGoalSpeedX = (remainingDistanceX > moveGoalSpeed) ? moveGoalSpeed : remainingDistanceX;
            var moveGoalSpeedY = (remainingDistanceY > moveGoalSpeed) ? moveGoalSpeed : remainingDistanceY;

            if (_moveGoal.X > Position.X)
            {
                Position = new Vector2(Position.X + moveGoalSpeedX, Position.Y);
            }
            if (_moveGoal.X < Position.X)
            {
                Position = new Vector2(Position.X - moveGoalSpeedX, Position.Y);
            }
            if (_moveGoal.Y > Position.Y)
            {
                Position = new Vector2(Position.X, Position.Y + moveGoalSpeedY);
            }
            if (_moveGoal.Y < Position.Y)
            {
                Position = new Vector2(Position.X, Position.Y - moveGoalSpeedY);
            }

            if (remainingDistanceX <= 0 && remainingDistanceY <= 0)
            {
                _moveGoal = _noMoveGoal;
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

        private Vector2 setMoveGoal(Vector2 moveGoal)
        {
            return moveGoal;
        }
    }
}
