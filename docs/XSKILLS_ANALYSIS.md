# XSkills Mod - Stat Modification & Entity Extension Analysis

## Overview
The xskills mod demonstrates advanced patterns for:
1. Entity extension through EntityBehaviors
2. Stat modification through effect systems
3. Integration with Vintage Story's update cycles
4. Server-client synchronization
5. Buff/debuff/condition tracking

---

## 1. ENTITY EXTENSION PATTERNS

### 1.1 Base Entity Behavior Extension

The mod uses a hierarchy of EntityBehaviors to extend entity capabilities:

```csharp
// XSkillsEntityBehavior - Base for all non-player entities
public class XSkillsEntityBehavior : EntityBehavior
{
    protected Combat combat;
    protected Farming farming;
    protected Husbandry husbandry;
    protected Mining mining;
    // ... other skills
    
    public override string PropertyName() => "XSkillsEntity";
    
    public XSkillsEntityBehavior(Entity entity) : base(entity)
    {
        // Load skill definitions from XLeveling system
        this.combat = XLeveling.Instance(entity.Api)?.GetSkill("combat") as Combat;
        this.farming = XLeveling.Instance(entity.Api)?.GetSkill("farming") as Farming;
        // ...
    }
    
    public override void Initialize(EntityProperties properties, JsonObject attributes)
    {
        base.Initialize(properties, attributes);
        this.xp = attributes["xp"].AsFloat(0.0f);
        
        // Hook into entity's health damage events
        EntityBehaviorHealth behaviorHealth = (this.entity.GetBehavior("health") as EntityBehaviorHealth);
        if (behaviorHealth != null) behaviorHealth.onDamaged += OnDamage;
    }
}

// XSkillsPlayerBehavior - Player-specific extensions
public class XSkillsPlayerBehavior : EntityBehavior
{
    private float timeSinceUpdate;
    private uint lastWeatherForecast;
    internal float HoursSlept { get; set; }
    
    public override string PropertyName() => "XSkillsPlayer";
    protected EntityBehaviorTemporalStabilityAffected TemporalAffected => 
        this.entity.GetBehavior<EntityBehaviorTemporalStabilityAffected>();
    protected EntityBehaviorHealth Health => 
        this.entity.GetBehavior<EntityBehaviorHealth>();
}

// XSkillsAnimalBehavior - Animal-specific extensions
public class XSkillsAnimalBehavior : XSkillsEntityBehavior
{
    public IPlayer Feeder 
    { 
        get 
        {
            string uid = entity.WatchedAttributes.GetString("owner");
            if (uid == null) return null;
            return entity.Api.World.PlayerByUid(uid); 
        }
        set 
        {
            if (value != null) entity.WatchedAttributes.SetString("owner", value.PlayerUID);
            else entity.WatchedAttributes.RemoveAttribute("owner");
        }
    }
    public bool Catchable { get; set; }
    
    public override string PropertyName() => "XSkillsAnimal";
}
```

**Key Patterns:**
- Entities are extended via registering EntityBehavior classes
- Custom data stored in WatchedAttributes for synchronization
- Behaviors cache references to other behaviors (Health, TemporalStability, etc.)
- Skill definitions loaded from central XLeveling system at construction

### 1.2 AffectedEntityBehavior - Effect Container

The XLib system provides a reusable container for managing effects:

