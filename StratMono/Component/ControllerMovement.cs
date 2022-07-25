using System;
using System.Numerics;
using Microsoft.Xna.Framework.Input;
using Nez;

namespace StratMono.Components
{
    public class ControllerMovement : Component, IUpdatable
    {
        public Vector2 MoveDirection = new Vector2(0, 0);
        
        private VirtualIntegerAxis _xAxisInput;
        private VirtualIntegerAxis _yAxisInput;
        public override void OnAddedToEntity()
        {
            // horizontal input from dpad, left stick or keyboard left/right
            _xAxisInput = new VirtualIntegerAxis();
            _xAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadLeftRight());
            _xAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickX());
            _xAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.A,Keys.D));

            // vertical input from dpad, left stick or keyboard up/down
            _yAxisInput = new VirtualIntegerAxis();
            _yAxisInput.Nodes.Add(new VirtualAxis.GamePadDpadUpDown());
            _yAxisInput.Nodes.Add(new VirtualAxis.GamePadLeftStickY());
            _yAxisInput.Nodes.Add(
                new VirtualAxis.KeyboardKeys(VirtualInput.OverlapBehavior.TakeNewer, Keys.W,Keys.S));
        }
        
        public void Update()
        {
            MoveDirection.X = _xAxisInput.Value;
            MoveDirection.Y = _yAxisInput.Value;
        }

        public override void OnRemovedFromEntity()
        {
            _xAxisInput.Deregister();
            _yAxisInput.Deregister();
        }
    }
}