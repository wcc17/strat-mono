using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StratMono.Components;
using StratMono.Entities;
using StratMono.System;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {
        private const string TiledMapEntityName = "tiled-map";
        private const string CameraEntityName = "camera";
        private const string CursorEntityName = "cursor";

        private const string CharacterSpriteName = "player";
        private const string CursorSpriteName = "tile_cursor";

        private SpriteAtlas _spriteAtlas;
        private GridSystem _gridSystem;

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
            _gridSystem = new GridSystem(tiledMap.TileWidth, tiledMap.TileHeight, tiledMap.WorldWidth, tiledMap.WorldHeight);
        }

        private void createCharacter()
        {
            var characterEntity = new GridEntity();
            characterEntity.AddComponent(createSpriteAnimator(CharacterSpriteName));
            characterEntity.AddComponent(new GridEntityMovement());
            addToGrid(characterEntity, 10, 12);
        }

        private void createGridCursor()
        {
            var cursorEntity = new GridEntity(CursorEntityName);
            cursorEntity.AddComponent(createSpriteAnimator(CursorSpriteName));
            cursorEntity.AddComponent(new GridCursorMovement());
            cursorEntity.AddComponent(new MouseMovement());
            addToGrid(cursorEntity, 5, 13);
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