# PantheonWars Religion System - Comprehensive Overview

## 1. Data Models

### ReligionData (Persistent Data Structure)
**File**: `/home/user/PantheonWars/PantheonWars/Data/ReligionData.cs`

#### Key Properties:
- **ReligionUID** (string): Unique identifier (GUID)
- **ReligionName** (string): Display name
- **Deity** (DeityType): Associated deity (Khoras, Lysa, Morthen, Aethra, Umbros, Tharos, Gaia, Vex)
- **FounderUID** (string): Player UID of the founder
- **MemberUIDs** (List<string>): Ordered list of member player UIDs (founder always first)
- **IsPublic** (bool): Whether anyone can join or if invite-only
- **Description** (string): Religion manifesto/description
- **Prestige** (int): Current prestige points
- **TotalPrestige** (int): Lifetime prestige (for ranking)
- **PrestigeRank** (PrestigeRank): Current rank (Fledgling, Established, Renowned, Legendary, Mythic)
- **UnlockedBlessings** (Dictionary<string, bool>): Unlocked blessings for the religion
- **CreationDate** (DateTime): When the religion was created

#### Key Methods:
- `AddMember(string playerUID)` - Adds a member to the religion
- `RemoveMember(string playerUID)` - Removes a member
- `IsMember(string playerUID)` - Checks membership
- `IsFounder(string playerUID)` - Checks if player is founder
- `GetMemberCount()` - Gets total members
- `UpdatePrestigeRank()` - Updates rank based on total prestige
- `AddPrestige(int amount)` - Adds prestige and updates rank
- `UnlockBlessing(string blessingId)` - Unlocks a blessing
- `IsBlessingUnlocked(string blessingId)` - Checks if blessing is unlocked

### PlayerReligionData (Persistent Player Data)
**File**: `/home/user/PantheonWars/PantheonWars/Data/PlayerReligionData.cs`

#### Key Properties:
- **PlayerUID** (string): Player's unique identifier
- **ReligionUID** (string?): UID of current religion (null if not in one)
- **ActiveDeity** (DeityType): Cached from current religion
- **Favor** (int): Current favor points (resets on religion switch)
- **TotalFavorEarned** (int): Lifetime favor (persists across switches)
- **FavorRank** (FavorRank): Rank of player (Initiate, Disciple, Zealot, Champion, Avatar)
- **UnlockedBlessings** (Dictionary<string, bool>): Player's unlocked blessings
- **LastReligionSwitch** (DateTime?): Last time player switched religions
- **KillCount** (int): Number of players killed in PvP
- **AccumulatedFractionalFavor** (float): Fractional favor for passive generation

#### Key Methods:
- `HasReligion()` - Checks if player has a religion
- `UpdateFavorRank()` - Updates rank based on total favor
- `AddFavor(int amount)` - Adds favor
- `AddFractionalFavor(float amount)` - Adds fractional favor (passive)
- `RemoveFavor(int amount)` - Removes favor (returns success)
- `ApplySwitchPenalty()` - Resets favor and blessings on switch

---

## 2. Management Systems

### ReligionManager (Business Logic)
**File**: `/home/user/PantheonWars/PantheonWars/Systems/ReligionManager.cs`
**Interface**: `/home/user/PantheonWars/PantheonWars/Systems/Interfaces/IReligionManager.cs`

#### Core Responsibilities:
- Manages all religion instances
- Handles member addition/removal
- Manages invitations
- Persists religion data to world storage

#### Key Methods:
```csharp
ReligionData CreateReligion(string name, DeityType deity, string founderUID, bool isPublic)
void AddMember(string religionUID, string playerUID)
void RemoveMember(string religionUID, string playerUID)
ReligionData? GetPlayerReligion(string playerUID)
ReligionData? GetReligion(string religionUID)
ReligionData? GetReligionByName(string name)
bool CanJoinReligion(string religionUID, string playerUID)
void InvitePlayer(string religionUID, string playerUID, string inviterUID)
bool HasInvitation(string playerUID, string religionUID)
void RemoveInvitation(string playerUID, string religionUID)
bool DeleteReligion(string religionUID, string requesterUID)
List<ReligionData> GetAllReligions()
List<ReligionData> GetReligionsByDeity(DeityType deity)
```

