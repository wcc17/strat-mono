using Microsoft.Xna.Framework;
using Nez.Sprites;
using StartMono.Util;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.Util;
using System;
using System.Collections.Generic;

namespace StratMono.States.Scene
{
    public class CharacterSelectedState : BaseState
    {
        private readonly Color BlueFill = new Color(99, 155, 255, 200);
        private readonly Color BlueOutline = new Color(91, 110, 225, 200);
        private readonly Color RedFill = new Color(217, 87, 99, 200);
        private readonly Color RedOutline = new Color(172, 50, 50, 200);

        public override BaseState Update(
            LevelScene scene, 
            GridSystem gridSystem)
        {

            BaseState nextState = this;

            var selectionChanged = CheckForNewSelection(scene, gridSystem);
            if (selectionChanged)
            {
                _removeHighlightsFromGrid(scene);

                if (scene.SelectedCharacter != null)
                {
                    // NOTE: assuming that not null is a selected character until something else could be selected
                    nextState = new CharacterSelectedState();
                    nextState.EnterState(scene, gridSystem);
                } else
                {
                    nextState = new DefaultState();
                    nextState.EnterState(scene, gridSystem);
                }
            }

            return nextState;
        }

        public override void EnterState(
            LevelScene scene,
            GridSystem gridSystem)
        {
            _setupMovementTileHighlights(scene, gridSystem);
        }

        private void _setupMovementTileHighlights(
            LevelScene scene, 
            GridSystem gridSystem)
        {
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
        }

        private void _removeHighlightsFromGrid(LevelScene scene)
        {
            List<GridTileHighlight> highlights = scene.EntitiesOfType<GridTileHighlight>();
            foreach (GridTileHighlight highlight in highlights)
            {
                scene.RemoveFromGrid(highlight);
            }
        }
    }
}
