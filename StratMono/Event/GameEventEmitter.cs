using System;
using System.Collections.Generic;
using System.Text;
using Nez.Systems;
using Microsoft.Xna.Framework;

namespace StratMono.Event
{
    public static class GameEventEmitter
    {
        public static Emitter<GameEventType, GameEvent> Emitter;

        static GameEventEmitter()
        {
            Emitter = new Emitter<GameEventType, GameEvent>();
        }
    }

    public enum GameEventType
    {
        CameraPositionChanged
    }

    public struct CameraGameEvent : GameEvent
    {
        public Vector2 cameraPosition;
    }

    public interface GameEvent
    {

    }

}
