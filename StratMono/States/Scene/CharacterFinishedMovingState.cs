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
    class CharacterFinishedMovingState : BaseState
    {
        private readonly string ActionMenuEntityName = "ActionMenu";

        private readonly Stack<GridTile> _returnPath;
        private bool _isAttackClicked = false;
        private bool _isWaitClicked = false;
        private bool _isCancelClicked = false;
        private List<GridTile> _tilesWithAttackableCharacters = new List<GridTile>();

        public CharacterFinishedMovingState(Stack<GridTile> returnPath) : base()
        {
            _returnPath = returnPath;
        }

        public override void EnterState(LevelScene scene)
        {
            List<GridTile> neighbors = scene.GridSystem.GetNeighborsOfTile(_returnPath.Peek());
            foreach(var gridTile in neighbors)
            {
                var characterEntity = scene.GetCharacterFromSelectedTile(gridTile);
                if (characterEntity != null && characterEntity.GetComponent<EnemyComponent>() != null)
                {
                    _tilesWithAttackableCharacters.Add(gridTile);
                }
            }

            var buttonDefinitions = new Dictionary<string, Action<Button>>();
            if (_tilesWithAttackableCharacters.Count > 0)
            {
                buttonDefinitions.Add("Attack", button => _isAttackClicked = true);
            }
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
