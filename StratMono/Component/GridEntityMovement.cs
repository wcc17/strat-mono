using System;
using System.Numerics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace StratMono.Components
{
    public class GridEntityMovement : Component, IUpdatable
    {
        public void Update()
        {
            //TODO: just a default direction for now, something will eventually change this
            Vector2 moveDirection = new Vector2(0, 1);
            string animationToPlay = "walk_down";

            if (moveDirection.X > 0)
            {
                animationToPlay = "walk_right";
            }
            else if (moveDirection.X < 0)
            {
                animationToPlay = "walk_left";
            }

            if (moveDirection.Y < 0)
            {
                animationToPlay = "walk_up";
            }
            else if (moveDirection.Y > 0)
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