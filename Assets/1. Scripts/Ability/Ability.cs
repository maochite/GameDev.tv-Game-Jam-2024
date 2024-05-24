using System;
using System.Collections.Generic;
using System.Security.Principal;
using Unit.Entity;
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
        public IEntity Entity { get; private set; }

        public AbilitySO AbilitySO { get; private set; }

        public float CurrentCooldown { get; private set; } = 0;

        public int Damage { get; private set; } = 0;
        public float Cooldown { get; private set; } = 0;
        public float AbilitySpeed { get; private set; } = 0;
        public float AbilitySize { get; private set; } = 0;


        public Ability(AbilitySO abilitySO, IEntity entity)
        {
            Entity = entity;
            AbilitySO = abilitySO;

            UpdateAbilityStats();
        }

        public bool TryCast(Vector3 target, out Coroutine abilityCoroutine)
        {
            abilityCoroutine = null;

            if (Time.time >= CurrentCooldown)
            {
                CurrentCooldown = Time.time + Cooldown;

                abilityCoroutine = AbilityInitializer.Instance.Initialize(new(this, Entity.Transform, target));
                return true;
            }

            return false;
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