```csharp
public class AffectedEntityBehavior : EntityBehavior
{
    public override string PropertyName() => "Affected";
    public Dictionary<string, Effect> Effects { get; private set; }
    protected Dictionary<EnumTool, float> MiningSpeedModifiers { get; private set; }
    protected float effectTimer;
    protected float triggerTimer;
    protected bool dirty; // Marks if tree needs updating
    
    public AffectedEntityBehavior(Entity entity) : base(entity)
    {
        system = entity.Api.ModLoader.GetModSystem<XEffectsSystem>();
        this.Effects = new Dictionary<string, Effect>();
        
        // Only players get mining speed modifiers
        if (this.entity as EntityPlayer != null)
        {
            this.MiningSpeedModifiers = new Dictionary<EnumTool, float>();
            this.entity.WatchedAttributes.GetOrAddTreeAttribute("immunities");
            
            for (EnumTool tool = 0; tool <= EnumTool.Scythe; tool++)
            {
                this.MiningSpeedModifiers.Add(tool, 1.0f);
            }
        }
    }
    
    public override void Initialize(EntityProperties properties, JsonObject attributes)
    {
        // Client-side: Listen to effect changes
        if (this.entity.Api.Side == EnumAppSide.Client)
        {
            entity.WatchedAttributes.RegisterModifiedListener("effects", 
                this.CreateEffectsFromTree);
        }
    }
    
    public bool AddEffect(Effect effect)
    {
        if (effect == null) return false;
        if (IsImmune(effect.EffectType.Name)) return false;
        if (!entity.Alive && effect.ExpiresAtDeath) return false;
        
        effect.Behavior = this;
        
        // Handle effect group conflicts
        foreach (Effect other in Effects.Values)
        {
            if (other.EffectType.Name == effect.EffectType.Name)
            {
                other.OnRenewed(effect);
                return true;
            }
            
            if (other.EffectType.EffectGroup != null &&
                effect.EffectType.EffectGroup != null &&
                other.EffectType.EffectGroup == effect.EffectType.EffectGroup)
            {
                other.OnRemoved();
                Effects.Remove(other.EffectType.Name);
                break;
            }
        }
        
        this.Effects.Add(effect.EffectType.Name, effect);
        MarkDirty();
        effect.OnStart();
        return true;
    }
    
    public bool RemoveEffect(string name, bool allowDisplayNames = false)
    {
        if (this.Effects.TryGetValue(name, out Effect effect))
        {
            effect.OnRemoved();
            MarkDirty();
            return this.Effects.Remove(name);
        }
        return false;
    }
    
    public void MarkDirty() => this.dirty = true;
    
    public bool IsAffectedBy(string name) => this.Effects.ContainsKey(name);
    
    public void AddMiningSpeedMultiplier(EnumTool tool, float multiplier)
    {
        if (this.MiningSpeedModifiers.ContainsKey(tool))
        {
            this.MiningSpeedModifiers[tool] *= multiplier;
        }
    }
    
    public void SetImmunity(string name, float duration)
    {
        ITreeAttribute immunities = entity.WatchedAttributes.GetTreeAttribute("immunities");
        if (immunities == null) return;
        float old = immunities.GetFloat(name);
        immunities.SetFloat(name, Math.Max(duration, old));
    }
}
```

---

## 2. STAT MODIFICATION PATTERNS

### 2.1 Direct Stat Modification via Stats API

