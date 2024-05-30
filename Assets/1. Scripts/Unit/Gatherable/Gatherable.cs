using Items;
using NaughtyAttributes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unit.Gatherables
{

    public class Gatherable : Unit<GatherableSO>
    {
        [Header("- Gatherable Specifics -")]

        [Header("For Non-Pool Prefab Placement")]
        [SerializeField] private GatherableSO nonPoolSO;

        [Header("Prefab Fields")]
        [SerializeField] private SpriteRenderer spriteObject;
        [SerializeField] private float shakeDuration;

        [SerializeField] private List<AnimationCurve> shakeVariations;
        private AnimationCurve shakeX;
        private AnimationCurve shakeZ;

        private float shakeTimer = 0f;
        private bool initialPositionSet = false;

        public void Start()
        {
            if(nonPoolSO != null)
            {
                AssignUnit(nonPoolSO);
            }
        }

        public override void AssignUnit(GatherableSO gatherableSO)
        {

            base.AssignUnit(gatherableSO);
            MaxHealth = gatherableSO.BaseHealth;
            CurrentHealth = MaxHealth;

            if (shakeVariations.Count > 0)
            {
                int randomIndex = UnityEngine.Random.Range(0, shakeVariations.Count);
                shakeX = shakeVariations[randomIndex];
                shakeZ = shakeVariations[randomIndex];
            }
        }

        [SerializeField, ReadOnly] private float currentHealth = 0;
        public override float CurrentHealth
        {
            get { return currentHealth; }

            protected set
            {
                if (!isActive) return;

                if (value < currentHealth)
                {
                    currentHealth = value;
                    GatherResponse();

                    if (currentHealth <= 0)
                    {
                        OnDeath();
                        Destroy(gameObject);
                        isActive = false;
                    }
                }

                else if (value > maxHealth)
                {
                    currentHealth = maxHealth;
                }

                else currentHealth = value;
            }
        }
        

        [SerializeField, ReadOnly] private float maxHealth = 0;
        public override float MaxHealth 
        { 
            get => maxHealth;

            protected set
            {
                if (!isActive) return;

                maxHealth = value;

                if (maxHealth < 1)
                {
                    maxHealth = 1;
                }

                if (currentHealth >= maxHealth)
                {
                    currentHealth = maxHealth;

                    //healthBar.ToggleHealthBar(false);
                }

                //healthBar.SetHealthBarValue(_currentHealth, _maxHealth);
            }
        }

        //May not implement this
        public override float HealthRegen 
        { 
            get => throw new NotImplementedException(); 
            protected set => throw new NotImplementedException(); 
        }

        public void Gather(float gatherAmount)
        {
            CurrentHealth -= gatherAmount;
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

        private void OnDeath()
        {
            foreach(ItemSO itemSO in UnitSO.ItemPool)
            {
                ItemObject itemObj = ItemManager.Instance.RequestItemObject(itemSO, transform.position, Quaternion.identity);
                itemObj.ScatterItem(transform.position + UnitSO.ItemDropOffset);
            }
        }
    }
}