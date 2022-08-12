using Nez.UI;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.UI;
using System;
using System.Collections.Generic;

namespace StratMono.States.FieldState
{
    class EnemySelectedState : BaseFieldState
    {
        private readonly string ActionMenuEntityName = "EnemyActionMenu";
        private bool _isCancelClicked = false;

        public override void EnterState(LevelScene scene)
        {
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

            HandleReadyForInput();
            if (!ReadyForInput)
            {
                _isCancelClicked = false;
                return this;
            }

            if (IsACancelButtonPressed() || _isCancelClicked)
            {
                MenuBuilder.DestroyMenu(scene.FindEntity(ActionMenuEntityName));

                var nextState = new DefaultState();
                return nextState;
            }

            return this;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
