using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StratMono.Components;
using StratMono.Entities;
using StratMono.System;
using System.Timers;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {
        private const string TiledMapEntityName = "tiled-map";
        private const string CameraEntityName = "camera";
        private const string CursorEntityName = "cursor";

        private const string CharacterSpriteName = "player";
        private const string CursorSpriteName = "tile_cursor";

        private const string TiledMapBoundsLayerName = "bounds";
        private const string TiledMapMoveCostLayerName = "move_cost";

        private SpriteAtlas _spriteAtlas;
        private GridSystem _gridSystem;

        private VirtualIntegerAxis _cursorMovementXAxisInput;
        private VirtualIntegerAxis _cursorMovementYAxisInput;
        private Vector2 _cursorMovementDirection = new Vector2(0, 0);
        private Timer _timer = new Timer();
        private bool _disableCursorControllerMovement;

        public override void Initialize()
        {
            //TODO: move out of scene?
            var defaultRenderer = new DefaultRenderer();
            this.AddRenderer(defaultRenderer);

            SetDesignResolution(1920, 1080, SceneResolutionPolicy.None);
            Screen.SetSize(1920, 1080);

            _spriteAtlas = Content.LoadSpriteAtlas("Content/roots.atlas");

            createTiledMap();
            createCamera();
            createGrid();
            createCharacter();
            createGridCursor();
        }

        public override void Update()
        {
            base.Update();

            if (Input.LeftMouseButtonPressed)
            {
                _gridSystem.SelectCurrentTile(Input.MousePosition);
            }

            // TODO: system?
            var cursorEntity = FindEntity(CursorEntityName);

            if (Input.MousePositionDelta.X > 0 || Input.MousePositionDelta.Y > 0)
            {
                cursorEntity.Position = new Vector2(
                    Input.MousePosition.X + (Camera.Position.X - Screen.Width / 2),
                    Input.MousePosition.Y + (Camera.Position.Y - Screen.Height / 2));
            } else
            {
                if (!_disableCursorControllerMovement)
                {
                    var movementSpeedOrWhatever = 64;
                    _cursorMovementDirection.X = _cursorMovementXAxisInput.Value;
                    _cursorMovementDirection.Y = _cursorMovementYAxisInput.Value;

                    if (_cursorMovementDirection.X > 0)
                    {
                        cursorEntity.Position = new Vector2(
                            cursorEntity.Position.X + movementSpeedOrWhatever, 
                            cursorEntity.Position.Y);
                    }

                    if (_cursorMovementDirection.X < 0)
                    {
                        cursorEntity.Position = new Vector2(
                            cursorEntity.Position.X - movementSpeedOrWhatever, 
                            cursorEntity.Position.Y);
                    }

                    if (_cursorMovementDirection.Y > 0)
                    {
                        cursorEntity.Position = new Vector2(
                            cursorEntity.Position.X, 
                            cursorEntity.Position.Y - movementSpeedOrWhatever);
                    }

                    if (_cursorMovementDirection.Y < 0)
                    {
                        cursorEntity.Position = new Vector2(
                            cursorEntity.Position.X, 
                            cursorEntity.Position.Y + movementSpeedOrWhatever);
                    }

                    if (_cursorMovementDirection.X != 0 || _cursorMovementDirection.Y != 0)
                    {
                        _disableCursorControllerMovement = true;

                        _timer = new Timer(50);
                        _timer.Enabled = true;
                        _timer.Elapsed += (source, e) =>
                        {
                            _disableCursorControllerMovement = false;
                            _timer.Enabled = false;
                        };
                    }
                }

                var bounds = Camera.Bounds;
                var cursorEntitySpriteAnimator = cursorEntity.GetComponent<SpriteAnimator>();
                if (cursorEntity.Position.X < bounds.X)
                {
                    cursorEntity.Position = new Vector2(
                        bounds.X, 
                        cursorEntity.Position.Y);
                }

                if (cursorEntity.Position.X + cursorEntitySpriteAnimator.Width > bounds.Right)
                {
                    cursorEntity.Position = new Vector2(
                        bounds.Right - cursorEntitySpriteAnimator.Width, 
                        cursorEntity.Position.Y);
                }

                if (cursorEntity.Position.Y < bounds.Top)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X,
                        bounds.Y);
                }

                if (cursorEntity.Position.Y + cursorEntitySpriteAnimator.Height > bounds.Bottom)
                {
                    cursorEntity.Position = new Vector2(
                        cursorEntity.Position.X,
                        bounds.Bottom - cursorEntitySpriteAnimator.Height);
                }
            }
            // TODO: end system?

            _gridSystem.SnapEntitiesToGrid(EntitiesOfType<GridEntity>());
        }

        private void createTiledMap()
        {
            var tiledMapEntity = CreateEntity(TiledMapEntityName);
            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map.tmx");
            var tiledMapRenderer = new TiledMapRenderer(tiledMap);
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
            _gridSystem = new GridSystem(
                new Point(tiledMap.TileWidth, tiledMap.TileHeight),
                new Point(tiledMap.WorldWidth, tiledMap.WorldHeight),
                tiledMapEntity.GetComponent<TiledMapRenderer>().TiledMap.GetObjectGroup("bounds"),
                tiledMapEntity.GetComponent<TiledMapRenderer>().TiledMap.GetObjectGroup("move_cost"));
        }

        private void createCharacter()
        {
            var characterEntity = new GridEntity();
            characterEntity.AddComponent(createSpriteAnimator(CharacterSpriteName));
            characterEntity.AddComponent(new CharacterMovement());
            addToGrid(characterEntity, 10, 12);
        }

        private void createGridCursor()
        {
            var cursorEntity = new GridEntity(CursorEntityName);
            var spriteAnimator = createSpriteAnimator(CursorSpriteName);
            spriteAnimator.Play("default", SpriteAnimator.LoopMode.PingPong);

            cursorEntity.AddComponent(spriteAnimator);
            addToGrid(cursorEntity, 5, 13);

            // TODO: consider moving elsewhere, see system comment in Update
            _cursorMovementXAxisInput = new VirtualIntegerAxis();
            _cursorMovementYAxisInput = new VirtualIntegerAxis();
            _cursorMovementXAxisInput.Nodes.Add(new VirtualAxis.GamePadRightStickX());
            _cursorMovementYAxisInput.Nodes.Add(new VirtualAxis.GamePadRightStickY());
        }

        private GridEntity addToGrid(GridEntity entity, int x, int y)
        {
            AddEntity(entity);
            _gridSystem.AddToGridTile(entity, x, y);
            return entity;
        }

        public SpriteAnimator createSpriteAnimator(string spriteName)
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
    }
}