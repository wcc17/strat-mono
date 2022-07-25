using System;
using Microsoft.Xna.Framework;
using Nez;
using StratMono.Event;

namespace StratMono.Components
{
    public class MouseMovement : Component, IUpdatable
    {
        public Vector2 MoveDirection = new Vector2(0, 0);
        public Vector2 CameraPosition = new Vector2(0, 0);

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();
            GameEventEmitter.Emitter.AddObserver(GameEventType.CameraPositionChanged, onCameraChange);
        }

        public override void OnRemovedFromEntity()
        {
            base.OnRemovedFromEntity();
            GameEventEmitter.Emitter.RemoveObserver(GameEventType.CameraPositionChanged, onCameraChange);
        }

        public void Update()
        {
            Entity.Position = new Vector2(
                Input.MousePosition.X + (CameraPosition.X - Screen.Width / 2), 
                Input.MousePosition.Y + (CameraPosition.Y - Screen.Height / 2));

            Console.WriteLine(CameraPosition);
            Console.WriteLine(Input.MousePosition);
            Console.WriteLine(Entity.Position);
            Console.WriteLine();
        }

        private void onCameraChange(GameEvent e) {
            var cameraGameEvent = (CameraGameEvent)e;
            CameraPosition = cameraGameEvent.cameraPosition;
        }
    }
}