using LabApi.Events.Arguments.PlayerEvents;
using LabApi.Events.Handlers;
using LabApi.Features.Extensions;
using LabApi.Features.Wrappers;
using Mirror;
using PlayerRoles;
using PlayerRoles.FirstPersonControl;
using PlayerRoles.FirstPersonControl.NetworkMessages;
using PlayerRoles.PlayableScps.Scp049.Zombies;
using PlayerRoles.PlayableScps.Scp1507;
using PlayerStatsSystem;
using RelativePositioning;
using RemoteAdmin.Communication;
using Respawning.NamingRules;
using System.Collections.Generic;
using System.Linq;
using TakChangesV5.DisguiseModule.UI;
using TakChangesV5.ForEvents.DisguiseModule.UI;
using UnityEngine;

// File for the whole logic of disguising

namespace TakChangesV5.DisguiseModule.Extension {
    public static class FakeRoleManager
    {
        // The reason of disguise changes (for Announce Hints)
        public enum DisguiseChangeReason : sbyte
        {
            NONE = 0, // If we dont want to show it in AnnounceHints
            ADD_BY_RA_CONSOLE = 1,
            REMOVED_BY_RA_CONSOLE = 2,
            REMOVED_BY_TIMER = 3
        }

        public struct FakeRole
        {
            public RoleTypeId Role;
            // public Dictionary<ReferenceHub, RoleTypeId> RoleToViewer;
            public float EndTime;
            public int TeamMode;
        }

        // Dict of all active disguises
        public static readonly Dictionary<ReferenceHub, FakeRole> FakeRoles = [];
        // If player changes role or leaves, his disguise will be cleared
        
        // #FOR TAKAIL:
        // LEAVE THIS SHIT AS IT IS, UNLESS IT BREAKS UR PLUGINS!!!! >:(
        static FakeRoleManager()
        {
            PlayerEvents.ChangingRole += PlayerEvents_ChangingRole;
            PlayerEvents.Left += PlayerEvents_OnPlayerLeft;
            FpcServerPositionDistributor.RoleSyncEvent += FpcServerPositionDistributor_RoleSyncEvent;
        }

        private static void PlayerEvents_ChangingRole(PlayerChangingRoleEventArgs ev)
        {
            if (ev.Player is null)
                return;

            if (ev.IsAllowed)
                RemoveFakeRole(ev.Player, DisguiseChangeReason.NONE);
        }

        private static void PlayerEvents_OnPlayerLeft(PlayerLeftEventArgs ev)
        {
            if (FakeRoles.ContainsKey(ev.Player.ReferenceHub))
                RemoveFakeRole(ev.Player, DisguiseChangeReason.NONE);

            if (ev.Player is null)
                return;
        }

        // Method for setting which observers will be fooled by the disguise
        public static bool ShouldSeeFakeRole(Player? disguised, Player? observer)
        {
            if (disguised == null || observer == null) {
                return false;
            }

            // Overwatch always see the truth
            if (observer.Role == RoleTypeId.Overwatch) {
                return false;
            }

            if (FakeRoles[disguised.ReferenceHub].TeamMode != 1) {
                return true;
            }

            if (observer.Role is RoleTypeId.Spectator or RoleTypeId.Destroyed or RoleTypeId.Filmmaker) {
                return true;
            }

            // Others see true identity only of their teammates!
            // #WARNING: When Flamingo HUMANS will be added I should revision this equality (check Factions)
            return disguised.Faction != observer.Faction;

        }

        // Adding FakeRole to player
        public static void AddFakeRole(this Player player, RoleTypeId roleType, DisguiseChangeReason reason, float duration = 31536000f, int teamMode = 1) {
            FakeRoles.Remove(player.ReferenceHub);

            if (player.Connection == null && player.IsReady) {
                return;
            }

            // Adding new player to disguise dictionary
            var value = FakeRoles[player.ReferenceHub] = new FakeRole
            {
                Role = roleType,
                // RoleToViewer = [],
                EndTime = Time.time + duration,
                TeamMode = teamMode
            };

            FakeRoles[player.ReferenceHub] = value;

            // Update UI according to the new fake role for the player
            // #NOTE: ITS OKAY THAT IT COMES AFTER NEW DATA, CUZ DATA EXISTS AND ITS BEING UPDATED
            DisguisedUI.RefreshDisguiseUI(player, reason);
        }
        

