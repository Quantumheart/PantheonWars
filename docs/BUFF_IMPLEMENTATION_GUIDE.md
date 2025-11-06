# PantheonWars Buff System Implementation Guide

Based on XSkills Analysis - Detailed Architectural Patterns

## Executive Summary

XSkills demonstrates a production-ready pattern for managing buffs/debuffs through:
1. **Effect-based system** with condition aggregation
2. **EntityBehavior extensions** for stat management
3. **Automatic synchronization** via WatchedAttributes
4. **Event-driven updates** integrated into game ticks

---

## RECOMMENDED ARCHITECTURE FOR PANTHEONWARS

### Layer 1: Buff Definition & Registry

```csharp
// BuffType - Define available buffs
public class BuffType
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public float Duration { get; set; }
    public int MaxStacks { get; set; }
    public bool AccumulateStacks { get; set; }
    public string BuffGroup { get; set; } // For exclusivity
    public Dictionary<string, float> StatModifiers { get; set; }
    public string IconCode { get; set; }
}

// BuffRegistry - Singleton registry
public class BuffRegistry
{
    private Dictionary<string, BuffType> buffs = new();
    
    public void Register(BuffType buff) => buffs[buff.Id] = buff;
    public BuffType Get(string id) => buffs.TryGetValue(id, out var b) ? b : null;
}
```

### Layer 2: Buff Entity Behavior

