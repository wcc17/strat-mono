using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace StratMono.Components
{
    public class GridSystem
    {
        public GridTile[,] GridTiles { get; }
        private int _gridTileWidth, _gridTileHeight;
        private Dictionary<string, Point> entityToGridTileMap = new Dictionary<string, Point>();

        public GridSystem(int tileWidth, int tileHeight, int worldWidth, int worldHeight)
        {
            _gridTileWidth = tileWidth * 2;
            _gridTileHeight = tileHeight * 2;
            var mapWidthInGridTiles = worldWidth / _gridTileWidth;
            var mapHeightInGridTiles = worldHeight / _gridTileHeight;

            GridTiles = new GridTile[mapWidthInGridTiles, mapHeightInGridTiles];
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
            gridEntity.SetPosition(worldPosition);

            return gridEntity;
        }

        public Vector2 GetNearestGridTile(Vector2 position)
        {
            var x = Math.Ceiling(position.X / _gridTileWidth);
            var y = Math.Ceiling(position.Y / _gridTileHeight);

            return new Vector2((int)x, (int)y);
        }

        public void SnapEntitiesToGrid(List<GridEntity> gridEntities)
        {
            //TODO: determine which grid tile they're closest to and snap them to it
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
