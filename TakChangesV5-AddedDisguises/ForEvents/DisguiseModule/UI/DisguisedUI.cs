using Christmas.Scp2536.Gifts;
using InventorySystem.Items.ToggleableLights.Flashlight;
using LabApi.Features.Wrappers;
using PlayerRoles;
using RemoteAdmin.Communication;
using RueI.API;
using RueI.API.Elements;
using RueI.API.Elements.Enums;
using RueI.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TakChangesV5.ForEvents.DisguiseModule.Extension;
using UnityEngine;
using static TakChangesV5.ForEvents.DisguiseModule.Extension.FakeRoleManager;

// This file manages the players' UIs when disguised
// It uses RueI dependency which is a plugin that shows multiple UI elements

namespace TakChangesV5.ForEvents.DisguiseModule.UI
{
    public static class DisguisedUI
    {
        private static readonly Tag FakeRoleNameTag = new("hud_fakerolename");
        private static readonly Tag FakeRoleDurationTag = new("hud_fakeroleduration");
        private static readonly Tag AnnounceHintShowDisguiseTag = new("hud_announcehintshowdisguise");

        public static void RefreshDisguiseUI(Player p, DisguiseChangeReason reason)
        {
            if (p == null) return;


            if (FakeRoles.TryGetValue(p.ReferenceHub, out FakeRole fakeRole) && (fakeRole.EndTime - Time.time > 0))
                ShowDisguise(p, fakeRole, reason);
            else
            {
                if (reason != DisguiseChangeReason.None) AnounceHintOrBroadcast(p, fakeRole, reason);

                RueDisplay.Get(p).Remove(FakeRoleDurationTag);
                RueDisplay.Get(p).Remove(FakeRoleNameTag);
            }
        }

        private static void ShowDisguise(Player player, FakeRole fakeRole, DisguiseChangeReason reason)
        {
            string text = BuildFakeRoleInfo(player, fakeRole);

            var infoElement = new BasicElement(50 /*YCoord*/, text)
            {
                ZIndex = 7,
                VerticalAlign = VerticalAlign.Down
            };

            var timerElement = new DynamicElement(
                position: 50 /*YCoord*/,
                contentGetter: rh =>
                {
                    var p = Player.Get(rh);

                    if (p == null) return string.Empty;

                    if (!FakeRoles.TryGetValue(p.ReferenceHub, out FakeRole currentFakeRole))
                    {
                        RueDisplay.Get(p).Remove(FakeRoleDurationTag);
                        return string.Empty;
                    }

                    float currentTimeLeft = currentFakeRole.EndTime - Time.time;

                    if (currentTimeLeft <= 0)
                    {
                        RefreshDisguiseUI(p, DisguiseChangeReason.RemovedByTimer);
                        return string.Empty;
                    }

                    if (currentFakeRole.EndTime >= 31536000f)
                        return string.Empty;

                    string formattedTime = FormatTime(currentTimeLeft);
                    string timerColor = GetTimerColor(currentTimeLeft);

                    return BuildFakeRoleDuration(timerColor, formattedTime);
                })
            {
                ZIndex = 7,
                VerticalAlign = VerticalAlign.Down,
                UpdateInterval = TimeSpan.FromSeconds(1)
            };

            RueDisplay.Get(player).Show(FakeRoleNameTag, infoElement);
            RueDisplay.Get(player).Show(FakeRoleDurationTag, timerElement);
            if (reason != DisguiseChangeReason.None) AnounceHintOrBroadcast(player, fakeRole, reason);
        }


        private static void AnounceHintOrBroadcast(Player player, FakeRole fakeRole, DisguiseChangeReason reason)
        {
            if (reason == DisguiseChangeReason.None) return;
            if (reason == DisguiseChangeReason.RemovedByRAConsole)
            {
                player.SendBroadcast(BuildAnnounceBroadcast(player, fakeRole, reason), 5, Broadcast.BroadcastFlags.Normal, true);
                return;
            }

            string announceText = BuildAnnounceHint(player, fakeRole, reason);
            var announceElement = new BasicElement(350, announceText)
            {
                ZIndex = 7
            };

            if (reason == DisguiseChangeReason.AddByRAConsole) RueDisplay.Get(player).Show(AnnounceHintShowDisguiseTag, announceElement, Math.Min(10f, fakeRole.EndTime - Time.time));
            if (reason == DisguiseChangeReason.RemovedByTimer) RueDisplay.Get(player).Show(AnnounceHintShowDisguiseTag, announceElement, 5f);
        }