```csharp
// In XSkillsPlayerBehavior - Fast Forward ability
protected void ApplyAbilitiesStability()
{
    if (this.adaptation == null) return;
    if (entity?.Stats == null) return;
    
    EntityBehaviorTemporalStabilityAffected temporalAffected = TemporalAffected;
    PlayerSkill playeAdaptation = this.entity.GetBehavior<PlayerSkillSet>()?
        [this.adaptation.Id];
    if (temporalAffected == null || playeAdaptation == null) return;
    
    double stability = temporalAffected.OwnStability;
    double change = oldStability - stability;
    
    if (change != 0.0)
    {
        PlayerAbility playerAbility = playeAdaptation[this.adaptation.FastForwardId];
        float value = (float)(playerAbility.FValue(0) * (1.0f - temporalAffected.OwnStability));
        
        // KEY PATTERN: Stats.Set(statName, modifierId, value, false)
        entity.Stats.Set("hungerrate", "ability-ff", value, false);
        entity.Stats.Set("miningSpeedMul", "ability-ff", value, false);
    }
}

// In XSkillsPlayerBehavior - On The Road ability
public void ApplyMovementAbilities()
{
    if (this.survival == null) return;
    if (this.entity.World == null) return;
    
    PlayerAbility ability = entity.GetBehavior<PlayerSkillSet>()?
        [survival.Id]?[survival.OnTheRoadId];
    if (ability == null) return;
    
    if (ability.Tier > 0)
    {
        EntityPos pos = entity.SidedPos;
        if (pos == null) return;
        
        int y1 = (int)(pos.Y - 0.05f);
        int y2 = (int)(pos.Y + 0.01f);
        Block belowBlock = this.entity.World.BlockAccessor.GetBlock(
            new BlockPos((int)pos.X, y1, (int)pos.Z, pos.Dimension));
        Block insideBlock = this.entity.World.BlockAccessor.GetBlock(
            new BlockPos((int)pos.X, y2, (int)pos.Z, pos.Dimension));
        
        float multiplier = belowBlock.WalkSpeedMultiplier * 
            (y1 == y2 ? 1.0f : insideBlock.WalkSpeedMultiplier);
        if (multiplier <= 1.0f)
        {
            entity.Stats.Set("walkspeed", "ability-ontheroads", 0.0f, false);
        }
        else
        {
            entity.Stats.Set("walkspeed", "ability-ontheroads", 
                ability.FValue(0), false);
        }
    }
    else
    {
        entity.Stats.Remove("walkspeed", "ability-ontheroads");
    }
}

// In Survival skill - Nudist ability
public void ApplyNudistAbility(IPlayer player)
{
    EntityAgent entity = player.Entity as EntityAgent;
    if (entity?.Stats == null) return;
    
    InventoryCharacter inv = player.InventoryManager.GetOwnInventory("character") 
        as InventoryCharacter;
    PlayerAbility playerAbility = player.Entity.GetBehavior<PlayerSkillSet>()?
        [this.Id]?[this.NudistId];
    
    if (playerAbility?.Tier <= 0) return;
    
    int clothCounter = 0;
    for (int i = (int)EnumCharacterDressType.ArmorHead; i < inv.Count; i++)
    {
        if (inv[i].Itemstack != null) clothCounter++;
    }
    
    // Stat modification based on item count
    entity.Stats.Set("walkspeed", "ability-nudist", 
        playerAbility.FValue(0) - clothCounter * playerAbility.FValue(1), false);
    entity.Stats.Set("maxhealthExtraPoints", "ability-nudist", 
        playerAbility.Value(2) - clothCounter * playerAbility.Value(3), false);
    entity.Stats.Set("hungerrate", "ability-nudist", 
        -playerAbility.FValue(4) + clothCounter * playerAbility.FValue(5), false);
}
```

