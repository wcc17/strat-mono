using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;

namespace StratMono.States
{
    public abstract class BaseState
    {
        public bool ReadyForInput = false;

        public abstract void EnterState(LevelScene scene);

        public abstract void ExitState(LevelScene scene);

        public abstract BaseState Update(LevelScene scene, GridEntity cursorEntity);

        protected virtual bool DidUserMakeSelection()
        {
            if (Input.LeftMouseButtonPressed
                || Input.GamePads[0].IsRightTriggerPressed()
                || Input.GamePads[0].IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                return true;
            }

            return false;
        }

        protected virtual void HandleReadyForInput()
        {
            // Gamepad buttons need a "debounce". This is the first time where it's been a problem
            // Pressing the button in one state causes that pressed/released status to leak into this 
            // state. Other states have movement, etc that add an artifical delay, so this wasn't needed
            // Just wait until the action buttons are completely not pressed before moving on
            // TODO: need to add this to BaseState somehow so that it could be re-used
            if (!ReadyForInput)
            {
                ReadyForInput = !Input.GamePads[0].IsButtonReleased(Buttons.A)
                    && !Input.GamePads[0].IsButtonReleased(Buttons.RightTrigger)
                    && !Input.GamePads[0].IsButtonPressed(Buttons.A)
                    && !Input.GamePads[0].IsButtonPressed(Buttons.RightTrigger)
                    && !Input.GamePads[0].IsButtonDown(Buttons.A)
                    && !Input.GamePads[0].IsButtonDown(Buttons.RightTrigger);
            }
        }

        protected virtual bool IsACancelButtonPressed()
        {
            return (Input.IsKeyPressed(Keys.Escape) || Input.GamePads[0].IsButtonPressed(Buttons.B));
        }

        protected virtual void CenterCameraOnPosition(LevelScene scene, Vector2 position)
        {
            var point = new Point((int)position.X, (int)position.Y);
            CenterCameraOnPosition(scene, point);
        }

        protected virtual void CenterCameraOnPosition(LevelScene scene, Point position)
        {
            // move the camera so that the selected tile is in the middle of the screen
            ((BoundedMovingCamera)scene.Camera).MoveGoal = new Vector2(
                position.X + (scene.GridSystem.GridTileWidth / 2),
                position.Y + (scene.GridSystem.GridTileHeight / 2));
        }
    }
}
