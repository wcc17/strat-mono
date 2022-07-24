using System;
using Nez;
using Nez.Sprites;

namespace StratMono.Components
{
    public class CharacterComponent : Component, IUpdatable
    {
        private SpriteAnimator _spriteAnimatorComponent;

        public override void OnAddedToEntity()
        {
            _spriteAnimatorComponent = Entity.GetComponent<SpriteAnimator>();
            this.AddComponent(new MoveDirectionComponent());
        }
        
        public void Update()
        {
            var moveComponent = Entity.GetComponent<MoveDirectionComponent>();
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

            if (animationToPlay == null)
            {
                if (_spriteAnimatorComponent.CurrentAnimation == null) return;

                _spriteAnimatorComponent.Sprite = _spriteAnimatorComponent.CurrentAnimation.Sprites[1];
                _spriteAnimatorComponent.Stop();
                
            } else if (!_spriteAnimatorComponent.IsAnimationActive(animationToPlay))
            {
                _spriteAnimatorComponent.Play(animationToPlay, SpriteAnimator.LoopMode.PingPong);
            }
        }
    }
}