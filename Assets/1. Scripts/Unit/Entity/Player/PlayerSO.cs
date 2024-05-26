using Ability;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entity
{
    [CreateAssetMenu(menuName = "Unit/Entity/PlayerSO")]
    public class PlayerSO : EntitySO
    {
        [field: SerializeField] public AbilityPrimarySO DefaultAbility { get; private set; }
    }

}