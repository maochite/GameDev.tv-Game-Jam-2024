using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

namespace Unit.Entities
{

    public class Enemy : Entity<EnemySO>
    {
        [Header("- Enemy Specifics -")]
        public EnemySO EnemySO => UnitSO;

        public override float CurrentHealth { get => throw new System.NotImplementedException(); protected set => throw new System.NotImplementedException(); }

        public override void AssignUnit(EnemySO enemySO)
        {
            base.AssignUnit(enemySO);
        }

        //protected override void Start();

        //protected override void Update();

        //public void AssignEnemy(EnemySO enemySO)
        //{
        //    AssignEntity(enemySO);
        //}

        //public override void UpdateEntityStats()
    }
    
}