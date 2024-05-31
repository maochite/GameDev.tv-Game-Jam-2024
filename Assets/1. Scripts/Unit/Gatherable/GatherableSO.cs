using Items;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

namespace Unit.Gatherables
{
    [CreateAssetMenu(menuName = "Unit/GatherableSO")]
    public class GatherableSO : UnitSO
    {
        [field: Header("Gatherable Specifics")]
        //[field: SerializeField] public Sprite Sprite { get; private set; }
        [field: SerializeField] public List<ItemSO> ItemPool { get; private set; }
        [field: SerializeField] public Vector3 ItemDropOffset { get; private set; }
        [field: SerializeField] public EntityActionAnimation GatheringAnimation { get; private set; }

    }
}