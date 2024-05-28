using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit
{

    public class UnitSO : ScriptableObject
    {
        [field: Header("- Unit Specifics -")]
        [field: SerializeField] public int BaseHealth { get; private set; }
    }

}