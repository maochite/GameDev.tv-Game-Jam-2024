using Ability;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entities
{
    [CreateAssetMenu(menuName = "Unit/Entity/EnemySO")]
    public class EnemySO : EntitySO
    {
        [field: Header("Enemy Specifics")]
        [field: SerializeField] public AbilityPrimarySO DefaultAbility { get; private set; }
    }
}