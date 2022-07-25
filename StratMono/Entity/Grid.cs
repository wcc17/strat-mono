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

        public Entity AddCharacterToGridTile(Character character, int x, int y)
        {
            //TODO: check if something is already here
            //TODO: check if this is a boundary 
            //TODO: probably not the best idea to rely on the sprite animator being there?

            GridTiles[x, y].OccupyingCharacter = character;
            var worldPosition = new Vector2(_gridTileWidth * x, _gridTileHeight * y);
            character.SetCharacterPosition(worldPosition);

            return character;
        }
    }

    public class GridTile
    {
        public Entity OccupyingCharacter { get; set; }

        public GridTile()
        {
            
        }
    }
}
