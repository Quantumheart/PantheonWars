# Religion System Architecture Diagram

## Component Relationships

```
┌─────────────────────────────────────────────────────────────────────────┐
│                          CLIENT SIDE                                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────┐    │
│  │  ReligionManagementDialog (GUI)                                  │    │
│  │  - Browse Religions Tab                                          │    │
│  │  - My Religion Tab (with kick buttons for founder)               │    │
│  │  - Sends ReligionActionRequestPacket("kick", religion, player)   │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                ↓                                           │
│  ┌──────────────────────────────────────────────────────────────────┐    │
│  │  MemberListRenderer                                              │    │
│  │  - Renders member list                                           │    │
│  │  - Shows kick button (if founder and non-founder member)         │    │
│  │  - Triggers onKickMember callback                                │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────┘
                                ↓
              ReligionActionRequestPacket("kick", ...)
                                ↓
┌─────────────────────────────────────────────────────────────────────────┐
│                          SERVER SIDE                                      │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                             │
│  ┌──────────────────────────────────────────────────────────────────┐    │
│  │  PantheonWarsSystem                                              │    │
│  │  OnReligionActionRequest(player, packet)                         │    │
│  │  - Validates: player is founder                                  │    │
│  │  - Calls: playerReligionDataManager.LeaveReligion(targetUID)     │    │
│  │  - Sends notifications to both players                           │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                ↓                                           │
│  ┌──────────────────────────────────────────────────────────────────┐    │
│  │  PlayerReligionDataManager                                       │    │
│  │  LeaveReligion(playerUID)                                        │    │
│  │  - Applies switch penalty (reset favor, clear blessings)         │    │
│  │  - Calls: religionManager.RemoveMember(religionUID, playerUID)   │    │
│  │  - Updates player data                                           │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                ↓                                           │
│  ┌──────────────────────────────────────────────────────────────────┐    │
│  │  ReligionManager                                                 │    │
│  │  RemoveMember(religionUID, playerUID)                            │    │
│  │  - Removes player from MemberUIDs list                           │    │
│  │  - If religion is now empty → delete religion                    │    │
│  │  - If founder left → transfer leadership to next member          │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                ↓                                           │
│  ┌──────────────────────────────────────────────────────────────────┐    │
│  │  ReligionData                                                    │    │
│  │  - MemberUIDs (List<string>)                                     │    │
│  │  - FounderUID (string)                                           │    │
│  │  - Persisted to world storage                                    │    │
│  └──────────────────────────────────────────────────────────────────┘    │
│                                                                             │
└─────────────────────────────────────────────────────────────────────────┘
                                ↓
          ReligionActionResponsePacket + ReligionStateChangedPacket
                                ↓
              Back to Client (GUI updates & notifications)
```

## Data Flow for Kick Action

```
User clicks "Kick" button on member
        ↓
MemberListRenderer.onKickMember(memberUID)
        ↓
ReligionManagementDialog.OnKickMemberClicked(memberUID)
        ↓
Send: ReligionActionRequestPacket("kick", religionUID, targetUID)
        ↓
[NETWORK]
        ↓
PantheonWarsSystem.OnReligionActionRequest()
        ↓
Validate: Is sender the founder?
    If NO → Return error "Only founder can kick"
    If YES → Continue
        ↓
Validate: Is target different from sender?
    If NO → Return error "Cannot kick yourself"
    If YES → Continue
        ↓
PlayerReligionDataManager.LeaveReligion(targetUID)
    - HandleReligionSwitch(targetUID)
      - Call ApplySwitchPenalty() on player data
        * Set Favor = 0
        * Set FavorRank = Initiate
        * Clear UnlockedBlessings
    - Call ReligionManager.RemoveMember(religionUID, targetUID)
      - Remove from MemberUIDs
      - Check if religion is now empty → delete
      - Check if founder was removed → transfer leadership
        ↓
Send: ReligionActionResponsePacket(success=true, "Kicked player from religion.")
        ↓
Send notification to kicked player (if online)
    - Chat message: "You have been kicked from {religionName}"
    - ReligionStateChangedPacket(HasReligion=false)
    - Refresh player HUD
```

## Core Objects Relationship

