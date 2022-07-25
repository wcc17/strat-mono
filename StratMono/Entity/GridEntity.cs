using Nez;
using Nez.Sprites;
using Microsoft.Xna.Framework;

namespace StratMono.Components
{
    public class GridEntity : Entity
    {

        public GridEntity() : base() { }

        public GridEntity(string name) : base(name) { }

        public override void Update()
        {
            base.Update();
        }

        public Vector2 SetPosition(Vector2 position)
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