using System;
using System.Collections.Generic;
using System.Linq;
using PantheonWars.Data;
using PantheonWars.Models;
using PantheonWars.Models.Enum;
using PantheonWars.Systems.Interfaces;
using Vintagestory.API.Server;
using Vintagestory.API.Util;

namespace PantheonWars.Systems
{
    /// <summary>
    /// Manages all religions and congregation membership
    /// </summary>
    public class ReligionManager : IReligionManager
    {
        private const string DATA_KEY = "pantheonwars_religions";
        private readonly ICoreServerAPI _sapi;
        private readonly Dictionary<string, ReligionData> _religions = new();
        private readonly Dictionary<string, List<string>> _invitations = new(); // playerUID -> list of religionUIDs

        public ReligionManager(ICoreServerAPI sapi)
        {
            _sapi = sapi;
        }

        /// <summary>
        /// Initializes the religion manager
        /// </summary>
        public void Initialize()
        {
            _sapi.Logger.Notification("[PantheonWars] Initializing Religion Manager...");

            // Register event handlers
            _sapi.Event.SaveGameLoaded += OnSaveGameLoaded;
            _sapi.Event.GameWorldSave += OnGameWorldSave;

            _sapi.Logger.Notification("[PantheonWars] Religion Manager initialized");
        }

        /// <summary>
        /// Creates a new religion
        /// </summary>
        public ReligionData CreateReligion(string name, DeityType deity, string founderUID, bool isPublic)
        {
            // Generate unique UID
            string religionUID = Guid.NewGuid().ToString();

            // Validate deity type
            if (deity == DeityType.None)
            {
                throw new ArgumentException("Religion must have a valid deity");
            }

            // Create religion data
            var religion = new ReligionData(religionUID, name, deity, founderUID)
            {
                IsPublic = isPublic
            };

            // Store in dictionary
            _religions[religionUID] = religion;

            _sapi.Logger.Notification($"[PantheonWars] Religion created: {name} (Deity: {deity}, Founder: {founderUID}, Public: {isPublic})");

            return religion;
        }

        /// <summary>
        /// Adds a member to a religion
        /// </summary>
        public void AddMember(string religionUID, string playerUID)
        {
            if (!_religions.TryGetValue(religionUID, out var religion))
            {
                _sapi.Logger.Error($"[PantheonWars] Cannot add member to non-existent religion: {religionUID}");
                return;
            }

            religion.AddMember(playerUID);
            _sapi.Logger.Debug($"[PantheonWars] Added player {playerUID} to religion {religion.ReligionName}");
        }

        /// <summary>
        /// Removes a member from a religion
        /// </summary>
        public void RemoveMember(string religionUID, string playerUID)
        {
            if (!_religions.TryGetValue(religionUID, out var religion))
            {
                _sapi.Logger.Error($"[PantheonWars] Cannot remove member from non-existent religion: {religionUID}");
                return;
            }

            bool removed = religion.RemoveMember(playerUID);

            if (removed)
            {
                _sapi.Logger.Debug($"[PantheonWars] Removed player {playerUID} from religion {religion.ReligionName}");

                // Handle founder leaving
                if (religion.IsFounder(playerUID))
                {
                    HandleFounderLeaving(religion);
                }

                // Delete religion if no members remain
                if (religion.GetMemberCount() == 0)
                {
                    _religions.Remove(religionUID);
                    _sapi.Logger.Notification($"[PantheonWars] Religion {religion.ReligionName} disbanded (no members remaining)");
                }
            }
        }

        /// <summary>
        /// Handles the founder leaving the religion
        /// </summary>
        private void HandleFounderLeaving(ReligionData religion)
        {
            // If there are other members, transfer founder to next member
            if (religion.GetMemberCount() > 0)
            {
                string newFounder = religion.MemberUIDs[0];
                religion.FounderUID = newFounder;
                _sapi.Logger.Notification($"[PantheonWars] Religion {religion.ReligionName} founder transferred to {newFounder}");
            }
        }

        /// <summary>
        /// Gets the religion a player belongs to
        /// </summary>
        public ReligionData? GetPlayerReligion(string playerUID)
        {
            return _religions.Values.FirstOrDefault(r => r.IsMember(playerUID));
        }

        /// <summary>
        /// Gets a religion by UID
        /// </summary>
        public ReligionData? GetReligion(string religionUID)
        {
            return _religions.TryGetValue(religionUID, out var religion) ? religion : null;
        }

