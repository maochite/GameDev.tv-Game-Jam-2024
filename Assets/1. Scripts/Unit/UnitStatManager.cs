using Ability;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using Unit.Entities;
using UnityEngine;

public class UnitStatManager : MonoBehaviour
{
    public enum StatModType
    {
        Health,
        HealthRegen,
        MovementSpeed,
        Damage,
        Cooldown,
        AbilitySize,
        AbilitySpeed,
        GatherSpeed,
        GatherDamage,
        RepairSpeed,
        LightRadius,
    }


    [Serializable]
    public class StatModifier
    {
        //Stat mods are represented as integers which are then modified by EntitymodValues for a final value

        [field: SerializeField] public EntityType EntityType { get; private set; }
        [field: SerializeField] public StatModType StatModType { get; private set; }
        [field: SerializeField] public int Value { get; private set; }
    }

    public class EntityModifications : StaticInstance<EntityModifications>
    {

        [Serializable]
        public class StatValueConstraints
        {
            [field: InfoBox("Min/Max Global Constraint Values", EInfoBoxType.Normal), Space]
            [field: SerializeField, MinMaxSlider(1, 5000)]
            public Vector2 Health { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(1, 5000)]
            public Vector2 HealthRegen { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(1, 100)]
            public Vector2 Damage { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(1, 20)]
            public Vector2 MovementSpeed { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(0.25f, 120)]
            public Vector2 Cooldown { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(0f, 35)]
            public Vector2 AbilitySpeed { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(0.25f, 10)]
            public Vector2 AbilitySize { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(0.25f, 35)]
            public Vector2 GatherSpeed { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(0.25f, 10)]
            public Vector2 GatherDamage { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(0.25f, 10)]
            public Vector2 RepairTime { get; private set; } = new(1, 1);

            [field: SerializeField, MinMaxSlider(1f, 35)]
            public Vector2 LightRadius { get; private set; } = new(1, 1);
        }

        [Serializable]
        public class EntityModValues
        {
            //These values represent the increase PER upgrade for every upgrade a player or enemy may have received

