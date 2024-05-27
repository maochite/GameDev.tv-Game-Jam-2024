using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

namespace Unit.Gatherables
{
    [CreateAssetMenu(menuName = "Unit/GatherableSO")]
    public class GatherableSO : UnitSO
    {
        [field: SerializeField] public ResourceType ResourceType { get; private set; }
        [field: SerializeField] public EntityActionAnimation GatheringAnimation { get; private set; }
    }
}