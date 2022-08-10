﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.UI;
using System;
using System.Collections.Generic;

namespace StratMono.States.Scene
{
    class CharacterFinishedMovingState : BaseState
    {
        private readonly string ActionMenuEntityName = "ActionMenu";

        private readonly Stack<GridTile> _returnPath;
        private bool _isWaitClicked = false;
        private bool _isCancelClicked = false;

        public CharacterFinishedMovingState(Stack<GridTile> returnPath) : base()
        {
            _returnPath = returnPath;
        }

        public override void EnterState(LevelScene scene)
        {
            var buttonDefinitions = new Dictionary<string, Action<Button>>();
            buttonDefinitions.Add("Wait", button => _isWaitClicked = true);
            buttonDefinitions.Add("Cancel", button => _isCancelClicked = true);

            var uiCanvasEntity = MenuBuilder.BuildActionMenu(
                scene.font,
                ActionMenuEntityName,
                buttonDefinitions);
            scene.AddEntity(uiCanvasEntity);
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            if (Input.IsKeyPressed(Keys.Escape) || _isCancelClicked)
            {
                MenuBuilder.DestroyMenu(scene.FindEntity(ActionMenuEntityName));

                var nextState = new CharacterMovingState(
                    _returnPath,
                    returnedToOriginalPosition: true);
                nextState.EnterState(scene);
                return nextState;
            }

            if (_isWaitClicked)
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
