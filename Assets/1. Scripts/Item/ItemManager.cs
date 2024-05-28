using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

namespace Items
{
    public class ItemManager : StaticInstance<ItemManager>
    {
        [field: SerializeField] public ItemObject ItemObjectPrefab{ get; private set; }

        [Header("Item Pooling")]
        [SerializeField] int initalPoolSize = 100;
        [SerializeField] int poolExtension = 25;

        [SerializeField, ReadOnly] int currentActive = 0;

        private HashSet<ItemObject> activeItems = new();
        private Queue<ItemObject> itemSystemPool = new();

        private void Start()
        {
            ExtendItemObjectPool(initalPoolSize);
        }

        public Item CreateNewItem(ItemSO itemSO)
        {
            return new(itemSO);
        }

        private void ExtendItemObjectPool(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                ItemObject item = Instantiate(ItemObjectPrefab, Vector3.zero, Quaternion.identity, transform);
                item.gameObject.SetActive(false);
                itemSystemPool.Enqueue(item);
            }
        }

        public ItemObject RequestItemObject(Item item, Vector3 pos, Quaternion rot)
        {
            if (!itemSystemPool.TryDequeue(out ItemObject itemObj))
            {
                ExtendItemObjectPool(poolExtension);
                itemObj = itemSystemPool.Dequeue();
            }


            itemObj.transform.SetPositionAndRotation(pos, rot);
            itemObj.AssignItem(item);
            itemObj.gameObject.SetActive(true);
            activeItems.Add(itemObj);

            currentActive = activeItems.Count;


            return itemObj;
        }

        public void ReturnItemObjectToPool(ItemObject itemObj)
        {
            if (!activeItems.Contains(itemObj))
            {
                Debug.LogWarning("Invalid ItemObject returned to ItemObject Pool");
                return;
            }

            activeItems.Remove(itemObj);

            itemObj.StopAllCoroutines();
            itemObj.gameObject.SetActive(false);
            itemSystemPool.Enqueue(itemObj);

            currentActive = activeItems.Count;
        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void SpawnItemTest()
        {

        }
    }
}