            [field: InfoBox("Percent Increases Per StatModifier", EInfoBoxType.Normal)]
            [field: SerializeField, Range(0.25f, 100)] public float Health { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float HealthRegen { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float Damage { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float Cooldown { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float MovementSpeed { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float AbilitySpeed { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float AbilitySize { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float GatherSpeed { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float GatherDamage { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float RepairSpeed { get; private set; } = 1;
            [field: SerializeField, Range(0.25f, 100)] public float LightRadius { get; private set; } = 1;
        }

        private Dictionary<StatModType, List<StatModifier>> currentPlayerModifiers = new()
        {
            [StatModType.Health] = new(),
            [StatModType.HealthRegen] = new(),
            [StatModType.LightRadius] = new(),
            [StatModType.Cooldown] = new(),
            [StatModType.MovementSpeed] = new(),
            [StatModType.Damage] = new(),
            [StatModType.GatherSpeed] = new(),
            [StatModType.RepairSpeed] = new(),
        };

        private Dictionary<StatModType, List<StatModifier>> currentEnemyModifiers = new()
        {
            [StatModType.Health] = new(),
            [StatModType.HealthRegen] = new(),
            [StatModType.MovementSpeed] = new(),
            [StatModType.Damage] = new(),
            [StatModType.Cooldown] = new(),
        };

        private Dictionary<StatModType, List<StatModifier>> currentConstructModifiers = new()
        {
            [StatModType.Health] = new(),
            [StatModType.HealthRegen] = new(),
            [StatModType.Damage] = new(),
            [StatModType.Cooldown] = new(),
            [StatModType.AbilitySize] = new(),
        };


        [field: Header("Constraint Settings")]
        [field: SerializeField] public StatValueConstraints GameStatConstraints { get; private set; }

        [field: Header("Modifier Settings")]
        [field: SerializeField] public EntityModValues PlayerModifierValues { get; private set; }
        [field: SerializeField] public EntityModValues EnemyModifierValues { get; private set; }
        [field: SerializeField] public EntityModValues ConstructModifierValues { get; private set; }


        [field: SerializeField, Header("Debug")] private StatModifier StatModifierToAdd;

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void AddTestModifiers()
        {
            InsertStatModifier(StatModifierToAdd);
        }

        [Button(enabledMode: EButtonEnableMode.Playmode)]
        private void RemoveAllModifiers()
        {
            foreach (List<StatModifier> modifierSet in currentPlayerModifiers.Values)
            {
                modifierSet.Clear();
            }

            foreach (List<StatModifier> modifierSet in currentEnemyModifiers.Values)
            {
                modifierSet.Clear();
            }

            if (PlayerManager.Instance.TryGetPlayer(out Player player))
            {
                //player.UpdateEntityStats();
            }

            EnemyManager.Instance.UpdateActiveEnemyModifiedStats();
        }

        public bool InsertStatModifier(StatModifier statModifier)
        {
            if (statModifier.EntityType == EntityType.Player)
            {
                currentPlayerModifiers[statModifier.StatModType].Add(statModifier);
            }

            else if (statModifier.EntityType == EntityType.Enemy)
            {
                currentEnemyModifiers[statModifier.StatModType].Add(statModifier);
            }

            else //construct
            {
                currentConstructModifiers[statModifier.StatModType].Add(statModifier);
            }

            if (PlayerManager.Instance.TryGetPlayer(out Player player))
            {
                //player.UpdateEntityStats();
            }

            EnemyManager.Instance.UpdateActiveEnemyModifiedStats();

            return true;
        }

        private struct RelativeEntityData
        {
            public Dictionary<StatModType, List<StatModifier>> Modifiers;
            public EntityModValues ModValues;
        }

        private RelativeEntityData GetRelativeEntityData(EntitySO entitySO)
        {
            Dictionary<StatModType, List<StatModifier>> modDict;
            EntityModValues modValues;

            if (entitySO is PlayerSO)
            {
                modDict = currentPlayerModifiers;
                modValues = PlayerModifierValues;
            }

            else if (entitySO is EnemySO)
            {
                modDict = currentEnemyModifiers;
                modValues = EnemyModifierValues;
            }

            else // entitySO is construct
            {
                modDict = currentConstructModifiers;
                modValues = ConstructModifierValues;
            }

            return new RelativeEntityData
            {
                Modifiers = modDict,
                ModValues = modValues,
            };
        }

        public float GetHealthModified(EntitySO entitySO)
        {
            var relativeData = GetRelativeEntityData(entitySO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.Health])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = entitySO.BaseHealth * (1 + (upgrades * relativeData.ModValues.Health / 100));
            return ClampValue(modifiedValue, GameStatConstraints.Health.x, GameStatConstraints.Health.y);
        }

        public float GetHealthRegenModified(EntitySO entitySO)
        {
            var relativeData = GetRelativeEntityData(entitySO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.HealthRegen])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = entitySO.BaseHealthRegen * (1 + (upgrades * relativeData.ModValues.HealthRegen / 100));
            return ClampValue(modifiedValue, GameStatConstraints.HealthRegen.x, GameStatConstraints.HealthRegen.y);
        }


        public float GetMovementModified(EntitySO entitySO)
        {
            var relativeData = GetRelativeEntityData(entitySO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.MovementSpeed])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = entitySO.BaseMovementSpeed * (1 + (upgrades * relativeData.ModValues.AbilitySpeed / 100));
            return ClampValue(modifiedValue, GameStatConstraints.MovementSpeed.x, GameStatConstraints.MovementSpeed.y);
        }

        public float GetDamageModified(EntitySO entitySO, AbilitySO abilitySO)
        {
            var relativeData = GetRelativeEntityData(entitySO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.Damage])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = abilitySO.AttributeData.Damage * (1 + (upgrades * relativeData.ModValues.Damage / 100));
            return ClampValue(modifiedValue, GameStatConstraints.Damage.x, GameStatConstraints.Damage.y);
        }

        public float GetCooldownModified(EntitySO entitySO, AbilitySO abilitySO)
        {
            var relativeData = GetRelativeEntityData(entitySO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.Cooldown])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = (1 - (upgrades * (relativeData.ModValues.Cooldown / 100))) * abilitySO.AttributeData.Cooldown;
            return ClampValue(modifiedValue, GameStatConstraints.Cooldown.x, GameStatConstraints.Cooldown.y);
        }

        public float GetAbilitySpeedModified(EntitySO entitySO, AbilitySO abilitySO)
        {
            var relativeData = GetRelativeEntityData(entitySO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.AbilitySpeed])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = abilitySO.MovementData.BaseSpeed * (1 + (upgrades * relativeData.ModValues.AbilitySpeed / 100));
            return ClampValue(modifiedValue, GameStatConstraints.AbilitySpeed.x, GameStatConstraints.AbilitySpeed.y);
        }

        public float GetAbilitySizeModified(EntitySO entitySO, AbilitySO abilitySO)
        {
            var relativeData = GetRelativeEntityData(entitySO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.AbilitySize])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = abilitySO.SizeData.BaseSize * (1 + (upgrades * relativeData.ModValues.AbilitySize / 100));
            return ClampValue(modifiedValue, GameStatConstraints.AbilitySize.x, GameStatConstraints.AbilitySize.y);
        }

        public float GetGatherSpeedModified(PlayerSO playerSO)
        {
            var relativeData = GetRelativeEntityData(playerSO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.GatherSpeed])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = (1 - (upgrades * (relativeData.ModValues.GatherSpeed / 100))) * playerSO.BaseGatheringTime;
            return ClampValue(modifiedValue, GameStatConstraints.GatherSpeed.x, GameStatConstraints.GatherSpeed.y);
        }

        public float GetGatherDamageModified(PlayerSO playerSO)
        {
            var relativeData = GetRelativeEntityData(playerSO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.GatherDamage])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = playerSO.BaseGatheringDamage * (1 + (upgrades * relativeData.ModValues.GatherDamage / 100));
            return ClampValue(modifiedValue, GameStatConstraints.GatherDamage.x, GameStatConstraints.GatherDamage.y);
        }

        public float GetRepairSpeedModified(PlayerSO playerSO)
        {
            var relativeData = GetRelativeEntityData(playerSO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.RepairSpeed])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = (1 - (upgrades * (relativeData.ModValues.RepairSpeed / 100))) * playerSO.BaseRepairTime;
            return ClampValue(modifiedValue, GameStatConstraints.RepairTime.x, GameStatConstraints.RepairTime.y);
        }

        public float GetLightRadiusModified(PlayerSO playerSO)
        {
            var relativeData = GetRelativeEntityData(playerSO);

            int upgrades = 0;
            foreach (StatModifier statModifier in relativeData.Modifiers[StatModType.LightRadius])
            {
                upgrades += statModifier.Value;
            }

            float modifiedValue = playerSO.BaseLightRadius * (1 + (upgrades * relativeData.ModValues.LightRadius / 100));
            return ClampValue(modifiedValue, GameStatConstraints.LightRadius.x, GameStatConstraints.LightRadius.y);
        }


        private int ClampValue(int value, int min, int max)
        {
            return value < min ? min : (value > max ? max : value);
        }

        private float ClampValue(float value, float min, float max)
        {
            return value < min ? min : (value > max ? max : value);
        }
    }
}
