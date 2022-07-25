using System;
using System.Numerics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace StratMono.Components
{
    public class GridCursorMovement : Component, IUpdatable
    {
        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            SpriteAnimator spriteAnimator = Entity.GetComponent<SpriteAnimator>();
            spriteAnimator.Play("default", SpriteAnimator.LoopMode.PingPong);
        }
        public void Update()
        {
        }
    }
}