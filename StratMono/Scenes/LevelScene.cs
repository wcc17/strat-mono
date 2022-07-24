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

        public override void Initialize()
        {
            //TODO: move out of scene?
            var defaultRenderer = new DefaultRenderer();
            this.AddRenderer(defaultRenderer);

            SetDesignResolution(1920, 1080, SceneResolutionPolicy.None);
            Screen.SetSize(1920, 1080);

            createTiledMap();
            createCamera();
            createCharacter();
        }

        private void createTiledMap()
        {
            var tiledMapEntity = CreateEntity(TiledMapEntityName);
            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map.tmx");
            var tiledMapRenderer = new TiledMapRenderer(tiledMap);

            Console.WriteLine(tiledMap.TileHeight);
            Console.WriteLine(tiledMap.TileWidth);
            Console.WriteLine(tiledMap.MaxTileHeight);
            Console.WriteLine(tiledMap.MaxTileWidth);

            var tileWidth = tiledMap.TileWidth;
            var tileHeight = tiledMap.TileHeight;
            var gridTileWidth = tileWidth * 2;
            var gridTileHeight = tileHeight * 2;

            var mapWidthInGridTiles = tiledMap.WorldWidth / gridTileWidth;
            var mapHeightInGridTiles = tiledMap.WorldHeight / gridTileHeight;

            Console.WriteLine(mapWidthInGridTiles);
            Console.WriteLine(mapHeightInGridTiles);

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

        private void createCharacter()
        {
            var atlas = Content.LoadSpriteAtlas("Content/roots.atlas");

            var characterEntity = new Character("character", atlas);
            AddEntity(characterEntity);
            characterEntity.Position = Screen.Center;
        }
    }
}