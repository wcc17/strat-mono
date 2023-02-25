using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using stratMono.States.BattleState;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.States.BattleState.Context;
using StratMono.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace StratMono.States.BattleState
{
    class PlayerChooseAttackOptionState : BaseBattleState
    {
        private bool _isAttackClicked = false;
        private BaseState _stateToReturnToAfterBattle;

        public PlayerChooseAttackOptionState(
            BattleContext battleContext,
            BaseState stateToReturnToAfterBattle) : base(battleContext)
        {
            _stateToReturnToAfterBattle = stateToReturnToAfterBattle;
        }

        public override void EnterState(LevelScene scene)
        {
            var buttonDefinitions = new Dictionary<string, Action<Button>>
            {
                { "Attack", button => _isAttackClicked = true },
                { "asdf", button => _isAttackClicked = true },
                { "fdsa", button => _isAttackClicked = true },
                { "asd", button => _isAttackClicked = true }
            };

            var menuEntity = MenuBuilder.BuildActionMenu(
                scene.font,
                ActionMenuEntityName,
                buttonDefinitions,
                MenuBuilder.ScreenPosition.BottomCenter);
            scene.AddEntity(menuEntity);
        }

        public override void ExitState(LevelScene scene)
        {
            var menuEntity = scene.FindEntity(ActionMenuEntityName);
            menuEntity.RemoveAllComponents();
            menuEntity.DetachFromScene();
            menuEntity.Destroy();
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            base.Update(scene, cursorEntity);

            HandleReadyForInput();
            if (!ReadyForInput)
            {
                _isAttackClicked = false;
                return this;
            }

            if (_isAttackClicked)
            {
                return new CharacterAttackState(
                    scene,
                    entityNameAttacking: BattlePlayerEntityName,
                    entityNameBeingAttacked: BattleNpcEntityName,
                    battleContext: CurrentBattleContext,
                    stateToReturnToAfterBattle: _stateToReturnToAfterBattle,
                    lastAttack: false);
            }

            return this;
        }
    }
}
