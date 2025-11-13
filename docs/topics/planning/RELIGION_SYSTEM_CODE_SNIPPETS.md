# Religion System - Code Snippets & Quick Reference

## Finding Religion-Related Code

### By Functionality

#### Creating a Religion
```csharp
// ReligionManager.cs
public ReligionData CreateReligion(string name, DeityType deity, string founderUID, bool isPublic)
{
    var religionUID = Guid.NewGuid().ToString();
    var religion = new ReligionData(religionUID, name, deity, founderUID)
    {
        IsPublic = isPublic
    };
    _religions[religionUID] = religion;
    return religion;
}
```

#### Adding a Member
```csharp
// Two-step process:
// 1. ReligionManager
_religionManager.AddMember(religionUID, playerUID);

// 2. PlayerReligionDataManager (higher level)
_playerReligionDataManager.JoinReligion(playerUID, religionUID);
```

#### Removing a Member (Kick)
```csharp
// Called during kick operation
_playerReligionDataManager.LeaveReligion(targetPlayerUID);

// This internally:
// 1. Calls HandleReligionSwitch() - applies penalties
// 2. Calls _religionManager.RemoveMember()
// 3. Clears player's religion affiliation
```

#### Checking Membership
```csharp
// ReligionData
bool isMember = religion.IsMember(playerUID);
bool isFounder = religion.IsFounder(playerUID);
int memberCount = religion.GetMemberCount();

// ReligionManager
var playerReligion = _religionManager.GetPlayerReligion(playerUID);
bool hasReligion = _religionManager.HasReligion(playerUID);
```

#### Checking Invitations (for private religions)
```csharp
// ReligionManager
bool canJoin = _religionManager.CanJoinReligion(religionUID, playerUID);
bool hasInvite = _religionManager.HasInvitation(playerUID, religionUID);
_religionManager.InvitePlayer(religionUID, playerUID, inviterUID);
_religionManager.RemoveInvitation(playerUID, religionUID);
```

---

## Kick Implementation Details

### Chat Command Entry Point
**File**: ReligionCommands.cs (lines 349-396)

```csharp
private TextCommandResult OnKickPlayer(TextCommandCallingArgs args)
{
    var targetPlayerName = (string)args[0];
    var player = args.Caller.Player as IServerPlayer;
    
    // Validation steps:
    // 1. Check if executor has a religion
    var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
    if (!playerData.HasReligion()) 
        return TextCommandResult.Error("You are not in any religion");
    
    // 2. Get the religion
    var religion = _religionManager.GetReligion(playerData.ReligionUID!);
    
    // 3. Check if executor is founder
    if (!religion.IsFounder(player.PlayerUID)) 
        return TextCommandResult.Error("Only the founder can kick members");
    
    // 4. Find target player
    var targetPlayer = _sapi.World.AllPlayers
        .FirstOrDefault(p => p.PlayerName.Equals(targetPlayerName, StringComparison.OrdinalIgnoreCase));
    
    if (targetPlayer == null) 
        return TextCommandResult.Error($"Player '{targetPlayerName}' not found");
    
    // 5. Check if target is a member
    if (!religion.IsMember(targetPlayer.PlayerUID))
        return TextCommandResult.Error($"{targetPlayerName} is not a member");
    
    // 6. Prevent self-kick
    if (targetPlayer.PlayerUID == player.PlayerUID)
        return TextCommandResult.Error("You cannot kick yourself");
    
    // 7. Execute kick
    _playerReligionDataManager.LeaveReligion(targetPlayer.PlayerUID);
    
    // 8. Notify target
    var targetServerPlayer = targetPlayer as IServerPlayer;
    if (targetServerPlayer != null)
        targetServerPlayer.SendMessage(
            GlobalConstants.GeneralChatGroup,
            $"You have been removed from {religion.ReligionName}",
            EnumChatType.Notification
        );
    
    return TextCommandResult.Success(
        $"{targetPlayerName} has been removed from {religion.ReligionName}");
}
```

### Server-Side Network Handler
**File**: PantheonWarsSystem.cs (lines 338-376)

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
                SendPlayerDataToClient(kickedPlayer); // Refresh HUD
                
                // Send state changed packet
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

### GUI Implementation
**File**: ReligionManagementDialog.cs (lines 442-448)

```csharp
private bool OnKickMemberClicked(string memberUID)
{
    // Send kick request to server
    _channel.SendPacket(
        new ReligionActionRequestPacket(
            "kick", 
            _playerReligionInfo?.ReligionUID ?? "", 
            memberUID
        )
    );
    RefreshData();
    return true;
}
```

