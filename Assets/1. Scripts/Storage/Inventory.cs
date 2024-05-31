using Cinemachine.Utility;
using Items;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unit;
using Unit.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Storage
{
    [Serializable]
    public class ResourceSlot
    {
        [field: SerializeField] public TMP_Text NumTextUI { get; private set; }
        public int NumResources { get; set; } = 0;


        public void UpdateResourceDisplay()
        {
            NumTextUI.enabled = true;
            NumTextUI.text = NumResources.ToString(); 
        }
    }

    [Serializable]
    public class InventoryData
    {
        public List<ItemSlot> InventorySlots { get; private set; }

        [field: Header("Saps")]
        [field: SerializeField] public ItemSlot RejuvenatingSap { get; private set; }
        [field: SerializeField] public ItemSlot EnergizingSap { get; private set; }
        [field: SerializeField] public ItemSlot StimulatingSap { get; private set; }
        [field: SerializeField] public ItemSlot InvigoratingSap { get; private set; }

        [field: Header("Gems")]
        [field: SerializeField] public ItemSlot RadiantRuby { get; private set; }
        [field: SerializeField] public ItemSlot DazzlingDiamond { get; private set; }
        [field: SerializeField] public ItemSlot EnchantedEmerald { get; private set; }
        [field: SerializeField] public ItemSlot AlluringAquamarine { get; private set; }

        [field: Header("Scrolls")]
        [field: SerializeField] public ItemSlot BlazingScroll { get; private set; }
        [field: SerializeField] public ItemSlot ConjureScroll { get; private set; }
        [field: SerializeField] public ItemSlot HellfireScroll { get; private set; }
        [field: SerializeField] public ItemSlot SolarScroll { get; private set; }

        [field: Header("Tools")]
        [field: SerializeField] public ItemSlot Torch { get; private set; }
        [field: SerializeField] public ItemSlot Hammer { get; private set; }
        [field: SerializeField] public ItemSlot Pickaxe { get; private set; }
        [field: SerializeField] public ItemSlot ChoppingAxe { get; private set; }

        public void InitializeInventoryData()
        {
            InventorySlots = new()
            {
                RejuvenatingSap,
                EnergizingSap,
                StimulatingSap,
                InvigoratingSap,
                RadiantRuby,
                DazzlingDiamond,
                EnchantedEmerald,
                AlluringAquamarine,
                BlazingScroll,
                ConjureScroll,
                HellfireScroll,
                SolarScroll,
                Torch,
                Hammer,
                Pickaxe,
                ChoppingAxe
            };
        }
    }


    public class Inventory : MonoBehaviour
    {

        [field: Header("Inventory Slots")]
        public const int InventorySize = 16;
        public const int MaxScrollLevel = 5;
        public const int MaxResources = 999;
        [field: SerializeField] public InventoryData InventoryData { get; private set; }

        [field: Header("Resource Slots")]
        [field: SerializeField] public ResourceSlot WoodSlot { get; private set; }
        [field: SerializeField] public ResourceSlot StoneSlot { get; private set; }
        [field: SerializeField] public ResourceSlot GoldSlot { get; private set; }

        [field: Header("Starting Items")]
        [field: SerializeField] public List<ItemSO> StartingItemsList { get; private set; }

        [field: Header("Item Add Debug")]
        [field: SerializeField] public ItemSO DebugItemToAdd { get; private set; }
        [field: SerializeField] public int DebugItemAmount { get; private set; }

        public void InitializeInventory()
        {
            InventoryData.InitializeInventoryData();
            foreach (var slot in InventoryData.InventorySlots)
            {
                slot.UpdateItemSlotDisplay();
            }

            ResolveResources();

            foreach (ItemSO itemSO in StartingItemsList)
            {
                AddItem(itemSO);
            }
        }

        public bool AddItem(ItemSO itemSO)
        {
            if(itemSO is ItemResourceSO resourceSO)
            {
                if (AddResource(resourceSO))
                {
                    ResolveResources();
                    return true;
                }

                else return false;
            }

            foreach (ItemSlot itemSlot in InventoryData.InventorySlots)
            {
                if (itemSlot.ItemSO == itemSO)
                {
                    if(itemSlot.NumItems >= itemSlot.MaxItems)
                    {
                        return false;
                    }

                    itemSlot.NumItems++;

                    ResolveItemSlot(itemSlot);
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
                    ResolveResources();
                    return true;
                }

                else return false;
            }

            foreach (ItemSlot itemSlot in InventoryData.InventorySlots)
            {
                if (itemSlot.ItemSO == itemSO)
                {
                    if (itemSlot.NumItems <= 0)
                    {
                        return false;
                    }

                    itemSlot.NumItems--;

                    ResolveItemSlot(itemSlot);
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

        private void ResolveItemSlot(ItemSlot itemSlot)
        {
            if (!PlayerManager.Instance.TryGetPlayer(out Player player)) return;

            if(itemSlot.ItemSO is ItemUpgradeSO itemUpgradeSO)
            {
                foreach(StatModifier modifier in itemUpgradeSO.StatModifiers)
                {
                    EntityStatsManager.Instance.InsertStatModifier(modifier);
                }
            }

            else if(itemSlot.ItemSO is ItemScrollSO itemScrollSO)
            {
                if (itemSlot.NumItems <= 0) return;

                int scrolLevel;

                if(itemSlot.NumItems > MaxScrollLevel)
                {
                    scrolLevel = MaxScrollLevel;
                }

                else scrolLevel = itemSlot.NumItems;

                if(itemScrollSO == InventoryData.BlazingScroll.ItemSO)
                {
                    player.LearnNewAbility(itemScrollSO.AbilitySOList[scrolLevel - 1], 0);
                }

                else if(itemSlot.ItemSO == InventoryData.ConjureScroll.ItemSO)
                {
                    player.LearnNewAbility(itemScrollSO.AbilitySOList[scrolLevel - 1], 1);
                }

                else if(itemScrollSO == InventoryData.HellfireScroll.ItemSO)
                {
                    player.LearnNewAbility(itemScrollSO.AbilitySOList[scrolLevel - 1], 2);
                }

                else if(itemScrollSO == InventoryData.SolarScroll.ItemSO)
                {
                    player.LearnNewAbility(itemScrollSO.AbilitySOList[scrolLevel - 1], 3);
                }

                else
                {
                    Debug.LogError("Incorrect Scroll/Ability configured for player and inventory");
                }
            }

            else
            {
                Debug.LogError("Incorrect item stored in item slot");
                return;
            }

            itemSlot.UpdateItemSlotDisplay();
        }

        private void ResolveResources()
        {
            WoodSlot.UpdateResourceDisplay();
            StoneSlot.UpdateResourceDisplay();
            GoldSlot.UpdateResourceDisplay();
        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void AddItemDebug()
        {
            for(int i = 0; i < DebugItemAmount; i++)
            {
                AddItem(DebugItemToAdd);
            }
        }
    }
}