#### Important Behaviors:
- Private religions require invitations to join
- When founder leaves/is removed, leadership transfers to next member
- Religion is deleted if it has no members remaining
- Member invitations are tracked in `Dictionary<string, List<string>>` (_invitations)

### PlayerReligionDataManager (Player Progression)
**File**: `/home/user/PantheonWars/PantheonWars/Systems/PlayerReligionDataManager.cs`
**Interface**: `/home/user/PantheonWars/PantheonWars/Systems/Interfaces/IPlayerReligionDataManager.cs`

#### Core Responsibilities:
- Manages player religion relationships
- Handles player progression (favor/blessings)
- Enforces religion switch cooldown (7 days)
- Persists player data to world storage

#### Key Methods:
```csharp
PlayerReligionData GetOrCreatePlayerData(string playerUID)
void JoinReligion(string playerUID, string religionUID)
void LeaveReligion(string playerUID)
void AddFavor(string playerUID, int amount, string reason = "")
void AddFractionalFavor(string playerUID, float amount, string reason = "")
bool RemoveFavor(string playerUID, int amount, string reason = "")
void UpdateFavorRank(string playerUID)
bool UnlockPlayerBlessing(string playerUID, string blessingId)
bool CanSwitchReligion(string playerUID)
TimeSpan? GetSwitchCooldownRemaining(string playerUID)
void HandleReligionSwitch(string playerUID)
```

#### Important Behaviors:
- **Religion Switch Cooldown**: 7 days between switches
- **Switch Penalty**: Resets current favor to 0, resets FavorRank to Initiate, clears unlocked blessings
- **Total Favor Persistence**: TotalFavorEarned persists across switches
- **Fractional Favor**: For passive favor generation, converts to integer when >= 1.0

---

## 3. Commands and Network

### ReligionCommands (Chat Commands)
**File**: `/home/user/PantheonWars/PantheonWars/Commands/ReligionCommands.cs`

#### Available Commands:
1. `/religion create <name> <deity> [public/private]` - Create a new religion
2. `/religion join <name>` - Join a religion
3. `/religion leave` - Leave current religion
4. `/religion list [deity]` - List all religions
5. `/religion info [name]` - Show religion information
6. `/religion members` - Show members of your religion
7. `/religion invite <playername>` - Invite a player (founder only)
8. **`/religion kick <playername>`** - **Kick a player from religion (founder only)**
9. `/religion disband` - Disband religion (founder only)
10. `/religion description <text>` - Set religion description (founder only)

### Kick Command Implementation
**File**: `/home/user/PantheonWars/PantheonWars/Commands/ReligionCommands.cs` (lines 349-396)

```csharp
private TextCommandResult OnKickPlayer(TextCommandCallingArgs args)
{
    var targetPlayerName = (string)args[0];
    var player = args.Caller.Player as IServerPlayer;
    
    // Check if player is in a religion
    var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
    if (!playerData.HasReligion()) 
        return TextCommandResult.Error("You are not in any religion");
    
    var religion = _religionManager.GetReligion(playerData.ReligionUID!);
    
    // Check if player is founder
    if (!religion.IsFounder(player.PlayerUID)) 
        return TextCommandResult.Error("Only the founder can kick members");
    
    // Find target player by name
    var targetPlayer = _sapi.World.AllPlayers
        .FirstOrDefault(p => p.PlayerName.Equals(targetPlayerName, StringComparison.OrdinalIgnoreCase));
    
    // Check if target is a member
    if (!religion.IsMember(targetPlayer.PlayerUID))
        return TextCommandResult.Error($"{targetPlayerName} is not a member");
    
    // Cannot kick yourself
    if (targetPlayer.PlayerUID == player.PlayerUID)
        return TextCommandResult.Error("You cannot kick yourself");
    
    // Kick the player
    _playerReligionDataManager.LeaveReligion(targetPlayer.PlayerUID);
    
    // Notify target if online
    var targetServerPlayer = targetPlayer as IServerPlayer;
    if (targetServerPlayer != null)
        targetServerPlayer.SendMessage(
            GlobalConstants.GeneralChatGroup,
            $"You have been removed from {religion.ReligionName}",
            EnumChatType.Notification
        );
    
    return TextCommandResult.Success($"{targetPlayerName} has been removed from {religion.ReligionName}");
}
```

