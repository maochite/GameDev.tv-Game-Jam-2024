using System;
using System.Collections.Generic;
using System.Security.Principal;
using Unit.Entities;
using UnityEngine;
using UnityEngine.VFX;

namespace Ability
{
    public enum AbilityStatType
    {
        Damage,
        Cooldown,
        AbilitySpeed,
        AbilitySize,
    };

    public abstract class Ability
    {
        public ICaster Caster { get; private set; }

        public AbilitySO AbilitySO { get; private set; }

        public int Damage { get; private set; } = 0;
        public float Cooldown { get; private set; } = 0;
        public float AbilitySpeed { get; private set; } = 0;
        public float AbilitySize { get; private set; } = 0;

        private float lastCooldownTime = 0;


        public Ability(AbilitySO abilitySO, ICaster caster)
        {
            Caster = caster;
            AbilitySO = abilitySO;

            //Temp
            Cooldown = AbilitySO.AttributeData.Cooldown;
            AbilitySpeed = AbilitySO.MovementData.BaseSpeed;
            AbilitySize = AbilitySO.SizeData.BaseSize;

            UpdateAbilityStats();
        }

        public bool TryCast(Vector3 target, out Coroutine abilityCoroutine)
        {
            abilityCoroutine = null;

            if (!IsCoolingDown())
            {
                lastCooldownTime = Time.time + Cooldown;

                abilityCoroutine = AbilityInitializer.Instance.Initialize(new(this, Caster.Transform, target));
                return true;
            }

            return false;
        }

        public bool IsCoolingDown()
        {
            if (Time.time < lastCooldownTime)
            {
                return true;
            }

            else return false;
        }

        public void UpdateAbilityStats()
        {
            //SetDamageStat(EntityModifications.Instance.GetDamageModified(Entity.EntitySO, AbilitySO));
            //SetCooldownStat(EntityModifications.Instance.GetCooldownModified(Entity.EntitySO, AbilitySO));
            //SetAbilitySpeedStat(EntityModifications.Instance.GetAbilitySpeedModified(Entity.EntitySO, AbilitySO));
            //SetAbilitySizeStat(EntityModifications.Instance.GetAbilitySizeModified(Entity.EntitySO, AbilitySO));
        }

        public void SetDamageStat(int damage)
        {
            if(damage < 1) damage = 1;

            Damage = damage;
        }

        public void SetCooldownStat(float cooldown)
        {
            if (cooldown < 0.1f) cooldown = 0.1f;

            Cooldown = cooldown;
        }

        public void SetAbilitySpeedStat(float abilitySpeed)
        {
            if (abilitySpeed < 0.1f) abilitySpeed = 0.1f;

            AbilitySpeed = abilitySpeed;
        }

        public void SetAbilitySizeStat(float abilitySize)
        {
            if (abilitySize < 0.1f) abilitySize = 0.1f;

            AbilitySize = abilitySize;
        }
    }
}