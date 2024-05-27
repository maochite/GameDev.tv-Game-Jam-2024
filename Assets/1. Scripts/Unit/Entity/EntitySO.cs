using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entities
{
    public class EntitySO : UnitSO
    {
        public EntityActionAnimation AttackAnimation { get; private set; } = EntityActionAnimation.Attack;
    }
}