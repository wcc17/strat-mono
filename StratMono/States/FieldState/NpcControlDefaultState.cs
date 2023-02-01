using Components;
using Components.Enemy;
using Nez.Sprites;
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
                for (var i = 0; i < scene.enemyEntities.Count; i++)
                {
                    scene.enemyEntities[i].GetComponent<TurnStateComponent>().reset();
                }

                return new PlayerControlDefaultState();
            }

            if (_enemy == null)
            {
                _enemy = scene.GetNextEnemy();
                _enemy.GetComponent<TurnStateComponent>().reset();
            }


            if (!_enemy.GetComponent<TurnStateComponent>().alreadyAttacked)
            {
                // decide if we can attack, return attack component if so
                var tilesWithAttackableCharacters = scene.GetImmediateTilesWithAttackableCharacters(_enemy.Position, true);
                if (tilesWithAttackableCharacters.Count > 0)
                {
                    // TODO: ideally we should check if there are more than one spots to attack. If so, pick the one with the lowest HP/the one least likely to kill us
                    var tileToAttack = tilesWithAttackableCharacters[0];

                    // NOTE: at this point, only one entity can occupy a tile, but this might change in the future
                    var characterToAttack = scene.GetCharacterFromSelectedTile(tileToAttack);

                    _enemy.GetComponent<TurnStateComponent>().alreadyAttacked = true;
                    return new BattleState.TransitionInState(_enemy, characterToAttack, this, goStraightToCombat: true);
                }
            }
            else
            {
                // if we've already attacked, we can't move, turn is done
                return endTurn(scene);
            }

            if (_enemy.GetComponent<TurnStateComponent>().alreadyMoved)
            {
                // end turn, we've already decided we can't attack
                return endTurn(scene);
            }
            else
            {
                // go ahead and move
                // TODO: in later difficulties, it would probably be best to determine if a movement and attacking another character would be better)
                _enemy.GetComponent<TurnStateComponent>().alreadyMoved = true;
                return new NpcStartMovementState(_enemy);
            }

        }

        private BaseState endTurn(LevelScene scene)
        {
            // end turn, we've already decided we can't attack
            _enemy.GetComponent<EnemyTurnStateComponent>().finishedTurn = true;
            _enemy = null;
            return this;
        }

        public override void ExitState(LevelScene scene) { }
    }
}
