using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.UI;
using System;
using System.Collections.Generic;

namespace StratMono.States.Scene
{
    class EnemySelectedState : BaseState
    {
        private readonly string ActionMenuEntityName = "EnemyActionMenu";
        private bool _isCancelClicked = false;
        private bool _readyForInput = false;

        public override void EnterState(LevelScene scene)
        {
            scene.RemoveHighlightsFromGrid();

            var buttonDefinitions = new Dictionary<string, Action<Button>>();
            buttonDefinitions.Add("Cancel", button => _isCancelClicked = true);

            var uiCanvasEntity = MenuBuilder.BuildActionMenu(
                scene.font,
                ActionMenuEntityName, 
                buttonDefinitions);
            scene.AddEntity(uiCanvasEntity);
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            (scene.Camera as BoundedMovingCamera).Update();

            // Gamepad buttons need a "debounce". This is the first time where it's been a problem
            // Pressing the button in one state causes that pressed/released status to leak into this 
            // state. Other states have movement, etc that add an artifical delay, so this wasn't needed
            // Just wait until the action buttons are completely not pressed before moving on
            // TODO: need to add this to BaseState somehow so that it could be re-used
            if (!_readyForInput)
            {
                _readyForInput = !Input.GamePads[0].IsButtonReleased(Buttons.A)
                    && !Input.GamePads[0].IsButtonReleased(Buttons.RightTrigger)
                    && !Input.GamePads[0].IsButtonPressed(Buttons.A)
                    && !Input.GamePads[0].IsButtonPressed(Buttons.RightTrigger)
                    && !Input.GamePads[0].IsButtonDown(Buttons.A)
                    && !Input.GamePads[0].IsButtonDown(Buttons.RightTrigger);
                _isCancelClicked = false;
                return this;
            }

            if (Input.IsKeyPressed(Keys.Escape) || _isCancelClicked)
            {
                MenuBuilder.DestroyMenu(scene.FindEntity(ActionMenuEntityName));

                var nextState = new DefaultState();
                nextState.EnterState(scene);
                return nextState;
            }

            return this;
        }
    }
}
