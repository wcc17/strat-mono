using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Sprites;
using Nez.Textures;

namespace StartMono.Util
{
    public static class PrimitiveShapeUtil
    {
        public static SpriteRenderer CreateRectangleOutlineSprite(int width, int height, Color color, int lineWidth, Vector2 origin)
        {
            Texture2D texture = new Texture2D(Core.GraphicsDevice, width, height);
            Color[] colors = new Color[width * height];
            for (var x = 0; x < width; x++)
            {
                for (var z = 0; z < lineWidth; z++)
                {
                    colors[(x * width) + z] = color;
                    colors[(x * width) + height - 1 - z] = color;
                }

                for (var y = 0; y < height; y++)
                {
                    if (x >= 0 && x < lineWidth)
                    {
                        colors[(x * width) + y] = color;
                    }

                    if (x <= width - 1 && x >= width - 1 - lineWidth)
                    {
                        colors[(x * width) + y] = color;
                    }
                }
            }
            texture.SetData(colors);

            Sprite sprite = new Sprite(texture, 0, 0, width, height);
            sprite.Origin = origin;
            SpriteRenderer shape = new SpriteRenderer(sprite);
            return shape;
        }

        public static SpriteRenderer CreateRectangleOutlineSprite(int width, int height, Color color, int lineWidth)
        {
            return CreateRectangleOutlineSprite(width, height, color, lineWidth, new Vector2(0, 0));
        }

        public static SpriteRenderer CreateRectangleSprite(int width, int height, Color color)
        {
            Texture2D texture = new Texture2D(Core.GraphicsDevice, 1, 1);
            Color[] colors = new Color[] { color };
            texture.SetData(colors);

            Sprite sprite = new Sprite(texture, 0, 0, width, height);
            sprite.Origin = new Vector2(0, 0);
            SpriteRenderer shape = new SpriteRenderer(sprite);
            return shape;
        }
    }
}
