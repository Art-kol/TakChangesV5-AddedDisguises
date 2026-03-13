using MapGeneration;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// File for UI that shows more readable data
namespace TakChangesV5.ForEvents.DisguiseModule.UI
{
    public class Translations
    {
        public static Dictionary<RoleTypeId, string> GameRoles = new Dictionary<RoleTypeId, string>
        {
            { RoleTypeId.Tutorial, "Tutorial" },
            { RoleTypeId.ClassD, "Class-D" },
            { RoleTypeId.Scientist, "Scientist" },
            { RoleTypeId.FacilityGuard, "Facility Guard" },
            { RoleTypeId.Filmmaker, "Film Maker" },
            { RoleTypeId.Spectator, "Spectator" },
            { RoleTypeId.None, "None" },
            { RoleTypeId.Destroyed, "Destroyed" },
            { RoleTypeId.Overwatch, "Overwatch" },
            { RoleTypeId.NtfPrivate, "MTF Private" },
            { RoleTypeId.NtfSergeant, "MTF Sergeant" },
            { RoleTypeId.NtfSpecialist, "MTF Specialist" },
            { RoleTypeId.NtfCaptain, "MTF Captain" },
            { RoleTypeId.ChaosConscript, "CI Conscript" },
            { RoleTypeId.ChaosRifleman, "CI Rifleman" },
            { RoleTypeId.ChaosRepressor, "CI Repressor" },
            { RoleTypeId.ChaosMarauder, "CI Marauder" },
            { RoleTypeId.Scp049, "SCP-049" },
            { RoleTypeId.Scp0492, "SCP-049-2" },
            { RoleTypeId.Scp079, "SCP-079" },
            { RoleTypeId.Scp096, "SCP-096" },
            { RoleTypeId.Scp106, "SCP-106" },
            { RoleTypeId.Scp173, "SCP-173" },
            { RoleTypeId.Scp939, "SCP-939" },
            { RoleTypeId.Scp3114, "SCP-3114" },
            { RoleTypeId.Flamingo, "Flamingo" },
            { RoleTypeId.AlphaFlamingo, "Alpha Flamingo" },
            { RoleTypeId.ZombieFlamingo, "Zombie Flamingo" },
            { RoleTypeId.NtfFlamingo, "MTF Flamingo" },
            { RoleTypeId.ChaosFlamingo, "Chaos Flamingo" }
        };
    }
}