### Member Renderer
**File**: MemberListRenderer.cs (lines 150-161)

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

## Common Operations

### Get All Members of a Religion
```csharp
var religion = _religionManager.GetReligion(religionUID);
if (religion != null)
{
    var memberNames = religion.MemberUIDs
        .Select(uid => _sapi.World.PlayerByUid(uid)?.PlayerName ?? "Unknown")
        .ToList();
}
```

### Get Player's Current Religion
```csharp
var religion = _religionManager.GetPlayerReligion(playerUID);
if (religion != null)
{
    var religionName = religion.ReligionName;
    var isFounder = religion.IsFounder(playerUID);
}
```

### Check if Player Has Permission for Action
```csharp
var playerData = _playerReligionDataManager.GetOrCreatePlayerData(playerUID);
if (!playerData.HasReligion())
{
    // Player not in any religion
}

var religion = _religionManager.GetReligion(playerData.ReligionUID!);
if (religion.IsFounder(playerUID))
{
    // Player is founder - can kick, invite, disband, edit description
}
else if (religion.IsMember(playerUID))
{
    // Player is member - can only view and leave
}
```

### Apply Switch Penalty
```csharp
// When player switches religions or is kicked:
var playerData = _playerReligionDataManager.GetOrCreatePlayerData(playerUID);

// Option 1: Automatic via LeaveReligion
_playerReligionDataManager.LeaveReligion(playerUID);

// Option 2: Manual
playerData.ApplySwitchPenalty();
// Results in:
// - Favor = 0
// - FavorRank = Initiate
// - UnlockedBlessings cleared
// - (TotalFavorEarned is preserved)
```

### Check Religion Switch Cooldown
```csharp
if (_playerReligionDataManager.CanSwitchReligion(playerUID))
{
    // Player can switch (either first time or cooldown expired)
}
else
{
    var remaining = _playerReligionDataManager.GetSwitchCooldownRemaining(playerUID);
    // Show remaining time: remaining?.Days, remaining?.Hours, etc.
}
```

---

## Data Persistence

### Loading Data
```csharp
// ReligionManager loads all religions on server start
_religionManager.OnSaveGameLoaded();

// PlayerReligionDataManager loads individual player data as they join
_playerReligionDataManager.LoadPlayerData(playerUID);
```

### Saving Data
```csharp
// ReligionManager saves when world saves
_religionManager.OnGameWorldSave();

// PlayerReligionDataManager saves when world saves or player disconnects
_playerReligionDataManager.SavePlayerData(playerUID);
_playerReligionDataManager.SaveAllPlayerData();
```

### Storage Keys
```csharp
// ReligionManager uses:
const string DATA_KEY = "pantheonwars_religions";  // Stores List<ReligionData>

// PlayerReligionDataManager uses:
const string DATA_KEY = "pantheonwars_playerreligiondata";
// Individual storage: $"{DATA_KEY}_{playerUID}" for each player
```

---

## Network Communication

### Action Request Packet
```csharp
// Send from client to server
var packet = new ReligionActionRequestPacket(
    action: "kick",           // "join", "leave", "kick", "invite", "disband"
    religionUID: religionId,
    targetPlayerUID: memberUID
);
_channel.SendPacket(packet);
```

### Action Response Packet
```csharp
// Sent from server back to client
var response = new ReligionActionResponsePacket(
    success: true,
    message: "Kicked player from religion.",
    action: "kick"
);
_serverChannel.SendPacket(response, player);
```

### State Changed Packet
```csharp
// Sent to notify of major state changes (kick, disband, etc.)
var packet = new ReligionStateChangedPacket
{
    Reason = "You have been kicked from {religionName}",
    HasReligion = false
};
_serverChannel.SendPacket(packet, kickedPlayer);
```

---

## Testing Patterns

### Mock Setup for Tests
```csharp
var _mockAPI = TestFixtures.CreateMockServerAPI();
var _religionManager = new ReligionManager(_mockAPI.Object);

// Create test religion
var religion = _religionManager.CreateReligion(
    "Test Religion", 
    DeityType.Khoras, 
    "founder-uid", 
    true
);

// Add member
_religionManager.AddMember(religion.ReligionUID, "player-uid");

// Verify
Assert.True(religion.IsMember("player-uid"));
```

---

## Common Error Scenarios

