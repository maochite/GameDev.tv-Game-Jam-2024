using Ability;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public class ItemAbilitySO : ItemSO
    {
        [field: SerializeField] public AbilityPrimarySO AbilityPrimarySO { get; private set; }
    }
}