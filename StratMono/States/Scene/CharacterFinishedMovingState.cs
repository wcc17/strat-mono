using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Nez;
using Nez.UI;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace StratMono.States.Scene
{
    class CharacterFinishedMovingState : BaseState
    {
        // in case the user cancels movement, we need to be able to go back
        private readonly Dictionary<GridTile, GridTile> _possiblePathsFromCharacter;
        private readonly GridTile _initialTile;
        private readonly GridTile _goalTile;

        public CharacterFinishedMovingState(
            Dictionary<GridTile, GridTile> possiblePaths,
            GridTile initialTile,
            GridTile goalTile) : base()
        {
            _possiblePathsFromCharacter = possiblePaths;
            _initialTile = initialTile;
            _goalTile = goalTile;
        }

        public override void EnterState(LevelScene scene)
        {
            var uiCanvas = new UICanvas();
            uiCanvas.RenderLayer = 0;
            var stage = uiCanvas.Stage;
            var table = stage.AddElement(new Table());
            table.SetBounds(10, 10, 150, 200);
            table.SetFillParent(false);

            PrimitiveDrawable background = new PrimitiveDrawable(Color.White);
            table.SetBackground(background);

            var button1 = new Button(ButtonStyle.Create(Color.Black, Color.DarkGray, Color.Green));
            button1.OnClicked += f => Console.WriteLine("hello?");
            var button2 = new Button(ButtonStyle.Create(Color.Black, Color.DarkGray, Color.Green));
            table.Add(button1).SetMinWidth(100).SetMinHeight(30);
            table.Row();
            table.Add(button2).SetMinWidth(100).SetMinHeight(30);

            var uiCanvasEntity = new Entity();
            uiCanvasEntity.AddComponent(uiCanvas);
            scene.AddEntity(uiCanvasEntity);
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            //Console.WriteLine("character finished moving state");

            if (Input.IsKeyPressed(Keys.Escape))
            {
                // passing initialTile as goalTile because we need to go backwards now
                var nextState = new CharacterMovingState(
                    _possiblePathsFromCharacter, 
                    _goalTile,
                    _initialTile, 
                    returnedToOriginalPosition: true);
                nextState.EnterState(scene);
                return nextState;
            }

            return this;
        }
    }
}
