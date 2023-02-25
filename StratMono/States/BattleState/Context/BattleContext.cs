using StratMono.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StratMono.States.BattleState.Context
{
    public class BattleContext
    {
        public CharacterGridEntity CharacterGridEntityAttacking;
        public CharacterGridEntity CharacterGridEntityBeingAttacked;
        public bool AttackerOnLeft;

        public BattleContext(CharacterGridEntity characterGridEntityAttacking, CharacterGridEntity characterGridEntityBeingAttacked, bool attackerOnLeft)
        {
            CharacterGridEntityAttacking = characterGridEntityAttacking;
            CharacterGridEntityBeingAttacked = characterGridEntityBeingAttacked;
            AttackerOnLeft = attackerOnLeft;
        }   
    }
}
