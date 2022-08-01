using System;
using System.Collections.Generic;
using System.Text;

namespace StratMono.Util
{
    public enum InputModeType
    {
        Controller,
        KeyboardMouse,
    }

    public static class InputMode
    {
        public static InputModeType CurrentInputMode = InputModeType.KeyboardMouse; 
    }
}
