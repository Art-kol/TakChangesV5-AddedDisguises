using CommandSystem;
using LabApi.Features.Wrappers;
using RemoteAdmin;
using System;
using TakChangesV5.ForEvents.DisguiseModule.Extension;
using UnityEngine.Rendering;
using static Mono.Security.X509.X520;
using static TakChangesV5.ForEvents.DisguiseModule.Extension.FakeRoleManager;
using Logger = LabApi.Features.Console.Logger;

namespace TakChangesV5.ForEvents.DisguiseModule.RemoteAdmin
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
            Player senderPlayer = Player.Get(playerSender.ReferenceHub);
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

            string arg = arguments.At(0).ToLower();
            
            // Unmask all
            if (arg == "all" || arg == "*")
            {
                //Logger.Info($"Unmask signal for all detected!");
                if (!sender.CheckPermission(PlayerPermissions.Noclip))
                {
                    response = "YOU HAVE NO RIGHTS HERE!";
                    return false;
                }

                RemoveFakeRolesForAll(DisguiseChangeReason.RemovedByRAConsole);
                response = $"Done! All players undisguised!";
                return true;
            }

            // Player ID parse
            if (!int.TryParse(arg, out int targetId))
            {
                response = $"Incorrect player ID: {arg}\n\n";
                return false;
            }

            // Null target player
            Player targetPlayer = Player.Get(targetId);
            if (targetPlayer == null)
            {
                response = $"Player with ID: ({targetId}) not found!";
                return false;
            }

            // Applying undisguise
            targetPlayer.RemoveFakeRole(DisguiseChangeReason.RemovedByRAConsole);
            response = $"Done! ({targetId}) {targetPlayer.Nickname} is now undisguised!";

            return true;
        }

        private string GetUsage()
        {
            return "How to use the command: unmask <player ID> \n" +
                   "Examples:\n" +
                   "unmask 1 - unmask player with ID 1 \n" +
                   "unmask * - unmask everyone (also works with 'all')\n\n";
        }
    }
}