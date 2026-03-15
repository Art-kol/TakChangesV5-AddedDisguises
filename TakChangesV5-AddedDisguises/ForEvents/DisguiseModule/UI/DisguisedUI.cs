using System;
using LabApi.Features.Wrappers;
using PlayerRoles;
using RueI.API;
using RueI.API.Elements;
using RueI.API.Elements.Enums;
using UnityEngine;
using static TakChangesV5.DisguiseModule.Extension.FakeRoleManager;
using static TakChangesV5.ForEvents.DisguiseModule.UI.UIHelpingStaticData;

// This file manages the players' UIs when disguised
// It uses RueI dependency which is a plugin that shows multiple UI elements

namespace TakChangesV5.DisguiseModule.UI {
    public static class DisguisedUI {
        private static readonly Tag _fakeRoleNameTag = new("hud_fakerolename");
        private static readonly Tag _fakeRoleDurationTag = new("hud_fakeroleduration");
        private static readonly Tag _announceHintShowDisguiseTag = new("hud_announcehintshowdisguise");

        public static void RefreshDisguiseUI(Player player, DisguiseChangeReason reason) {
            
            if (FakeRoles.TryGetValue(player.ReferenceHub, out var fakeRole)) {
                ShowDisguise(player, fakeRole, reason);

                if (fakeRole.DisguiseDuration > 0)
                {
                    ShowDisguiseTimer(player, fakeRole, reason);
                }

            }
            else {
                if (reason != DisguiseChangeReason.NONE) {
                    AnnounceHintOrBroadcast(player, fakeRole, reason);
                }

                RueDisplay.Get(player).Remove(_fakeRoleDurationTag);
                RueDisplay.Get(player).Remove(_fakeRoleNameTag);
            }
        }

        private static void ShowDisguise(Player player, FakeRole fakeRole, DisguiseChangeReason reason) {
            var text = BuildFakeRoleInfo(player, fakeRole);

            var infoElement = new BasicElement(50 /*YCoord*/, text) {
                ZIndex = 7,
                VerticalAlign = VerticalAlign.Down
            };

            RueDisplay.Get(player).Show(_fakeRoleNameTag, infoElement);
            if (reason != DisguiseChangeReason.NONE) {
                AnnounceHintOrBroadcast(player, fakeRole, reason);
            }
        }

        private static void ShowDisguiseTimer(Player player, FakeRole fakeRole, DisguiseChangeReason reason)
        {
            var formattedTime = FormatTime(fakeRole.DisguiseDuration);
            var timerColor = GetTimerColor(fakeRole.DisguiseDuration);
            var text = BuildFakeRoleDuration(timerColor, formattedTime);

            var timerElement = new BasicElement(50, text) {
                ZIndex = 7,
                VerticalAlign = VerticalAlign.Down
            };

            RueDisplay.Get(player).Remove(_fakeRoleDurationTag);
            RueDisplay.Get(player).Show(_fakeRoleDurationTag, timerElement);
        }


        // =============== BUILDERS ===============
        private static string BuildFakeRoleInfo(Player player, FakeRole fakeRole) {
            if (!fakeRole.Role.TryGetRoleTemplate(out PlayerRoleBase fakeRoleBase)) {
                return string.Empty;
            }

            var fakeRoleColor = "#" + ColorUtility.ToHtmlStringRGB(fakeRoleBase.RoleColor);
            var fakeRoleName = GameRoles[fakeRole.Role];


            return $"<size=25><space=-900><color=white><b>🎭 Disguise</b></color> \n <space=-900><color={fakeRoleColor}>{fakeRoleName}</color></size>";
        }

        private static string BuildFakeRoleDuration(string timerColor, string formattedTime) => $"<size=25><color=white><b>⏰ Duration</b></color> \n <color={timerColor}>{formattedTime}</color></size>";


