using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unit.Gatherables
{

    public class Gatherable : Unit<GatherableSO>
    {
        [Header("For Non-Pool Prefab Placement")]
        [SerializeField] private GatherableSO nonPoolSO;

        [Header("Prefab Fields")]
        [SerializeField] private SpriteRenderer spriteObject;
        [SerializeField] private float shakeDuration;

        [SerializeField] private List<AnimationCurve> shakeVariations;
        private AnimationCurve shakeX;
        private AnimationCurve shakeZ;

        private float shakeTimer = 0f;
        private bool isDestroyed = false;
        private bool initialPositionSet = false;
        private int currentHealth = 0;

        public void Start()
        {
            if(nonPoolSO != null)
            {
                AssignGatherable(nonPoolSO);
            }
        }

        public void AssignGatherable(GatherableSO gatherableSO)
        {
            MaxHealth = gatherableSO.DefaultHealth;
            currentHealth = MaxHealth;
            UnitSO = gatherableSO;

            if (shakeVariations.Count > 0)
            {
                int randomIndex = Random.Range(0, shakeVariations.Count);
                shakeX = shakeVariations[randomIndex];
                shakeZ = shakeVariations[randomIndex];
            }
        }

        public override int CurrentHealth
        {
            get { return currentHealth; }

            protected set
            {
                if (isDestroyed) return;

                if (value < currentHealth)
                {
                    currentHealth--;
                    GatherResponse();

                    if (currentHealth <= 0)
                    {
                        Destroy(gameObject);
                        isDestroyed = true;
                    }
                }
            }
        }

        public void Gather()
        {
            //Gathering removes 1 health from the gatherable
            CurrentHealth -= 1;
        }

        void Update()
        {
            if (shakeTimer > 0)
            {
                float shakeAmountX = shakeX.Evaluate(1f - (shakeTimer / shakeDuration));
                float shakeAmountZ = shakeZ.Evaluate(1f - (shakeTimer / shakeDuration));

                Vector3 shakeOffset = new(shakeAmountX, 0f, shakeAmountZ);
                spriteObject.transform.localPosition = shakeOffset;

                shakeTimer -= Time.deltaTime;
            }
        }

        private void GatherResponse()
        {
            spriteObject.transform.localPosition = Vector3.zero;
            shakeTimer = shakeDuration;
        }
    }
}