using Microsoft.Xna.Framework;
using Nez.Sprites;
using StartMono.Util;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.Util;
using System;
using System.Collections.Generic;

namespace StratMono.States.Scene
{
    public class CharacterSelectedState : BaseState
    {
        public override void EnterState(LevelScene scene) 
        {
            scene.RemoveHighlightsFromGrid();
            scene.SetupMovementTileHighlights();
        }

        public override BaseState Update(LevelScene scene, Vector2 cursorEntityPosition)
        {
            scene.GridSystem.Update(scene.EntitiesOfType<GridEntity>());

            if (DidUserMakeSelection())
            {
                GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntityPosition);
                CharacterGridEntity selectedCharacter = GetCharacterFromSelectedTile(selectedTile);

                var movementInformation = scene.CharacterGridMovementInfo;
                bool noSelectedCharacter = selectedCharacter == null;
                bool userReselectedTile = selectedTile == scene.SelectedTile;
                bool tileNotInRange = !movementInformation.TilesInRangeOfCharacter.Contains(selectedTile);
                
                if (noSelectedCharacter && userReselectedTile)
                {
                    return goToDefaultState(scene, selectedTile);
                }

                if (noSelectedCharacter && tileNotInRange)
                {
                    return goToDefaultState(scene, selectedTile);
                }

                if (selectedCharacter == scene.SelectedCharacter)
                {
                    return goToDefaultState(scene, selectedTile);
                }

                if (selectedCharacter != null)
                {
                    // the user meant to switch to a different character, re-enter characterselectedState
                    UpdateSceneSelections(scene, selectedTile, selectedCharacter);
                    CenterCameraOnPosition(scene, selectedTile.Position);

                    var nextState = new CharacterSelectedState();
                    nextState.EnterState(scene);
                    return nextState;
                }

                return goToCharacterMovingState(scene, selectedTile);
            }

            return this;
        }

        private BaseState goToDefaultState(LevelScene scene, GridTile selectedTile)
        {
            // the user meant to deselect and go back to default state
            UpdateSceneSelections(scene, selectedTile, null);
            CenterCameraOnPosition(scene, selectedTile.Position);
            var nextState = new DefaultState();
            nextState.EnterState(scene);
            return nextState;
        }

        private BaseState goToCharacterMovingState(LevelScene scene, GridTile selectedTile)
        {
            Dictionary<GridTile, GridTile> allPathsFromCharacter 
                = scene.CharacterGridMovementInfo.PathsFromCharacterToTilesInRange;

            GridTile nextTile = selectedTile;
            Stack<GridTile> pathToTake = new Stack<GridTile>();
            while (nextTile != null)
            {
                pathToTake.Push(nextTile);
                allPathsFromCharacter.TryGetValue(nextTile, out nextTile);
            }

            scene.SelectedCharacter.AddComponent(new GridEntityMoveToGoal(pathToTake));

            var nextState = new CharacterMovingState();
            nextState.EnterState(scene);
            return nextState;
        }

    }
}
