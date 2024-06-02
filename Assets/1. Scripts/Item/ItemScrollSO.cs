using Ability;
using Items;
using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unit.Constructs;
using UnityEngine;

namespace Unit.Entities
{
    [CreateAssetMenu(menuName = "Item/ItemScroll")]
    public class ItemScrollSO : ItemSO
    {
        [field: InfoBox("Each Ability Scroll can have multiple levels", EInfoBoxType.Normal)]
        [field: SerializeField] public AbilitySO[] AbilitySOList;
        [field: SerializeField] public ConstructSO[] ConstructSOList;
    }
}