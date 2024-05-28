using Storage;
using System;
using System.Collections;
using Unit.Entities;
using UnityEngine;



namespace Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class ItemObject : MonoBehaviour
    {
        [Serializable]
        public class ItemDrop
        {
            [field: SerializeField, Range(0, 50)] public float ScatterForce { get; private set; } = 1f;
            [field: SerializeField, Range(0, 1)] public float SpawnOffset { get; private set; } = 0.25f;
            [field: SerializeField, Range(0, 50)] public float HomingSpeed { get; private set; } = 5f;
            [field: SerializeField, Range(0, 5)] public float HomingDelay { get; private set; } = 1f;
            [field: SerializeField] public Vector3 HomingTargetOffset { get; private set; } = Vector3.zero;
        }

        Rigidbody rigidBody;
        SpriteRenderer spriteRenderer;

        public ItemSO ItemSO { get; private set; }
        [field: SerializeField] public ItemDrop ItemDropData { get; private set; }

        private Coroutine HomingCoroutineRef;
        private bool canHome = false;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void AssignItemSO(ItemSO itemSO)
        {
            ItemSO = itemSO;
            spriteRenderer.sprite = ItemSO.Sprite;
            canHome = false;
        }

        public void ScatterItem(Vector3 pos)
        {
            rigidBody.detectCollisions = true;
            rigidBody.isKinematic = false;

            Vector3 scatterDirection = UnityEngine.Random.insideUnitSphere + Vector3.up;
            scatterDirection.Normalize();
            rigidBody.AddForce(scatterDirection * ItemDropData.ScatterForce, ForceMode.Impulse);
            transform.position = pos + UnityEngine.Random.insideUnitSphere * ItemDropData.SpawnOffset;

            HomingCoroutineRef = StartCoroutine(HomingDelayCoroutine());
        }

        public void DropItem(Vector3 pos)
        {
            transform.position = pos;
            HomingCoroutineRef = StartCoroutine(HomingDelayCoroutine());
        }

        public IEnumerator HomingDelayCoroutine()
        {
            yield return new WaitForSeconds(ItemDropData.HomingDelay);
            canHome = true;
        }

        public void CollectItem()
        {
            if (InventoryManager.Instance.AddItem(ItemSO))
            {
                ItemManager.Instance.ReturnItemObjectToPool(this);
            }
        }

    
        private void FixedUpdate()
        {

            if (PlayerManager.Instance.TryGetPlayer(out Player player))
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

                ////collect item logic
                if (distance < player.CollectionRadius)
                {
                    CollectItem();
                }

                if (canHome && distance <= player.ItemMagnetRadius)
                {

                    Vector3 targetPosition = player.transform.position + ItemDropData.HomingTargetOffset;
                    Vector3 targetDirection = (targetPosition - transform.position).normalized;

                    rigidBody.MovePosition(transform.position + ItemDropData.HomingSpeed * Time.fixedDeltaTime * targetDirection);

                    rigidBody.detectCollisions = false;
                    rigidBody.isKinematic = true;
                }

                else
                {
                    rigidBody.detectCollisions = true;
                    rigidBody.isKinematic = false;
                }
            }
        }
    }
}