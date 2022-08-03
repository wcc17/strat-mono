using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StratMono.Entities;
using Nez.Tiled;
using Nez;
using StratMono.Components;

namespace StratMono.System
{
    public class GridSystem
    {
        private GridTile[,] _gridTiles;
        private int _gridTileWidth, _gridTileHeight;
        private int _mapWidthInGridTiles, _mapHeightInGridTiles;
        private Dictionary<string, Point> _entityToGridTileMap = new Dictionary<string, Point>();

        private readonly Point _noSelectedTile = new Point(-1, -1);
        private Point _selectedTile = new Point(-1, -1);

        public GridSystem(
            Point tileDimensions,
            Point worldDimensions,
            TmxObjectGroup boundsObjectGroup,
            TmxObjectGroup moveCostObjectGroup)
        {
            _gridTileWidth = tileDimensions.X * 2;
            _gridTileHeight = tileDimensions.Y * 2;
            _mapWidthInGridTiles = worldDimensions.X / _gridTileWidth;
            _mapHeightInGridTiles = worldDimensions.Y / _gridTileHeight;
            _gridTiles = new GridTile[_mapWidthInGridTiles, _mapHeightInGridTiles];

            setupGridTiles(boundsObjectGroup, moveCostObjectGroup);
        }

        public void Update(Entity cursorEntity, List<GridEntity> gridEntities, BoundedMovingCamera camera)
        {
            handleInput(cursorEntity.Position, camera);
            snapEntitiesToGrid(gridEntities);
        }

        public GridEntity AddToGridTile(GridEntity gridEntity, int x, int y)
        {
            _entityToGridTileMap.Add(gridEntity.Name, new Point(x, y));
            _gridTiles[x, y].AddToTile(gridEntity);

            var worldPosition = new Vector2(_gridTileWidth * x, _gridTileHeight * y);
            gridEntity.Position = worldPosition;

            return gridEntity;
        }

        public void RemoveFromGridTile(string name)
        {
            if (_entityToGridTileMap.ContainsKey(name))
            {
                Point oldGridTileCoords = _entityToGridTileMap[name];
                _entityToGridTileMap.Remove(name);
                _gridTiles[oldGridTileCoords.X, oldGridTileCoords.Y].RemoveFromTile(name);
            }
        }

        public List<GridEntity> GetEntitiesFromSelectedTile()
        {
            if (!_selectedTile.Equals(_noSelectedTile))
            {
                return _gridTiles[(int)_selectedTile.X, (int)_selectedTile.Y].OccupyingEntities;
            }

            return null;
        }

