using System;
using System.Runtime.InteropServices;

namespace StratMono
{
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            AttachConsole(-1);
            
            using var game = new StratMonoGame();
            game.IsFixedTimeStep = true;
            game.TargetElapsedTime = TimeSpan.FromSeconds(1f / 30);
            game.Run();
        }
        
        [DllImport("kernel32.dll")]
        static extern bool AttachConsole(int dwProcessId);
    }
}