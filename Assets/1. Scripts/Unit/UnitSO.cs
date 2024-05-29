using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{

    public class UnitSO : ScriptableObject
    {
        [field: Header("- Unit Specifics -")]
        [field: SerializeField] public float BaseHealth { get; private set; }
        [field: SerializeField] public float BaseHealthRegen { get; private set; }
    }

}