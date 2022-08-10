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

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            base.Update(scene, cursorEntity);

            if (DidUserMakeSelection())
            {
                GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntity.Position);
                CharacterGridEntity selectedCharacter = scene.GetCharacterFromSelectedTile(selectedTile);

                var movementInformation = scene.CharacterGridMovementInfo;
                bool noSelectedCharacter = selectedCharacter == null;
                bool userReselectedTile = selectedTile == scene.SelectedTile;
                bool tileNotInRange = !movementInformation.TilesInRangeOfCharacter.Contains(selectedTile);
                bool userSelectedEnemy = selectedCharacter != null && selectedCharacter.GetComponent<EnemyComponent>() != null;

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

                if (userSelectedEnemy)
                {
                    // character meant to choose an enemy character, open their menu instead
                    return goToEnemySelectedState(scene, selectedTile, selectedCharacter);
                }

                if (selectedCharacter != null)
                {
                    // the user meant to switch to a different character, re-enter characterselectedState
                    return goToCharacterSelectedState(scene, selectedTile, selectedCharacter);
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

        private BaseState goToEnemySelectedState(
            LevelScene scene, 
            GridTile selectedTile, 
            CharacterGridEntity selectedCharacter)
        {
            UpdateSceneSelections(scene, selectedTile, selectedCharacter);
            CenterCameraOnPosition(scene, selectedTile.Position);

            var nextState = new EnemySelectedState();
            nextState.EnterState(scene);
            return nextState;
        }

        private BaseState goToCharacterSelectedState(
            LevelScene scene, 
            GridTile selectedTile, 
            CharacterGridEntity selectedCharacter)
        {
            UpdateSceneSelections(scene, selectedTile, selectedCharacter);
            CenterCameraOnPosition(scene, selectedTile.Position);

            var nextState = new CharacterSelectedState();
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

            var nextState = new CharacterMovingState(pathToTake);
            nextState.EnterState(scene);
            return nextState;
        }

    }
}