### Network Packets

#### ReligionActionRequestPacket
**File**: `/home/user/PantheonWars/PantheonWars/Network/ReligionActionRequestPacket.cs`

```csharp
public class ReligionActionRequestPacket
{
    public string Action { get; set; } = string.Empty; // "join", "leave", "kick", "invite", "disband"
    public string ReligionUID { get; set; } = string.Empty;
    public string TargetPlayerUID { get; set; } = string.Empty; // For kick/invite actions
}
```

#### ReligionActionResponsePacket
**File**: `/home/user/PantheonWars/PantheonWars/Network/ReligionActionResponsePacket.cs`

```csharp
public class ReligionActionResponsePacket
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
}
```

---

## 4. Server-Side Kick Handling

**File**: `/home/user/PantheonWars/PantheonWars/PantheonWarsSystem.cs` (lines 338-376)

### Kick Processing Flow:

1. **Founder Check**: Only religion founder can kick
2. **Target Validation**: 
   - Founder cannot kick themselves
   - Target must be a member of the religion
3. **Action**: Call `_playerReligionDataManager.LeaveReligion(targetUID)`
4. **Notification**:
   - Sender receives: "Kicked player from religion."
   - Kicked player receives chat notification if online
   - Kicked player's HUD is refreshed
   - Kicked player receives ReligionStateChangedPacket

### Code:
```csharp
case "kick":
    var religionForKick = _religionManager!.GetPlayerReligion(fromPlayer.PlayerUID);
    if (religionForKick != null && religionForKick.FounderUID == fromPlayer.PlayerUID)
    {
        if (packet.TargetPlayerUID != fromPlayer.PlayerUID)
        {
            _playerReligionDataManager!.LeaveReligion(packet.TargetPlayerUID);
            message = "Kicked player from religion.";
            success = true;
            
            // Notify kicked player if online
            var kickedPlayer = _sapi!.World.PlayerByUid(packet.TargetPlayerUID) as IServerPlayer;
            if (kickedPlayer != null)
            {
                kickedPlayer.SendMessage(0,
                    $"You have been kicked from {religionForKick.ReligionName}.",
                    EnumChatType.Notification);
                SendPlayerDataToClient(kickedPlayer);
                
                // Send religion state changed packet
                var statePacket = new ReligionStateChangedPacket
                {
                    Reason = $"You have been kicked from {religionForKick.ReligionName}",
                    HasReligion = false
                };
                _serverChannel!.SendPacket(statePacket, kickedPlayer);
            }
        }
        else
        {
            message = "You cannot kick yourself.";
        }
    }
    else
    {
        message = "Only the founder can kick members.";
    }
    break;
```

---

## 5. GUI/UI

### ReligionManagementDialog
**File**: `/home/user/PantheonWars/PantheonWars/GUI/ReligionManagementDialog.cs`

#### Tabs:
1. **Browse Religions** - Browse and join religions, filter by deity
2. **My Religion** - View your current religion details and manage members (if founder)

#### Founder Controls:
- Invite Player button
- Edit Description button
- Disband button
- **Kick button** (for each non-founder member)

#### Member List Display:
- Player name (with [Founder] tag if applicable)
- Favor rank and current favor
- Kick button (only for founder viewing, only for non-founder members)

### MemberListRenderer
**File**: `/home/user/PantheonWars/PantheonWars/GUI/UI/Renderers/Components/MemberListRenderer.cs`

