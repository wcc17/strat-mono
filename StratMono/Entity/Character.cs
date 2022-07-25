using System;
using System.Linq;
using Nez;
using Nez.Sprites;
using Microsoft.Xna.Framework;

namespace StratMono.Components
{
    public class Character : Entity
    {
        public override void Update()
        {
            base.Update();

            var movement = GetComponent<AiMovement>();
            var moveDirection = movement.MoveDirection;

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
                    return;
                }

                spriteAnimator.Sprite = spriteAnimator.CurrentAnimation.Sprites[1];
                spriteAnimator.Stop();
                
            } else if (!spriteAnimator.IsAnimationActive(animationToPlay))
            {
                spriteAnimator.Play(animationToPlay, SpriteAnimator.LoopMode.PingPong);
            }
        }

        public SpriteAnimator CreateSpriteAnimatorForCharacter(SpriteAtlas atlas, string characterName)
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

        public Vector2 SetCharacterPosition(Vector2 position)
        {
            if (HasComponent<SpriteAnimator>())
            {
                var spriteAnimator = GetComponent<SpriteAnimator>();
                Position = new Vector2(position.X - (spriteAnimator.Width / 2), position.Y - (spriteAnimator.Height / 2));
            } else
            {
                Position = position;
            }

            return position;
        }
    }
}