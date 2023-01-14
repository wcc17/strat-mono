using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StratMono.Entities;
using Nez.Tiled;
using StratMono.Util;
using Nez;
using StratMono.Components;

namespace StratMono.System
{
    public class GridSystem
    {
        private GridTile[,] _gridTiles;
        private int _mapWidthInGridTiles, _mapHeightInGridTiles;
        private Dictionary<string, Point> _entityToGridTileMap = new Dictionary<string, Point>();

        public int GridTileWidth, GridTileHeight;

        public GridSystem(
            Point tileDimensions,
            Point worldDimensions,
            TmxObjectGroup boundsObjectGroup,
            TmxObjectGroup moveCostObjectGroup)
        {
            GridTileWidth = tileDimensions.X * 2;
            GridTileHeight = tileDimensions.Y * 2;
            _mapWidthInGridTiles = worldDimensions.X / GridTileWidth;
            _mapHeightInGridTiles = worldDimensions.Y / GridTileHeight;
            _gridTiles = new GridTile[_mapWidthInGridTiles, _mapHeightInGridTiles];

            setupGridTiles(boundsObjectGroup, moveCostObjectGroup);
        }

        public void Update(List<GridEntity> gridEntities)
        {
            snapEntitiesToGrid(gridEntities);
        }

        public GridEntity AddToGridTile(GridEntity gridEntity, int x, int y)
        {
            _entityToGridTileMap.Add(gridEntity.Name, new Point(x, y));
            _gridTiles[x, y].AddToTile(gridEntity);

            var worldPosition = new Vector2(GridTileWidth * x, GridTileHeight * y);
            gridEntity.Position = worldPosition;

            return gridEntity;
        }

        // TODO: this is being called a ton by snapEntitiestoGrid, might be worth revisiting how its done
        public void RemoveFromGridTile(string name)
        {
            if (_entityToGridTileMap.ContainsKey(name))
            {
                Point oldGridTileCoords = _entityToGridTileMap[name];
                _entityToGridTileMap.Remove(name);
                _gridTiles[oldGridTileCoords.X, oldGridTileCoords.Y].RemoveFromTile(name);
            }
        }

        public GridTile GetTileForNextClosestEntity(Point coordinates, bool isClosestEntityEnemy)
        {
            var minDistance = float.MaxValue;
            GridTile minDistanceTile = null;

            for (var x = 0; x < _mapWidthInGridTiles; x++)
            {
                for (var y = 0; y < _mapHeightInGridTiles; y++)
                {
                    GridTile tile = _gridTiles[x, y];
                    if (tile.OccupyingEntities.Count > 0)
                    {
                        foreach (Entity entity in tile.OccupyingEntities)
                        {
                            var occupyingEntityIsEnemyForPlayer = isClosestEntityEnemy && entity.GetComponent<EnemyComponent>() != null;
                            var occupyingEntityIsPlayerForEnemy = !isClosestEntityEnemy && entity.GetComponent<EnemyComponent>() == null;

                            if (occupyingEntityIsEnemyForPlayer || occupyingEntityIsPlayerForEnemy)
                            {
                                var distance = Vector2.Distance(coordinates.ToVector2(), tile.Coordinates.ToVector2());
                                if (distance < minDistance)
                                {
                                    minDistance = distance;
                                    minDistanceTile = tile;
                                }
                            }
                        }
                    }
                }
            }

            return minDistanceTile;
        }

        public GridTile GetPointClosestToAnotherPointWithinRange(Point point, Point anotherPoint, int range)
        {
            var distance = Vector2.Distance(point.ToVector2(), anotherPoint.ToVector2());
            var t = range / distance;
            var nextClosestPoint = new Vector2(
                ((1 - t) * point.X) + (t * anotherPoint.X),
                ((1 - t) * point.Y) + (t * anotherPoint.Y)
            );

            return GetGridTileFromCoords(new Point((int)nextClosestPoint.X, (int)nextClosestPoint.Y));
        }

        public CharacterGridMovementInformation IdentifyPossibleTilesToMoveToTile(GridTile tile, int maxMovementCost)
        {
            var startTile = _gridTiles[tile.Coordinates.X, tile.Coordinates.Y];
            var frontier = new SimplePriorityQueue<GridTile>();
            var cameFrom = new Dictionary<GridTile, GridTile>();
            var costSoFar = new Dictionary<GridTile, int>();
            var inaccessibleTilesInRange = new HashSet<GridTile>();

            frontier.Enqueue(startTile, 0);
            cameFrom.Add(startTile, null); 
            costSoFar.Add(startTile, 0);

            while (frontier.Count > 0)
            {
                var currentTile = frontier.Dequeue();
                var costToGetHere = costSoFar[currentTile];

                if (costToGetHere < maxMovementCost)
                {
                    List<GridTile> neighbors = GetNeighborsOfTile(currentTile);
                    foreach (GridTile neighbor in neighbors)
                    {
                        if (neighbor.CharacterCanMoveThroughThisTile)
                        {
                            var newCost = costToGetHere + neighbor.MoveCost;

                            if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                            {
                                costSoFar.Add(neighbor, newCost);
                                cameFrom.Add(neighbor, currentTile);
                                frontier.Enqueue(neighbor, newCost);
                            }
                        } else
                        {
                            inaccessibleTilesInRange.Add(neighbor);
                        }
                    }
                } 
            }

            var tilesWithinRange = new HashSet<GridTile>(inaccessibleTilesInRange);
            foreach (var costTile in costSoFar.Keys)
            {
                tilesWithinRange.Add(costTile);
            }

            return new CharacterGridMovementInformation(cameFrom, tilesWithinRange);
        }

