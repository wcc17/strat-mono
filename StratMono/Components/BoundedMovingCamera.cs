using System;
using System.Collections.Generic;
using System.Text;
using Nez;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StratMono.Util;

namespace StratMono.Components
{
    public class BoundedMovingCamera : Camera
    {
        private readonly int _cameraMoveSpeed = 1000;
        private readonly float _cameraMoveLerp = 0.7f;
        private readonly int _cameraMoveGoalSpeed = 2000;
        
        private readonly float _maximumZoom = 0.4f;
        private readonly float _minimumZoom = -0.4f;
        private readonly float _zoomSpeed = 0.2f;

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
            adjustPositionForBounds();

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
            Zoom += _zoomSpeed;
            Zoom = (Zoom > _maximumZoom) ? _maximumZoom : Zoom;
        }

        private void decreaseZoom()
        {
            Console.WriteLine("scroll down");
            Zoom -= _zoomSpeed;
            Zoom = (Zoom < _minimumZoom) ? _minimumZoom : Zoom;
        }

        private void handleMovement()
        {
            Vector2 desiredPosition = new Vector2(Position.X, Position.Y);

            if (_cameraMovementDirection.X > 0)
            {
                desiredPosition.X += (_cameraMoveSpeed * Time.DeltaTime);
            }
            if (_cameraMovementDirection.X < 0)
            {
                desiredPosition.X -= (_cameraMoveSpeed * Time.DeltaTime);
            }
            if (_cameraMovementDirection.Y > 0)
            {
                desiredPosition.Y += (_cameraMoveSpeed * Time.DeltaTime);
            }
            if (_cameraMovementDirection.Y < 0)
            {
                desiredPosition.Y -= (_cameraMoveSpeed * Time.DeltaTime);
            }

            setPositionWithLerp(desiredPosition);
        }

        private void handleMoveGoal()
        {
            Vector2 desiredPosition = new Vector2(Position.X, Position.Y);
            if (_moveGoal.X > desiredPosition.X)
            {
                desiredPosition.X += (_cameraMoveGoalSpeed * Time.DeltaTime);
                desiredPosition.X = (desiredPosition.X > _moveGoal.X) ? _moveGoal.X : desiredPosition.X;
            }
            if (_moveGoal.X < desiredPosition.X)
            {
                desiredPosition.X -= (_cameraMoveGoalSpeed * Time.DeltaTime);
                desiredPosition.X = (desiredPosition.X < _moveGoal.X) ? _moveGoal.X : desiredPosition.X;
            }
            if (_moveGoal.Y > desiredPosition.Y)
            {
                desiredPosition.Y += (_cameraMoveGoalSpeed * Time.DeltaTime);
                desiredPosition.Y = (desiredPosition.Y > _moveGoal.Y) ? _moveGoal.Y : desiredPosition.Y;
            }
            if (_moveGoal.Y < desiredPosition.Y)
            {
                desiredPosition.Y -= (_cameraMoveGoalSpeed * Time.DeltaTime);
                desiredPosition.Y = (desiredPosition.Y < _moveGoal.Y) ? _moveGoal.Y : desiredPosition.Y;
            }

            if (desiredPosition.X == _moveGoal.X && desiredPosition.Y == _moveGoal.Y)
            {
                Position = desiredPosition; // skipping lerp when we've gotten close enough to the destination
                _moveGoal = _noMoveGoal;
                return;
            }

            setPositionWithLerp(desiredPosition);
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

            //setPositionWithLerp(desiredPosition); this looked bad
            Position = desiredPosition;
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
