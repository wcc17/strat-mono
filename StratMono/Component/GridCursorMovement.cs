using Nez;
using Nez.Sprites;

namespace StratMono.Components
{
    public class GridCursorMovement : Component
    {
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            SpriteAnimator spriteAnimator = Entity.GetComponent<SpriteAnimator>();
            spriteAnimator.Play("default", SpriteAnimator.LoopMode.PingPong);
        }
    }
}