using System;
using Nez;
using Microsoft.Xna.Framework;

namespace StratMono.Components
{
    public class Grid
    {
        public GridTile[,] GridTiles { get; }

        public Grid(int tileWidth, int tileHeight, int worldWidth, int worldHeight)
        {
            var gridTileWidth = tileWidth * 2;
            var gridTileHeight = tileHeight * 2;
            var mapWidthInGridTiles = worldWidth / gridTileWidth;
            var mapHeightInGridTiles = worldHeight / gridTileHeight;

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
    }

    public class GridTile
    {
        public GridTile()
        {
            
        }
    }
}
