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
    class CharacterSelectAttackState : BaseState
    {
        private readonly Stack<GridTile> _returnPath;
        private readonly List<GridTile> _tilesWithAttackableCharacters;

        public CharacterSelectAttackState(
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
                GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntity.Position);
                if (_tilesWithAttackableCharacters.Contains(selectedTile))
                {
                    Console.WriteLine("start the attack");
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
            var nextState = new CharacterSelectActionState(_returnPath);
            return nextState;
        }
    }
}
