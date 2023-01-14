using Nez;
using Nez.UI;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.UI;
using System;
using System.Collections.Generic;

namespace StratMono.States.FieldState
{
    class CharacterSelectActionState : BaseFieldState
    {
        private readonly string ActionMenuEntityName = "ActionMenu";

        private readonly Stack<GridTile> _returnPath;
        private bool _isAttackClicked = false;
        private bool _isWaitClicked = false;
        private bool _isCancelClicked = false;
        private List<GridTile> _tilesWithAttackableCharacters = new List<GridTile>();

        public CharacterSelectActionState(Stack<GridTile> returnPath) : base()
        {
            _returnPath = returnPath;
        }

        public override void EnterState(LevelScene scene)
        {
            var buttonDefinitions = setupButtonDefinitions(scene);

            var menuEntity = MenuBuilder.BuildActionMenu(
                scene.font,
                ActionMenuEntityName,
                buttonDefinitions);
            scene.AddEntity(menuEntity);
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            HandleReadyForInput();
            if (!ReadyForInput)
            {
                _isAttackClicked = false;
                _isWaitClicked = false;
                _isCancelClicked = false;
                return this;
            }

            if (IsACancelButtonPressed() || _isCancelClicked)
            {
                MenuBuilder.DestroyMenu(scene.FindEntity(ActionMenuEntityName));
                return (_returnPath != null) ? goToCharacterMovingState(scene) : goToDefaultState(scene);
            }

            if (_isWaitClicked)
            {
                // Character will be done with turn after waiting completes
                scene.FinishSelectedCharactersTurn();

                MenuBuilder.DestroyMenu(scene.FindEntity(ActionMenuEntityName));
                return goToDefaultState(scene);
            }

            if (_isAttackClicked)
            {
                MenuBuilder.DestroyMenu(scene.FindEntity(ActionMenuEntityName));
                return goToCharacterSelectAttackState(scene);
            }

            return this;
        }

        public override void ExitState(LevelScene scene) { }

        private BaseFieldState goToCharacterMovingState(LevelScene scene)
        {
            var nextState = new CharacterMovingState(
                        _returnPath,
                        returnedToOriginalPosition: true);
            return nextState;
        }

        private BaseFieldState goToDefaultState(LevelScene scene)
        {
            var nextState = new PlayerControlDefaultState();
            return nextState;
        }

        private BaseFieldState goToCharacterSelectAttackState(LevelScene scene)
        {
            var nextState = new CharacterSelectAttackState(_returnPath, _tilesWithAttackableCharacters);
            return nextState;
        }

        private Dictionary<string, Action<Button>> setupButtonDefinitions(LevelScene scene)
        {
            var buttonDefinitions = new Dictionary<string, Action<Button>>();
            
            if (!scene.SelectedCharacterAlreadyFinishedTurn())
            {
                GridTile selectedCharacterTile = scene.GridSystem.GetNearestTileAtPosition(scene.SelectedCharacter.Position);
                List<GridTile> neighbors = scene.GridSystem.GetNeighborsOfTile(selectedCharacterTile);

                foreach (var gridTile in neighbors)
                {
                    var characterEntity = scene.GetCharacterFromSelectedTile(gridTile);
                    if (characterEntity != null && characterEntity.GetComponent<EnemyComponent>() != null)
                    {
                        _tilesWithAttackableCharacters.Add(gridTile);
                    }
                }

                if (_tilesWithAttackableCharacters.Count > 0)
                {
                    buttonDefinitions.Add("Attack", button => _isAttackClicked = true);
                }

                buttonDefinitions.Add("Wait", button => _isWaitClicked = true);
            }

            buttonDefinitions.Add("Cancel", button => _isCancelClicked = true);

            return buttonDefinitions;
        }
    }
}
