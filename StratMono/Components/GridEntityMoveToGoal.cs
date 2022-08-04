using Microsoft.Xna.Framework;
using Nez;
using StratMono.System;
using System;
using System.Collections.Generic;
using StratMono.Util;

namespace StratMono.Components
{
    public class GridEntityMoveToGoal : Component, IUpdatable
    {
        private readonly int _moveSpeed = 10;
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
                Entity.Position = new Vector2(entityPosition.X + distanceToTravel, entityPosition.Y);

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
                Entity.Position = new Vector2(entityPosition.X, entityPosition.Y + distanceToTravel);

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

        private int getDistanceToTravel(int nextGridTileCoordinate, int entityPositionCoordinate)
        {
            var distanceToNextTile = Math.Abs(nextGridTileCoordinate - entityPositionCoordinate);
            var distanceToTravel = (Math.Abs(distanceToNextTile) > _moveSpeed) ? _moveSpeed : distanceToNextTile;
            distanceToTravel = (nextGridTileCoordinate < entityPositionCoordinate) ? -distanceToTravel : distanceToTravel;

            return distanceToTravel;
        }
    }
}
