using Items;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Storage
{
    [Serializable]
    public class ResourceSlot
    {
        [field: SerializeField] public TMP_Text NumTextUI { get; private set; }
        public int NumResources { get; set; } = 0;
    }

    [Serializable]
    public class ItemSlot
    {
        [field: SerializeField] public ItemSO ItemSO { get; private set; }
        [field: SerializeField] public int MaxItems { get; private set; }
        [field: SerializeField] public TMP_Text NumTextUI  { get; private set; }
        public int NumItems { get; set; } = 0;
    }


    public class InventoryManager : StaticInstance<InventoryManager>
    {

        [field: Header("Inventory Slots")]
        public const int InventorySize = 16;
        public const int MaxResources = 999;
        [field: SerializeField] public ItemSlot[] Inventory { get; private set; } = new ItemSlot[InventorySize];

        [field: Header("Resource Slots")]
        [field: SerializeField] public ResourceSlot WoodSlot { get; private set; }
        [field: SerializeField] public ResourceSlot StoneSlot { get; private set; }
        [field: SerializeField] public ResourceSlot GoldSlot { get; private set; }

        public bool AddItem(ItemSO itemSO)
        {
            if(itemSO is ItemResourceSO resourceSO)
            {
                if (AddResource(resourceSO))
                {
                    UpdateResources();
                    return true;
                }

                else return false;
            }

            foreach (ItemSlot itemSlot in Inventory)
            {
                if (itemSlot.ItemSO == itemSO)
                {
                    if(itemSlot.NumItems >= itemSlot.MaxItems)
                    {
                        return false;
                    }

                    itemSlot.NumItems++;

                    UpdateItemSlot(itemSlot);
                    return true;
                }
            }

            return false;
        }

        public bool RemoveItem(ItemSO itemSO)
        {
            if (itemSO is ItemResourceSO resourceSO)
            {
                if (RemoveResource(resourceSO))
                {
                    UpdateResources();
                    return true;
                }

                else return false;
            }

            foreach (ItemSlot itemSlot in Inventory)
            {
                if (itemSlot.ItemSO == itemSO)
                {
                    if (itemSlot.NumItems <= 0)
                    {
                        return false;
                    }

                    itemSlot.NumItems--;

                    UpdateItemSlot(itemSlot);
                    return true;
                }
            }

            return false;
        }

        private bool AddResource(ItemResourceSO itemResourceSO)
        {
            switch (itemResourceSO.ResourceType)
            {
                case ResourceType.Wood:
                    if (WoodSlot.NumResources < MaxResources)
                    {
                        WoodSlot.NumResources++;
                        return true;
                    }
                    break;
                case ResourceType.Stone:
                    if (StoneSlot.NumResources < MaxResources)
                    {
                        StoneSlot.NumResources++;
                        return true;
                    }
                    break;
                case ResourceType.Gold:
                    if (GoldSlot.NumResources < MaxResources)
                    {
                        GoldSlot.NumResources++;
                        return true;
                    }
                    break;
                default:
                    Debug.LogError("Resource Logic is broken");
                    break;
            }

            return false;
        }

        private bool RemoveResource(ItemResourceSO itemResourceSO)
        {
            switch (itemResourceSO.ResourceType)
            {
                case ResourceType.Wood:
                    if (WoodSlot.NumResources > 0)
                    {
                        WoodSlot.NumResources--;
                        return true;
                    }
                    break;
                case ResourceType.Stone:
                    if (StoneSlot.NumResources > 0)
                    {
                        StoneSlot.NumResources--;
                        return true;
                    }
                    break;
                case ResourceType.Gold:
                    if (GoldSlot.NumResources > 0)
                    {
                        GoldSlot.NumResources--;
                        return true;
                    }
                    break;
                default:
                    Debug.LogError("Resource Logic is broken");
                    break;
            }

            return false;
        }

        private void UpdateItemSlot(ItemSlot itemSlot)
        {

        }

        private void UpdateResources()
        {

        }

        private void OnValidate()
        {
            if (Inventory.Length != InventorySize)
            {
                Debug.LogWarning("Inventory Size must be 16 slots");

                ItemSlot[] newArray = new ItemSlot[16];

                for (int i = 0; i < newArray.Length; i++)
                {
                    if (i < Inventory.Length)
                    {
                        newArray[i] = Inventory[i];
                    }
                }

                Inventory = newArray;
            }
        }
    }
}