﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.UI;
using StartMono.Util;
using StratMono.Components;
using StratMono.Entities;
using StratMono.States.Scene;
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

        private const string TiledMapEntityName = "tiled-map";
        private const string CameraEntityName = "camera";
        private const string CursorEntityName = "cursor";

        private const string CharacterSpriteName = "player";
        private const string CursorSpriteName = "tile_cursor";

        private SpriteAtlas _spriteAtlas;
        private TileCursorSystem _tileCursorSystem;
        private BaseState _state = new DefaultState();

        public GridSystem GridSystem;
        public CharacterGridMovementInformation CharacterGridMovementInfo = null;

        public CharacterGridEntity SelectedCharacter;
        public GridTile SelectedTile = null;

        public override void Initialize()
        {
            //ClearColor = Color.Black;

            var defaultRenderer = new DefaultRenderer();
            this.AddRenderer(defaultRenderer);

            //SetDesignResolution(1920, 1080, SceneResolutionPolicy.ShowAllPixelPerfect);
            Screen.SetSize(1920, 1080);
            Screen.IsFullscreen = true;

            _spriteAtlas = Content.LoadSpriteAtlas("Content/roots.atlas");
            _tileCursorSystem = new TileCursorSystem();

            createTiledMap();
            createCamera();
            createGrid();
            createCharacter();
            createGridCursorEntity();

            //var uiCanvas = new UICanvas();
            //uiCanvas.RenderLayer = 0;
            //var stage = uiCanvas.Stage;
            //var table = stage.AddElement(new Table());
            //table.SetBounds(10, 10, 150, 200);
            //table.SetFillParent(false);

            //PrimitiveDrawable background = new PrimitiveDrawable(Color.White);
            //table.SetBackground(background);

            //var button1 = new Button(ButtonStyle.Create(Color.Black, Color.DarkGray, Color.Green));
            //button1.OnClicked += f => Console.WriteLine("hello?");
            //var button2 = new Button(ButtonStyle.Create(Color.Black, Color.DarkGray, Color.Green));
            //table.Add(button1).SetMinWidth(100).SetMinHeight(30);
            //table.Row();
            //table.Add(button2).SetMinWidth(100).SetMinHeight(30);

            //var uiCanvasEntity = new Entity();
            //uiCanvasEntity.AddComponent(uiCanvas);
            //AddEntity(uiCanvasEntity);

        }

        public override void Update()
        {
            base.Update();

            updateInputMode();

            var cursorEntity = FindEntity(CursorEntityName);

            _tileCursorSystem.Update(
                cursorEntity,
                Camera);

            _state = _state.Update(this, cursorEntity.Position);
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

                AddToGrid(tileHighlight, tile.Coordinates.X, tile.Coordinates.Y);
            }
        }

        public void RemoveHighlightsFromGrid()
        {
            List<GridTileHighlight> highlights = EntitiesOfType<GridTileHighlight>();
            foreach (GridTileHighlight highlight in highlights)
            {
                RemoveFromGrid(highlight);
            }
        }

        private void createTiledMap()
        {
            var tiledMapEntity = CreateEntity(TiledMapEntityName);
            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map.tmx");
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
            //Camera.ZoomIn(5);
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

        private void createCharacter()
        {
            var tiledMapEntity = FindEntity(TiledMapEntityName);
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            for (var i = 0; i < 500; i++)
            {
                var characterEntity = new CharacterGridEntity();
                var spriteAnimator = createSpriteAnimator(CharacterSpriteName);
                spriteAnimator.RenderLayer = (int)RenderLayer.Character;
                characterEntity.AddComponent(spriteAnimator);
                characterEntity.AddComponent(new CharacterAnimatedMovement());

                int x = Nez.Random.Range(5, tiledMap.WorldWidth / 64);
                int y = Nez.Random.Range(5, tiledMap.WorldHeight / 64);
                AddToGrid(characterEntity, x, y);
            }
        }

        private void createGridCursorEntity()
        {
            var cursorEntity = new GridEntity(CursorEntityName);
            var spriteAnimator = createSpriteAnimator(CursorSpriteName);
            spriteAnimator.RenderLayer = (int)RenderLayer.Cursor;
            spriteAnimator.Play("default", SpriteAnimator.LoopMode.PingPong);
            cursorEntity.AddComponent(spriteAnimator);
            AddToGrid(cursorEntity, 5, 13);
        }

        private SpriteAnimator createSpriteAnimator(string spriteName)
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
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.RightShoulder) 
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.LeftShoulder))
                {
                    InputMode.CurrentInputMode = InputModeType.Controller;
                }
            }
        }
    }
}