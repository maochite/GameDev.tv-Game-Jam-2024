using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

namespace Unit.Entity
{

    public class Enemy : Entity<EnemySO>
    {
        public EnemySO EnemySO => UnitSO;

        public override void AssignEntity(EnemySO enemySO)
        {
            base.AssignEntity(enemySO);
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