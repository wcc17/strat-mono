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
    class DefaultState : BaseBattleState
    {
        private static readonly string ActionMenuEntityName = "battleactionmenu";
        private bool _isAttackClicked = false;

        public override void EnterState(LevelScene scene)
        {
            var buttonDefinitions = new Dictionary<string, Action<Button>>();
            buttonDefinitions.Add("Attack", button => _isAttackClicked = true);
            buttonDefinitions.Add("asdf", button => _isAttackClicked = true);
            buttonDefinitions.Add("fdsa", button => _isAttackClicked = true);
            buttonDefinitions.Add("asd", button => _isAttackClicked = true);

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
                    BattlePlayerEntityName,
                    BattleNpcEntityName,
                    attackerOnLeft: true);
            }

            return this;
        }
    }
}
