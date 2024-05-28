using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public enum ResourceType
    {
        Wood,
        Stone,
        Gold
    }

    [CreateAssetMenu(menuName = "Item/ItemResourceSO")]
    public class ItemResourceSO : ItemSO
    {
        [field: SerializeField] public ResourceType ResourceType { get; private set; }
    }
}