#### Kick Button Logic:
```csharp
// Kick button (only if not founder and not self)
if (!member.IsFounder && member.PlayerUID != currentPlayerUID)
{
    var kickButtonX = x + width - kickButtonWidth - padding;
    var kickButtonY = y + (height - 22f) / 2;
    if (ButtonRenderer.DrawSmallButton(drawList, "Kick", kickButtonX, kickButtonY, kickButtonWidth, 22f))
    {
        api.World.PlaySoundAt(new AssetLocation("pantheonwars:sounds/click"),
            api.World.Player.Entity, null, false, 8f, 0.5f);
        onKickMember.Invoke(member.PlayerUID);
    }
}
```

---

## 6. Permission System

### Current Permission Model:

#### Founder-Only Actions:
- ✅ Kick members
- ✅ Invite players
- ✅ Disband religion
- ✅ Edit description

#### All Member Actions:
- ✅ Leave religion
- ✅ View religion information
- ✅ View member list

### Restrictions:
- Cannot join if already in a religion (must leave first)
- Cannot join private religions without invitation
- Cannot kick yourself
- Only founder can disband
- 7-day cooldown on religion switches
- Switching religions resets favor/blessings (switch penalty)

---

## 7. Rank Systems

### Prestige Ranks (Religion-Wide)
- **Fledgling**: 0-499 total prestige
- **Established**: 500-1999
- **Renowned**: 2000-4999
- **Legendary**: 5000-9999
- **Mythic**: 10000+

### Favor Ranks (Individual Player)
- **Initiate**: 0-499 total favor earned
- **Disciple**: 500-1999
- **Zealot**: 2000-4999
- **Champion**: 5000-9999
- **Avatar**: 10000+

---

## 8. Key Files Reference

| File | Purpose |
|------|---------|
| `ReligionData.cs` | Religion persistent data model |
| `PlayerReligionData.cs` | Player religion progression data |
| `ReligionManager.cs` | Religion management business logic |
| `PlayerReligionDataManager.cs` | Player progression & religion relationships |
| `ReligionCommands.cs` | Chat command handlers |
| `PantheonWarsSystem.cs` | Server-side request handling & networking |
| `ReligionManagementDialog.cs` | GUI for religion management |
| `MemberListRenderer.cs` | Renders member list with kick functionality |
| `ReligionActionRequestPacket.cs` | Network packet for actions |
| `ReligionActionResponsePacket.cs` | Network response packet |

---

## 9. Key Findings for Ban/Kicked Members Feature

### No Existing Ban System
The current system has:
- ✅ **Kick** functionality (implemented)
- ❌ **Ban** functionality (not implemented)
- ❌ **Restrictions/Blacklist** functionality (not implemented)

### Kick Flow:
1. Chat: `/religion kick <playername>` → ReligionCommands.OnKickPlayer()
2. Network: `ReligionActionRequestPacket` → PantheonWarsSystem.OnReligionActionRequest()
3. Action: `LeaveReligion()` called on PlayerReligionDataManager
4. Result: Player removed from religion, notification sent

### To Implement Ban/Restriction Feature, You Would Need:
1. **Add to ReligionData**:
   - `Dictionary<string, BanReason> BannedMembers` - Store banned player UIDs and reasons
   - `DateTime? BanExpiration` - Optional expiration time

2. **Add to ReligionManager**:
   - `BanMember(religionUID, playerUID, reason, expirationDate)`
   - `UnbanMember(religionUID, playerUID)`
   - `IsBanned(religionUID, playerUID)`
   - `GetBannedMembers(religionUID)`

3. **Modify CanJoinReligion()**:
   - Add check: `if (religionManager.IsBanned(religionUID, playerUID)) return false`

4. **Add Chat Command**:
   - `/religion ban <playername> [reason] [days]`
   - `/religion unban <playername>`

5. **Add Network Packets**:
   - `BanPlayerPacket` / `UnbanPlayerPacket`

