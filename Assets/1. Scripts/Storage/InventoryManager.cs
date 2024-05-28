using Items;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Storage
{
    [Serializable]
    public class ResourceStorage
    {
        [field: SerializeField, ReadOnly] public int Wood { get; set; } = 0;
        [field: SerializeField, ReadOnly] public int Stone { get; set; } = 0;
        [field: SerializeField, ReadOnly] public int Gold { get; set; } = 0;
    }

    [Serializable]
    public class ItemSlot
    {
        [field: SerializeField] public ItemSO ItemSO { get; private set; }
        [field: SerializeField] public int MaxItems { get; private set; }
        public int NumItems { get; set; } = 0;
    }


    public class InventoryManager : StaticInstance<InventoryManager>
    {

        public const int InventorySize = 16;
        public const int MaxResources = 9999;
        [field: SerializeField] public ItemSlot[] Inventory { get; private set; } = new ItemSlot[InventorySize];
        [field: SerializeField, ReadOnly] public ResourceStorage ResourceStorage { get; private set; }

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
                    if (ResourceStorage.Wood < MaxResources)
                    {
                        ResourceStorage.Wood++;
                        return true;
                    }
                    break;
                case ResourceType.Stone:
                    if (ResourceStorage.Stone < MaxResources)
                    {
                        ResourceStorage.Stone++;
                        return true;
                    }
                    break;
                case ResourceType.Gold:
                    if (ResourceStorage.Gold < MaxResources)
                    {
                        ResourceStorage.Gold++;
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
                    if (ResourceStorage.Wood > 0)
                    {
                        ResourceStorage.Wood--;
                        return true;
                    }
                    break;
                case ResourceType.Stone:
                    if (ResourceStorage.Stone > 0)
                    {
                        ResourceStorage.Stone--;
                        return true;
                    }
                    break;
                case ResourceType.Gold:
                    if (ResourceStorage.Gold > 0)
                    {
                        ResourceStorage.Gold--;
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