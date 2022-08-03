using System;
using Microsoft.Xna.Framework;
using Nez.Sprites;
using StartMono.Util;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.Util;

namespace StratMono.States.Scene
{
    public class DefaultState : BaseState
    {
        private readonly Color BlueFill = new Color(99, 155, 255, 200);
        private readonly Color BlueOutline = new Color(91, 110, 225, 200);
        private readonly Color RedFill = new Color(217, 87, 99, 200);
        private readonly Color RedOutline = new Color(172, 50, 50, 200);

        public override BaseState HandleSelectedCharacter(
            LevelScene scene, 
            GridSystem gridSystem)
        {
            if (scene.SelectedCharacter == null)
            {
                return this;
            }

            // TODO: should actually store how far a character can travel somewhere (maxMovementCost)
            var tilesInRange = gridSystem.IdentifyPossibleTilesToMoveToFromSelectedTile(5);
            foreach (GridTile tile in tilesInRange)
            {
                GridEntity tileHighlight = new GridTileHighlight("highlight" + tile.Id);
                SpriteRenderer outline;
                SpriteRenderer shape;
                if (tile.CharacterCanMoveThroughThisTile)
                {
                    outline = PrimitiveShapeUtil.CreateRectangleOutlineSprite(64, 64, BlueOutline, 2);
                    shape = PrimitiveShapeUtil.CreateRectangleSprite(64, 64, BlueFill);
                }
                else
                {
                    outline = PrimitiveShapeUtil.CreateRectangleOutlineSprite(64, 64, RedOutline, 2);
                    shape = PrimitiveShapeUtil.CreateRectangleSprite(64, 64, RedFill);
                }

                shape.RenderLayer = (int)RenderLayer.TileHighlight;
                outline.RenderLayer = (int)RenderLayer.TileHighlightOutline;
                tileHighlight.AddComponent(outline);
                tileHighlight.AddComponent(shape);

                Point tileCoordinates = gridSystem.GetTileCoordinates(tile);
                scene.AddToGrid(tileHighlight, tileCoordinates.X, tileCoordinates.Y);
            }

            return new CharacterSelectedState();
        }
    }
}