        public GridTile GetNearestTileAtPosition(Vector2 position)
        {
            var x = Math.Floor(position.X / GridTileWidth);
            var y = Math.Floor(position.Y / GridTileHeight);

            // Don't let any entity go off of the left side or top of screen
            x = (x < 0) ? 0 : x;
            y = (y < 0) ? 0 : y;

            // Don't let any entity go off of right side of bottom of screen
            x = (x >= _mapWidthInGridTiles) ? _mapWidthInGridTiles - 1 : x;
            y = (y >= _mapHeightInGridTiles) ? _mapHeightInGridTiles - 1 : y;

            return _gridTiles[(int)x, (int)y];
        }

        public GridTile GetGridTileFromCoords(Point coords)
        {
            return _gridTiles[coords.X, coords.Y];
        }

        public List<GridTile> GetNeighborsOfTile(GridTile tile)
        {
            var x = tile.Coordinates.X;
            var y = tile.Coordinates.Y;
            List<GridTile> neighbors = new List<GridTile>();

            if (x - 1 >= 0)
            {
                neighbors.Add(_gridTiles[x - 1, y]);
            }
            if (x + 1 < _mapWidthInGridTiles)
            {
                neighbors.Add(_gridTiles[x + 1, y]);
            }
            if (y - 1 >= 0)
            {
                neighbors.Add(_gridTiles[x, y - 1]);
            }
            if (y + 1 < _mapHeightInGridTiles)
            {
                neighbors.Add(_gridTiles[x, y + 1]);
            }

            return neighbors;
        }

        private Vector2 getGridTilePosition(Point gridTile)
        {
            return new Vector2(gridTile.X * GridTileWidth, gridTile.Y * GridTileHeight);
        }

        private void snapEntitiesToGrid(List<GridEntity> gridEntities)
        {
            foreach (var gridEntity in gridEntities)
            {
                var gridTile = GetNearestTileAtPosition(gridEntity.Position);
                RemoveFromGridTile(gridEntity.Name);
                AddToGridTile(gridEntity, (int)gridTile.Coordinates.X, (int)gridTile.Coordinates.Y);
            }
        }

        private void setupGridTiles(TmxObjectGroup boundsObjectGroup, TmxObjectGroup moveCostObjectGroup)
        {
            for (var x = 0; x < _mapWidthInGridTiles; x++)
            {
                for (var y = 0; y < _mapHeightInGridTiles; y++)
                {
                    var gridTileId = convert2dPointTo1dIndex(new Point(x, y), _mapWidthInGridTiles);
                    var gridTile = new GridTile(gridTileId, x, y, x * GridTileWidth, y * GridTileHeight);
                    _gridTiles[x, y] = gridTile;
                }
            }

            iterateObjectGroupRects(boundsObjectGroup, (x, y, obj) => 
            { 
                _gridTiles[(int)x, (int)y].IsAccessible = false; 
            });

            iterateObjectGroupRects(moveCostObjectGroup, (x, y, obj) =>
            {
                _gridTiles[(int)x, (int)y].MoveCost = Int32.Parse(obj.Properties["cost"]);
            });
        }

        private void iterateObjectGroupRects(TmxObjectGroup objectGroup, Action<int, int, TmxObject> iterateAction)
        {
            foreach (var obj in objectGroup.Objects)
            {
                var boundsRect = new Rectangle(
                    (int)obj.X,
                    (int)obj.Y,
                    (int)obj.Width,
                    (int)obj.Height
                );

                var startTileX = boundsRect.X / GridTileWidth;
                var endTileX = (boundsRect.X + boundsRect.Width) / GridTileWidth;
                for (var x = startTileX; x < endTileX; x++)
                {
                    var startTileY = boundsRect.Y / GridTileHeight;
                    var endTileY = (boundsRect.Y + boundsRect.Height) / GridTileHeight;
                    for (var y = startTileY; y < endTileY; y++)
                    {
                        iterateAction((int)x, (int)y, obj);
                    }
                }
            }
        }

        private Point convert1dIndexTo2dPoint(int index, int rowLength, int columnLength)
        {
            return new Point(index / rowLength, index % columnLength);
        }

        private int convert2dPointTo1dIndex(Point point, int rowLength)
        {
            return (point.X * rowLength) + point.Y;
        }
    }
}
