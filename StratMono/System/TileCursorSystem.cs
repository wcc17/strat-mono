using System;
using Nez;
using Microsoft.Xna.Framework;
using Nez.Sprites;
using StratMono.Util;

namespace StratMono.System
{
    public class TileCursorSystem
    {
        private readonly float CursorMovementControllerDpadDelay = 0.1f;
        private readonly float CursorMovementControllerStickDelay = 0.01f;
        private readonly int ControllerCursorMoveSpeed = 64;

        private VirtualIntegerAxis _cursorMovementXAxisInput;
        private VirtualIntegerAxis _cursorMovementYAxisInput;
        private Vector2 _cursorMovementDirection = new Vector2(0, 0);
        private bool _disableCursorControllerMovement;

        public TileCursorSystem()
        {
            _cursorMovementXAxisInput = new VirtualIntegerAxis();
            _cursorMovementXAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _cursorMovementXAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());

            _cursorMovementYAxisInput = new VirtualIntegerAxis();
            _cursorMovementYAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _cursorMovementYAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
        }

        public void Update(Entity cursorEntity, Camera camera)
        {
            if (InputMode.CurrentInputMode == InputModeType.KeyboardMouse)
            {
                cursorEntity.Position = camera.MouseToWorldPoint();
            }
            else if (InputMode.CurrentInputMode == InputModeType.Controller)
            {
                handleControllerMovement(cursorEntity);
            }

            adjustPositionForCameraBounds(cursorEntity, camera.Bounds);
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
                        cursorEntity.Position.X + ControllerCursorMoveSpeed,
                        cursorEntity.Position.Y);
                }

                if (_cursorMovementDirection.X < 0)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X - ControllerCursorMoveSpeed,
                        cursorEntity.Position.Y);
                }

                if (_cursorMovementDirection.Y > 0)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X,
                        cursorEntity.Position.Y + ControllerCursorMoveSpeed);
                }

                if (_cursorMovementDirection.Y < 0)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X,
                        cursorEntity.Position.Y - ControllerCursorMoveSpeed);
                }

                if (_cursorMovementDirection.X != 0 || _cursorMovementDirection.Y != 0)
                {
                    var controllerDelay = CursorMovementControllerStickDelay;
                    // The delay for the left stick should be much shorter than the dpad, dpad is for finer control when using a controller
                    if (Input.GamePads[0].IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadLeft)
                        || Input.GamePads[0].IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadRight)
                        || Input.GamePads[0].IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadUp)
                        || Input.GamePads[0].IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadDown))
                    {
                        controllerDelay = CursorMovementControllerDpadDelay;
                    }

                    _disableCursorControllerMovement = true;
                    var timer = Core.Schedule(controllerDelay, repeats: false, (timer) =>
                    {
                        _disableCursorControllerMovement = false;
                    });
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