```csharp
// PantheonBuffBehavior - Attaches to entities with buffs
public class PantheonBuffBehavior : EntityBehavior
{
    public override string PropertyName() => "PantheonBuffs";
    
    private Dictionary<string, PantheonBuff> activeBuffs = 
        new Dictionary<string, PantheonBuff>();
    private float updateTimer = 0f;
    private BuffRegistry buffRegistry;
    
    public PantheonBuffBehavior(Entity entity) : base(entity)
    {
        buffRegistry = entity.Api.ModLoader.GetModSystem<PantheonWarsModSystem>()
            ?.BuffRegistry;
    }
    
    public override void Initialize(EntityProperties properties, JsonObject attributes)
    {
        base.Initialize(properties, attributes);
        
        if (this.entity.Api.Side == EnumAppSide.Client)
        {
            entity.WatchedAttributes.RegisterModifiedListener("pantheonBuffs", 
                this.SyncBuffsFromTree);
        }
    }
    
    /// <summary>
    /// Add a buff to this entity
    /// </summary>
    public bool ApplyBuff(string buffTypeId, float intensity = 1.0f, 
        EntityAgent sourceEntity = null)
    {
        BuffType buffType = buffRegistry?.Get(buffTypeId);
        if (buffType == null) return false;
        
        EntityPlayer player = entity as EntityPlayer;
        if (player?.Entity == null) return false;
        
        PantheonBuff buff = new PantheonBuff
        {
            TypeId = buffTypeId,
            Duration = buffType.Duration,
            MaxStacks = buffType.MaxStacks,
            Stacks = 1,
            Intensity = intensity,
            Runtime = 0f,
            StartTime = entity.World.Calendar.ElapsedMilliseconds
        };
        
        // Check for buff group conflicts
        if (!string.IsNullOrEmpty(buffType.BuffGroup))
        {
            foreach (var existingBuff in activeBuffs.Values)
            {
                BuffType existingType = buffRegistry.Get(existingBuff.TypeId);
                if (existingType?.BuffGroup == buffType.BuffGroup)
                {
                    // Replace conflicting buff
                    RemoveBuff(existingBuff.TypeId);
                    break;
                }
            }
        }
        
        // Handle existing buff
        if (activeBuffs.TryGetValue(buffTypeId, out var existing))
        {
            if (buffType.AccumulateStacks)
            {
                existing.Intensity += intensity;
            }
            else
            {
                existing.Intensity = (existing.Intensity + intensity) / 2f;
            }
            existing.Stacks = Math.Min(existing.Stacks + 1, buffType.MaxStacks);
            existing.Runtime = 0f; // Refresh duration
        }
        else
        {
            activeBuffs[buffTypeId] = buff;
        }
        
        ApplyBuffStatModifications(buffTypeId, intensity);
        MarkDirty();
        return true;
    }
    
    /// <summary>
    /// Remove a buff
    /// </summary>
    public bool RemoveBuff(string buffTypeId)
    {
        if (activeBuffs.TryGetValue(buffTypeId, out var buff))
        {
            RemoveBuffStatModifications(buffTypeId);
            activeBuffs.Remove(buffTypeId);
            MarkDirty();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Check if entity is affected by a buff
    /// </summary>
    public bool HasBuff(string buffTypeId) => activeBuffs.ContainsKey(buffTypeId);
    
    /// <summary>
    /// Get current buff intensity
    /// </summary>
    public float GetBuffIntensity(string buffTypeId)
    {
        if (activeBuffs.TryGetValue(buffTypeId, out var buff))
            return buff.Intensity * buff.Stacks;
        return 0f;
    }
    
    // Stat modification
    private void ApplyBuffStatModifications(string buffTypeId, float intensity)
    {
        BuffType buffType = buffRegistry?.Get(buffTypeId);
        if (buffType?.StatModifiers == null) return;
        
        EntityAgent agent = entity as EntityAgent;
        if (agent?.Stats == null) return;
        
        foreach (var stat in buffType.StatModifiers)
        {
            string modifierId = $"buff-{buffTypeId}";
            float value = stat.Value * intensity;
            agent.Stats.Set(stat.Key, modifierId, value, false);
        }
    }
    
    private void RemoveBuffStatModifications(string buffTypeId)
    {
        BuffType buffType = buffRegistry?.Get(buffTypeId);
        if (buffType?.StatModifiers == null) return;
        
        EntityAgent agent = entity as EntityAgent;
        if (agent?.Stats == null) return;
        
        foreach (var stat in buffType.StatModifiers)
        {
            string modifierId = $"buff-{buffTypeId}";
            agent.Stats.Remove(stat.Key, modifierId);
        }
    }
    
    // Update cycle
    public override void OnGameTick(float deltaTime)
    {
        updateTimer += deltaTime;
        if (updateTimer < 0.1f) return; // Update every 100ms
        
        updateTimer = 0f;
        
        List<string> toRemove = new();
        foreach (var buffEntry in activeBuffs)
        {
            buffEntry.Value.Runtime += 0.1f;
            if (buffEntry.Value.Runtime >= buffEntry.Value.Duration)
            {
                toRemove.Add(buffEntry.Key);
            }
        }
        
        foreach (var buffId in toRemove)
        {
            RemoveBuff(buffId);
        }
        
        if (toRemove.Count > 0 && entity.Api.Side == EnumAppSide.Server)
        {
            SyncBuffsToTree();
        }
    }
    
    // Synchronization
    private void SyncBuffsToTree()
    {
        TreeAttribute buffTree = new TreeAttribute();
        foreach (var buff in activeBuffs.Values)
        {
            ITreeAttribute buffData = buff.ToTree();
            buffTree.SetAttribute(buff.TypeId, buffData);
        }
        entity.WatchedAttributes.SetAttribute("pantheonBuffs", buffTree);
        entity.WatchedAttributes.MarkPathDirty("pantheonBuffs");
    }
    
    private void SyncBuffsFromTree()
    {
        TreeAttribute buffTree = entity.WatchedAttributes
            .GetTreeAttribute("pantheonBuffs") as TreeAttribute;
        if (buffTree == null)
        {
            activeBuffs.Clear();
            return;
        }
        
        activeBuffs.Clear();
        foreach (var key in buffTree.Keys)
        {
            var buffData = buffTree.GetTreeAttribute(key);
            var buff = new PantheonBuff();
            buff.FromTree(buffData);
            activeBuffs[key] = buff;
        }
    }
    
    private void MarkDirty()
    {
        if (entity.Api.Side == EnumAppSide.Server)
            SyncBuffsToTree();
    }
}

// PantheonBuff - Individual buff instance
public class PantheonBuff
{
    public string TypeId { get; set; }
    public float Duration { get; set; }
    public float Runtime { get; set; }
    public int MaxStacks { get; set; }
    public int Stacks { get; set; }
    public float Intensity { get; set; }
    public long StartTime { get; set; }
    
    public ITreeAttribute ToTree()
    {
        TreeAttribute tree = new();
        tree.SetString("typeId", TypeId);
        tree.SetFloat("duration", Duration);
        tree.SetFloat("runtime", Runtime);
        tree.SetInt("maxStacks", MaxStacks);
        tree.SetInt("stacks", Stacks);
        tree.SetFloat("intensity", Intensity);
        tree.SetLong("startTime", StartTime);
        return tree;
    }
    
    public void FromTree(ITreeAttribute tree)
    {
        TypeId = tree.GetString("typeId", "");
        Duration = tree.GetFloat("duration", 0);
        Runtime = tree.GetFloat("runtime", 0);
        MaxStacks = tree.GetInt("maxStacks", 1);
        Stacks = tree.GetInt("stacks", 1);
        Intensity = tree.GetFloat("intensity", 1.0f);
        StartTime = tree.GetLong("startTime", 0);
    }
}
```

### Layer 3: Stat Integration

```csharp
// Example: War Banner ability applies multiple stat buffs
public class WarBannerBuff
{
    public static void Apply(IServerPlayer player)
    {
        PantheonBuffBehavior buffBehavior = player.Entity
            .GetBehavior<PantheonBuffBehavior>();
        if (buffBehavior == null) return;
        
        // Define buff stats
        BuffType warBannerBuff = new BuffType
        {
            Id = "warbanner",
            DisplayName = "War Banner",
            Duration = 30f, // 30 seconds
            MaxStacks = 3,
            AccumulateStacks = false,
            BuffGroup = "groupbuff", // Conflicts with other group buffs
            StatModifiers = new Dictionary<string, float>
            {
                { "damage", 0.25f },        // +25% damage
                { "meleeWeaponArmor", 0.15f }, // +15% armor
                { "receivedDamageMultiplier", 0.85f } // -15% damage taken
            },
            IconCode = "pantheonwars:warbanner"
        };
        
        buffBehavior.ApplyBuff("warbanner", intensity: 1.0f);
    }
}
```

