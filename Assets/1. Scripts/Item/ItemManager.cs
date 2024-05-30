using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;
using static System.TimeZoneInfo;

namespace Items
{
    public class ItemManager : StaticInstance<ItemManager>
    {


        [field: SerializeField] public ItemObject ItemObjectPrefab { get; private set; }
        [field: SerializeField] public Material ItemObjectMaterial { get; private set; }

        [field: Header("Item Pooling")]
        [field: SerializeField, Range(0, 1000)] public int InitalPoolSize { get; private set; } = 100;
        [field: SerializeField, Range(0, 1000)] public int PoolExtension { get; private set; } = 25;

        [SerializeField, ReadOnly] int currentActive = 0;

        private readonly HashSet<ItemObject> activeItems = new();
        private readonly Queue<ItemObject> itemSystemPool = new();

        private void Start()
        {
            //ExtendItemObjectPool(initalPoolSize);
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

        public ItemObject RequestItemObject(ItemSO itemSO, Vector3 pos, Quaternion rot)
        {
            if (!itemSystemPool.TryDequeue(out ItemObject itemObj))
            {
                ExtendItemObjectPool(PoolExtension);
                itemObj = itemSystemPool.Dequeue();
            }

            itemObj.GetComponent<SpriteRenderer>().material = ItemObjectMaterial;
            itemObj.transform.SetPositionAndRotation(pos, rot);
            itemObj.AssignItemSO(itemSO);
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

            Destroy(itemObj.GetComponent<SpriteRenderer>().material);
            itemObj.StopAllCoroutines();
            itemObj.gameObject.SetActive(false);
            itemSystemPool.Enqueue(itemObj);

            currentActive = activeItems.Count;
        }

        public void Update()
        {

        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void SpawnItemTest()
        {

        }
    }
}