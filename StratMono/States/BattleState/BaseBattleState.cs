using Microsoft.Xna.Framework;
using Nez;
using StratMono.Components.Character;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.States.BattleState.Context;
using StratMono.UI;
using System;

namespace StratMono.States.BattleState
{
    public class BaseBattleState : BaseState
    {
        public readonly string BattlePlayerEntityName = "BattlePlayerCharacter";
        public readonly string BattleNpcEntityName = "BattleNpcCharacter";
        public readonly string ScreenOverlayEntityName = "screenoverlay";
        public readonly string ActionMenuEntityName = "battleactionmenu";

        public readonly string LeftStatsEntityName = "BattleLeftSideStats";
        public readonly string RightStatsEntityName = "BattleRightSideStats";

        public BattleContext CurrentBattleContext;
        public bool ShouldShowBattleStats = true;

        private readonly int StatsBoxWidth = 325;
        private readonly int StatsBoxHeight = 100;

        public BaseBattleState(BattleContext battleContext)
        {
            CurrentBattleContext = battleContext;
        }

        public override void EnterState(LevelScene scene)
        {
        }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            var leftStatsMenuEntity = scene.FindEntity(LeftStatsEntityName);
            if (leftStatsMenuEntity != null)
            {
                leftStatsMenuEntity.Destroy();
            }

            var npcStatsMenuEntity = scene.FindEntity(RightStatsEntityName);
            if (npcStatsMenuEntity != null)
            {
                npcStatsMenuEntity.Destroy();
            }

            if (ShouldShowBattleStats)
            {
                var attackingHealth = CurrentBattleContext.CharacterGridEntityAttacking.GetComponent<Health>();
                var attackedHealth = CurrentBattleContext.CharacterGridEntityBeingAttacked.GetComponent<Health>();
                if (!CurrentBattleContext.AttackerOnLeft)
                {
                    attackingHealth = CurrentBattleContext.CharacterGridEntityBeingAttacked.GetComponent<Health>();
                    attackedHealth = CurrentBattleContext.CharacterGridEntityAttacking.GetComponent<Health>();
                }

                leftStatsMenuEntity = MenuBuilder.BuildStaticTextBox(
                    LeftStatsEntityName,
                    createStatsString(attackingHealth.currentHealth, attackingHealth.maxHealth),
                    MenuBuilder.ScreenPosition.BottomLeftCenter,
                    Color.White,
                    Color.Black,
                    StatsBoxWidth,
                    StatsBoxHeight);
                scene.AddEntity(leftStatsMenuEntity);

                npcStatsMenuEntity = MenuBuilder.BuildStaticTextBox(
                    RightStatsEntityName,
                    createStatsString(attackedHealth.currentHealth, attackedHealth.maxHealth),
                    MenuBuilder.ScreenPosition.BottomRightCenter,
                    Color.White,
                    Color.Black,
                    StatsBoxWidth,
                    StatsBoxHeight);
                scene.AddEntity(npcStatsMenuEntity);
            }

            return this;
        }

        public override void ExitState(LevelScene scene)
        {
        }

        private string createStatsString(float health, int maxHealth)
        {
            return $"HP: {(int)health}/{maxHealth}\nMP: 100/100";
        }
    }
}
