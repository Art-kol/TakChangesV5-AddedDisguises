using System;
using CommandSystem;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using static TakChangesV5.DisguiseModule.Extension.FakeRoleManager;
using static TakChangesV5.ForEvents.DisguiseModule.UI.UIHelpingStaticData;

namespace TakChangesV5.DisguiseModule.Commands.RemoteAdmin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class UnmaskCommand : ICommand, IUsageProvider
    {
        public string Command => "unmask";
        public string[] Aliases => new[] { "undisguise"};
        public string Description => "Removes player's disguises";
        public string[] Usage { get; } = { "player ID/* if all" };

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (sender is not PlayerCommandSender playerSender)
            {
                response = "This command cannot be executed if the command sender isnt on the server!";
                return false;
            }

            // #TAKAIL
            if (!sender.CheckPermission(PlayerPermissions.Noclip))
            {
                response = "YOU HAVE NO RIGHTS HERE!";
                return false;
            }

            // Null Command Sender
            var senderPlayer = Player.Get(playerSender?.ReferenceHub);
            if (senderPlayer == null)
            {
                response = "Command sender not recognised (= null)!";
                return false;
            }

            // Not enough arguments
            if (arguments.Count < 1)
            {
                response = $"Incorrect format!\n\n" + GetUsage();
                return false;
            }

            var arg = arguments.At(0).ToLower();
            
            // Unmask all
            if (arg is "all" or "*")
            {
                //Logger.Info($"Unmask signal for all detected!");
                if (!sender.CheckPermission(PlayerPermissions.Noclip))
                {
                    response = "YOU HAVE NO RIGHTS HERE!";
                    return false;
                }

                RemoveFakeRolesForAll(DisguiseChangeReason.REMOVED_BY_RA_CONSOLE);
                response = $"Done! All players undisguised!";
                return true;
            }

            // Player ID parse
            if (!int.TryParse(arg, out var targetId))
            {
                response = $"Incorrect player ID: {arg}\n\n";
                return false;
            }

            // Null target player
            var targetPlayer = Player.Get(targetId);
            if (targetPlayer == null)
            {
                response = $"Player with ID: ({targetId}) not found!";
                return false;
            }

            // Applying undisguise
            targetPlayer.RemoveFakeRole(DisguiseChangeReason.REMOVED_BY_RA_CONSOLE);
            response = $"Done! ({targetId}) {targetPlayer.Nickname} is now undisguised!";

            return true;
        }

        private static string GetUsage()
        {
            return "How to use the command: unmask <player ID> \n" +
                   "Examples:\n" +
                   "unmask 1 - unmask player with ID 1 \n" +
                   "unmask * - unmask everyone (also works with 'all')\n\n";
        }
    }
}