```
┌─────────────────────────────────────────────────────────────────┐
│                      ReligionData                                │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ - ReligionUID: string (unique)                           │   │
│  │ - ReligionName: string                                   │   │
│  │ - FounderUID: string (player UID)                        │   │
│  │ - MemberUIDs: List<string> [founder always first]        │   │
│  │ - Deity: DeityType                                       │   │
│  │ - IsPublic: bool                                         │   │
│  │ - Prestige/TotalPrestige: int                            │   │
│  │ - PrestigeRank: PrestigeRank enum                        │   │
│  │ - UnlockedBlessings: Dict<string, bool>                  │   │
│  │ - Description: string                                    │   │
│  └──────────────────────────────────────────────────────────┘   │
│  - Persisted via ReligionManager                                │
└─────────────────────────────────────────────────────────────────┘
                              ↑
                              │ (1 per religion)
                              │
┌─────────────────────────────────────────────────────────────────┐
│                   PlayerReligionData                             │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │ - PlayerUID: string (unique)                             │   │
│  │ - ReligionUID: string? (current religion or null)        │   │
│  │ - Favor: int (resets on switch, current)                 │   │
│  │ - TotalFavorEarned: int (lifetime, persists)             │   │
│  │ - FavorRank: FavorRank enum                              │   │
│  │ - ActiveDeity: DeityType (cached)                        │   │
│  │ - UnlockedBlessings: Dict<string, bool>                  │   │
│  │ - LastReligionSwitch: DateTime? (cooldown)               │   │
│  │ - AccumulatedFractionalFavor: float                       │   │
│  │ - KillCount: int                                         │   │
│  └──────────────────────────────────────────────────────────┘   │
│  - Persisted via PlayerReligionDataManager                      │
└─────────────────────────────────────────────────────────────────┘
                      ↑ (1 per player)
                      │
        One player in many religions
        One religion has many players
```

## Permission Hierarchy

```
┌─────────────────────────────────────────────────────────────┐
│              FOUNDER (FounderUID)                            │
├─────────────────────────────────────────────────────────────┤
│ CAN:                                                         │
│ ✅ Kick members                                              │
│ ✅ Invite players                                            │
│ ✅ Edit description                                          │
│ ✅ Disband religion                                          │
│ ✅ Leave (transfers leadership)                              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│            MEMBER (in MemberUIDs)                            │
├─────────────────────────────────────────────────────────────┤
│ CAN:                                                         │
│ ✅ View religion info                                        │
│ ✅ View member list                                          │
│ ✅ Leave religion                                            │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│          NON-MEMBER (not in MemberUIDs)                      │
├─────────────────────────────────────────────────────────────┤
│ CAN:                                                         │
│ ✅ Join (if public or invited)                               │
│ ✅ View religion info                                        │
└─────────────────────────────────────────────────────────────┘
```

## Key Behavioral Rules

### Founder Rights
- Only founder can kick members
- Only founder can invite players
- Only founder can edit description
- Only founder can disband religion
- Cannot be kicked (must leave voluntarily or be forced out if religion disbanded)

### Member Rules
- Cannot join if already in a religion (must leave first)
- Cannot join private religions without invitation
- Cannot kick themselves (self-removal only via leave)
- Cannot access founder-only actions
- Cannot view member list (except in "My Religion" tab when in a religion)
- Automatically removed from religion if founder removes them

### Religion Rules
- Religion is deleted if it becomes empty (no members)
- Founder role transfers automatically if founder leaves
- Private religions require explicit invitations
- Public religions anyone can join
- Leadership can only be transferred via founder removal, not manually

### Switch Rules
- 7-day cooldown between religion switches
- Switching religions applies penalty:
  * Current favor reset to 0
  * FavorRank reset to Initiate
  * All player-specific unlocked blessings cleared
  * TotalFavorEarned (lifetime stat) is preserved

## Message Flow Example: Founder Kicks Member

```
STEP 1: User Action
  Player A (Founder) clicks "Kick" on Player B in member list
  
STEP 2: Client sends packet
  ReligionActionRequestPacket {
    Action: "kick",
    ReligionUID: "religion-123",
    TargetPlayerUID: "player-b-uid"
  }
  
STEP 3: Server receives and validates
  - Verify Player A is founder of religion-123 ✓
  - Verify Player B is in religion-123 ✓
  - Verify Player A ≠ Player B ✓
  
STEP 4: Server executes kick
  - Call LeaveReligion("player-b-uid")
    * Apply switch penalty to Player B
    * Remove Player B from MemberUIDs
    
STEP 5: Server sends responses
  To Player A (founder): ReligionActionResponsePacket {
    Success: true,
    Message: "Kicked player from religion.",
    Action: "kick"
  }
  
  To Player B (if online): 
    - Chat notification: "You have been kicked from {religionName}"
    - ReligionStateChangedPacket { HasReligion: false, ... }
    - Refresh HUD
    
STEP 6: Client updates UI
  - Player A sees success message
  - Player B's "My Religion" tab is empty/disabled
  - Both see updated member list
```
