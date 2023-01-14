using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System;
using System.Collections.Generic;

namespace StratMono.States.FieldState
{
    class PlayerCharacterSelectAttackState : BaseFieldState
    {
        private readonly Stack<GridTile> _returnPath;
        private readonly List<GridTile> _tilesWithAttackableCharacters;

        public PlayerCharacterSelectAttackState(
            Stack<GridTile> returnPath,
            List<GridTile> tilesWithAttackableCharacters)
        {
            _returnPath = returnPath;
            _tilesWithAttackableCharacters = tilesWithAttackableCharacters;
        }

        public override void EnterState(LevelScene scene)
        {
            foreach(GridTile tile in _tilesWithAttackableCharacters)
            {
                scene.CreateAndAddPositiveTileHighlight(tile);
            }
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            scene.SceneTileCursorSystem.Update(cursorEntity, scene.Camera);
            scene.GridSystem.Update(scene.EntitiesOfType<GridEntity>());

            if (DidUserMakeSelection())
            {
                // Character will be done with turn after attack finishes, but this is a point of no return so its safe to mark as completed here for now
                scene.FinishCharactersTurn(scene.SelectedCharacter.Id);

                GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntity.Position);
                if (_tilesWithAttackableCharacters.Contains(selectedTile))
                {
                    scene.CharacterBeingAttacked = scene.GetCharacterFromSelectedTile(selectedTile);
                    return goToBattleInitialState(scene);
                }
            }

            if (IsACancelButtonPressed())
            {
                return goToCharacterSelectActionState(scene, cursorEntity);
            }

            return this;
        }

        public override void ExitState(LevelScene scene)
        {
            scene.RemoveHighlightsFromGrid();
        }

        private BaseState goToCharacterSelectActionState(LevelScene scene, GridEntity cursorEntity)
        {
            cursorEntity.Position = scene.SelectedCharacter.Position;
            var nextState = new PlayerCharacterSelectActionState(_returnPath);
            return nextState;
        }

        private BaseState goToBattleInitialState(LevelScene scene)
        {
            var nextState = new BattleState.TransitionInState(
                scene.SelectedCharacter,
                scene.CharacterBeingAttacked,
                new FieldState.PlayerControlDefaultState());
            return nextState;
        }
    }
}
