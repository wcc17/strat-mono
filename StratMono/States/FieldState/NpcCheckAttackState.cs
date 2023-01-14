using Components.Enemy;
using Microsoft.Xna.Framework;
using StratMono.Components;
using StratMono.Entities;
using StratMono.Scenes;
using StratMono.System;
using StratMono.Util;
using System;
using System.Collections.Generic;
using System.Text;

namespace StratMono.States.FieldState
{
    public class NpcCheckAttackState : BaseFieldState
    {
        private CharacterGridEntity _enemy;

        public NpcCheckAttackState(CharacterGridEntity enemy)
        {
            this._enemy = enemy;
        }

        public override void EnterState(LevelScene scene) { }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            return base.Update(scene, cursorEntity);

            //var enemyTurnState = _enemy.GetComponent<EnemyTurnStateComponent>();
            //if (!enemyTurnState.alreadyAttacked)
            //{
            //    // decide if we can attack, return attack component if so
            //} else
            //{
            //    // if we've already attacked, we can't move, turn is done
            //}

            //if (enemyTurnState.alreadyMoved)
            //{
            //    // end turn, we've already decided we can't attack
            //} else
            //{
            //    // go ahead and move
            //}
        }

        public override void ExitState(LevelScene scene) { }
    }
}
