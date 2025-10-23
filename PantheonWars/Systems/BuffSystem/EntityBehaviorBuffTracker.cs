using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace PantheonWars.Systems.BuffSystem
{
    /// <summary>
    /// Entity behavior that tracks active buffs/debuffs and applies their stat modifications
    /// </summary>
    public class EntityBehaviorBuffTracker : EntityBehavior
    {
        private List<ActiveEffect> activeEffects = new List<ActiveEffect>();
        private float timeSinceLastUpdate = 0f;
        private const float UPDATE_INTERVAL = 0.5f; // Update every 0.5 seconds

        private Dictionary<string, float> lastAppliedModifiers = new Dictionary<string, float>();

        public override string PropertyName() => "PantheonWarsBuffTracker";

        public EntityBehaviorBuffTracker(Entity entity) : base(entity)
        {
        }

        public override void Initialize(EntityProperties properties, JsonObject attributes)
        {
            base.Initialize(properties, attributes);

            // Hook into damage events to apply damage modifiers
            EntityBehaviorHealth behaviorHealth = entity.GetBehavior<EntityBehaviorHealth>();
            if (behaviorHealth != null)
            {
                behaviorHealth.onDamaged += OnEntityDamaged;
            }

            // Load active effects from entity attributes if they exist
            LoadEffectsFromAttributes();
        }

        public override void OnGameTick(float deltaTime)
        {
            base.OnGameTick(deltaTime);

            timeSinceLastUpdate += deltaTime;

            if (timeSinceLastUpdate >= UPDATE_INTERVAL)
            {
                UpdateEffects(timeSinceLastUpdate);
                timeSinceLastUpdate = 0f;
            }
        }

        /// <summary>
        /// Add a new effect to this entity
        /// </summary>
        public void AddEffect(ActiveEffect effect)
        {
            if (effect == null) return;

            // Check if effect already exists - if so, refresh duration
            ActiveEffect existing = activeEffects.FirstOrDefault(e => e.EffectId == effect.EffectId);
            if (existing != null)
            {
                // Refresh duration and update modifiers
                existing.DurationRemaining = effect.DurationRemaining;
                existing.StatModifiers = effect.StatModifiers;
            }
            else
            {
                activeEffects.Add(effect);
            }

            // Apply stat modifiers immediately
            ApplyAllStatModifiers();

            // Persist effects
            SaveEffectsToAttributes();
        }

        /// <summary>
        /// Remove an effect by ID
        /// </summary>
        public bool RemoveEffect(string effectId)
        {
            ActiveEffect effect = activeEffects.FirstOrDefault(e => e.EffectId == effectId);
            if (effect != null)
            {
                activeEffects.Remove(effect);

                // Reapply all stat modifiers (this will remove the old ones)
                ApplyAllStatModifiers();

                // Persist changes
                SaveEffectsToAttributes();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all active effects on this entity
        /// </summary>
        public List<ActiveEffect> GetActiveEffects()
        {
            return new List<ActiveEffect>(activeEffects);
        }

        /// <summary>
        /// Check if entity has a specific effect active
        /// </summary>
        public bool HasEffect(string effectId)
        {
            return activeEffects.Any(e => e.EffectId == effectId);
        }

        /// <summary>
        /// Update all active effects (decrement duration, remove expired)
        /// </summary>
        private void UpdateEffects(float deltaTime)
        {
            if (activeEffects.Count == 0) return;

            List<ActiveEffect> expiredEffects = new List<ActiveEffect>();

            // Update durations
            foreach (var effect in activeEffects)
            {
                effect.UpdateDuration(deltaTime);

                if (effect.IsExpired())
                {
                    expiredEffects.Add(effect);
                }
            }

            // Remove expired effects
            if (expiredEffects.Count > 0)
            {
                foreach (var expired in expiredEffects)
                {
                    activeEffects.Remove(expired);

                    // Notify player
                    if (entity is EntityPlayer player && player.Player is IServerPlayer serverPlayer)
                    {
                        serverPlayer.SendMessage(
                            GlobalConstants.InfoLogChatGroup,
                            $"[{expired.EffectId}] effect has worn off.",
                            EnumChatType.Notification
                        );
                    }
                }

                // Reapply stat modifiers after removing expired effects
                ApplyAllStatModifiers();

                // Persist changes
                SaveEffectsToAttributes();
            }
        }

        /// <summary>
        /// Apply all active stat modifiers to entity stats
        /// </summary>
        private void ApplyAllStatModifiers()
        {
            if (entity.Stats == null) return;

            // First, remove all previously applied modifiers
            foreach (var kvp in lastAppliedModifiers)
            {
                string statName = kvp.Key;
                entity.Stats.Remove(statName, "pantheonwars-buff");
            }
            lastAppliedModifiers.Clear();

            // Now accumulate all active modifiers
            Dictionary<string, float> accumulatedModifiers = new Dictionary<string, float>();

            foreach (var effect in activeEffects)
            {
                foreach (var modifier in effect.StatModifiers)
                {
                    if (!accumulatedModifiers.ContainsKey(modifier.Key))
                    {
                        accumulatedModifiers[modifier.Key] = 0f;
                    }

                    // Stack modifiers additively
                    accumulatedModifiers[modifier.Key] += modifier.Value;
                }
            }

            // Apply accumulated modifiers
            foreach (var modifier in accumulatedModifiers)
            {
                entity.Stats.Set(modifier.Key, "pantheonwars-buff", modifier.Value, false);
                lastAppliedModifiers[modifier.Key] = modifier.Value;
            }
        }

        /// <summary>
        /// Modify incoming damage based on active effects
        /// </summary>
        private float OnEntityDamaged(float damage, DamageSource dmgSource)
        {
            if (activeEffects.Count == 0) return damage;

            float damageMultiplier = 1.0f;

            // Check for damage resistance buffs
            foreach (var effect in activeEffects)
            {
                if (effect.StatModifiers.ContainsKey("receivedDamageMultiplier"))
                {
                    // receivedDamageMultiplier is a multiplier (1.0 = normal, 0.5 = 50% reduction)
                    damageMultiplier *= effect.StatModifiers["receivedDamageMultiplier"];
                }
            }

            return damage * damageMultiplier;
        }

        /// <summary>
        /// Get the total damage multiplier this entity should deal (for buffs like War Banner)
        /// </summary>
        public float GetOutgoingDamageMultiplier()
        {
            float multiplier = 1.0f;

            foreach (var effect in activeEffects)
            {
                if (effect.StatModifiers.ContainsKey("meleeDamageMultiplier"))
                {
                    multiplier += effect.StatModifiers["meleeDamageMultiplier"];
                }

                if (effect.StatModifiers.ContainsKey("rangedDamageMultiplier"))
                {
                    multiplier += effect.StatModifiers["rangedDamageMultiplier"];
                }
            }

            return multiplier;
        }

        /// <summary>
        /// Check if entity is marked (for Hunter's Mark debuff)
        /// </summary>
        public float GetReceivedDamageMultiplier()
        {
            float multiplier = 1.0f;

            foreach (var effect in activeEffects)
            {
                if (effect.StatModifiers.ContainsKey("receivedDamageAmplification"))
                {
                    // This is for debuffs like Hunter's Mark that make target take MORE damage
                    multiplier += effect.StatModifiers["receivedDamageAmplification"];
                }
            }

            return multiplier;
        }

        /// <summary>
        /// Save active effects to entity attributes for persistence
        /// </summary>
        private void SaveEffectsToAttributes()
        {
            TreeAttribute effectsTree = new TreeAttribute();
            effectsTree.SetInt("count", activeEffects.Count);

            for (int i = 0; i < activeEffects.Count; i++)
            {
                effectsTree[$"effect_{i}"] = activeEffects[i].ToTreeAttribute();
            }

            entity.WatchedAttributes["pantheonwars_effects"] = effectsTree;
            entity.WatchedAttributes.MarkPathDirty("pantheonwars_effects");
        }

        /// <summary>
        /// Load active effects from entity attributes
        /// </summary>
        private void LoadEffectsFromAttributes()
        {
            ITreeAttribute effectsTree = entity.WatchedAttributes.GetTreeAttribute("pantheonwars_effects");
            if (effectsTree == null) return;

            int count = effectsTree.GetInt("count");
            activeEffects.Clear();

            for (int i = 0; i < count; i++)
            {
                ITreeAttribute effectTree = effectsTree.GetTreeAttribute($"effect_{i}");
                if (effectTree != null)
                {
                    ActiveEffect effect = ActiveEffect.FromTreeAttribute(effectTree);
                    if (effect != null)
                    {
                        activeEffects.Add(effect);
                    }
                }
            }

            // Apply loaded effects
            if (activeEffects.Count > 0)
            {
                ApplyAllStatModifiers();
            }
        }
    }
}
