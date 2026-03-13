using CommandSystem;
using LabApi.Features.Wrappers;
using PlayerRoles;
using RemoteAdmin;
using System;
using System.Collections.Generic;
using System.Linq;
using TakChangesV5.ForEvents.DisguiseModule.Extension;
using static PlayerArms;
using static TakChangesV5.ForEvents.DisguiseModule.Extension.FakeRoleManager;

namespace TakChangesV5.ForEvents.DisguiseModule.RemoteAdmin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MaskCommand : ICommand, IUsageProvider
    {
        public string Command => "mask";
        public string[] Aliases => new[] { "disguise" };
        public string Description => "Disguises a player as another class";

        public string[] Usage { get; } = { "player ID/* if all", "class ID", "optional: '0' or '1' (1 - teammates will see the real role)", "optional: disguise time in seconds" };

        // Forbidden disguises List
        private static readonly HashSet<RoleTypeId> UndisguisableRoles = new()
        {
            RoleTypeId.None,
            RoleTypeId.Destroyed,
            RoleTypeId.Spectator,
            RoleTypeId.Scp079,
            RoleTypeId.CustomRole,
            RoleTypeId.Overwatch,
            RoleTypeId.Filmmaker
        };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "This command cannot be executed if the command sender isnt on the server!";
                return false;
            }

            // #TAKAIL
            // Access check
            if (!sender.CheckPermission(PlayerPermissions.Noclip))
            {
                response = "YOU HAVE NO RIGHTS HERE!";
                return false;
            }

            // Null Command Sender
            Player senderPlayer = Player.Get(playerSender.ReferenceHub);
            if (senderPlayer == null)
            {
                response = "Command sender not recognised (= null)!";
                return false;
            }

            // Not enough arguments
            if (arguments.Count < 2)
            {
                response = $"Incorrect format!\n\n" + GetUsage() + GetClassList();
                return false;
            }

            // Class ID parse
            if (!int.TryParse(arguments.At(1), out int classId))
            {
                response = $"Incorrect class ID: ({arguments.At(1)})\n\n" + GetClassList();
                return false;
            }

            RoleTypeId classType = (RoleTypeId)classId;
            // If this disguise cant be applied (spectator as disguise, overwatch as disguise etc)
            if (!Enum.IsDefined(typeof(RoleTypeId), classType) || UndisguisableRoles.Contains(classType))
            {
                response = $"Class with ID: ({classId}) does not exist or cannot be set as a disguise!\n\n" + GetClassList();
                return false;
            }

            // teamMode parse
            int teamMode = 1;
            if (arguments.Count >= 3)
            {
                if (!int.TryParse(arguments.At(2), out teamMode) || (teamMode != 0 && teamMode != 1))
                {
                    response = $"Incorrect teamMode: {arguments.At(2)} (must be 0 or 1)";
                    return false;
                }
            }

            // Time parse
            float duration = 31536000f; // 31536000f = inf (but if one round lasts for like 1 year we are fucked)
            if (arguments.Count >= 4)
            {
                if (!float.TryParse(arguments.At(3), out duration) || duration <= 0 || duration >= 1000000)
                {
                    response = $"Incorrect duration: {arguments.At(3)}s (must be > 0s and < 1 million s)";
                    return false;
                }
            }

            string arg0 = arguments.At(0).ToLower();
            // If disguise everyone
            if (arg0 == "all" || arg0 == "*")
            {
                // Get all players that can be disguised
                List<Player> validPlayers = new List<Player>();
                int maskedCount = 0;
                int skippedCount = 0;

                foreach (var player in Player.List)
                {
                    if (!(player == null || UndisguisableRoles.Contains(player.Role))) validPlayers.Add(player);
                    else skippedCount++;
                }

                // Apply disguise to all valid players
                foreach (var player in validPlayers)
                {
                    player.AddFakeRole(classType, DisguiseChangeReason.AddByRAConsole, duration, teamMode);
                    maskedCount++;
                }

                string durationText = duration <= 1000002f ? $" for {duration} seconds" : "";

                if (skippedCount > 0) response = $"Done! {maskedCount} players masked as ({classId}) {classType}{durationText}. Skipped {skippedCount - 1} players with undisguisable roles.";
                else response = $"Done! {maskedCount} players masked as ({classId}) {classType}{durationText}.";

                return true;
            }

            // Player's ID parse
            if (!int.TryParse(arg0, out int targetId))
            {
                response = $"Incorrect player ID: ({arg0})\n\n" + GetUsage();
                return false;
            }

            // Null target player
            Player targetPlayer = Player.Get(targetId);
            if (targetPlayer == null)
            {
                response = $"Player with ID: ({targetId}) not found!";
                return false;
            }

            // If this player can be disguised (not Spectator, Overwatch, SCP-079, CustomRole, Filmmaker, Destroyed)
            if (!Enum.IsDefined(typeof(RoleTypeId), targetPlayer.Role) || UndisguisableRoles.Contains(targetPlayer.Role))
            {
                response = $"The player has an undisguisable class at the moment!";
                return false;
            }

            // Applying disguise
            targetPlayer.AddFakeRole(classType, DisguiseChangeReason.AddByRAConsole, duration, teamMode);
            if (duration <= 1000002f) response = $"Done! ({targetId}) {targetPlayer.Nickname} is now disguised as ({classId}) {classType} for {duration} seconds!";
            else response = $"Done! ({targetId}) {targetPlayer.Nickname} is now disguised as ({classId}) {classType}!";
            return true;
        }

        private string GetUsage()
        {
            return "How to use the command: mask <player ID> <class ID> [optional: disguise time in seconds]\n" +
                   "Examples:\n" +
                   "mask 1 3 - mask player with ID 1 as SCP-106 (he has ClassID 3) \n" +
                   "mask 1 3 0 - mask player with ID 1 as SCP-106 (he has ClassID 3) and everyone (including player's teammates) will be fooled by the disguise\n" +
                   "mask 5 10 1 60 - mask player with ID 5 as Zombie (he has ClassID 10) for 60 seconds, teammates won't be fooled by the disguise\n\n";
        }

        private string GetClassList()
        {
            return "Classes IDs:\n" +
                   "0 - SCP-173\n" +
                   "1 - Class-D\n" +
                   "2 - Spectator (not possible to disguise / be disguised as)\n" +
                   "3 - SCP-106\n" +
                   "4 - NTF Specialist\n" +
                   "5 - SCP-049\n" +
                   "6 - Scientist\n" +
                   "7 - SCP-079 (not possible to disguise / be disguised as)\n" +
                   "8 - Chaos Conscript\n" +
                   "9 - SCP-096\n" +
                   "10 - SCP-049-2\n" +
                   "11 - NTF Sergeant\n" +
                   "12 - NTF Captain\n" +
                   "13 - NTF Private\n" +
                   "14 - Tutorial\n" +
                   "15 - Facility Guard\n" +
                   "16 - SCP-939\n" +
                   "17 - Role for modders (not possible to disguise / be disguised as)\n" +
                   "18 - Chaos Rifleman\n" +
                   "19 - Chaos Marauder\n" +
                   "20 - Chaos Repressor\n" +
                   "21 - Overwatch (not possible to disguise / be disguised as)\n" +
                   "22 - Filmmaker (not possible to disguise / be disguised as)\n" +
                   "23 - SCP-3114";
        }
    }
}