### Layer 4: Damage Calculation Integration

```csharp
// Hook into damage events
[HarmonyPostfix]
[HarmonyPatch(typeof(EntityBehaviorHealth), "ReceiveDamage")]
public static void ReceiveDamagePostfix(EntityBehaviorHealth __instance, 
    ref float damageAmount, DamageSource dmgSource)
{
    Entity entity = __instance.entity;
    PantheonBuffBehavior buffBehavior = entity.GetBehavior<PantheonBuffBehavior>();
    if (buffBehavior == null) return;
    
    // Check for damage reduction buffs
    float armorBuff = buffBehavior.GetBuffIntensity("shieldbuff");
    if (armorBuff > 0)
    {
        damageAmount *= (1f - armorBuff);
    }
}
```

---

## IMPLEMENTATION CHECKLIST

### Phase 1: Core Infrastructure
- [ ] Create PantheonBuffBehavior EntityBehavior
- [ ] Create BuffType and BuffRegistry
- [ ] Implement buff apply/remove logic
- [ ] Implement stat modification via entity.Stats.Set/Remove
- [ ] Register PantheonBuffBehavior globally

### Phase 2: Integration
- [ ] Hook into OnGameTick for buff expiration
- [ ] Implement WatchedAttributes synchronization
- [ ] Test server-client buff sync
- [ ] Add buff immunity tracking

### Phase 3: Ability Integration
- [ ] Update Ability classes to apply buffs
- [ ] Create specialized buff classes (ConditionBuff, DurationBuff)
- [ ] Implement buff group exclusivity
- [ ] Add stack handling

### Phase 4: UI & Polish
- [ ] Display active buffs in HUD
- [ ] Add buff tooltips with stat changes
- [ ] Add buff duration display
- [ ] Add buff icons

---

## KEY DIFFERENCES FROM XSKILLS

| Feature | XSkills | Recommended for PW |
|---------|---------|-------------------|
| **Base Type** | Effect/Condition | Buff/BuffType |
| **Behavior** | AffectedEntityBehavior | PantheonBuffBehavior |
| **Effect Groups** | EffectGroup field | BuffGroup field |
| **Stat Mod** | StatEffect subclass | Direct Stats.Set() in behavior |
| **Synchronization** | WatchedAttributes "effects" | WatchedAttributes "pantheonBuffs" |
| **Expiration** | Multiple ExpireState flags | Duration-based only |
| **Stacking** | Accumulates flag | AccumulateStacks boolean |

---

## CRITICAL IMPLEMENTATION NOTES

### 1. Server-Only Updates
Always check entity.Api.Side:
```csharp
if (entity.Api.Side == EnumAppSide.Server)
{
    // Apply stats, remove buffs, etc.
}
```

### 2. Deferred Stat Updates
Always use false for deferred update:
```csharp
entity.Stats.Set("damage", "buff-warbanner", 0.25f, false);
// Don't use: entity.Stats.Set(..., true);
```

### 3. Modifier ID Convention
Use namespaced IDs to avoid conflicts:
```csharp
string modifierId = $"buff-{buffTypeId}"; // "buff-warbanner"
entity.Stats.Set(statName, modifierId, value, false);
```

### 4. Update Frequency
Respect game tick rates:
```csharp
// Bad: Updates every tick
public override void OnGameTick(float deltaTime)
{
    ApplyBuffs(); // Too frequent
}

// Good: Updates every 100ms
updateTimer += deltaTime;
if (updateTimer >= 0.1f)
{
    ApplyBuffs();
    updateTimer = 0f;
}
```

### 5. WatchedAttributes Listeners
Register ONLY on client side:
```csharp
if (this.entity.Api.Side == EnumAppSide.Client)
{
    entity.WatchedAttributes.RegisterModifiedListener("pantheonBuffs", 
        this.SyncBuffsFromTree);
}
```

---

## FILE REFERENCES

Key XSkills files referenced:

1. **/xskills/src/EntityBehaviors/XSkillsPlayerBehavior.cs**
   - OnGameTick pattern
   - Stat.Set() usage
   - WatchedAttributes usage

2. **/xlib/src/xeffects/AffectedEntityBehavior.cs**
   - Effect lifecycle management
   - Synchronization pattern
   - Dirty marking system

3. **/xlib/src/xeffects/Effect/Effect.cs**
   - Expiration logic
   - Stacking behavior
   - Immunity handling

4. **/xlib/src/xeffects/Effect/StatEffect.cs**
   - Stat modification pattern
   - OnStart/OnEnd hooks

5. **/xskills/src/Patches/Combat/ModSystemWearableStatsPatch.cs**
   - Harmony patch example
   - Transpiler usage for stat modification