        public HashSet<GridTile> IdentifyPossibleTilesToMoveToFromSelectedTile(int maxMovementCost)
        {
            var startTile = _gridTiles[_selectedTile.X, _selectedTile.Y];
            var frontier = new SimplePriorityQueue<GridTile>();
            var cameFrom = new Dictionary<GridTile, GridTile>();
            var costSoFar = new Dictionary<GridTile, int>();
            var inaccessibleTilesInRange = new HashSet<GridTile>();

            frontier.Enqueue(startTile, 0);
            cameFrom.Add(startTile, null); //TODO: this is all the paths we can take to a certain tile
            costSoFar.Add(startTile, 0);

            while (frontier.Count > 0)
            {
                var currentTile = frontier.Dequeue();
                var costToGetHere = costSoFar[currentTile];

                if (costToGetHere < maxMovementCost)
                {
                    List<GridTile> neighbors = getNeighborsOfTile(currentTile);
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
            foreach (var tile in costSoFar.Keys)
            {
                tilesWithinRange.Add(tile);
            }

            return tilesWithinRange;
        }

        public Point GetTileCoordinates(GridTile tile)
        {
            return this.convert1dIndexTo2dPoint(tile.Id, _mapWidthInGridTiles, _mapHeightInGridTiles);
        }

        private void handleInput(Vector2 currentCursorPosition, BoundedMovingCamera camera)
        {
            if (Input.LeftMouseButtonPressed 
                || Input.GamePads[0].IsRightTriggerPressed()
                || Input.GamePads[0].IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                selectCurrentTile(currentCursorPosition, camera);
            }
        }

        private Point getNearestGridTile(Vector2 position)
        {
            var x = Math.Floor(position.X / _gridTileWidth);
            var y = Math.Floor(position.Y / _gridTileHeight);

            // Don't let any entity go off of the left side or top of screen
            x = (x < 0) ? 0 : x;
            y = (y < 0) ? 0 : y;

            // Don't let any entity go off of right side of bottom of screen
            x = (x >= _mapWidthInGridTiles) ? _mapWidthInGridTiles - 1 : x;
            y = (y >= _mapHeightInGridTiles) ? _mapHeightInGridTiles - 1 : y;

            return new Point((int)x, (int)y);
        }

        private Vector2 getGridTilePosition(Point gridTile)
        {
            return new Vector2(gridTile.X * _gridTileWidth, gridTile.Y * _gridTileHeight);
        }

        private void selectCurrentTile(Vector2 currentCursorPosition, BoundedMovingCamera camera)
        {
            Point nearestGridTile = getNearestGridTile(currentCursorPosition);

            // if no tile is currently selected or if the selected tile is different from the current
            if (_selectedTile.Equals(_noSelectedTile) || !_selectedTile.Equals(nearestGridTile))
            {
                _selectedTile = nearestGridTile;

                // move the camera so that the selected tile is in the middle of the screen
                var selectedTilePosition = getGridTilePosition(_selectedTile);
                camera.MoveGoal = new Vector2(
                    selectedTilePosition.X + (_gridTileWidth / 2),
                    selectedTilePosition.Y + (_gridTileHeight / 2));
            } else
            {
                _selectedTile = _noSelectedTile;
            }
        }

        private void snapEntitiesToGrid(List<GridEntity> gridEntities)
        {
            foreach (var gridEntity in gridEntities)
            {
                Point gridTile = getNearestGridTile(gridEntity.Position);
                RemoveFromGridTile(gridEntity.Name);
                AddToGridTile(gridEntity, (int)gridTile.X, (int)gridTile.Y);
            }
        }

        private void setupGridTiles(TmxObjectGroup boundsObjectGroup, TmxObjectGroup moveCostObjectGroup)
        {
            for (var x = 0; x < _mapWidthInGridTiles; x++)
            {
                for (var y = 0; y < _mapHeightInGridTiles; y++)
                {
                    var gridTileId = convert2dPointTo1dIndex(new Point(x, y), _mapWidthInGridTiles);
                    var gridTile = new GridTile(gridTileId);
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

                var startTileX = boundsRect.X / _gridTileWidth;
                var endTileX = (boundsRect.X + boundsRect.Width) / _gridTileWidth;
                for (var x = startTileX; x < endTileX; x++)
                {
                    var startTileY = boundsRect.Y / _gridTileHeight;
                    var endTileY = (boundsRect.Y + boundsRect.Height) / _gridTileHeight;
                    for (var y = startTileY; y < endTileY; y++)
                    {
                        iterateAction((int)x, (int)y, obj);
                    }
                }
            }
        }

        private List<GridTile> getNeighborsOfTile(GridTile tile)
        {
            var gridCoords = convert1dIndexTo2dPoint(tile.Id, _mapWidthInGridTiles, _mapHeightInGridTiles);
            var x = gridCoords.X;
            var y = gridCoords.Y;
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

        private Point convert1dIndexTo2dPoint(int index, int rowLength, int columnLength)
        {
            return new Point(index / rowLength, index % columnLength);
        }

        private int convert2dPointTo1dIndex(Point point, int rowLength)
        {
            return (point.X * rowLength) + point.Y;
        }
    }

    public class GridTile
    {
        public int Id; // NOTE: also doubles as the index - can be converted to exact x,y coordinate by converting from 1d index to 2d index
        public List<GridEntity> OccupyingEntities = new List<GridEntity>();
        public int MoveCost;

        private bool _isAccessible;
        public bool IsAccessible 
        {
            get 
            {
                return _isAccessible;
            }

            set
            {
                _isAccessible = value;
                if (!_isAccessible)
                {
                    _characterCanMoveThroughThisTile = false;
                }
            }
        }

        private bool _characterCanMoveThroughThisTile;
        public bool CharacterCanMoveThroughThisTile 
        {
            get 
            {
                return _characterCanMoveThroughThisTile;
            }

            set 
            {
                if (!_isAccessible)
                {
                    _characterCanMoveThroughThisTile = false;
                }

                _characterCanMoveThroughThisTile = value;
            }
        }

        public GridTile(int id)
        {
            Id = id;
            MoveCost = 1;
            IsAccessible = true;
            CharacterCanMoveThroughThisTile = true;
        }

        public void AddToTile(GridEntity gridEntity)
        {
            OccupyingEntities.Add(gridEntity);

            if (gridEntity.GetType() == typeof(CharacterGridEntity))
            {
                CharacterCanMoveThroughThisTile = false;
            }
        }

        public void RemoveFromTile(string gridEntityName)
        {
            for (int i = OccupyingEntities.Count - 1; i >= 0; i--)
            {
                if (OccupyingEntities[i].Name.Equals(gridEntityName))
                {
                    if (OccupyingEntities[i].GetType() == typeof(CharacterGridEntity))
                    {
                        CharacterCanMoveThroughThisTile = true;
                    }

                    OccupyingEntities.RemoveAt(i);
                    break;
                }
            }
        }

        public override bool Equals(object obj)
        {
            var item = obj as GridTile;

            if (item == null)
            {
                return false;
            }

            return this.Id.Equals(item.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}
