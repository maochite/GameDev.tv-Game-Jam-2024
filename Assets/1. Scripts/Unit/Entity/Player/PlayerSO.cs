using Ability;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Entities
{
    [CreateAssetMenu(menuName = "Unit/Entity/PlayerSO")]
    public class PlayerSO : EntitySO
    {
        [field: Header("- Player Specifics -")]
        [field: SerializeField] public AbilityPrimarySO DefaultAbility { get; private set; }
        [field: SerializeField, Range(0.25f, 2)] public float BaseGatheringTime { get; private set; } = 0.5f;
        [field: SerializeField, Range(1, 5)] public float BaseGatherRadius { get; private set; } = 1f;
        [field: SerializeField, Range(1, 10)] public float BaseItemMagnetRadius { get; private set; } = 3f;
        [field: SerializeField, Range(1, 5)] public float BaseCollectionRadius { get; private set; } = 1f;
    }

}