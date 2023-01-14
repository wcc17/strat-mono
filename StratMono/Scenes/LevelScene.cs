﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.BitmapFonts;
using Nez.Sprites;
using StartMono.Util;
using StratMono.Components;
using StratMono.Entities;
using StratMono.States;
using StratMono.System;
using StratMono.Util;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {
        private readonly Color BlueFill = new Color(99, 155, 255, 200);
        private readonly Color BlueOutline = new Color(91, 110, 225, 200);
        private readonly Color RedFill = new Color(217, 87, 99, 200);
        private readonly Color RedOutline = new Color(172, 50, 50, 200);
        private readonly Color YellowFill = new Color(244, 211, 94, 200);
        private readonly Color YellowOutline = new Color(230, 190, 100, 200);
        private readonly int OutlineWidth = 2;

        private const string TiledMapEntityName = "tiled-map";
        private const string CameraEntityName = "camera";
        private const string CursorEntityName = "cursor";

        private const string CharacterSpriteName = "player";
        private const string Npc1SpriteName = "npc1";
        private const string Npc2SpriteName = "npc2";
        private const string Npc3SpriteName = "npc3";
        private const string CursorSpriteName = "tile_cursor";

        private SpriteAtlas _spriteAtlas;
        private BaseState _state = new States.FieldState.DefaultState();

        public BitmapFont font;
        public TileCursorSystem SceneTileCursorSystem;
        public GridSystem GridSystem;
        public CharacterGridMovementInformation CharacterGridMovementInfo = null;

        public CharacterGridEntity SelectedCharacter;
        public GridTile SelectedTile = null;
        public CharacterGridEntity CharacterBeingAttacked;

        public override void Initialize()
        {
            var screenSpaceRenderLayers = new int[]
            {
                (int)RenderLayer.UI,
                (int)RenderLayer.Battle,
                (int)RenderLayer.BattleLevelSeparator,
            };

            var worldRenderLayers = new int[]
            {
                (int)RenderLayer.Cursor,
                (int)RenderLayer.Character,
                (int)RenderLayer.TileHighlightOutline,
                (int)RenderLayer.TileHighlight,
                (int)RenderLayer.TileMap,
            };

            var screenSpaceRenderer = new ScreenSpaceRenderer(0, screenSpaceRenderLayers);
            var renderLayerRenderer = new RenderLayerRenderer(screenSpaceRenderLayers.Length, worldRenderLayers);
            AddRenderer(screenSpaceRenderer);
            AddRenderer(renderLayerRenderer);

            //SetDesignResolution(1920, 1080, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);
            Screen.IsFullscreen = true;

            _spriteAtlas = Content.LoadSpriteAtlas("Content/roots.atlas");

            createTiledMap();
            createCamera();
            createGrid();
            createCharacters_fitToInitialCameraView(numberOfTeamCharacters: 6, numberOfEnemies: 15);
            //createCharacters_randomFullMap(numberOfCharacters: 300);
            createGridCursorEntity();
        }

        public override void Update()
        {
            base.Update();
            updateInputMode();
            updateState();
        }

        public GridEntity AddToGrid(GridEntity entity, int x, int y)
        {
            AddEntity(entity);
            GridSystem.AddToGridTile(entity, x, y);
            return entity;
        }

        public void RemoveFromGrid(GridEntity entity)
        {
            GridSystem.RemoveFromGridTile(entity.Name);
            entity.Destroy();
        }

        public void SetupMovementTileHighlights()
        {
            // TODO: should actually store how far a character can travel somewhere (maxMovementCost)
            CharacterGridMovementInfo = GridSystem.IdentifyPossibleTilesToMoveToTile(SelectedTile, 5);
            foreach (GridTile tile in CharacterGridMovementInfo.TilesInRangeOfCharacter)
            {
                if (tile.CharacterCanMoveThroughThisTile)
                {
                    CreateAndAddPositiveTileHighlight(tile);
                }
                else if (tile.Position == new Point((int)SelectedCharacter.Position.X, (int)SelectedCharacter.Position.Y))
                {
                    CreateAndAddActiveTileHighlight(tile);
                }
                else
                {
                    CreateAndAddNegativeTileHighlight(tile);
                }
            }
        }

        public GridEntity CreateAndAddPositiveTileHighlight(GridTile gridTile)
        {
            return CreateAndAddTileHighlight(gridTile, BlueOutline, BlueFill);
        }

        public GridEntity CreateAndAddNegativeTileHighlight(GridTile gridTile)
        {
            return CreateAndAddTileHighlight(gridTile, RedOutline, RedFill);
        }

        public GridEntity CreateAndAddActiveTileHighlight(GridTile gridTile)
        {
            return CreateAndAddTileHighlight(gridTile, YellowOutline, YellowFill);
        }

        private void updateState()
        {
            var previousState = _state;
            _state = _state.Update(this, (GridEntity)FindEntity(CursorEntityName));
            if (previousState != _state)
            {
                Console.WriteLine("Previous State: " + previousState.ToString());
                Console.WriteLine("New State: " + _state.ToString());
                Console.WriteLine();

                previousState.ExitState(this);
                _state.EnterState(this);
            }
        }

        private GridEntity CreateAndAddTileHighlight(GridTile gridTile, Color outlineColor, Color fillColor)
        {
            GridEntity tileHighlight = new GridTileHighlight("highlight" + gridTile.Id);

            SpriteRenderer outline = PrimitiveShapeUtil.CreateRectangleOutlineSprite(
                64, 64, outlineColor, OutlineWidth);
            SpriteRenderer shape = PrimitiveShapeUtil.CreateRectangleSprite(
                64, 64, fillColor);

            shape.RenderLayer = (int)RenderLayer.TileHighlight;
            outline.RenderLayer = (int)RenderLayer.TileHighlightOutline;
            tileHighlight.AddComponent(outline);
            tileHighlight.AddComponent(shape);

            AddToGrid(tileHighlight, gridTile.Coordinates.X, gridTile.Coordinates.Y);

            return tileHighlight;
        }

        public void RemoveHighlightsFromGrid()
        {
            List<GridTileHighlight> highlights = EntitiesOfType<GridTileHighlight>();
            foreach (GridTileHighlight highlight in highlights)
            {
                RemoveFromGrid(highlight);
            }
        }

        public CharacterGridEntity GetCharacterFromSelectedTile(GridTile selectedTile)
        {
            List<GridEntity> entitiesOnSelectedTile = selectedTile.OccupyingEntities;
            if (entitiesOnSelectedTile != null)
            {
                foreach (var entity in entitiesOnSelectedTile)
                {
                    if (entity.GetType() == typeof(CharacterGridEntity))
                    {
                        return (CharacterGridEntity)entity;
                    }
                }
            }

            return null;
        }

        public SpriteAnimator CreateSpriteAnimator(string spriteName)
        {
            var animationNames = _spriteAtlas.AnimationNames
                .Where(animationName => animationName.Contains(spriteName))
                .ToList();

            SpriteAnimator animator = new SpriteAnimator();
            foreach (var animationName in animationNames)
            {
                var name = animationName.Replace(spriteName + "_", "");
                animator.AddAnimation(
                    name,
                    _spriteAtlas.GetAnimation(animationName)
                );
            }

            return animator;
        }

        private void createTiledMap()
        {
            var tiledMapEntity = CreateEntity(TiledMapEntityName);
            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map_2.tmx");
            var tiledMapRenderer = new TiledMapRenderer(tiledMap);

            tiledMapRenderer.RenderLayer = (int)RenderLayer.TileMap;
            tiledMapEntity.AddComponent(tiledMapRenderer);
        }

        private void createCamera()
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            var cameraEntity = CreateEntity(CameraEntityName);
            var levelBounds = new RectangleF(Vector2.Zero, new Vector2(tiledMap.WorldWidth, tiledMap.WorldHeight));
            Camera = cameraEntity.AddComponent(new BoundedMovingCamera(levelBounds));
        }

        private void createGrid()
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;
            GridSystem = new GridSystem(
                new Point(tiledMap.TileWidth, tiledMap.TileHeight),
                new Point(tiledMap.WorldWidth, tiledMap.WorldHeight),
                tiledMapEntity.GetComponent<TiledMapRenderer>().TiledMap.GetObjectGroup("bounds"),
                tiledMapEntity.GetComponent<TiledMapRenderer>().TiledMap.GetObjectGroup("move_cost"));
        }

        private void createCharacters_randomFullMap(int numberOfCharacters)
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            for (var i = 0; i < numberOfCharacters; i++)
            {
                var characterEntity = new CharacterGridEntity();

                SpriteAnimator spriteAnimator;
                int npc = Nez.Random.Range(0, 4);
                switch (npc)
                {
                    case 0:
                        spriteAnimator = CreateSpriteAnimator(Npc1SpriteName);
                        characterEntity.SpriteName = Npc1SpriteName;
                        characterEntity.AddComponent(new EnemyComponent());
                        break;
                    case 1:
                        spriteAnimator = CreateSpriteAnimator(Npc2SpriteName);
                        characterEntity.SpriteName = Npc2SpriteName;
                        characterEntity.AddComponent(new EnemyComponent());
                        break;
                    case 2:
                        spriteAnimator = CreateSpriteAnimator(Npc3SpriteName);
                        characterEntity.SpriteName = Npc3SpriteName;
                        characterEntity.AddComponent(new EnemyComponent());
                        break;
                    default:
                        spriteAnimator = CreateSpriteAnimator(CharacterSpriteName);
                        characterEntity.SpriteName = CharacterSpriteName;
                        break;
                }


                spriteAnimator.RenderLayer = (int)RenderLayer.Character;
                characterEntity.AddComponent(spriteAnimator);
                characterEntity.AddComponent(new CharacterAnimatedMovement());
                placeCharacterOnRandomTile(5, tiledMap.WorldWidth / 64 - 5, 5, tiledMap.WorldWidth / 64 - 5, characterEntity);
            }
        }

        private void createCharacters_fitToInitialCameraView(int numberOfTeamCharacters, int numberOfEnemies)
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            for (var i = 0; i < numberOfTeamCharacters; i++)
            {
                var characterEntity = new CharacterGridEntity();
                SpriteAnimator spriteAnimator;

                spriteAnimator = CreateSpriteAnimator(CharacterSpriteName);
                characterEntity.SpriteName = CharacterSpriteName;

                spriteAnimator.RenderLayer = (int)RenderLayer.Character;
                characterEntity.AddComponent(spriteAnimator);
                characterEntity.AddComponent(new CharacterAnimatedMovement());

                placeCharacterOnRandomTile(5, 25, 5, 15, characterEntity);
            }

            for (var i = 0; i < numberOfEnemies; i++)
            {
                var characterEntity = new CharacterGridEntity();
                SpriteAnimator spriteAnimator;

                int npc = Nez.Random.Range(0, 3);
                switch (npc)
                {
                    case 0:
                        spriteAnimator = CreateSpriteAnimator(Npc1SpriteName);
                        characterEntity.SpriteName = Npc1SpriteName;
                        characterEntity.AddComponent(new EnemyComponent());
                        break;
                    case 1:
                        spriteAnimator = CreateSpriteAnimator(Npc2SpriteName);
                        characterEntity.SpriteName = Npc2SpriteName;
                        characterEntity.AddComponent(new EnemyComponent());
                        break;
                    case 2:
                        spriteAnimator = CreateSpriteAnimator(Npc3SpriteName);
                        characterEntity.SpriteName = Npc3SpriteName;
                        characterEntity.AddComponent(new EnemyComponent());
                        break;
                    default:
                        spriteAnimator = new SpriteAnimator(); //NOTE: should not happen
                        break;
                }

                spriteAnimator.RenderLayer = (int)RenderLayer.Character;
                characterEntity.AddComponent(spriteAnimator);
                characterEntity.AddComponent(new CharacterAnimatedMovement());
                placeCharacterOnRandomTile(5, 25, 5, 15, characterEntity);
            }
        }

        private void placeCharacterOnRandomTile(int xMin, int xMax, int yMin, int yMax, GridEntity characterEntity)
        {
            int x = Nez.Random.Range(xMin, xMax);
            int y = Nez.Random.Range(yMin, yMax);
            bool tileInaccessible = true;
            while (tileInaccessible)
            {
                x = Nez.Random.Range(xMin, xMax);
                y = Nez.Random.Range(yMin, yMax);
                tileInaccessible = !GridSystem.GetGridTileFromCoords(new Point(x, y)).IsAccessible
                    || GetCharacterFromSelectedTile(GridSystem.GetGridTileFromCoords(new Point(x, y))) != null;
            }

            AddToGrid(characterEntity, x, y);
        }

        private void createGridCursorEntity()
        {
            SceneTileCursorSystem = new TileCursorSystem();
            var cursorEntity = new GridEntity(CursorEntityName);
            var spriteAnimator = CreateSpriteAnimator(CursorSpriteName);
            spriteAnimator.RenderLayer = (int)RenderLayer.Cursor;
            spriteAnimator.Play("default", SpriteAnimator.LoopMode.PingPong);
            cursorEntity.AddComponent(spriteAnimator);
            AddToGrid(cursorEntity, 5, 13);
        }

        //TODO: needs to be updated as more controls are added
        private void updateInputMode()
        {
            if (Input.CurrentKeyboardState.GetPressedKeys().Length > 0
                || Input.MousePositionDelta.X > 0
                || Input.MousePositionDelta.Y > 0)
            {
                InputMode.CurrentInputMode = InputModeType.KeyboardMouse;
            }

            // NOTE: only supporting one gamepad for now
            if (Input.GamePads.Length > 0 && Input.GamePads[0].IsConnected())
            {
                var gamepad = Input.GamePads[0];
                if (gamepad.IsLeftStickDown()
                    || gamepad.IsLeftStickUp()
                    || gamepad.IsLeftStickRight()
                    || gamepad.IsLeftStickLeft()
                    || gamepad.IsRightStickDown()
                    || gamepad.IsRightStickUp()
                    || gamepad.IsRightStickRight()
                    || gamepad.IsRightStickLeft()
                    || gamepad.IsRightTriggerPressed()
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.B)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.RightShoulder) 
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.LeftShoulder)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadLeft)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadRight)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadUp)
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.DPadDown))
                {
                    InputMode.CurrentInputMode = InputModeType.Controller;
                }
            }
        }
    }
}