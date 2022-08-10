using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using StratMono.Util;
using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Nez.BitmapFonts;

namespace StratMono.UI
{
    public static class MenuBuilder
    {
        private static readonly Point TablePosition = new Point(50, 50);
        private static readonly int TableWidth = 330;
        private static readonly int TableHeight = 220;
        private static readonly int ButtonWidth = 300;
        private static readonly int ButtonHeight = 80;
        private static readonly int ButtonPadding = 20;

        public static Entity BuildActionMenu(
            BitmapFont font,
            string entityName, 
            Action<Button> onWaitButtonClick, 
            Action<Button> onCancelButtonClick)
        {
            var uiCanvas = new UICanvas();
            uiCanvas.RenderLayer = (int)RenderLayer.UI;

            var stage = uiCanvas.Stage;
            var skin = Skin.CreateDefaultSkin();
            var table = createTable(stage);
            var waitButton = createButton(skin, "Wait", onWaitButtonClick);
            var cancelButton = createButton(skin, "Cancel", onCancelButtonClick);
            
            stage.SetGamepadFocusElement(waitButton);
            stage.GamepadActionButtons = new Buttons[] { Buttons.A, Buttons.RightTrigger };
            
            addButtonsToTable(
                table, 
                new Button[] { waitButton, cancelButton });            

            var uiCanvasEntity = new Entity(entityName);
            uiCanvasEntity.AddComponent(uiCanvas);
            return uiCanvasEntity;
        }

        public static void DestroyMenu(Entity menuEntity)
        {
            menuEntity.RemoveAllComponents();
            menuEntity.Destroy();
        }

        private static Table createTable(Stage stage)
        {
            var table = stage.AddElement(new Table());
            table.SetX(TablePosition.X);
            table.SetY(TablePosition.Y);
            table.SetWidth(TableWidth);
            table.SetHeight(TableHeight);

            PrimitiveDrawable background = new PrimitiveDrawable(Color.White);
            table.SetBackground(background);

            return table;
        }

        private static Button createButton(
            Skin skin, 
            string labelText,
            Action<Button> onClicked)
        {
            var button = new Button(skin);
            button.ShouldUseExplicitFocusableControl = true;
            button.Add(new Label(labelText, Graphics.Instance.BitmapFont, Color.Black, 5));
            button.OnClicked += onClicked;
            return button;
        }

        private static void addButtonsToTable(Table table, Button[] buttons)
        {
            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].GamepadUpElement = ((i - 1) > -1) ? buttons[i - 1] : buttons[buttons.Length - 1];
                buttons[i].GamepadDownElement = ((i + 1) < buttons.Length) ? buttons[i + 1] : buttons[0];

                if (i != buttons.Length - 1)
                {
                    table.Add(buttons[i])
                        .SetMinWidth(ButtonWidth)
                        .SetMinHeight(ButtonHeight)
                        .SetPadBottom(ButtonPadding);
                }
                else
                {
                    table.Add(buttons[i])
                        .SetMinWidth(ButtonWidth)
                        .SetMinHeight(ButtonHeight);
                }

                table.Row();
            }
        }
    }
}
