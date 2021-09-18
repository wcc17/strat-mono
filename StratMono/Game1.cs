using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.Sprites;

namespace StratMono
{
    public class Game1 : Nez.Core
    {

        private SpriteAnimator _animator;
        private Keys? _previousKeyPressed;
        
        public Game1()
        {
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            Scene = new Scene();
            // default to 1280x720 with no SceneResolutionPolicy
            // TODO: set this stuff inside the scene:
            Scene.SetDesignResolution(1920, 1080, Scene.SceneResolutionPolicy.None);
            Screen.SetSize(1920, 1080);
            
            var atlas = Content.LoadSpriteAtlas("Content/roots.atlas");
            var entity = Scene.CreateEntity("player");
            
            List<string> playerAnimationNames = new List<string>();
            foreach (string animationName in atlas.AnimationNames)
            {
                if (animationName.Contains("player"))
                {
                    playerAnimationNames.Add(animationName);
                }
            }
            
            _animator = entity.AddComponent<SpriteAnimator>();
            foreach (string playerAnimationName in playerAnimationNames)
            {
                _animator.AddAnimation(
                    playerAnimationName,
                    atlas.GetAnimation(playerAnimationName)
                );
            }

            entity.Position = Screen.Center;
            _animator.Play("player_walk_left");
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || 
                Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.W))
            {
                if (_previousKeyPressed != Keys.W)
                {
                    _animator.Play("player_walk_up");
                }
                
                _previousKeyPressed = Keys.W;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.A))
            {
                if (_previousKeyPressed != Keys.A)
                {
                    _animator.Play("player_walk_left");
                }

                _previousKeyPressed = Keys.A;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.S))
            {
                if (_previousKeyPressed != Keys.S)
                {
                    _animator.Play("player_walk_down");
                }
                
                _previousKeyPressed = Keys.S;
            }
            
            if (Keyboard.GetState().IsKeyDown(Keys.D))
            {
                if (_previousKeyPressed != Keys.D)
                {
                    _animator.Play("player_walk_right");
                }

                _previousKeyPressed = Keys.D;
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