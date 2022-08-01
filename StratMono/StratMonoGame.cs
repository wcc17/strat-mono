using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using StratMono.Scenes;

namespace StratMono
{
    public class StratMonoGame : Core
    {

        public StratMonoGame()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
        }

        protected override void Initialize()
        {
            base.Initialize();
            Scene = new LevelScene();

            //Graphics.Instance.Batcher.ShouldRoundDestinations = false;
            // This fixed the weird issue where 1 pixel borders/whatever were being cut out and not shown
            Batcher.UseFnaHalfPixelMatrix = true;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}