using System;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using Nez;
using Nez.Sprites;
using Nez.Tiled;
using StratMono.Components;

namespace StratMono.Scenes
{
    public class LevelScene : Scene
    {

        public override void Initialize()
        {
            // default to 1280x720 with no SceneResolutionPolicy
            // TODO: set this stuff inside the scene:
            SetDesignResolution(1920, 1080, SceneResolutionPolicy.None);
            Screen.SetSize(1920, 1080);

            var tiledMap = Content.LoadTiledMap("Content/assets/tiles/test_scene_map.tmx");
            var tiledMapEntity = CreateEntity("tiled-map");
            tiledMapEntity.AddComponent(new TiledMapRenderer(tiledMap));
            
            var atlas = Content.LoadSpriteAtlas("Content/roots.atlas");
            var characterName = "player";
            var characterEntity = CreateEntity(characterName);
            characterEntity.AddComponent(new CharacterComponent());
            characterEntity.AddComponent(new MoveComponent());
            characterEntity.AddComponent(createSpriteAnimatorForCharacter(atlas, characterName));
            characterEntity.Position = Screen.Center;

            var cameraEntity = CreateEntity("camera");
            cameraEntity.AddComponent(new MoveComponent());
            Camera = cameraEntity.AddComponent(new CameraComponent());
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