using Components;
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
    class PlayerCharacterSelectActionState : BaseFieldState
    {
        private readonly string ActionMenuEntityName = "ActionMenu";

        private readonly Stack<GridTile> _returnPath;
        private bool _isAttackClicked = false;
        private bool _isWaitClicked = false;
        private bool _isCancelClicked = false;
        private List<GridTile> _tilesWithAttackableCharacters = new List<GridTile>();

        public PlayerCharacterSelectActionState(Stack<GridTile> returnPath) : base()
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
                scene.SelectedCharacter.GetComponent<TurnStateComponent>().finishedTurn = true;

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
                        scene.SelectedCharacter,
                        1000f, //TODO: should load this with entity from tiled or whatever
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
            var nextState = new PlayerCharacterSelectAttackState(_returnPath, _tilesWithAttackableCharacters);
            return nextState;
        }

        private Dictionary<string, Action<Button>> setupButtonDefinitions(LevelScene scene)
        {
            var buttonDefinitions = new Dictionary<string, Action<Button>>();
            
            if (!scene.SelectedCharacter.GetComponent<TurnStateComponent>().finishedTurn)
            {
                _tilesWithAttackableCharacters = scene.GetImmediateTilesWithAttackableCharacters(scene.SelectedCharacter.Position, false);

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
