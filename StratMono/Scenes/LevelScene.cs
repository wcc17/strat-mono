using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using StratMono.Components;
using StratMono.Entities;
using StratMono.System;
using StratMono.Util;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {
        enum UpdateOrder : int
        {
            TileMap = 0,
            TileHighlight = 1,
            Character = 2,
            Cursor = 3,
        }

        private const string TiledMapEntityName = "tiled-map";
        private const string CameraEntityName = "camera";
        private const string CursorEntityName = "cursor";

        private const string CharacterSpriteName = "player";
        private const string CursorSpriteName = "tile_cursor";

        private const string TiledMapBoundsLayerName = "bounds";
        private const string TiledMapMoveCostLayerName = "move_cost";

        private SpriteAtlas _spriteAtlas;
        private GridSystem _gridSystem;
        private TileCursorSystem _tileCursorSystem;

        public override void Initialize()
        {
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

            int opacity = 200;
            Color blueFill = new Color(99, 155, 255, opacity);
            Color blueOutline = new Color(91, 110, 225, opacity);
            GridEntity shapeEntity1 = new GridEntity("shape1");
            shapeEntity1.AddComponent(createRectangleOutlineSprite(64, 64, blueOutline, 3));
            shapeEntity1.AddComponent(createRectangleSprite(64, 64, blueFill));
            shapeEntity1.SetUpdateOrder((int) UpdateOrder.TileHighlight);
            addToGrid(shapeEntity1, 6, 6);

            Color redFill = new Color(217, 87, 99, opacity);
            Color redOutline = new Color(172, 50, 50, opacity);
            GridEntity shapeEntity2 = new GridEntity("shape2");
            shapeEntity2.AddComponent(createRectangleOutlineSprite(64, 64, redOutline, 3));
            shapeEntity2.AddComponent(createRectangleSprite(64, 64, redFill));
            shapeEntity2.SetUpdateOrder((int)UpdateOrder.TileHighlight);
            addToGrid(shapeEntity2, 7, 6);
        }

        public override void Update()
        {
            base.Update();

            updateInputMode();

            var cursorEntity = FindEntity(CursorEntityName);
            _tileCursorSystem.Update(
                cursorEntity, 
                Camera);
            _gridSystem.Update(
                cursorEntity,
                EntitiesOfType<GridEntity>(),
                (BoundedMovingCamera)Camera);

            Console.WriteLine(cursorEntity.Position);
            Console.WriteLine(cursorEntity.LocalPosition);
            Console.WriteLine(cursorEntity.GetComponent<SpriteAnimator>().Bounds);
            Console.WriteLine(cursorEntity.GetComponent<SpriteAnimator>());
            Console.WriteLine();
        }

        private void createTiledMap()
        {
            var tiledMapEntity = CreateEntity(TiledMapEntityName);
            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map.tmx");
            var tiledMapRenderer = new TiledMapRenderer(tiledMap);
            tiledMapEntity.AddComponent(tiledMapRenderer);
            tiledMapEntity.SetUpdateOrder((int)UpdateOrder.TileMap);
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
            _gridSystem = new GridSystem(
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

            for (var i = 0; i < 100; i++)
            {
                var characterEntity = new GridEntity();
                characterEntity.AddComponent(createSpriteAnimator(CharacterSpriteName));
                characterEntity.AddComponent(new CharacterMovement());
                characterEntity.SetUpdateOrder((int)UpdateOrder.Character);

                int x = Nez.Random.Range(5, tiledMap.WorldWidth / 64);
                int y = Nez.Random.Range(5, tiledMap.WorldHeight / 64);
                addToGrid(characterEntity, x, y);

            }
        }

        private void createGridCursorEntity()
        {
            var cursorEntity = new GridEntity(CursorEntityName);
            var spriteAnimator = createSpriteAnimator(CursorSpriteName);
            spriteAnimator.Play("default", SpriteAnimator.LoopMode.PingPong);
            cursorEntity.AddComponent(spriteAnimator);
            cursorEntity.SetUpdateOrder((int)UpdateOrder.Cursor);
            addToGrid(cursorEntity, 5, 13);
        }

        private GridEntity addToGrid(GridEntity entity, int x, int y)
        {
            AddEntity(entity);
            _gridSystem.AddToGridTile(entity, x, y);
            return entity;
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
                    || gamepad.IsButtonPressed(Microsoft.Xna.Framework.Input.Buttons.A))
                {
                    InputMode.CurrentInputMode = InputModeType.Controller;
                }
            }
        }

        private SpriteRenderer createRectangleOutlineSprite(int width, int height, Color color, int lineWidth)
        {
            Texture2D texture = new Texture2D(Core.GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];
            for (var x = 0; x < width; x++)
            {
                for (var z = 0; z < lineWidth; z++)
                {
                    colors[(x * width) + z] = color;
                    colors[(x * width) + height - 1 - z] = color;
                }

                for (var y = 0; y < height; y++)
                {
                    if (x >= 0 && x < lineWidth)
                    {
                        colors[(x * width) + y] = color;
                    }

                    if (x <= width - 1 && x >= width - 1 - lineWidth)
                    {
                        colors[(x * width) + y] = color;
                    }
                }
            }
            texture.SetData(colors);

            Sprite sprite = new Sprite(texture, 0, 0, width, height);
            sprite.Origin = new Vector2(0, 0);
            SpriteRenderer shape = new SpriteRenderer(sprite);
            return shape;
        }

        private SpriteRenderer createRectangleSprite(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(Core.GraphicsDevice, 1, 1);
            Color[] colors = new Color[] { color };
            texture.SetData(colors);

            Sprite sprite = new Sprite(texture, 0, 0, width, height);
            sprite.Origin = new Vector2(0, 0);
            SpriteRenderer shape = new SpriteRenderer(sprite);
            return shape;
        }
    }
}