using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using StratMono.Util;
using System;
using Microsoft.Xna.Framework.Input;
using Nez.BitmapFonts;
using System.Collections.Generic;

namespace StratMono.UI
{
    public static class MenuBuilder
    {
        private static readonly Point TablePosition = new Point(50, 50);
        private static readonly int ButtonWidth = 300;
        private static readonly int ButtonHeight = 80;
        private static readonly int ButtonPadding = 20;
        private static readonly int TableWidthPadding = 30;
        private static readonly int TableHeightPadding = 20;

        public static Entity BuildActionMenu(
            BitmapFont font,
            string entityName, 
            Dictionary<string, Action<Button>> buttonDefinitions)
        {
            var uiCanvas = new UICanvas();
            uiCanvas.RenderLayer = (int)RenderLayer.UI;

            var stage = uiCanvas.Stage;
            stage.GamepadActionButtons = new Buttons[] { Buttons.A, Buttons.RightTrigger };

            var skin = Skin.CreateDefaultSkin();
            var table = createTable(stage);

            List<Button> buttons = new List<Button>();
            foreach(string buttonText in buttonDefinitions.Keys)
            {
                var button = createButton(skin, buttonText, buttonDefinitions[buttonText]);
                buttons.Add(button);
            }
            table.SetWidth(ButtonWidth + TableWidthPadding);
            table.SetHeight((ButtonHeight * buttons.Count) + (buttons.Count * TableHeightPadding));

            stage.SetGamepadFocusElement(buttons[0]);
            
            addButtonsToTable(table, buttons);

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

        private static void addButtonsToTable(Table table, List<Button> buttons)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].GamepadUpElement = ((i - 1) > -1) ? buttons[i - 1] : buttons[buttons.Count - 1];
                buttons[i].GamepadDownElement = ((i + 1) < buttons.Count) ? buttons[i + 1] : buttons[0];

                if (i != buttons.Count - 1)
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
