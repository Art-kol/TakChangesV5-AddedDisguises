using CommandSystem;
using LabApi.Features.Wrappers;
using PlayerRoles;
using RemoteAdmin;
using System;
using System.Text;
using TakChangesV5.ForEvents.DisguiseModule.Extension;
using UnityEngine;

namespace TakChangesV5.ForEvents.DisguiseModule.RemoteAdmin
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    public class MaskListCommand : ICommand
    {
        public string Command => "masks";
        public string[] Aliases => new[] { "disguises" };
        public string Description => "Shows all active disguises";

        public bool Execute(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            // Permission check
            // #TAKAIL
            if (!sender.CheckPermission(PlayerPermissions.Noclip))
            {
                response = "YOU HAVE NO RIGHTS HERE!";
                return false;
            }

            var disguises = FakeRoleManager.FakeRoles;

            if (disguises == null || disguises.Count == 0)
            {
                response = "There are currently NO active disguises.";
                return true;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Active disguises: {disguises.Count}\n");

            foreach (var info in disguises)
            {
                Player player = Player.Get(info.Key);
                var fakeRole = info.Value;

                if (player == null)
                    continue;

                float remainingTime = fakeRole.EndTime >= 31536000f
                    ? -1
                    : fakeRole.EndTime - Time.time;

                string timeText = remainingTime < 0
                    ? "Infinite"
                    : $"{remainingTime:F1}s";

                sb.AppendLine(
                    $"ID: {player.PlayerId} | " +
                    $"Name: {player.Nickname} | " +
                    $"Real Role: {player.Role} | " +
                    $"Disguise: {fakeRole.Role} | " +
                    $"Fooling teammates?: {(fakeRole.TeamMode == 0? "YES" : "NO")} | " +
                    $"Time: {timeText}"
                );
            }

            response = sb.ToString();
            return true;
        }
    }
}
