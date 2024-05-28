using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Items
{
    public class Item
    {
        public Item(ItemSO itemSO)
        {
            ItemSO = itemSO;
        }

        public ItemSO ItemSO { get; private set; }
    }
}