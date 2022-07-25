using System;
using Microsoft.Xna.Framework;
using Nez;
using StratMono.Components;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {
        private const string TiledMapEntityName = "tiled-map";
        private const string CameraEntityName = "camera";
        private Grid _grid;

        public override void Initialize()
        {
            //TODO: move out of scene?
            var defaultRenderer = new DefaultRenderer();
            this.AddRenderer(defaultRenderer);

            SetDesignResolution(1920, 1080, SceneResolutionPolicy.None);
            Screen.SetSize(1920, 1080);

            createTiledMap();
            createCamera();
            createGrid();
            createCharacter();
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
            _grid = new Grid(tiledMap.TileWidth, tiledMap.TileHeight, tiledMap.WorldWidth, tiledMap.WorldHeight);
        }

        private void createCharacter()
        {
            var atlas = Content.LoadSpriteAtlas("Content/roots.atlas");

            var characterEntity = new Character();
            characterEntity.AddComponent(characterEntity.CreateSpriteAnimatorForCharacter(atlas, "player"));
            characterEntity.AddComponent(new AiMovement());
            addCharacterToGrid(characterEntity, 10, 12);
        }

        private Character addCharacterToGrid(Character character, int x, int y)
        {
            AddEntity(character);
            _grid.AddCharacterToGridTile(character, x, y);
            return character;
        }
    }
}