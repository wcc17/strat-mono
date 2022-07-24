using System.Linq;
using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using StratMono.Components;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {
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
            var tiledMapEntity = CreateEntity("tiled-map");
            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map.tmx");
            var tiledMapRenderer = new TiledMapRenderer(tiledMap);

            tiledMapEntity.AddComponent(tiledMapRenderer);
        }

        private void createCamera()
        {
            var tiledMapEntity = FindEntity("tiled-map");
            var tiledMapRenderer = tiledMapEntity.GetComponent<TiledMapRenderer>();
            var tiledMap = tiledMapRenderer.TiledMap;

            var cameraEntity = CreateEntity("camera");
            var levelBounds = new RectangleF(Vector2.Zero, new Vector2(tiledMap.WorldWidth, tiledMap.WorldHeight));
            Camera = cameraEntity.AddComponent(new BoundedMovingCamera(levelBounds));
        }

        private void createCharacter()
        {
            var atlas = Content.LoadSpriteAtlas("Content/roots.atlas");
            var characterName = "player";

            var characterEntity = CreateEntity(characterName);
            characterEntity.AddComponent(new CharacterComponent());
            characterEntity.AddComponent(createSpriteAnimatorForCharacter(atlas, characterName));
            characterEntity.Position = Screen.Center;
        }

        private SpriteAnimator createSpriteAnimatorForCharacter(SpriteAtlas atlas, string characterName)
        {
            var playerAnimationNames = atlas.AnimationNames
                .Where(animationName => animationName.Contains(characterName))
                .ToList();

            SpriteAnimator animator = new SpriteAnimator(); 
            foreach (var playerAnimationName in playerAnimationNames)
            {
                var animationName = playerAnimationName.Replace(characterName + "_", "");
                animator.AddAnimation(
                    animationName,
                    atlas.GetAnimation(playerAnimationName)
                );
            }

            return animator;
        }
        
    }
}