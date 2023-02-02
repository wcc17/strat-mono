using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;

namespace StratMono.Util
{
    class RotationEntityUtil
    {

        public static Entity CreateRotationEntity(Entity childEntity)
        {
            // NOTE: can use this to debug the relationship between the parent rotationEntity and the child attacked entity
            //SpriteRenderer outline = PrimitiveShapeUtil.CreateRectangleOutlineSprite(
            //    64, 64, Color.Red, 3);
            //outline.RenderLayer = (int)RenderLayer.UI;
            //rotationEntity.AddComponent(outline);

            var spriteAnimator = childEntity.GetComponent<SpriteAnimator>();
            var spriteRenderer = childEntity.GetComponent<SpriteRenderer>();

            var width = spriteRenderer.Sprite.SourceRect.Width;
            float offset = width / 2;

            var rotationEntity = new Entity();

            // Set the parent entity's position so that when offsetting the child entity, it appears to not change its position
            // The offset is half of the child entity's width, which is the center of the child entity
            rotationEntity.Position = new Vector2(
                childEntity.Position.X + (offset * childEntity.Scale.X),
                childEntity.Position.Y + (offset * childEntity.Scale.Y));

            // Set the parent entity's scale to match the child and reset the child's scale so that it doesn't double
            rotationEntity.Scale = childEntity.Scale;
            childEntity.Scale = new Vector2(1f);

            // Set the child entity's local position so that it is centered over the parent's origin (should be (0, 0) if not this next line won't work
            childEntity.LocalPosition = new Vector2(-offset, -offset);

            // Set the child entity's transform parent to the rotationEntity so that it rotates with the parent
            childEntity.Parent = rotationEntity.Transform;

            return rotationEntity;
        }

        public static void ResetRotationEntity(Entity childEntity)
        {
            var oldScale = -childEntity.Parent.Scale;
            childEntity.Scale = oldScale;
            childEntity.Parent = null;
        }
    }
}