        // BUILDERS for announcing:
        private static void AnnounceHintOrBroadcast(Player player, FakeRole fakeRole, DisguiseChangeReason reason)
        {
            switch (reason)
            {
                case DisguiseChangeReason.NONE:
                    return;
                case DisguiseChangeReason.REMOVED_BY_RA_CONSOLE:
                    player.SendBroadcast(BuildAnnounceBroadcast(reason), 5, Broadcast.BroadcastFlags.Normal, true);
                    return;
                case DisguiseChangeReason.ADD_BY_RA_CONSOLE:
                case DisguiseChangeReason.REMOVED_BY_TIMER:
                default:
                    break;
            }

            var announceText = BuildAnnounceHint(fakeRole, reason);
            var announceElement = new BasicElement(350, announceText) { ZIndex = 7 };

            switch (reason)
            {
                case DisguiseChangeReason.ADD_BY_RA_CONSOLE:
                    RueDisplay.Get(player).Show(_announceHintShowDisguiseTag, announceElement, Math.Min(10f, Math.Abs(fakeRole.DisguiseDuration)));
                    break;
                case DisguiseChangeReason.REMOVED_BY_TIMER:
                    RueDisplay.Get(player).Show(_announceHintShowDisguiseTag, announceElement, 5f);
                    break;
            }
        }

        private static string BuildAnnounceHint(FakeRole fakeRole, DisguiseChangeReason reason) {
            var result = string.Empty;
            switch (reason) {
                case DisguiseChangeReason.ADD_BY_RA_CONSOLE: {
                    if (!fakeRole.Role.TryGetRoleTemplate(out PlayerRoleBase fakeRoleBase)) {
                        return string.Empty;
                    }

                    var fakeRoleColor = "#" + ColorUtility.ToHtmlStringRGB(fakeRoleBase.RoleColor);
                    var fakeRoleName = GameRoles[fakeRole.Role];

                    result = $"<size=30><color=white>You have been disguised as <color={fakeRoleColor}>{fakeRoleName}</color></color>";
                    if (fakeRole.DisguiseDuration > 0) {
                        result += $" <color=white>for <color={GetTimerColor(fakeRole.DisguiseDuration)}>{FormatTime(fakeRole.DisguiseDuration)}</color></color>!";
                    }
                    else {
                        result += $"!";
                    }

                    if (fakeRole.TeamMode == 0) {
                        result += "<color=white>\n Your teammates <color=yellow>ARE FOOLED BY YOUR DISGUISE</color>.</color></size>";
                    }
                    else {
                        result += "<color=white>\n Your teammates are not fooled by your disguise.</color></size>";
                    }

                    break;
                }
                case DisguiseChangeReason.REMOVED_BY_RA_CONSOLE:
                    result = $"<size=30><color=white>Your disguise has been spontaneously worn off. \n Now <color=yellow>everyone sees your true role!</color></color></size>";
                    break;
                case DisguiseChangeReason.REMOVED_BY_TIMER:
                    result = $"<size=30><color=white>Your disguise is worn off. \n Now <color=yellow>everyone sees your true role!</color></color></size>";
                    break;
                case DisguiseChangeReason.NONE:
                default:
                    break;
            }

            return result;
        }

        private static string BuildAnnounceBroadcast(DisguiseChangeReason reason) {
            var result = string.Empty;
            // !!!!!!! NEEDS TO BE FIXED LATER. I CANT FIGURE OUT HOW TO DESTROY DYNAMIC ELEMENT AFTER RUNNING
            if (reason == DisguiseChangeReason.REMOVED_BY_RA_CONSOLE) {
                result = $"<size=40><color=white>Your disguise has been spontaneously worn off. \n Now <color=yellow>everyone sees your true role!</color></color></size>";
            }

            return result;
        }


        // ============= HELPERS ===============
        private static string FormatTime(float seconds) {
            if (seconds <= 0) {
                return "00:00";
            }

            var time = TimeSpan.FromSeconds(seconds);

            return time.TotalHours >= 1 ? $"{(int)time.TotalHours:D2}:{time.Minutes:D2}:{time.Seconds:D2}" : $"{time.Minutes:D2}:{time.Seconds:D2}";
        }

        private static string GetTimerColor(float timeLeft) {
            return timeLeft switch {
                > 15 => "#50C878",
                > 5 => "#FFD700",
                _ => "#D70040"
            };
        }
    }
}