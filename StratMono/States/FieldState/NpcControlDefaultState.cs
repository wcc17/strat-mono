using Microsoft.Xna.Framework;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.Util;
using System;
using System.Collections.Generic;

namespace StratMono.States.FieldState
{
    public class NpcControlDefaultState : BaseFieldState
    {
        public override void EnterState(LevelScene scene) { }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            base.Update(scene, cursorEntity);

            if (scene.AllEnemiesFinishedTurn())
            {
                scene.ResetFinishedTurns();
                return new PlayerControlDefaultState();
            }

            // See if we can immediately attack a character (TODO: in later difficulties, it would probably be best to determine if a movement and attacking another character would be better)
            CharacterGridEntity nextEnemy = scene.GetNextEnemy();
            if (scene.GetImmediateTilesWithAttackableCharacters(nextEnemy.Position, true).Count > 0)
            {
                // attack and move on
                Console.WriteLine("should have attacked");
            }

            // Gather all places that this character could possibly move to
            GridTile nextEnemyTile = scene.GridSystem.GetNearestTileAtPosition(nextEnemy.Position);
            CharacterGridMovementInformation movementInformation = scene.GetPossibleTilesToMoveTo(nextEnemyTile, 5); // TODO: should actually store how far a character can travel somewhere (maxMovementCost)
            Dictionary<GridTile, int> gridTileScores = new Dictionary<GridTile, int>();
            foreach (var tile in movementInformation.TilesInRangeOfCharacter)
            {
                if (tile.CharacterCanMoveThroughThisTile)
                {
                    List<GridTile> neighborTilesWithAttackable = scene.GetImmediateTilesWithAttackableCharacters(new Vector2(tile.Position.X, tile.Position.Y), true);
                    gridTileScores.Add(tile, neighborTilesWithAttackable.Count);
                }
            }

            // Determine the best place for this character to move to
            GridTile bestTile = null;
            int bestTileScore = -1;
            foreach (KeyValuePair<GridTile, int> entry in gridTileScores)
            {
                if (entry.Value > bestTileScore)
                {
                    bestTileScore = entry.Value;
                    bestTile = entry.Key;
                }
            }

            // TODO: this will need to be set after actually attacking when that is implemented. for now, is safe to set before moving, turn will be done directly after moving
            scene.FinishCharactersTurn(nextEnemy.Id);

            // Now actually move
            return goToCharacterMovingState(scene, bestTile, movementInformation.PathsFromCharacterToTilesInRange, nextEnemy);

            // Attack if possible
            // TODO: how?

            /**
             *  steps:
             *      determine next best action
             *      
             *      are there any team characters in range to attack?
             *          TODO: if there are more than one, what are the conditions to decide which to attack
             *          if there is only one, attack the closest one
             *          
             *      if there are no team characters in range to attack, move
             *          short term goal: move to the spot that will get us closest to the next closest character 
             *          
             *          long term goal: score spots based on a combination of factors that could be manipulated/ignored to affect difficulty of game
             *              what characters are in range of a spot that will result in most possible nearby team characters (could be done today, but not until the rest of the scoring stuff is ready)
             *              what characters are in range of a spot that will result in most low health characters near that spot
             *              what characters are in range of a spot that could potentially kill this enemy on next turn
             *                  does the character have an attack strong against this enemy?
             *                  does the character have a chance of killing the enemy in one hit next turn?
             *          
             *      once moved, decide whether to attack (if theres a team character in range) or just wait until next turn 
             */

            //return this;

            //BaseFieldState nextState = this;
            //if (DidUserMakeSelection())
            //{
            //    // default state doesn't care if selected tile or character changed
            //    GridTile selectedTile = scene.GridSystem.GetNearestTileAtPosition(cursorEntity.Position);
            //    CharacterGridEntity selectedCharacter = scene.GetCharacterFromSelectedTile(selectedTile);

            //    UpdateSceneSelections(scene, selectedTile, selectedCharacter);
            //    CenterCameraOnPosition(scene, selectedTile.Position);

            //    if (selectedCharacter != null && selectedCharacter.GetComponent<EnemyComponent>() != null)
            //    {
            //        nextState = new EnemySelectedState();
            //    }
            //    else if (selectedCharacter != null)
            //    {
            //        nextState = new CharacterSelectedState();
            //    }
            //}
            //return nextState;
        }

        private BaseFieldState goToCharacterMovingState(LevelScene scene, GridTile selectedTile, Dictionary<GridTile, GridTile> allPathsFromCharacter, CharacterGridEntity enemyToMove)
        {
            GridTile nextTile = selectedTile;
            Stack<GridTile> pathToTake = new Stack<GridTile>();
            while (nextTile != null)
            {
                pathToTake.Push(nextTile);
                allPathsFromCharacter.TryGetValue(nextTile, out nextTile);
            }

            var nextState = new CharacterMovingState(pathToTake, enemyToMove, isPlayerCharacter: false);
            return nextState;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