        /// <summary>
        /// Gets a religion by name
        /// </summary>
        public ReligionData? GetReligionByName(string name)
        {
            return _religions.Values.FirstOrDefault(r =>
                r.ReligionName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets the active deity for a player
        /// </summary>
        public DeityType GetPlayerActiveDeity(string playerUID)
        {
            var religion = GetPlayerReligion(playerUID);
            return religion?.Deity ?? DeityType.None;
        }

        /// <summary>
        /// Checks if a player can join a religion
        /// </summary>
        public bool CanJoinReligion(string religionUID, string playerUID)
        {
            if (!_religions.TryGetValue(religionUID, out var religion))
            {
                return false;
            }

            // Check if already a member
            if (religion.IsMember(playerUID))
            {
                return false;
            }

            // Check if public or has invitation
            if (religion.IsPublic)
            {
                return true;
            }

            return HasInvitation(playerUID, religionUID);
        }

        /// <summary>
        /// Invites a player to a religion
        /// </summary>
        public void InvitePlayer(string religionUID, string playerUID, string inviterUID)
        {
            if (!_religions.TryGetValue(religionUID, out var religion))
            {
                _sapi.Logger.Error($"[PantheonWars] Cannot invite to non-existent religion: {religionUID}");
                return;
            }

            // Validate inviter is a member
            if (!religion.IsMember(inviterUID))
            {
                _sapi.Logger.Warning($"[PantheonWars] Player {inviterUID} cannot invite to religion they're not in");
                return;
            }

            // Add invitation
            if (!_invitations.ContainsKey(playerUID))
            {
                _invitations[playerUID] = new List<string>();
            }

            if (!_invitations[playerUID].Contains(religionUID))
            {
                _invitations[playerUID].Add(religionUID);
                _sapi.Logger.Debug($"[PantheonWars] Player {playerUID} invited to religion {religion.ReligionName}");
            }
        }

        /// <summary>
        /// Checks if a player has an invitation to a religion
        /// </summary>
        public bool HasInvitation(string playerUID, string religionUID)
        {
            return _invitations.TryGetValue(playerUID, out var invites) && invites.Contains(religionUID);
        }

        /// <summary>
        /// Removes an invitation (called after accepting or declining)
        /// </summary>
        public void RemoveInvitation(string playerUID, string religionUID)
        {
            if (_invitations.TryGetValue(playerUID, out var invites))
            {
                invites.Remove(religionUID);
            }
        }

        /// <summary>
        /// Gets all invitations for a player
        /// </summary>
        public List<string> GetPlayerInvitations(string playerUID)
        {
            return _invitations.TryGetValue(playerUID, out var invites) ? invites : new List<string>();
        }

        /// <summary>
        /// Checks if a player has a religion
        /// </summary>
        public bool HasReligion(string playerUID)
        {
            return GetPlayerReligion(playerUID) != null;
        }

        /// <summary>
        /// Gets all religions
        /// </summary>
        public List<ReligionData> GetAllReligions()
        {
            return _religions.Values.ToList();
        }

        /// <summary>
        /// Gets religions by deity
        /// </summary>
        public List<ReligionData> GetReligionsByDeity(DeityType deity)
        {
            return _religions.Values.Where(r => r.Deity == deity).ToList();
        }

        /// <summary>
        /// Deletes a religion (founder only)
        /// </summary>
        public bool DeleteReligion(string religionUID, string requesterUID)
        {
            if (!_religions.TryGetValue(religionUID, out var religion))
            {
                return false;
            }

            // Only founder can delete
            if (!religion.IsFounder(requesterUID))
            {
                return false;
            }

            _religions.Remove(religionUID);
            _sapi.Logger.Notification($"[PantheonWars] Religion {religion.ReligionName} disbanded by founder");
            return true;
        }

        #region Persistence

        public void OnSaveGameLoaded()
        {
            LoadAllReligions();
        }

        public void OnGameWorldSave()
        {
            SaveAllReligions();
        }

        /// <summary>
        /// Loads all religions from world storage
        /// </summary>
        private void LoadAllReligions()
        {
            try
            {
                byte[]? data = _sapi.WorldManager.SaveGame.GetData(DATA_KEY);
                if (data != null)
                {
                    var religionsList = SerializerUtil.Deserialize<List<ReligionData>>(data);
                    if (religionsList != null)
                    {
                        _religions.Clear();
                        foreach (var religion in religionsList)
                        {
                            _religions[religion.ReligionUID] = religion;
                        }
                        _sapi.Logger.Notification($"[PantheonWars] Loaded {_religions.Count} religions");
                    }
                }
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Failed to load religions: {ex.Message}");
            }
        }

        /// <summary>
        /// Saves all religions to world storage
        /// </summary>
        private void SaveAllReligions()
        {
            try
            {
                var religionsList = _religions.Values.ToList();
                byte[] data = SerializerUtil.Serialize(religionsList);
                _sapi.WorldManager.SaveGame.StoreData(DATA_KEY, data);
                _sapi.Logger.Debug($"[PantheonWars] Saved {religionsList.Count} religions");
            }
            catch (Exception ex)
            {
                _sapi.Logger.Error($"[PantheonWars] Failed to save religions: {ex.Message}");
            }
        }

        #endregion
    }
}
