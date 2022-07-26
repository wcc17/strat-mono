using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StratMono.Entities;

namespace StratMono.System
{
    public class GridSystem
    {
        public GridTile[,] GridTiles { get; }
        private int _gridTileWidth, _gridTileHeight;
        private int _mapWidthInGridTiles, _mapHeightInGridTiles;
        private Dictionary<string, Point> entityToGridTileMap = new Dictionary<string, Point>();

        public GridSystem(int tileWidth, int tileHeight, int worldWidth, int worldHeight)
        {
            _gridTileWidth = tileWidth * 2;
            _gridTileHeight = tileHeight * 2;
            _mapWidthInGridTiles = worldWidth / _gridTileWidth;
            _mapHeightInGridTiles = worldHeight / _gridTileHeight;

            GridTiles = new GridTile[_mapWidthInGridTiles, _mapHeightInGridTiles];
            for (var x = 0; x < GridTiles.GetLength(0); x++)
            {
                for (var y = 0; y < GridTiles.GetLength(1); y++)
                {
                    var gridTile = new GridTile();
                    GridTiles[x, y] = gridTile;
                }
            }
        }

        public GridEntity AddToGridTile(GridEntity gridEntity, int x, int y)
        {
            if (gridEntity.Name == "cursor")
            {
                //TODO: this behaves differently?
            }

            entityToGridTileMap.Add(gridEntity.Name, new Point(x, y));
            GridTiles[x, y].OccupyingEntity = gridEntity;

            var worldPosition = new Vector2(_gridTileWidth * x, _gridTileHeight * y);
            gridEntity.Position = worldPosition;

            return gridEntity;
        }

        public Vector2 GetNearestGridTile(Vector2 position)
        {
            var x = Math.Floor(position.X / _gridTileWidth);
            var y = Math.Floor(position.Y / _gridTileHeight);

            // Don't let any entity go off of the left side or top of screen
            x = (x < 0) ? 0 : x;
            y = (y < 0) ? 0 : y;

            //// Don't let any entity go off of right side of bottom of screen
            x = (x >= _mapWidthInGridTiles) ? _mapWidthInGridTiles - 1 : x;
            y = (y >= _mapHeightInGridTiles) ? _mapHeightInGridTiles - 1 : y;

            return new Vector2((int)x, (int)y);
        }

        public void SnapEntitiesToGrid(List<GridEntity> gridEntities)
        {
            foreach (var gridEntity in gridEntities)
            {
                Vector2 gridTile = GetNearestGridTile(gridEntity.Position);
                RemoveFromGridTile(gridEntity.Name);
                AddToGridTile(gridEntity, (int)gridTile.X, (int)gridTile.Y);
            }
        }

        public void RemoveFromGridTile(string name)
        {
            if (entityToGridTileMap.ContainsKey(name))
            {
                Point oldGridTileCoords = entityToGridTileMap[name];
                entityToGridTileMap.Remove(name);
                GridTiles[oldGridTileCoords.X, oldGridTileCoords.Y].OccupyingEntity = null;
            }
        }
    }

    public class GridTile
    {
        public GridEntity OccupyingEntity { get; set; }

        public GridTile()
        {
            
        }
    }
}
