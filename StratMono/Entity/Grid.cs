using System;
using Nez;
using Microsoft.Xna.Framework;
using Nez.Sprites;

namespace StratMono.Components
{
    public class Grid
    {
        public GridTile[,] GridTiles { get; }
        private int _gridTileWidth, _gridTileHeight;

        public Grid(int tileWidth, int tileHeight, int worldWidth, int worldHeight)
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
            //TODO: check if something is already here
            //TODO: check if this is a boundary 

            //TODO: need to figure out how to remove old OccupyingEntity
            //GridTiles[x, y].OccupyingEntity = gridEntity;

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
    }

    public class GridTile
    {
        public GridEntity OccupyingEntity { get; set; }

        public GridTile()
        {
            
        }
    }
}
