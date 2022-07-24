using System;
using System.Linq;
using Nez;
using Nez.Sprites;

namespace StratMono.Components
{
    public class Character : Entity
    {
        private SpriteAtlas _spriteAtlas;

        public Character(string entityName, SpriteAtlas atlas) : base(entityName) 
        {
            _spriteAtlas = atlas;
        }

        public override void OnAddedToScene()
        {
            AddComponent(createSpriteAnimatorForCharacter(_spriteAtlas, "player"));
            AddComponent(new MoveDirectionComponent());
        }
        
        public override void Update()
        {
            base.Update();

            var moveComponent = GetComponent<MoveDirectionComponent>();
            var moveDirection = moveComponent.MoveDirection;

            string animationToPlay = null;
            if (moveDirection.X > 0)
            {
                animationToPlay = "walk_right";
            } else if (moveDirection.X < 0)
            {
                animationToPlay = "walk_left";
            }

            if (moveDirection.Y < 0)
            {
                animationToPlay = "walk_up";
            } else if (moveDirection.Y > 0)
            {
                animationToPlay = "walk_down";
            }


            var spriteAnimator = GetComponent<SpriteAnimator>();
            if (animationToPlay == null)
            {
                if (spriteAnimator.CurrentAnimation == null)
                {
                    Console.WriteLine("no current animation");
                    return;
                }

                spriteAnimator.Sprite = spriteAnimator.CurrentAnimation.Sprites[1];
                spriteAnimator.Stop();
                
            } else if (!spriteAnimator.IsAnimationActive(animationToPlay))
            {
                spriteAnimator.Play(animationToPlay, SpriteAnimator.LoopMode.PingPong);
            }
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