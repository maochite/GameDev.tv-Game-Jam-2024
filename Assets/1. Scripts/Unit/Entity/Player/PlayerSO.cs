using Ability;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unit.Constructs;
using UnityEngine;

namespace Unit.Entities
{
    [CreateAssetMenu(menuName = "Unit/Entity/PlayerSO")]
    public class PlayerSO : EntitySO
    {
        public EntityActionAnimation BuildAnimation { get; private set; } = EntityActionAnimation.Summon;

        [field: Header("- Player Specifics -")]
        [field: SerializeField, Range(1, 50)] public float BaseLightRadius { get; private set; } = 5f;
        [field: SerializeField, Range(0.25f, 5)] public float BaseRepairTime { get; private set; } = 1f;
        [field: SerializeField, Range(0.25f, 2)] public float BaseGatheringTime { get; private set; } = 0.5f;
        [field: SerializeField, Range(0.25f, 2)] public float BaseBuildingTime { get; private set; } = 1.5f;
        [field: SerializeField, Range(1, 5)] public float BaseGatheringDamage { get; private set; } = 1f;
        [field: SerializeField, Range(1, 10)] public float BaseGatherRadius { get; private set; } = 1f;
        [field: SerializeField, Range(1, 10)] public float BaseItemMagnetRadius { get; private set; } = 3f;
        [field: SerializeField, Range(1, 10)] public float BaseCollectionRadius { get; private set; } = 1f;
    }

}