        public static void RemoveFakeRole(this Player player, DisguiseChangeReason reason)
        {
            // #NOTE: REMOVAL FROM DICT MUST COME AFTER UI UPDATE

            if (!FakeRoles.ContainsKey(player.ReferenceHub))
                return;

            // Update UI according to the removal of fake role for the player
            DisguisedUI.RefreshDisguiseUI(player, reason);

            // Remove the player's disguise if it exists in dictionary
            FakeRoles.Remove(player.ReferenceHub);      
        }

        public static void RemoveFakeRolesForAll(DisguiseChangeReason reason)
        {
            // #NOTE: REMOVAL FROM DICT MUST COME AFTER UI UPDATE

            // Remove all disguises and Clear UIs (Only used in 'Unmask *')
            foreach (var kvp in FakeRoles.ToDictionary(kvp => kvp.Key, kvp => kvp.Value))
            {
                DisguisedUI.RefreshDisguiseUI(Player.Get(kvp.Key), reason);
            }

            FakeRoles.Clear();
        }

        private static void UpdateTimers() {
            // Updating timers for temporary disguises
            // #WARNING technically works seperately from UI, so yeah...
            foreach (var fakeRole in FakeRoles.Where(kvp => kvp.Value.EndTime > 0 && Time.time >= kvp.Value.EndTime)) {
                DisguisedUI.RefreshDisguiseUI(Player.Get(fakeRole.Key), DisguiseChangeReason.REMOVED_BY_TIMER);
                FakeRoles.Remove(fakeRole.Key);
            }
        }

        public static RoleTypeId FpcServerPositionDistributor_RoleSyncEvent(ReferenceHub hub, ReferenceHub receiver, RoleTypeId roleType, NetworkWriter writer)
        {
            // This Method is constantly called.

            // Update the timers for temporary disguises
            UpdateTimers();

            var disguisedPlayer = Player.Get(hub);
            var observerPlayer = Player.Get(receiver);

            if (disguisedPlayer == null ||
                observerPlayer == null ||
                !FakeRoles.TryGetValue(hub, out FakeRole fakeRole) ||
                !ShouldSeeFakeRole(disguisedPlayer, observerPlayer)) {
                return roleType;
            }

            var returnRole = fakeRole.Role;
            WriteExtraForRole(hub, returnRole, writer);
            return returnRole;

        }

        // This is the core logic of applying disguises for players
        // #Note: Some info here might seem useless and overkill, but it is essential for correct disguising
        private static void WriteExtraForRole(ReferenceHub hub, RoleTypeId roleType, NetworkWriter writer)
        {
            PlayerRoleBase roleBase = roleType.GetRoleBase();

            if (roleBase is HumanRole humanRole)
            {
                // Checking UsesUnitNames
                UnitNamingRule rule;
                if (NamingRulesManager.TryGetNamingRule(humanRole.Team, out rule))
                {
                    writer.WriteByte(0); // UnitNameId = 0
                }
            }

            if (roleBase is ZombieRole)
            {
                // _syncMaxHealth
                writer.WriteUShort((ushort)Mathf.Clamp(Mathf.CeilToInt(hub.playerStats.GetModule<HealthStat>().MaxValue), 0, 65535));
                // _showConfirmationBox
                writer.WriteBool(true);
            }

            if (roleBase is Scp1507Role)
            {
                // ServerSpawnReason
                writer.WriteByte((byte)hub.roleManager.CurrentRole.ServerSpawnReason);
            }

            FpcStandardRoleBase fpcStandardRoleBase = roleBase as FpcStandardRoleBase;
            if (fpcStandardRoleBase != null)
            {
                if (hub.roleManager.CurrentRole is FpcStandardRoleBase fpcStandardRoleBase2)
                {
                    fpcStandardRoleBase = fpcStandardRoleBase2;
                }

                ushort syncH = 0;
                fpcStandardRoleBase?.FpcModule.MouseLook.GetSyncValues(0, out syncH, out var _);
                // RelativePosition
                writer.WriteRelativePosition(new(hub));
                // syncH
                writer.WriteUShort(syncH);
            }
        }
    }
}