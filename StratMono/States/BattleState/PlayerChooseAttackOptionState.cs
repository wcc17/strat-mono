using Microsoft.Xna.Framework;
using Nez;
using Nez.UI;
using stratMono.States.BattleState;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace StratMono.States.BattleState
{
    class PlayerChooseAttackOptionState : BaseBattleState
    {
        private bool _isAttackClicked = false;
        private CharacterGridEntity _characterAttacking;
        private CharacterGridEntity _characterBeingAttacked;
        private BaseState _stateToReturnToAfterBattle;

        public PlayerChooseAttackOptionState(
            CharacterGridEntity characterAttacking,
            CharacterGridEntity characterBeingAttacked,
            BaseState stateToReturnToAfterBattle)
        {
            _characterAttacking = characterAttacking;
            _characterBeingAttacked = characterBeingAttacked;
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

            var menuWidth = (int)menuEntity.GetComponent<UICanvas>().Width;
            var menuHeight = (int)menuEntity.GetComponent<UICanvas>().Height;
            var menuPosition = new Vector2(
                Screen.Width / 2 - menuWidth / 2,
                Screen.Height / 2 - menuHeight / 2
            );
            menuEntity.Position = menuPosition;
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
                    characterGridEntityAttacking: _characterAttacking,
                    characterGridEntityBeingAttacked: _characterBeingAttacked,
                    attackerOnLeft: true,
                    stateToReturnToAfterBattle: _stateToReturnToAfterBattle,
                    lastAttack: false);
            }

            return this;
        }
    }
}