        // =============== BUILDERS ===============
        private static string BuildFakeRoleInfo(Player player, FakeRole fakeRole)
        {
            if (!fakeRole.Role.TryGetRoleTemplate(out PlayerRoleBase fakeRoleBase))
                return string.Empty;

            string fakeRoleColor = "#" + ColorUtility.ToHtmlStringRGB(fakeRoleBase.RoleColor);
            string fakeRoleName = Translations.GameRoles[fakeRole.Role];


            return $"<size=25><space=-900><color=white><b>🎭 Disguise</b></color> \n <space=-900><color={fakeRoleColor}>{fakeRoleName}</color></size>";
        }

        private static string BuildFakeRoleDuration(string timerColor, string formattedTime) =>
            $"<size=25><color=white><b>⏰ Duration</b></color> \n <color={timerColor}>{formattedTime}</color></size>";

        private static string BuildAnnounceHint(Player player, FakeRole fakeRole, DisguiseChangeReason reason)
        {
            string result = string.Empty;
            if (reason == DisguiseChangeReason.AddByRAConsole)
            {
                if (!fakeRole.Role.TryGetRoleTemplate(out PlayerRoleBase fakeRoleBase))
                    return string.Empty;
                string fakeRoleColor = "#" + ColorUtility.ToHtmlStringRGB(fakeRoleBase.RoleColor);
                string fakeRoleName = Translations.GameRoles[fakeRole.Role];

                result = $"<size=30><color=white>You have been disguised as <color={fakeRoleColor}>{fakeRoleName}</color></color>";
                if (fakeRole.EndTime <= 1000002f) result += $" <color=white>for <color={GetTimerColor(fakeRole.EndTime - Time.time)}>{FormatTime(fakeRole.EndTime - Time.time)}</color></color>!";
                else result += $"!";
                if (fakeRole.TeamMode == 0) result += "<color=white>\n Your teammates <color=yellow>ARE FOOLED BY YOUR DISGUISE</color>.</color></size>";
                else result += "<color=white>\n Your teammates are not fooled by your disguise.</color></size>";
            }
            else if (reason == DisguiseChangeReason.RemovedByRAConsole)
            {
                result = $"<size=30><color=white>Your disguise has been spontaneously worn off. \n Now <color=yellow>everyone sees your true role!</color></color></size>";
            }
            else if (reason == DisguiseChangeReason.RemovedByTimer)
            {
                result = $"<size=30><color=white>Your disguise is worn off. \n Now <color=yellow>everyone sees your true role!</color></color></size>";
            }

            return result;
        }

        private static string BuildAnnounceBroadcast(Player player, FakeRole fakeRole, DisguiseChangeReason reason)
        {
            string result = string.Empty;
            // !!!!!!! NEEDS TO BE FIXED LATER. I CANT FIGURE OUT HOW TO DESTROY DYNAMIC ELEMENT AFTER RUNNING
            if (reason == DisguiseChangeReason.RemovedByRAConsole) result = $"<size=40><color=white>Your disguise has been spontaneously worn off. \n Now <color=yellow>everyone sees your true role!</color></color></size>";

            return result;
        }



        // ============= HELPERS ===============
        private static string FormatTime(float seconds)
        {
            if (seconds <= 0)
                return "00:00";

            TimeSpan time = TimeSpan.FromSeconds(seconds);

            if (time.TotalHours >= 1)
                return $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}";
            else
                return $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private static string GetTimerColor(float timeLeft)
        {
            if (timeLeft > 15)
                return "#50C878";
            else if (timeLeft > 5)
                return "#FFD700";
            else
                return "#D70040";
        }
    }
}
