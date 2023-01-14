using Components.Enemy;
using StratMono.Entities;
using StratMono.Scenes;
using System;

namespace StratMono.States.FieldState
{
    public class NpcControlDefaultState : BaseFieldState
    {
        CharacterGridEntity _enemy;

        public NpcControlDefaultState(CharacterGridEntity enemy = null) 
        {
            this._enemy = enemy;
        }

        public override void EnterState(LevelScene scene) { }

        public override BaseState Update(LevelScene scene, GridEntity cursorEntity)
        {
            base.Update(scene, cursorEntity);

            if (scene.AllEnemiesFinishedTurn())
            {
                scene.ResetFinishedTurns();
                return new PlayerControlDefaultState();
            }

            if (_enemy == null)
            {
                _enemy = scene.GetNextEnemy();
            }

            EnemyTurnStateComponent enemyTurnState = getEnemyTurnState();

            if (!enemyTurnState.alreadyAttacked)
            {
                // decide if we can attack, return attack component if so
                if (scene.GetImmediateTilesWithAttackableCharacters(_enemy.Position, true).Count > 0)
                {
                    // attack and move on
                    Console.WriteLine("should have attacked");
                    enemyTurnState.alreadyAttacked = true;
                    return endTurn(scene);
                }
            }
            else
            {
                // if we've already attacked, we can't move, turn is done
                return endTurn(scene);
            }

            if (enemyTurnState.alreadyMoved)
            {
                // end turn, we've already decided we can't attack
                return endTurn(scene);
            }
            else
            {
                // go ahead and move
                // TODO: in later difficulties, it would probably be best to determine if a movement and attacking another character would be better)
                enemyTurnState.alreadyMoved = true;
                return new NpcStartMovementState(_enemy);
            }

        }

        private EnemyTurnStateComponent getEnemyTurnState()
        {
            EnemyTurnStateComponent enemyTurnState = _enemy.GetComponent<EnemyTurnStateComponent>();
            if (enemyTurnState == null)
            {
                enemyTurnState = new EnemyTurnStateComponent();
                _enemy.AddComponent(enemyTurnState);
            }
            else
            {
                enemyTurnState.reset();
            }

            return enemyTurnState;
        }

        private BaseState endTurn(LevelScene scene)
        {
            // end turn, we've already decided we can't attack
            scene.FinishCharactersTurn(_enemy.Id);
            _enemy = null;
            return this;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
