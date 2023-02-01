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
            // TODO: three major problems here:
            // on the distance scores, it's possible for the character to end up on a tile that a player is standing on, rather than the one next to it
            // on the next closest entity tile, we don't consider the "tilesInRangeOfEnemy" variable at all. We need to include this
            // in the Gridsystem, when we add or remove from tile, we use the "CharacterCanMoveThroughThisTile". But this is only set for characters stepping through enemies, not the other way around 

            // Gather all places that this character could possibly move to in order to attack a character
            Dictionary<GridTile, int> attackableScores = new Dictionary<GridTile, int>();
            Dictionary<GridTile, float> distanceScores = new Dictionary<GridTile, float>();
            foreach (var tile in tilesInRangeOfEnemy)
            {
                if (tile.CharacterCanMoveThroughThisTile 
                    && tile.OccupyingEntities.Count > 0 
                    && tile.OccupyingEntities[0].GetComponent<EnemyComponent>() == null)
                {
                    Console.WriteLine("should not be able to move to this tile?");
                }

                if (tile.CharacterCanMoveThroughThisTile)
                {
                    List<GridTile> neighborTilesWithAttackable = scene.GetImmediateTilesWithAttackableCharacters(tile.Position.ToVector2(), true);
                    attackableScores.Add(tile, neighborTilesWithAttackable.Count);
                }
                else
                {
                    foreach (var entity in tile.OccupyingEntities)
                    {
                        if (entity.GetComponent<EnemyComponent>() == null) // if the other entity is not an enemy
                        {
                            var distance = Vector2.Distance(tile.Coordinates.ToVector2(), enemyTileCoordinates.ToVector2());
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

            // if there isn't a single character in range to move to, try to move as close as possible to another non-enemy character
            if (bestTileScore == 0 && (bestDistance > _maxMovementCost))
            {
                GridTile nextClosestEntityTile = scene.GridSystem.GetTileForNextClosestEntity(enemyTileCoordinates, false);
                bestTile = scene.GridSystem.GetPointClosestToAnotherPointWithinRange(enemyTileCoordinates, nextClosestEntityTile.Coordinates, _maxMovementCost);
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
