using Microsoft.Xna.Framework;
using Nez;
using StratMono.System;
using System.Collections.Generic;
using StratMono.Util;

namespace StratMono.Components
{
    public class GridEntityMoveToGoal : Component, IUpdatable
    {
        private readonly int _moveSpeed = 1000;
        private Stack<GridTile> _pathToTake;

        public GridEntityMoveToGoal(Stack<GridTile> pathToTake)
        {
            _pathToTake = pathToTake;
        }

        public void Update()
        {
            Point entityPosition = new Point((int)Entity.Position.X, (int)Entity.Position.Y);
            GridTile nextGridTile = _pathToTake.Peek();
            if (entityPosition.Equals(nextGridTile.Position))
            {
                _pathToTake.Pop();

                if (_pathToTake.Count == 0)
                {
                    CharacterAnimatedMovement animatedMovement = Entity.GetComponent<CharacterAnimatedMovement>();
                    if (animatedMovement != null)
                    {
                        animatedMovement.MoveDirection = MovementDirection.DOWN;
                    }

                    Enabled = false;
                    return;
                }

                nextGridTile = _pathToTake.Peek();
            }

            handleMovement(nextGridTile, entityPosition);
        }

        private void handleMovement(GridTile nextGridTile, Point entityPosition)
        {
            CharacterAnimatedMovement animatedMovement = Entity.GetComponent<CharacterAnimatedMovement>();

            if (entityPosition.X != nextGridTile.Position.X)
            {
                var distanceToTravel = getDistanceToTravel(nextGridTile.Position.X, entityPosition.X);

                var newX = entityPosition.X + distanceToTravel;
                Entity.Position = new Vector2(newX, entityPosition.Y);

                if (animatedMovement != null)
                {
                    if (distanceToTravel > 0)
                    {
                        animatedMovement.MoveDirection = MovementDirection.RIGHT;
                    }
                    else
                    {
                        animatedMovement.MoveDirection = MovementDirection.LEFT;
                    }
                }
            }
            else
            {
                var distanceToTravel = getDistanceToTravel(nextGridTile.Position.Y, entityPosition.Y);

                var newY = entityPosition.Y + distanceToTravel;
                Entity.Position = new Vector2(entityPosition.X, newY);

                if (animatedMovement != null)
                {
                    if (distanceToTravel > 0)
                    {
                        animatedMovement.MoveDirection = MovementDirection.DOWN;
                    }
                    else
                    {
                        animatedMovement.MoveDirection = MovementDirection.UP;
                    }
                }
            }
        }

        private float getDistanceToTravel(int nextGridTileCoordinate, int entityPositionCoordinate)
        {
            var distanceToNextTile = nextGridTileCoordinate - entityPositionCoordinate;
            float distanceToTravel = _moveSpeed * Time.DeltaTime;

            if (distanceToNextTile < 0)
            {
                distanceToTravel = -distanceToTravel;
                if (distanceToTravel < distanceToNextTile)
                {
                    distanceToTravel = distanceToNextTile;
                }
            } else
            {
                if (distanceToTravel > distanceToNextTile)
                {
                    distanceToTravel = distanceToNextTile;
                }
            }

            return distanceToTravel;
        }
    }
}
