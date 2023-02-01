using Components.Character.Enemy;
using Microsoft.Xna.Framework;
using States.Shared;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.Util;
using System;
using System.Collections.Generic;

namespace StratMono.States.FieldState
{
    public class NpcStartMovementState : BaseFieldState
    {
        private readonly int _maxMovementCost = 5; // TODO: should actually store how far a character can travel somewhere
        private CharacterGridEntity _enemy;

        public NpcStartMovementState(CharacterGridEntity enemy)
        {
            this._enemy = enemy;
        }

        public override void EnterState(LevelScene scene) { }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            base.Update(scene, cursorEntity);

            GridTile enemyTile = scene.GridSystem.GetNearestTileAtPosition(_enemy.Position);
            CharacterGridMovementInformation movementInformation = scene.GetPossibleTilesToMoveTo(enemyTile, _maxMovementCost);
            GridTile bestTile = determineBestTileToMoveTo(scene, movementInformation.TilesInRangeOfCharacter, enemyTile.Coordinates);

            // Now actually move
            return goToCharacterMovingState(bestTile, movementInformation.PathsFromCharacterToTilesInRange, _enemy);
        }

        private GridTile determineBestTileToMoveTo(LevelScene scene, HashSet<GridTile> tilesInRangeOfEnemy, Point enemyTileCoordinates)
        {
            // Gather all places that this character could possibly move to in order to attack a character
            Dictionary<GridTile, int> attackableScores = new Dictionary<GridTile, int>();
            foreach (var tile in tilesInRangeOfEnemy)
            {

                if (tile.CharacterCanMoveThroughThisTile)
                {
                    List<GridTile> neighborTilesWithAttackable = scene.GetImmediateTilesWithAttackableCharacters(tile.Position.ToVector2(), true);
                    attackableScores.Add(tile, neighborTilesWithAttackable.Count);
                }
            }

            // Determine the best place for this character to move to
            GridTile bestTile = null;
            int bestTileScore = -1;
            foreach (KeyValuePair<GridTile, int> entry in attackableScores)
            {
                if (entry.Value > bestTileScore)
                {
                    bestTileScore = entry.Value;
                    bestTile = entry.Key;
                }
            }

            // if there isn't a single character in range to move to, try to move as close as possible to another non-enemy character
            if (bestTileScore == 0)
            {
                GridTile nextClosestEntityTile = scene.GridSystem.GetTileForNextClosestEntity(enemyTileCoordinates, false);
                var range = _maxMovementCost;
                do
                {
                    if (range == 0)
                    {
                        Console.WriteLine("should not happen");
                    }

                    bestTile = scene.GridSystem.GetPointClosestToAnotherPointWithinRange(enemyTileCoordinates, nextClosestEntityTile.Coordinates, range);
                    range -= 1;
                } while (!bestTile.CharacterCanMoveThroughThisTile || !tilesInRangeOfEnemy.Contains(bestTile));
            }

            return bestTile;

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
        }

        private BaseFieldState goToCharacterMovingState(GridTile selectedTile, Dictionary<GridTile, GridTile> allPathsFromCharacter, CharacterGridEntity enemyToMove)
        {
            GridTile nextTile = selectedTile;
            Stack<GridTile> pathToTake = new Stack<GridTile>();
            while (nextTile != null)
            {
                pathToTake.Push(nextTile);
                allPathsFromCharacter.TryGetValue(nextTile, out nextTile);
            }

            return new DelayState(
                nextState: new CharacterMovingState(pathToTake, enemyToMove, 800f, isPlayerCharacter: false), //TODO: should load moveSpeed with entity from tiled or whatever)
                timeToDelay: 1f
            );

        }

        public override void ExitState(LevelScene scene) { }
    }
}