**Key Patterns:**
- `entity.Stats.Set(statName, modifierId, value, false)` - Apply stat modifier
- `entity.Stats.Remove(statName, modifierId)` - Remove stat modifier
- `entity.Stats.GetBlended(statName)` - Get final blended stat value
- Modifier IDs are namespaced (e.g., "ability-ontheroads", "effect-condition-name")
- Stats update is deferred (false parameter = don't immediate update)

### 2.2 Effect-Based Stat Modification

The StatEffect class automatically manages stat modifications:

```csharp
public class StatEffect : Effect
{
    public string StatName { get; protected set; }
    private string EffectStatId { get; set; }
    
    public StatEffect(EffectType effectType, float duration, string statName, 
        int maxStacks = 1, int stacks = 1, float intensity = 0.0f) :
        base(effectType, duration, maxStacks, stacks, intensity)
    {
        this.StatName = statName ?? "";
    }
    
    public override void OnStart()
    {
        base.OnStart();
        EntityFloatStats stats = null;
        try
        {
            stats = Entity.Stats[this.StatName];
        }
        catch(KeyNotFoundException exception) 
        {  
            this.Entity.Api.Logger.Error(exception);
        }
        
        if (stats == null) return;
        
        // Generate unique ID for this effect's modifier
        int cc = 0;
        if (EffectStatId == null)
        {
            do
            {
                EffectStatId = "effect-" + this.EffectType.Name + cc;
                cc++;
            } while (stats.ValuesByKey.ContainsKey(EffectStatId));
        }
        
        // Apply stat modifier
        Entity.Stats.Set(this.StatName, EffectStatId, ResultingIntensity(), false);
    }
    
    public override void OnEnd()
    {
        base.OnEnd();
        if (EffectStatId == null) return;
        Entity.Stats.Remove(this.StatName, EffectStatId);
    }
    
    public override void Update(float intensity, int stacks = 0)
    {
        base.Update(intensity, stacks);
        if (EffectStatId == null) return;
        Entity.Stats.Set(this.StatName, EffectStatId, ResultingIntensity(), false);
    }
}
```

### 2.3 Compound Effects via Condition

The Condition class aggregates multiple effects:

```csharp
public class Condition : Effect
{
    protected Dictionary<string, Effect> Effetcs { get; set; } = 
        new Dictionary<string, Effect>();
    public bool SynchronizedMaxStackSize { get; protected set; }
    public bool SynchronizedInterval { get; protected set; }
    
    public virtual void AddEffect(Effect effect, bool shouldStart)
    {
        if (effect == null) return;
        if (this.Effetcs.ContainsKey(effect.EffectType.Name)) return;
        
        this.Effetcs.Add(effect.EffectType.Name, effect);
        effect.Behavior = this.Behavior;
        
        if (this.SynchronizedMaxStackSize)
        {
            effect.MaxStacks = this.MaxStacks;
            effect.Stacks = this.Stacks;
        }
        if (this.SynchronizedInterval)
        {
            effect.Interval = this.Interval;
        }
        if (shouldStart) effect.OnStart();
    }
    
    public virtual void SetIntensity(string name, float intensity)
    {
        this.Effetcs.TryGetValue(name, out Effect effect);
        if (effect != null) effect.Update(intensity);
    }
}

// Usage in XSkillsPlayerBehavior - Adrenaline Rush ability
Condition effect = effectSystem?.CreateEffect("adrenalinerush") as Condition;
if (effect != null)
{
    effect.Duration = playerAbility.Value(3);
    effect.MaxStacks = 1;
    effect.Stacks = 1;
    
    // Set intensities of child effects
    effect.SetIntensity("walkspeed", playerAbility.FValue(1));
    effect.SetIntensity("receivedDamageMultiplier", 
        1.0f - playerAbility.FValue(2));
    
    TriggerEffect trigger = effect.Effect("trigger") as TriggerEffect;
    if (trigger != null)
    {
        trigger.EffectDuration = playerAbility.Value(4);
        trigger.EffectIntensity = -0.2f;
    }
    affected.AddEffect(effect);
    affected.MarkDirty();
}
```

---

## 3. INTEGRATION WITH UPDATE CYCLES

### 3.1 Game Tick Integration

```csharp
public override void OnGameTick(float deltaTime)
{
    if (this.entity == null) return;
    
    timeSinceUpdate += deltaTime;
    
    if(timeSinceUpdate >= 1.0f) // Update every 1 second
    {
        ApplyAbilitiesHealth();      // Health-related abilities
        ApplyAbilitiesStability();   // Temporal stability abilities
        ApplyAbilitiesOxygen();      // Underwater abilities
        ApplyMovementAbilities();    // Movement stat abilities
        timeSinceUpdate = 0.0f;
    }
    
    if (entity.Api.Side == EnumAppSide.Client)
    {
        // Client-only updates
        if (lastWeatherForecast < (uint)this.entity.World.Calendar.TotalDays)
        {
            PlayerAbility ability = entity.GetBehavior<PlayerSkillSet>()?
                [survival.Id]?[survival.MeteorologistId];
            if (ability?.Tier > 0)
            {
                Survival.GenerateWeatherForecast(entity.Api, entity.Pos, 
                    ability.Value(0), ability.FValue(1));
            }
            lastWeatherForecast = (uint)this.entity.World.Calendar.TotalDays;
        }
    }
}
```

### 3.2 Effect System Update Cycle

```csharp
public override void OnGameTick(float deltaTime)
{
    List<Effect> toRemove = new List<Effect>();
    effectTimer += deltaTime;
    triggerTimer += deltaTime;
    
    if (effectTimer >= system.Config.effectInterval) // Usually 0.1-0.2s
    {
        effectTimer = Math.Min(effectTimer, system.Config.effectInterval * 1.5f);
        
        foreach (Effect effect in this.Effects.Values)
        {
            if (effect.OnTick(effectTimer)) // Returns true if interval triggered
            {
                MarkDirty();
            }
            if (effect.ShouldExpire())
            {
                MarkDirty();
                toRemove.Add(effect);
            }
        }
        
        foreach (Effect effect in toRemove)
        {
            effect.OnExpires();
            this.Effects.Remove(effect.EffectType.Name);
        }
        
        // Update immunities
        ITreeAttribute immunities = entity.WatchedAttributes.GetTreeAttribute("immunities");
        if (immunities != null)
        {
            List<string> toRemove2 = new List<string>();
            foreach (KeyValuePair<string, IAttribute> pair in immunities)
            {
                float value = (float)pair.Value.GetValue() - effectTimer;
                if (value < 0.0f)
                {
                    toRemove2.Add(pair.Key);
                }
                else immunities.SetFloat(pair.Key, value);
            }
            foreach (string key in toRemove2)
            {
                immunities.RemoveAttribute(key);
            }
        }
        
        effectTimer = 0;
        if (this.dirty && this.entity.Api.Side == EnumAppSide.Server)
        {
            dirty = false;
            this.UpdateTree();
        }
    }
}
```

---

## 4. DAMAGE CALCULATION HOOKS

### 4.1 Damage Event Interception

```csharp
// Damage dealing (outgoing)
public virtual float OnDamage(float damage, DamageSource dmgSource)
{
    EntityPlayer byPlayer = /* extract attacking player */;
    if (this.combat == null || byPlayer == null) return damage;
    
    PlayerSkillSet playerSkillSet = byPlayer.GetBehavior<PlayerSkillSet>();
    PlayerSkill playerSkill = playerSkillSet?[this.combat.Id];
    if (playerSkill == null) return damage;
    
    // Example: Swordsman ability
    PlayerAbility playerAbility = playerSkill[combat.SwordsmanId];
    damage *= 1.0f + playerAbility.SkillDependentFValue();
    
    // Example: Vampire ability
    playerAbility = playerSkill[combat.VampireId];
    if (playerAbility.Tier > 0)
    {
        float health = damage * playerAbility.FValue(0);
        EntityBehaviorHealth playerHealth = 
            (byPlayer.GetBehavior("health") as EntityBehaviorHealth);
        if (playerHealth != null) 
            playerHealth.Health = Math.Min(playerHealth.Health + health, 
                playerHealth.MaxHealth);
    }
    
    return damage;
}

// Damage received (incoming) - Server only
public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
{
    if (system.Api.Side == EnumAppSide.Client) return;
    
    // General damage triggers
    List<EffectTrigger> triggers = system.Trigger["damage"];
    if (triggers != null)
    {
        foreach (EffectTrigger trigger in triggers)
        {
            if (trigger is not DamageTrigger damageTrigger) continue;
            if (damageTrigger.ShouldTrigger(damageSource, this.entity, damage))
            {
                Effect effect = trigger.ToTrigger.CreateEffect();
                effect.Update(trigger.ScaledIntensity(
                    damageTrigger.DamageIntensity * damage));
                this.AddEffect(effect);
                this.MarkDirty();
            }
        }
    }
}

// Damage multiplier effect
public class ReceivedDamageMultiplierEffect : Effect
{
    public override void OnStart()
    {
        base.OnStart();
        EntityBehaviorHealth health = this.Entity?.GetBehavior<EntityBehaviorHealth>();
        if (health == null) return;
        health.onDamaged += OnDamaged;
    }
    
    public override void OnEnd()
    {
        base.OnEnd();
        EntityBehaviorHealth health = this.Entity?.GetBehavior<EntityBehaviorHealth>();
        if (health == null) return;
        health.onDamaged -= OnDamaged;
    }
    
    public float OnDamaged(float damage, DamageSource dmgSource)
    {
        return damage * ResultingIntensity();
    }
}
```

### 4.2 Custom Damage Source Detection

```csharp
// In XSkillsEntityBehavior - Detecting weapon/tool types
private static CollectibleObject COProjectiles(DamageSource dmgSource)
{
    ItemStack stack = (dmgSource as IWeaponDamageSource)?.Weapon;
    if (stack != null) return stack.Collectible;
    ProjectileEntity projectile2 = dmgSource.SourceEntity as ProjectileEntity;
    if (projectile2 == null) return null;
    if (projectile2.WeaponStack != null) return projectile2.WeaponStack.Collectible;
    return projectile2.ProjectileStack?.Collectible;
}

public virtual float OnDamage(float damage, DamageSource dmgSource)
{
    EnumTool? tool = null; 
    if (dmgSource.SourceEntity != null)
    {
        EntityProjectile projectile = dmgSource.SourceEntity as EntityProjectile;
        CollectibleObject collectible = null;
        if (projectile != null) collectible = projectile.ProjectileStack?.Collectible;
        else if (dmgSource.SourceEntity.Class.Contains("Projectile") && 
                !dmgSource.SourceEntity.Class.Contains("Spell"))
        {
            try
            {
                collectible = COProjectiles(dmgSource);
            }
            catch (System.IO.FileNotFoundException) {}
        }
        
        if (collectible != null)
        {
            tool = collectible.Tool;
            if (tool == null)
            {
                if (collectible.Code.Path.Contains("arrow")) tool = EnumTool.Bow;
            }
        }
        else if (dmgSource.SourceEntity is EntityThrownStone) tool = EnumTool.Sling;
    }
    
    // Apply weapon-specific bonuses
    switch (tool)
    {
        case EnumTool.Sword:
        case EnumTool.Club:
            damage *= 1.0f + swordAbility.SkillDependentFValue();
            break;
        case EnumTool.Bow:
            damage *= 1.0f + archerAbility.SkillDependentFValue();
            break;
        // ... other weapon types
    }
    
    return damage;
}
```

---

## 5. CLIENT-SERVER SYNCHRONIZATION

### 5.1 WatchedAttributes for Automatic Synchronization

```csharp
// Setting watched attributes (server sends to clients automatically)
public IPlayer Feeder 
{ 
    set 
    {
        if (value != null) 
            entity.WatchedAttributes.SetString("owner", value.PlayerUID);
        else 
            entity.WatchedAttributes.RemoveAttribute("owner");
    }
}

// Modifying hunger (auto-synchronized)
public void ApplyAbilitiesMeatShield()
{
    ITreeAttribute hungerTree = this.entity.WatchedAttributes.GetTreeAttribute("hunger");
    PlayerAbility playerAbility = playerSkillSet[survival.Id]?[survival.MeatShieldId];
    if (playerAbility != null && hungerTree != null)
    {
        float saturation = hungerTree.GetFloat("currentsaturation");
        float saturationLoss = damage * playerAbility.Value(1);
        if (saturation > saturationLoss)
        {
            hungerTree.SetFloat("currentsaturation", saturation - saturationLoss);
            this.entity.WatchedAttributes.MarkPathDirty("hunger");
            damage *= 1.0f - playerAbility.FValue(0);
        }
    }
}
```

### 5.2 Effect Tree Synchronization

```csharp
public override void Initialize(EntityProperties properties, JsonObject attributes)
{
    if (this.entity.Api.Side == EnumAppSide.Client)
    {
        // Client: Listen to effect changes from server
        entity.WatchedAttributes.RegisterModifiedListener("effects", 
            this.CreateEffectsFromTree);
    }
}

public void UpdateTree()
{
    TreeAttribute effectTree = new TreeAttribute();
    foreach (string key in this.Effects.Keys)
    {
        Effect effect = this.Effects[key];
        effectTree.SetAttribute(key, effect.ToTree());
    }
    entity.WatchedAttributes.SetAttribute("effects", effectTree);
    entity.WatchedAttributes.MarkPathDirty("effects");
}

public void CreateEffectsFromTree(TreeAttribute effectTree)
{
    List<string> toRemove = new List<string>();
    foreach (KeyValuePair<string, Effect> pair in this.Effects) 
    {
        ITreeAttribute tree = effectTree.GetTreeAttribute(pair.Key);
        effectTree.RemoveAttribute(pair.Key);
        if (tree == null)
        {
            toRemove.Add(pair.Key);
        }
        else
        {
            pair.Value.FromTree(tree);
        }
    }
    foreach(string key in toRemove)
    {
        this.RemoveEffect(key);
    }
    
    foreach (string key in effectTree.Keys)
    {
        ITreeAttribute tree = effectTree.GetTreeAttribute(key);
        Effect effect = system.CreateEffect(key);
        if (effect == null) continue;
        effect.FromTree(tree);
        this.AddEffect(effect);
    }
}
```

### 5.3 Harmony Patches for Stat Modification Interception

```csharp
// Patch to modify weapon stats based on quality
[HarmonyPostfix]
[HarmonyPatch("FromItemStack")]
public static void FromItemStackPostfix(ref ItemStackMeleeWeaponStats __result, 
    ItemStack stack)
{
    float quality = stack?.Attributes.TryGetFloat("quality") ?? 0.0f;
    if (quality > 0.0f)
    {
        __result = new ItemStackMeleeWeaponStats(
            __result.DamageMultiplier * (1.0f + quality * 0.02f),
            __result.DamageBonus,
            __result.DamageTierBonus,
            __result.AttackSpeed,
            __result.BlockTierBonus,
            __result.ParryTierBonus,
            __result.ThrownDamageMultiplier * (1.0f + quality * 0.02f),
            __result.ThrownDamageTierBonus,
            __result.ThrownAimingDifficulty * (1.0f - quality * 0.01f),
            __result.ThrownProjectileSpeedMultiplier,
            __result.KnockbackMultiplier * (1.0f + quality * 0.01f),
            (int)(__result.ArmorPiercingBonus * (1.0f + quality * 0.01f))
        );
    }
}

// Transpiler patch to insert custom logic
[HarmonyTranspiler]
[HarmonyPatch("applyShieldProtection")]
public static IEnumerable<CodeInstruction> applyShieldProtectionTranspiler(
    IEnumerable<CodeInstruction> instructions)
{
    List<CodeInstruction> code = new List<CodeInstruction>(instructions);
    int begin = -1;
    for (int ii = 0; ii < code.Count; ++ii)
    {
        if (code[ii].opcode == OpCodes.Callvirt)
        {
            MethodInfo info = code[ii].operand as MethodInfo;
            if (info.Name == "NextDouble")
            {
                ii++;
                if (code[ii].opcode == OpCodes.Stloc_S)
                {
                    begin = ii + 1;
                    break;
                }
            }
        }
    }
    if (begin == -1) return code;
    
    MethodInfo method = typeof(ModSystemWearableStatsPatch).GetMethod(
        "ApplyShieldAbilities");
    List<CodeInstruction> newCode = new()
    {
        new CodeInstruction(OpCodes.Ldarg_1),       // player
        new CodeInstruction(OpCodes.Ldloca_S, 8),   // flatdmgabsorb
        new CodeInstruction(OpCodes.Ldloca_S, 9),   // chance
        new CodeInstruction(OpCodes.Ldloc_S, 7),    // usetype
        new CodeInstruction(OpCodes.Call, method)
    };
    code.InsertRange(begin, newCode);
    return code;
}
```

---

## 6. BUFF/DEBUFF TRACKING SYSTEM

### 6.1 Effect Framework

```csharp
[Flags]
public enum ExpireState : int
{
    Endless = 0x0000,           // Never expires
    Death = 0x0001,             // Expires on entity death
    Time = 0x0002,              // Expires after duration
    Intensity = 0x0004,         // Expires when intensity reaches 0
    Accumulates = 0x0008,       // Stacks intensity instead of averaging
}

public class Effect
{
    public EffectType EffectType { get; private set; }
    public float Duration { get; set; }
    public float Runtime { get; set; }
    public float Interval { get; set; }
    public float LastTriggered { get; protected set; }
    public int MaxStacks { get; set; }
    public int Stacks { get; set; }
    public float Intensity { get; protected set; }
    public ExpireState ExpireState { get; set; }
    public float ImmunityDuration { get; protected set; }
    public bool Running { get; protected set; }
    
    public bool ExpiresAtDeath { get; set; }
    public bool ExpiresOverTime { get; set; }
    public bool ExpiresThroughIntensity { get; set; }
    public bool Accumulates { get; set; }
    public float TimeLeft { get => Duration - Runtime; }
    
    // Virtual methods for customization
    public virtual void OnStart() => Running = true;
    public virtual void OnEnd() => Running = false;
    public virtual void OnInterval() { }
    public virtual void OnExpires() { if (ImmunityDuration > 0) Behavior?.SetImmunity(EffectType.Name, ImmunityDuration); OnEnd(); }
    public virtual void OnDeath() { if (ExpiresAtDeath) OnEnd(); }
    public virtual void OnRemoved() => OnEnd();
    public virtual void OnRenewed(Effect other) { /* Stack/merge logic */ }
    public virtual bool OnTick(float dt) { /* Handle intervals */ }
    public virtual bool ShouldExpire() { /* Check expiration conditions */ }
}
```

### 6.2 Condition - Compound Effects

```csharp
public class Condition : Effect
{
    protected Dictionary<string, Effect> Effetcs { get; set; } = 
        new Dictionary<string, Effect>();
    public bool SynchronizedMaxStackSize { get; protected set; }
    public bool SynchronizedInterval { get; protected set; }
    
    public virtual void AddEffect(Effect effect, bool shouldStart)
    {
        if (effect == null) return;
        if (this.Effetcs.ContainsKey(effect.EffectType.Name)) return;
        
        this.Effetcs.Add(effect.EffectType.Name, effect);
        effect.Behavior = this.Behavior;
        if (shouldStart) effect.OnStart();
    }
    
    public virtual void SetIntensity(string name, float intensity)
    {
        this.Effetcs.TryGetValue(name, out Effect effect);
        if (effect != null) effect.Update(intensity);
    }
    
    public virtual Effect Effect(string name)
    {
        this.Effetcs.TryGetValue(name, out Effect effect);
        return effect;
    }
}
```

### 6.3 Effect Usage Example: Bloodlust

```csharp
// In XSkillsEntityBehavior.OnEntityDeath
PlayerAbility playerAbility = playerSkill[this.combat.BloodlustId];
if (playerAbility.Tier > 0)
{
    AffectedEntityBehavior affected = byPlayer.GetBehavior<AffectedEntityBehavior>();
    if (affected == null ||
        affected.IsAffectedBy("adrenalineRush") ||
        affected.IsAffectedBy("exhaustion")) return;
    
    XEffectsSystem effectSystem = combat.XLeveling.Api.ModLoader
        .GetModSystem<XEffectsSystem>();
    Condition effect = effectSystem?.CreateEffect("bloodlust") as Condition;
    if (effect != null)
    {
        effect.Duration = playerAbility.Value(2);
        effect.MaxStacks = playerAbility.Value(3);
        effect.Stacks = 1;
        effect.SetIntensity("meleeWeaponsDamage", playerAbility.FValue(0));
        effect.SetIntensity("receivedDamageMultiplier", playerAbility.FValue(1));
        affected.AddEffect(effect);
        affected.MarkDirty();
    }
}
```

---

## 7. KEY ARCHITECTURAL PATTERNS FOR ADAPTATION

### Pattern 1: Two-Layer Behavior System
```
EntityBehavior (XSkillsEntityBehavior)
    └─ Manages skill definitions and damage calculation
       └─ Hooks into health events
       
AffectedEntityBehavior
    └─ Manages active effects/conditions
       └─ Handles stat application via Effects
       └─ Manages effect lifecycle and expiration
```

### Pattern 2: Deferred Stat Updates
```
1. Collect all modifications during tick/event
2. Apply to Stats API (entity.Stats.Set())
3. Defer updates (false parameter)
4. API automatically blends all modifications
```

### Pattern 3: Effect-Driven Stat Management
```
Buff/Debuff → Effect/Condition → SetIntensity()
    ↓
StatEffect hooks into OnStart/OnEnd
    ↓
StatEffect automatically manages Stats.Set/Remove
    ↓
Client synchronizes via WatchedAttributes
```

### Pattern 4: Hierarchical Synchronization
```
Server-side changes:
1. Modify WatchedAttributes
2. MarkPathDirty()
3. Automatically sent to clients
4. Client listeners react

Client-side listeners:
- RegisterModifiedListener("attributes_path", callback)
- Client recreates local state from server data
```

---

## ADAPTATION FOR PANTHEONWARS

For the PantheonWars buff system, consider:

1. **Create BuffEffect base class** inheriting from Effect
2. **Implement BuffBehavior** extending AffectedEntityBehavior
3. **Use Condition for compound buffs** (e.g., War Banner = damage + armor + morale)
4. **Apply stat modifications** via Stats.Set() during ability execution
5. **Use WatchedAttributes** for server-client synchronization
6. **Hook into OnGameTick()** for periodic buff checks
7. **Leverage Harmony patches** to intercept damage calculations
8. **Implement immunity system** for buff conflicts