| Error | Cause | Check |
|-------|-------|-------|
| "You are not in any religion" | Player has no religion | `playerData.HasReligion()` |
| "Only the founder can kick members" | Non-founder trying to kick | `religion.IsFounder(playerUID)` |
| "You cannot kick yourself" | Trying to kick own UID | `targetUID != playerUID` |
| "{name} is not a member" | Target not in religion | `religion.IsMember(targetUID)` |
| "This religion is private and you have not been invited" | Private religion without invite | `CanJoinReligion()` check |
| "You must wait X days before switching religions" | Cooldown not expired | `CanSwitchReligion()` check |

---

## Extension Points for Ban System

### Adding Ban Support to ReligionData
```csharp
// Add these properties to ReligionData class:
[ProtoMember(13)]
public Dictionary<string, DateTime> BannedPlayers { get; set; } = new();

[ProtoMember(14)]
public Dictionary<string, string> BanReasons { get; set; } = new();

// Add these methods to ReligionData class:
public void BanPlayer(string playerUID, string reason = "")
{
    BannedPlayers[playerUID] = DateTime.UtcNow;
    if (!string.IsNullOrEmpty(reason))
        BanReasons[playerUID] = reason;
}

public void UnbanPlayer(string playerUID)
{
    BannedPlayers.Remove(playerUID);
    BanReasons.Remove(playerUID);
}

public bool IsBanned(string playerUID)
{
    return BannedPlayers.ContainsKey(playerUID);
}

public bool IsBanExpired(string playerUID, int banDays = 7)
{
    if (!BannedPlayers.TryGetValue(playerUID, out var banTime))
        return false;
    
    return DateTime.UtcNow >= banTime.AddDays(banDays);
}
```

### Modifying CanJoinReligion to Check Bans
```csharp
// In ReligionManager.cs
public bool CanJoinReligion(string religionUID, string playerUID)
{
    if (!_religions.TryGetValue(religionUID, out var religion)) 
        return false;

    // Check if already a member
    if (religion.IsMember(playerUID)) 
        return false;

    // Check if banned
    if (religion.IsBanned(playerUID) && !religion.IsBanExpired(playerUID))
        return false; // Permanently banned or ban not expired

    // Check if public or has invitation
    if (religion.IsPublic) 
        return true;

    return HasInvitation(playerUID, religionUID);
}
```

### Adding Ban Command
```csharp
// In ReligionCommands.cs
.BeginSubCommand("ban")
.WithDescription("Ban a player from your religion (founder only)")
.WithArgs(
    _sapi.ChatCommands.Parsers.Word("playername"),
    _sapi.ChatCommands.Parsers.OptionalAll("reason"))
.HandleWith(OnBanPlayer)
.EndSubCommand()

private TextCommandResult OnBanPlayer(TextCommandCallingArgs args)
{
    var targetPlayerName = (string)args[0];
    var reason = args.Parsers.Count > 1 ? (string?)args[1] : "No reason provided";
    
    var player = args.Caller.Player as IServerPlayer;
    if (player == null) 
        return TextCommandResult.Error("Command can only be used by players");
    
    var playerData = _playerReligionDataManager.GetOrCreatePlayerData(player.PlayerUID);
    if (!playerData.HasReligion()) 
        return TextCommandResult.Error("You are not in any religion");
    
    var religion = _religionManager.GetReligion(playerData.ReligionUID!);
    if (!religion.IsFounder(player.PlayerUID)) 
        return TextCommandResult.Error("Only the founder can ban members");
    
    var targetPlayer = _sapi.World.AllPlayers
        .FirstOrDefault(p => p.PlayerName.Equals(targetPlayerName, StringComparison.OrdinalIgnoreCase));
    
    if (targetPlayer == null) 
        return TextCommandResult.Error($"Player '{targetPlayerName}' not found");
    
    if (!religion.IsMember(targetPlayer.PlayerUID))
        return TextCommandResult.Error($"{targetPlayerName} is not a member");
    
    // Ban the player
    religion.BanPlayer(targetPlayer.PlayerUID, reason);
    
    // Kick them from the religion
    _playerReligionDataManager.LeaveReligion(targetPlayer.PlayerUID);
    
    // Notify
    if (targetPlayer is IServerPlayer targetServerPlayer)
        targetServerPlayer.SendMessage(
            GlobalConstants.GeneralChatGroup,
            $"You have been banned from {religion.ReligionName}. Reason: {reason}",
            EnumChatType.Notification);
    
    return TextCommandResult.Success($"{targetPlayerName} has been banned from {religion.ReligionName}");
}
```

