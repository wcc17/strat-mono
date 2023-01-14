using Microsoft.Xna.Framework;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using System.Collections.Generic;
using StratMono.Util;
using Nez;
using StratMono.States;
using StratMono.States.FieldState;

namespace States.Shared
{
    public class DelayState : BaseFieldState
    {
        private BaseState _nextState;
        private float _timeToDelay;
        private float _currentTime;

        public DelayState(BaseState nextState, float timeToDelay) : base()
        {
            _nextState = nextState;
            _timeToDelay = timeToDelay;

            _currentTime = 0f;
        }

        public override void EnterState(LevelScene scene) { }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            _currentTime += Time.DeltaTime;
            if (_currentTime > _timeToDelay)
            {
                return _nextState;
            }

            return this;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
