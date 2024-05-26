using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Gatherables
{
    
    [CreateAssetMenu(menuName = "Unit/GatherableSO")]
    public class GatherableSO : UnitSO
    {
        [field: SerializeField] public ResourceType ResourceType { get; private set; }
    }
}