using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using StratMono.Util;
using System;

namespace StratMono.UI
{
    public static class MenuBuilder
    {
        public static Entity BuildActionMenu(
            string entityName, 
            Action<Button> onButtonOneClick, 
            Action<Button> onButtonTwoClick)
        {
            var uiCanvas = new UICanvas();
            uiCanvas.RenderLayer = (int)RenderLayer.UI;

            var stage = uiCanvas.Stage;

            var table = stage.AddElement(new Table());
            table.SetX(20);
            table.SetY(20);
            table.SetWidth(350);
            table.SetHeight(500);
            table.SetFillParent(false);
            PrimitiveDrawable background = new PrimitiveDrawable(Color.White);
            table.SetBackground(background);

            var skin = Skin.CreateDefaultSkin();
            
            var button1 = new Button(skin);
            var button2 = new Button(skin);

            button1.OnClicked += onButtonOneClick;
            button1.ShouldUseExplicitFocusableControl = true;
            button1.GamepadDownElement = button2;
            button1.GamepadUpElement = button2;

            button2.OnClicked += onButtonTwoClick;
            button2.ShouldUseExplicitFocusableControl = true;
            button2.GamepadDownElement = button1;
            button2.GamepadUpElement = button1;

            stage.SetGamepadFocusElement(button1);
            
            table.Add(button1).SetMinWidth(300).SetMinHeight(80);
            table.Row();
            table.Add(button2).SetMinWidth(300).SetMinHeight(80);
            table.Row();

            var uiCanvasEntity = new Entity(entityName);
            uiCanvasEntity.AddComponent(uiCanvas);

            return uiCanvasEntity;
        }

        public static void DestroyMenu(Entity menuEntity)
        {
            menuEntity.RemoveAllComponents();
            menuEntity.Destroy();
        }
    }
}
