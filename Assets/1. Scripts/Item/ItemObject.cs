using Ability;
using Items;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;
using UnityEngine.Windows;
using static Ability.AbilitySO.Composition;


namespace Items
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(SphereCollider))]
    public class ItemObject : MonoBehaviour
    {
        Rigidbody rigidBody;
        SpriteRenderer spriteRenderer;

        public Item Item { get; private set; }
        [field: SerializeField] public Vector3 HomingTargetOffset { get; private set; }
        [field: SerializeField, Range(0.01f, 1)] public float CollectionRange { get; private set; } = 0.1f;

        private Coroutine HomingCoroutineRef;
        private bool canHome = false;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void AssignItem(Item item)
        {
            Item = item;
            spriteRenderer.sprite = Item.ItemSO.Sprite;
            canHome = false;
        }

        public void DropItem(Vector3 pos)
        {
            var itemDrop = Item.ItemSO.ItemDropBehaviour;

            rigidBody.detectCollisions = true;
            rigidBody.isKinematic = false;

            if (itemDrop.ItemDropType == ItemDropType.Scatter)
            {
                Vector3 scatterDirection = Random.insideUnitSphere + Vector3.up;
                scatterDirection.Normalize();
                rigidBody.AddForce(scatterDirection * itemDrop.ScatterForce, ForceMode.Impulse);
                transform.position = pos + Random.insideUnitSphere * itemDrop.SpawnOffset;

            }

            else
            {
                transform.position = pos;
            }

            if (itemDrop.CanHome)
            {
                HomingCoroutineRef = StartCoroutine(HomingDelayCoroutine());
            }
        }

        public IEnumerator HomingDelayCoroutine()
        {
            var itemDrop = Item.ItemSO.ItemDropBehaviour;
            yield return new WaitForSeconds(itemDrop.HomingDelay);
            canHome = true;
        }

    
        private void FixedUpdate()
        {
            var playerRef = PlayerManager.Instance;

    
            if (playerRef.TryGetPlayerPosition(out Vector3 playerPos))
            {
                float distance = (Vector3.Distance(transform.position, playerPos));

                ////collect item logic
                if (distance < CollectionRange)
                {
                    ItemManager.Instance.ReturnItemObjectToPool(this);
                    return;
                }

                var itemDrop = Item.ItemSO.ItemDropBehaviour;

                if (canHome && distance <= itemDrop.HomingRange)
                {

                    Vector3 targetPosition = playerPos + HomingTargetOffset;
                    Vector3 targetDirection = (targetPosition - transform.position).normalized;

                    rigidBody.MovePosition(transform.position + targetDirection * itemDrop.HomingSpeed * Time.fixedDeltaTime);

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