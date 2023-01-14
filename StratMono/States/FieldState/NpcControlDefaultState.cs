using Microsoft.Xna.Framework;
using Nez;
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
        readonly int _maxMovementCost = 5; // TODO: should actually store how far a character can travel somewhere 

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

            // Gather all places that this character could possibly move to in order to attack a character
            GridTile nextEnemyTile = scene.GridSystem.GetNearestTileAtPosition(nextEnemy.Position);
            CharacterGridMovementInformation movementInformation = scene.GetPossibleTilesToMoveTo(nextEnemyTile, _maxMovementCost);
            Dictionary<GridTile, int> attackableScores = new Dictionary<GridTile, int>();
            Dictionary<GridTile, float> distanceScores = new Dictionary<GridTile, float>();
            foreach (var tile in movementInformation.TilesInRangeOfCharacter)
            {
                if (tile.CharacterCanMoveThroughThisTile)
                {
                    List<GridTile> neighborTilesWithAttackable = scene.GetImmediateTilesWithAttackableCharacters(tile.Position.ToVector2(), true);
                    attackableScores.Add(tile, neighborTilesWithAttackable.Count);
                } else
                {
                    foreach(var entity in tile.OccupyingEntities)
                    {
                        if (entity.GetComponent<EnemyComponent>() == null) // if the other entity is not an enemy
                        {
                            var distance = Vector2.Distance(tile.Coordinates.ToVector2(), nextEnemyTile.Coordinates.ToVector2());
                            distanceScores.Add(tile, distance);
                        }
                    }
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

            // If there wasn't a single tile to move to that would put this enemy in range of a character to attack, then find the nearest character and move towards it
            // TODO: a better move would be to find out how to place the enemy closest to the largest group of characters. starting with simplest first though
            float bestDistance = float.MaxValue;
            if (bestTileScore == 0)
            {
                foreach (KeyValuePair<GridTile, float> entry in distanceScores)
                {
                    if (entry.Value < bestDistance)
                    {
                        bestDistance = entry.Value;
                        bestTile = entry.Key;
                    }
                }
            }

            // if there isn't a single character in range to move to, we have to explore outside of the movable range
            // TODO: ask the grid system for the closest character regardless of the current characters range
            // determine the spot within our range that would put us closest to that character
            if (bestTileScore == 0 && (bestDistance > _maxMovementCost))
            {
                GridTile nextClosestEntityTile = scene.GridSystem.GetTileForNextClosestEntity(nextEnemyTile.Coordinates, false);
                bestTile = scene.GridSystem.GetPointClosestToAnotherPointWithinRange(nextEnemyTile.Coordinates, nextClosestEntityTile.Coordinates, _maxMovementCost);
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

            return new DelayState(
                nextState: new CharacterMovingState(pathToTake, enemyToMove, 800f, isPlayerCharacter: false), //TODO: should load moveSpeed with entity from tiled or whatever)
                timeToDelay: 1f
            );

        }

        public override void ExitState(LevelScene scene) { }
    }
}
