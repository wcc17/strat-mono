using System;
using System.Collections.Generic;
using System.Text;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StratMono.Util;

namespace StratMono.Components
{
    public class BoundedMovingCamera : Camera, IUpdatable
    {
        private readonly int _cameraMoveSpeed = 50;
        private readonly float _cameraMoveLerp = 0.2f;
        private readonly float maximumZoom = 0.3f;
        private readonly float minimumZoom = -0.3f;
        private readonly float zoomSpeed = 0.1f;

        private readonly Rectangle _levelBounds;
        private VirtualIntegerAxis _cameraMovementXAxisInput;
        private VirtualIntegerAxis _cameraMovementYAxisInput;
        private Vector2 _cameraMovementDirection = new Vector2(0, 0);
        private int _previousScrollWheelValue = 0;
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
            updateZoom();
            adjustPositionForBounds(); //TODO: I like the way using lerp works here, but it doesn't look as great when zooming out

            Entity.Transform.RoundPosition();
        }

        private void updatePosition()
        {
            _cameraMovementDirection.X = _cameraMovementXAxisInput.Value;
            _cameraMovementDirection.Y = _cameraMovementYAxisInput.Value;

            if (_cameraMovementDirection.X != 0 || _cameraMovementDirection.Y != 0)
            {
                _moveGoal = _noMoveGoal;
                handleMovement();
            }

            if (!_moveGoal.Equals(_noMoveGoal))
            {
                handleMoveGoal();
            }
        }

        private void updateZoom()
        {
            if (InputMode.CurrentInputMode == InputModeType.KeyboardMouse)
            {
                var delta = Input.CurrentMouseState.ScrollWheelValue - _previousScrollWheelValue;
                _previousScrollWheelValue = Input.CurrentMouseState.ScrollWheelValue;
                if (delta > 0)
                {
                    increaseZoom();
                }
                else if (delta < 0)
                {
                    decreaseZoom();
                }
            } else
            {
                if (Input.GamePads[0].IsButtonPressed(Buttons.RightShoulder))
                {
                    increaseZoom();
                }
                if (Input.GamePads[0].IsButtonPressed(Buttons.LeftShoulder))
                {
                    decreaseZoom();
                }
            }
        }

        private void increaseZoom()
        {
            Console.WriteLine("scroll up");
            Zoom += zoomSpeed;
            Zoom = (Zoom > maximumZoom) ? maximumZoom : Zoom;
        }

        private void decreaseZoom()
        {
            Console.WriteLine("scroll down");
            Zoom -= zoomSpeed;
            Zoom = (Zoom < minimumZoom) ? minimumZoom : Zoom;
        }

        private void handleMovement()
        {
            Vector2 desiredPosition = new Vector2(Position.X, Position.Y);

            if (_cameraMovementDirection.X > 0)
            {
                desiredPosition.X = Position.X + _cameraMoveSpeed;
            }
            if (_cameraMovementDirection.X < 0)
            {
                desiredPosition.X = Position.X - _cameraMoveSpeed;
            }
            if (_cameraMovementDirection.Y > 0)
            {
                desiredPosition.Y = Position.Y + _cameraMoveSpeed;
            }
            if (_cameraMovementDirection.Y < 0)
            {
                desiredPosition.Y = Position.Y - _cameraMoveSpeed;
            }

            setPositionWithLerp(desiredPosition);
        }

        private void handleMoveGoal()
        {
            var moveGoalSpeed = 250;
            var remainingDistanceX = Math.Abs(_moveGoal.X - Position.X);
            var remainingDistanceY = Math.Abs(_moveGoal.Y - Position.Y);
            var moveGoalSpeedX = (remainingDistanceX > moveGoalSpeed) ? moveGoalSpeed : remainingDistanceX;
            var moveGoalSpeedY = (remainingDistanceY > moveGoalSpeed) ? moveGoalSpeed : remainingDistanceY;

            Vector2 desiredPosition = new Vector2(Position.X, Position.Y);
            if (_moveGoal.X > Position.X)
            {
                desiredPosition.X += moveGoalSpeedX;
            }
            if (_moveGoal.X < Position.X)
            {
                desiredPosition.X -= moveGoalSpeedX;
            }
            if (_moveGoal.Y > Position.Y)
            {
                desiredPosition.Y += moveGoalSpeedY;
            }
            if (_moveGoal.Y < Position.Y)
            {
                desiredPosition.Y -= moveGoalSpeedY;
            }

            setPositionWithLerp(desiredPosition);

            if (remainingDistanceX <= 0 && remainingDistanceY <= 0)
            {
                _moveGoal = _noMoveGoal;
            }
        }

        private void adjustPositionForBounds()
        {
            var bounds = Bounds;
            var desiredPosition = new Vector2(Position.X, Position.Y);
            if (bounds.X < _levelBounds.Left)
            {
                desiredPosition.X = bounds.Width / 2;
            }

            if ((bounds.X + bounds.Width) > _levelBounds.Right)
            {
                desiredPosition.X = _levelBounds.Right - (bounds.Width / 2);
            }

            if (bounds.Y < _levelBounds.Top)
            {
                desiredPosition.Y = bounds.Height / 2;
            }

            if ((bounds.Y + bounds.Height) > _levelBounds.Bottom)
            {
                desiredPosition.Y = _levelBounds.Bottom - (bounds.Height / 2);
            }

            setPositionWithLerp(desiredPosition);
        }

        private void setPositionWithLerp(Vector2 desiredPosition)
        {
            var desiredPositionDelta = new Vector2(desiredPosition.X - Position.X, desiredPosition.Y - Position.Y);
            Position = Vector2.Lerp(Position, Position + desiredPositionDelta, _cameraMoveLerp);
        }

        private Vector2 setMoveGoal(Vector2 moveGoal)
        {
            return moveGoal;
        }
    }
}
