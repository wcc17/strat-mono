using System;
using System.Timers;
using Nez;
using Microsoft.Xna.Framework;
using Nez.Sprites;
using StratMono.Util;

namespace StratMono.System
{
    class TileCursorSystem
    {
        private readonly int controllerCursorMoveSpeed = 64;
        private VirtualIntegerAxis _cursorMovementXAxisInput;
        private VirtualIntegerAxis _cursorMovementYAxisInput;
        private Vector2 _cursorMovementDirection = new Vector2(0, 0);
        private Timer _timer = new Timer();
        private bool _disableCursorControllerMovement;

        public TileCursorSystem()
        {
            _cursorMovementXAxisInput = new VirtualIntegerAxis();
            _cursorMovementYAxisInput = new VirtualIntegerAxis();
            _cursorMovementXAxisInput.Nodes.Add(new VirtualAxis.GamePadRightStickX());
            _cursorMovementYAxisInput.Nodes.Add(new VirtualAxis.GamePadRightStickY());
        }

        public void Update(Entity cursorEntity, Camera camera)
        {
            if (InputMode.CurrentInputMode == InputModeType.KeyboardMouse)
            {
                handleMouseMovement(cursorEntity, camera.Position);
            }
            else if (InputMode.CurrentInputMode == InputModeType.Controller)
            {
                handleControllerMovement(cursorEntity);
            }

            adjustPositionForCameraBounds(cursorEntity, camera.Bounds);
        }

        private void handleMouseMovement(Entity cursorEntity, Vector2 cameraPosition)
        {
            cursorEntity.Position = new Vector2(
                Input.MousePosition.X + (cameraPosition.X - Screen.Width / 2),
                Input.MousePosition.Y + (cameraPosition.Y - Screen.Height / 2));
        }

        private void handleControllerMovement(Entity cursorEntity)
        {
            if (!_disableCursorControllerMovement)
            {
                _cursorMovementDirection.X = _cursorMovementXAxisInput.Value;
                _cursorMovementDirection.Y = _cursorMovementYAxisInput.Value;

                if (_cursorMovementDirection.X > 0)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X + controllerCursorMoveSpeed,
                        cursorEntity.Position.Y);
                }

                if (_cursorMovementDirection.X < 0)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X - controllerCursorMoveSpeed,
                        cursorEntity.Position.Y);
                }

                if (_cursorMovementDirection.Y > 0)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X,
                        cursorEntity.Position.Y - controllerCursorMoveSpeed);
                }

                if (_cursorMovementDirection.Y < 0)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X,
                        cursorEntity.Position.Y + controllerCursorMoveSpeed);
                }

                if (_cursorMovementDirection.X != 0 || _cursorMovementDirection.Y != 0)
                {
                    _disableCursorControllerMovement = true;

                    // TODO: replace with https://github.com/prime31/Nez/blob/master/FAQs/Nez-Core.md#timermanager
                    _timer = new Timer(50);
                    _timer.Enabled = true;
                    _timer.Elapsed += (source, e) =>
                    {
                        _disableCursorControllerMovement = false;
                        _timer.Enabled = false;
                    };
                }
            }
        }

        private void adjustPositionForCameraBounds(Entity cursorEntity, Rectangle cameraBounds)
        {
            var cursorEntitySpriteAnimator = cursorEntity.GetComponent<SpriteAnimator>();
            if (cursorEntity.Position.X < cameraBounds.X)
            {
                cursorEntity.Position = new Vector2(
                    cameraBounds.X,
                    cursorEntity.Position.Y);
            }

            if (cursorEntity.Position.X + cursorEntitySpriteAnimator.Width > cameraBounds.Right)
            {
                cursorEntity.Position = new Vector2(
                    cameraBounds.Right - cursorEntitySpriteAnimator.Width,
                    cursorEntity.Position.Y);
            }

            if (cursorEntity.Position.Y < cameraBounds.Top)
            {
                cursorEntity.Position = new Vector2(
                    cursorEntity.Position.X,
                    cameraBounds.Y);
            }

            if (cursorEntity.Position.Y + cursorEntitySpriteAnimator.Height > cameraBounds.Bottom)
            {
                cursorEntity.Position = new Vector2(
                    cursorEntity.Position.X,
                    cameraBounds.Bottom - cursorEntitySpriteAnimator.Height);
            }
        }
    }
}
