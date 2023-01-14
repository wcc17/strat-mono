using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System.Collections.Generic;

namespace StratMono.States.FieldState
{
    public class CharacterSelectedState : BaseFieldState
    {
        public override void EnterState(LevelScene scene) 
        {
            if (!scene.SelectedCharacterAlreadyFinishedTurn())
            {
                scene.SetupMovementTileHighlights();
            }
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            base.Update(scene, cursorEntity);

            if (scene.SelectedCharacterAlreadyFinishedTurn())
            {
                return goToCharacterSelectActionState(
                    scene, 
                    scene.GridSystem.GetNearestTileAtPosition(cursorEntity.Position), 
                    scene.SelectedCharacter);
            }

            if (DidUserMakeSelection())
            {
                GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntity.Position);
                CharacterGridEntity selectedCharacter = scene.GetCharacterFromSelectedTile(selectedTile);

                var movementInformation = scene.CharacterGridMovementInfo;
                bool noSelectedCharacter = selectedCharacter == null;
                bool userReselectedTile = selectedTile == scene.SelectedTile;
                bool tileNotInRange = !movementInformation.TilesInRangeOfCharacter.Contains(selectedTile);
                bool tileIsAccessible = selectedTile.CharacterCanMoveThroughThisTile;
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
                    return goToCharacterSelectActionState(scene, selectedTile, selectedCharacter);
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

                if (tileIsAccessible)
                { 
                    // TODO: if tile is not accessible, could potentially play a noise to signal to the user that they can't move there
                    return goToCharacterMovingState(scene, selectedTile);
                }
            }

            if (IsACancelButtonPressed())
            {
                return goToDefaultState(scene, null);
            }

            return this;
        }

        public override void ExitState(LevelScene scene)
        {
            scene.RemoveHighlightsFromGrid();
        }

        private BaseFieldState goToDefaultState(LevelScene scene, GridTile selectedTile)
        {
            // the user meant to deselect and go back to default state
            UpdateSceneSelections(scene, selectedTile, null);
            if (selectedTile != null)
            {
                CenterCameraOnPosition(scene, selectedTile.Position);
            }

            var nextState = new PlayerControlDefaultState();
            return nextState;
        }

        private BaseFieldState goToEnemySelectedState(
            LevelScene scene, 
            GridTile selectedTile, 
            CharacterGridEntity selectedCharacter)
        {
            UpdateSceneSelections(scene, selectedTile, selectedCharacter);
            CenterCameraOnPosition(scene, selectedTile.Position);

            var nextState = new EnemySelectedState();
            return nextState;
        }

        private BaseFieldState goToCharacterSelectedState(
            LevelScene scene, 
            GridTile selectedTile, 
            CharacterGridEntity selectedCharacter)
        {
            UpdateSceneSelections(scene, selectedTile, selectedCharacter);
            CenterCameraOnPosition(scene, selectedTile.Position);

            var nextState = new CharacterSelectedState();
            return nextState;
        }

        private BaseFieldState goToCharacterMovingState(LevelScene scene, GridTile selectedTile)
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
            return nextState;
        }

        private BaseFieldState goToCharacterSelectActionState(
            LevelScene scene, 
            GridTile selectedTile, 
            CharacterGridEntity selectedCharacter)
        {
            UpdateSceneSelections(scene, selectedTile, selectedCharacter);
            var nextState = new CharacterSelectActionState(null);
            return nextState;
        }
    }
}
