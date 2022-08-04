using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;
using StratMono.Util;

namespace StratMono.Components
{
    public class CharacterAnimatedMovement : Component, IUpdatable
    {
        public Vector2 MoveDirection = MovementDirection.DOWN;

        public void Update()
        {
            string animationToPlay = "walk_down";

            if (MoveDirection.X > 0)
            {
                animationToPlay = "walk_right";
            }
            else if (MoveDirection.X < 0)
            {
                animationToPlay = "walk_left";
            }

            if (MoveDirection.Y < 0)
            {
                animationToPlay = "walk_up";
            }
            else if (MoveDirection.Y > 0)
            {
                animationToPlay = "walk_down";
            }

            var spriteAnimator = Entity.GetComponent<SpriteAnimator>();
            if (animationToPlay == null)
            {
                if (spriteAnimator.CurrentAnimation == null)
                {
                    return;
                }

                spriteAnimator.Sprite = spriteAnimator.CurrentAnimation.Sprites[1];
                spriteAnimator.Stop();

            }
            else if (!spriteAnimator.IsAnimationActive(animationToPlay))
            {
                spriteAnimator.Play(animationToPlay, SpriteAnimator.LoopMode.PingPong);
            }
        }
    }
}