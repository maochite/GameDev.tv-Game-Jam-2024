using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entities
{
    public class EntitySO : UnitSO
    {
        [field: Header("Entity Specifics")]
        public EntityActionAnimation AttackAnimation { get; private set; } = EntityActionAnimation.Attack;
        [field: SerializeField, Range(1, 20)] public float BaseMovementSpeed { get; protected set; } = 5f;
        [field: SerializeField, Range(0.25f, 5)] public float BaseAttackTime { get; protected set; } = 1f;
        [field: SerializeField, Range(0.5f, 20)] public float BaseAttackRadius { get; protected set; } = 1f;
    }
}