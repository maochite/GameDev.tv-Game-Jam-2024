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
        public enum FlickerState
        {
            FlickerIn,
            FlickerOut,
            Pause,
        }

        private const float returnTime = 60f;
        private float returnTimer = 0f;

        [field: Header("Item Flicker Effect")]
        [field: SerializeField, Range(0.1f, 10)] public float TransitionDuration { get; private set; } = 1;
        [field: SerializeField, Range(0.1f, 10)] public float FlickerStartDelay { get; private set; } = 1;
        [field: SerializeField, Range(0.1f, 10)] public float FlickerTransitionDelay { get; private set; } = 1;
        [field: SerializeField] public Color TargetColor { get; private set; }
        private Material material;
        private Color originalColor;
        private float stateTimer = 0f;
        private FlickerState flickerState;
        private float flickerCooldownTimer = 0f;
        private bool flickerCooldownFlag = false;


        [field: Header("Item Flicker Effect")]
        [field: SerializeField, Range(0, 50)] public float ScatterForce { get; private set; } = 1f;
        [field: SerializeField, Range(0, 1)] public float SpawnOffset { get; private set; } = 0.25f;
        [field: SerializeField, Range(0, 50)] public float HomingSpeed { get; private set; } = 5f;
        [field: SerializeField, Range(0, 5)] public float HomingDelay { get; private set; } = 1f;
        [field: SerializeField] public Vector3 HomingTargetOffset { get; private set; } = Vector3.zero;


        Rigidbody rigidBody;
        SpriteRenderer spriteRenderer;

        public ItemSO ItemSO { get; private set; }

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
            spriteRenderer.sprite = ItemSO.WorldSprite;
            material = spriteRenderer.material;
            originalColor = material.GetColor("_EmissionColor");
            canHome = false;

            stateTimer = 0f;
            flickerState = FlickerState.FlickerIn;
            flickerCooldownTimer = 0f;
            flickerCooldownFlag = false;
            returnTimer = 0f;
        }

        public void ScatterItem(Vector3 pos)
        {
            rigidBody.detectCollisions = true;
            rigidBody.isKinematic = false;

            Vector3 scatterDirection = UnityEngine.Random.insideUnitSphere + Vector3.up;
            scatterDirection.Normalize();
            rigidBody.AddForce(scatterDirection * ScatterForce, ForceMode.Impulse);
            transform.position = pos + UnityEngine.Random.insideUnitSphere * SpawnOffset;

            HomingCoroutineRef = StartCoroutine(HomingDelayCoroutine());
        }

        public void DropItem(Vector3 pos)
        {
            transform.position = pos;
            HomingCoroutineRef = StartCoroutine(HomingDelayCoroutine());
        }

        public IEnumerator HomingDelayCoroutine()
        {
            yield return new WaitForSeconds(HomingDelay);
            canHome = true;
        }

        public void CollectItem()
        {
            if(Player.Instance.Inventory.AddItem(ItemSO))
            {
                Destroy(material);
                ItemManager.Instance.ReturnItemObjectToPool(this);
            }
        }

        private void Update()
        {
            returnTimer += Time.deltaTime;

            if(returnTimer > returnTime)
            {
                Destroy(material);
                ItemManager.Instance.ReturnItemObjectToPool(this);
            }

            if (!flickerCooldownFlag)
            {
                flickerCooldownTimer += Time.deltaTime;
                if (flickerCooldownTimer >= FlickerStartDelay)
                {
                    flickerCooldownFlag = true;
                }
                return;
            }

            stateTimer += Time.deltaTime;

            if (flickerState == FlickerState.FlickerIn)
            {
                float t = Mathf.Clamp01(stateTimer / TransitionDuration);

                if (t >= 1.0f)
                {
                    flickerState = FlickerState.FlickerOut;
                    material.SetColor("_EmissionColor", TargetColor);
                    stateTimer = 0f;
                }

                else
                {
                    var newColor = Color.Lerp(originalColor, TargetColor, t);
                    material.SetColor("_EmissionColor", newColor);
                }

            }

            else if(flickerState == FlickerState.FlickerOut)
            {
                float t = Mathf.Clamp01(stateTimer / TransitionDuration);
                if (t >= 1.0f)
                {
                    flickerState = FlickerState.Pause;
                    material.SetColor("_EmissionColor", originalColor);
                    stateTimer = 0f;
                }

                else
                {
                    var newColor = Color.Lerp(TargetColor, originalColor, t);
                    material.SetColor("_EmissionColor", newColor);
                }
            }

            else
            {

                stateTimer += Time.deltaTime;

                if (stateTimer >= FlickerTransitionDelay)
                {
                    flickerState = FlickerState.FlickerIn;
                    stateTimer = 0;
                }
              
            }
        }



        private void FixedUpdate()
        {

            if (Player.Instance != null)
            {
                float distance = Vector3.Distance(transform.position, Player.Instance.transform.position);

                ////collect item logic
                if (distance < Player.Instance.CollectionRadius)
                {
                    CollectItem();
                }

                if (canHome && distance <= Player.Instance.ItemMagnetRadius)
                {

                    Vector3 targetPosition = Player.Instance.transform.position + HomingTargetOffset;
                    Vector3 targetDirection = (targetPosition - transform.position).normalized;

                    rigidBody.MovePosition(transform.position + HomingSpeed * Time.fixedDeltaTime * targetDirection);

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

