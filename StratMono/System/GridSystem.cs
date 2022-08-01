using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StratMono.Entities;
using Nez.Tiled;
using Nez;

namespace StratMono.System
{
    public class GridSystem
    {
        public GridTile[,] GridTiles { get; }
        private int _gridTileWidth, _gridTileHeight;
        private int _mapWidthInGridTiles, _mapHeightInGridTiles;
        private Dictionary<string, Point> entityToGridTileMap = new Dictionary<string, Point>();

        private readonly Vector2 NoSelectedTile = new Vector2(-1, -1);
        public Vector2 selectedTile = new Vector2(-1, -1);

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
            GridTiles = new GridTile[_mapWidthInGridTiles, _mapHeightInGridTiles];

            setupGridTiles(boundsObjectGroup, moveCostObjectGroup);
        }

        public void Update(Entity cursorEntity, List<GridEntity> gridEntities)
        {
            handleInput(cursorEntity.Position);
            snapEntitiesToGrid(gridEntities);
        }

        public GridEntity AddToGridTile(GridEntity gridEntity, int x, int y)
        {
            entityToGridTileMap.Add(gridEntity.Name, new Point(x, y));
            GridTiles[x, y].AddToTile(gridEntity);

            var worldPosition = new Vector2(_gridTileWidth * x, _gridTileHeight * y);
            gridEntity.Position = worldPosition;

            return gridEntity;
        }

        private void handleInput(Vector2 currentCursorPosition)
        {
            if (Input.LeftMouseButtonPressed 
                || Input.GamePads[0].IsRightTriggerPressed()
                || Input.GamePads[0].IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
            {
                selectCurrentTile(currentCursorPosition);
                Console.WriteLine(selectedTile);
            }
        }

        private Vector2 getNearestGridTile(Vector2 position)
        {
            var x = Math.Floor(position.X / _gridTileWidth);
            var y = Math.Floor(position.Y / _gridTileHeight);

            // Don't let any entity go off of the left side or top of screen
            x = (x < 0) ? 0 : x;
            y = (y < 0) ? 0 : y;

            // Don't let any entity go off of right side of bottom of screen
            x = (x >= _mapWidthInGridTiles) ? _mapWidthInGridTiles - 1 : x;
            y = (y >= _mapHeightInGridTiles) ? _mapHeightInGridTiles - 1 : y;

            return new Vector2((int)x, (int)y);
        }

        private void removeFromGridTile(string name)
        {
            if (entityToGridTileMap.ContainsKey(name))
            {
                Point oldGridTileCoords = entityToGridTileMap[name];
                entityToGridTileMap.Remove(name);
                GridTiles[oldGridTileCoords.X, oldGridTileCoords.Y].RemoveFromTile(name);
            }
        }

        private void selectCurrentTile(Vector2 currentCursorPosition)
        {
            Vector2 nearestGridTile = getNearestGridTile(currentCursorPosition);
            if (selectedTile.Equals(NoSelectedTile)) // if no tile is currently selected
            {
                selectedTile = nearestGridTile;
            } else if (!selectedTile.Equals(nearestGridTile)) //if selected tile doesn't equal the new selected tile
            {
                selectedTile = nearestGridTile;
            } else
            {
                selectedTile = NoSelectedTile;
            }
        }

        private void snapEntitiesToGrid(List<GridEntity> gridEntities)
        {
            foreach (var gridEntity in gridEntities)
            {
                Vector2 gridTile = getNearestGridTile(gridEntity.Position);
                removeFromGridTile(gridEntity.Name);
                AddToGridTile(gridEntity, (int)gridTile.X, (int)gridTile.Y);
            }
        }

        private void setupGridTiles(TmxObjectGroup boundsObjectGroup, TmxObjectGroup moveCostObjectGroup)
        {
            for (var x = 0; x < GridTiles.GetLength(0); x++)
            {
                for (var y = 0; y < GridTiles.GetLength(1); y++)
                {
                    var gridTile = new GridTile();
                    GridTiles[x, y] = gridTile;
                }
            }

            iterateObjectGroupRects(boundsObjectGroup, (x, y, obj) => 
            { 
                GridTiles[(int)x, (int)y].isAccessible = false; 
            });

            iterateObjectGroupRects(moveCostObjectGroup, (x, y, obj) =>
            {
                GridTiles[(int)x, (int)y].moveCost = Int32.Parse(obj.Properties["cost"]);
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
    }

    public class GridTile
    {
        public Dictionary<string, GridEntity> OccupyingEntities = new Dictionary<string, GridEntity>();
        public int moveCost;
        public bool isAccessible;

        public GridTile()
        {
            isAccessible = true;
            moveCost = 1;
        }

        public void AddToTile(GridEntity gridEntity)
        {
            OccupyingEntities.Add(gridEntity.Name, gridEntity);
        }

        public void RemoveFromTile(string gridEntityName)
        {
            if (OccupyingEntities.ContainsKey(gridEntityName))
            {
                OccupyingEntities.Remove(gridEntityName);
            }
        }
    }
}
