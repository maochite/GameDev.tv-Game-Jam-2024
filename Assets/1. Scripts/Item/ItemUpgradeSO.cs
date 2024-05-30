using Ability;
using Items;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

namespace Unit.Entities
{
    [CreateAssetMenu(menuName = "Item/ItemUpgrade")]
    public class ItemUpgradeSO : ItemSO
    {
        [field: SerializeField] public List<StatModifier> StatModifiers;
    }
}