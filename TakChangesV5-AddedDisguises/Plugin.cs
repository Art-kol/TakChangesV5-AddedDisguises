using InventorySystem.Items.Usables;
//using HarmonyLib;
using LabApi.Events.Handlers;
using LabApi.Features.Console;
using LabApi.Features.Stores;
using LabApi.Features.Wrappers;
using LabApi.Loader.Features.Plugins;
using MEC;
//using Newtonsoft.Json;
using PlayerRoles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
//using TakChangesV5.DataStore;
// using TakChangesV5.Events.PlayerHandlers;
//using TakChangesV5.Events.ScpHandlers;
// using TakChangesV5.Events.ServerHandlers;
//using TakChangesV5.Models;
using UserSettings.ServerSpecific;

namespace TakChangesV5
{
    public class TakChanges : Plugin<CustomConfig>
    {
        public static TakChanges Plugin = new();

        //private readonly Harmony _harmony = new("com.takchanges.patch");
        //public readonly List<BulletHolesDetails> BulletHoleLocations = [];

        //// SCP Swap/Switch Vars
        //public readonly List<SwapObject> DisconnectedPlayers = [];

        //// Global Vars
        //public readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };
        //public readonly StringBuilder LogBuffer = new();
        //public readonly List<SwapObject> QueuedSwaps = [];

        //// Misc Vars
        //public readonly List<string> RainbowTagList = [];
        //public readonly string[] RankColors = ["pink", "red", "light_green", "crimson", "cyan", "aqua", "deep_pink", "tomato", "yellow", "magenta", "blue_green", "orange", "lime", "green", "emerald", "carmine", "mint", "pumpkin"];

        //// Coroutines
        //public readonly List<CoroutineHandle> RoundLongCoroutines = [];
        //public readonly List<string> SwapEntrees = [];

        //// Logging Vars
        //public readonly List<WebhookObject> WebhookQueue = [];
        //public List<KeyValuePair<string, RoleTypeId>> ActiveSwitchRequests = [];
        //public List<Player> AfkReplacementPlayers = [];
        //public CoroutineHandle AfkRespawnCoroutineHandle;
        //public List<BaseGameKeycard>? BaseGameKeycards;
        //public PollModel? CurrentPoll = null;
        //public SwapObject? CurrentScpSwap = null;

        // Event Vars
        // public bool EventQueued = false;
        // public bool EventRound = false;

        //// Stats Vars
        //public KeyValuePair<string, TimeSpan> FirstEscape = new("", TimeSpan.Zero);
        //public string RoundId = DateTime.Now.Ticks.ToString();
        //public DateTime RoundStartTime;
        //public Player? Scp106 = null;
        //public bool SpawnWavesEnabled = true;

        ////Poll vars
        //public List<string> VotedPlayers = [];

        // Plugin Info
        public override string Name => "TakChanges";
        public override string Description => "Helpful commands and RASP integration.";
        public override string Author => "Takail & Shirosaka";
        public override Version Version { get; } = new(5, 0, 0, 0);
        public override Version RequiredApiVersion { get; } = new("1.1.4");

        //// Spectator Chat
        ////public List<ChatMessage> ChatMessages = [];

        public override void Enable()
        {
            // Init Stuff
            Plugin = this;
            //_harmony.PatchAll();
            Logger.Info($"Takchanges v{Version} enabled.");
            //CustomDataStoreManager.RegisterStore<PlayerDataStore>();
            Config ??= new CustomConfig();

            //ServerSpecificSettingsSync.DefinedSettings = [
            //    new SSGroupHeader("RedLine Settings", hint: "Settings for things on the RedLine server."),
            //    //new SSTwoButtonsSetting(001, "Enable Spectator Chat", "Enable", "Disable", hint: "Whether the spectator text chat should be enabled."),
            //    //new SSSliderSetting(002, "Chat Scale", 1, 50, 18, true, hint: "Changes the scale of the chat. (Default size is 18)"),
            //    new SSTwoButtonsSetting(001, "AFK Replace Opt-Out", "Opt-In", "Opt-Out", hint: "When opted out you will not be chosen as a replacement player when an MTF/Chaos is AFK."),
            //    new SSTwoButtonsSetting(002, "(Staff Only) Enable Staff Spectator Overlay", "Enable", "Disable", hint: "Only works for staff members.")
            //];

            //BaseGameKeycards = JsonConvert.DeserializeObject<List<BaseGameKeycard>>(File.ReadAllText(HelperMethods.ConfigFolder("basegamekeycards.json")));

            //// Server Events
            //ServerEvents.RoundEnded += ServerHandler.OnRoundEnded;
            //ServerEvents.RoundStarted += ServerHandler.OnRoundStart;
            //ServerEvents.RoundRestarted += ServerHandler.OnRestartingRound;
            //ServerEvents.WaveRespawned += ServerHandler.OnWaveRespawn;
            
            
            // ServerEvents.WaitingForPlayers += ServerHandler.OnWaitingForPlayers;
            
            
            //ServerEvents.CommandExecuted += ServerHandler.OnCommandExecuted;

            //// Player Events
            //PlayerEvents.ReportedPlayer += PlayerHandler.OnLocalReporting;
            //PlayerEvents.ReportedCheater += PlayerHandler.OnCheaterReporting;
            //PlayerEvents.Banned += PlayerHandler.OnBanned;
            //PlayerEvents.Kicked += PlayerHandler.OnKicked;
            
            
            // PlayerEvents.ChangedRole += PlayerHandler.OnPlayerChangeRole;
            
            
            //PlayerEvents.ChangedSpectator += PlayerHandler.OnPlayerChangeSpectator;
            //PlayerEvents.Cuffed += PlayerHandler.OnCuffed;
            //PlayerEvents.Uncuffed += PlayerHandler.OnUnCuffed;
            //PlayerEvents.Hurting += PlayerHandler.OnPlayerHurting;
            //PlayerEvents.Hurt += PlayerHandler.OnPlayerHurt;
            //PlayerEvents.Death += PlayerHandler.OnPlayerDeath;
            //PlayerEvents.Escaped += PlayerHandler.OnEscaped;
            //PlayerEvents.Joined += PlayerHandler.OnJoined;


            // PlayerEvents.Left += PlayerHandler.OnPlayerLeft;
            
            
            //PlayerEvents.Muted += PlayerHandler.OnMuted;
            //PlayerEvents.UsedItem += PlayerHandler.OnPlayerUsedItem;
            //PlayerEvents.PickedUpItem += PlayerHandler.PlayerEventsOnPickedUpItem;
            //PlayerEvents.PreAuthenticating += PlayerHandler.OnPreAuthenticating;
            //PlayerEvents.InteractedElevator += PlayerHandler.OnInteractedElevator;
            //PlayerEvents.PlacingBulletHole += PlayerHandler.OnPlacingBulletHole;
            //PlayerEvents.ShootingWeapon += PlayerHandler.OnPlayerShooting;
            //PlayerEvents.Jumped += PlayerHandler.OnPlayerJumped;

            //// Objective events
            //ObjectiveEvents.Completed += ObjectiveHandler.OnObjectiveCompleted;
            //// 914 Events
            //Scp914Events.ProcessedPlayer += Scp914Handler.OnProcessedPlayer;

            //// SSS Events
            ////ServerSpecificSettingsSync.ServerOnSettingValueReceived += ChatCore.Events.ChatSettingUpdated.SettingUpdated;
        }

        public override void Disable()
        {
            //// Init Stuff
            Plugin = this;
            //_harmony.UnpatchAll();
            Logger.Info($"Takchanges v{Version} disabled.");
            //CustomDataStoreManager.UnregisterStore<PlayerDataStore>();
            Config = new CustomConfig();

            //// Server Events
            //ServerEvents.RoundEnded -= ServerHandler.OnRoundEnded;
            //ServerEvents.RoundStarted -= ServerHandler.OnRoundStart;
            //ServerEvents.RoundRestarted -= ServerHandler.OnRestartingRound;
            //ServerEvents.WaveRespawned -= ServerHandler.OnWaveRespawn;
            
            
            // ServerEvents.WaitingForPlayers -= ServerHandler.OnWaitingForPlayers;
            
            
            //ServerEvents.CommandExecuted -= ServerHandler.OnCommandExecuted;

            //// Player Events
            //PlayerEvents.ReportedPlayer -= PlayerHandler.OnLocalReporting;
            //PlayerEvents.ReportedCheater -= PlayerHandler.OnCheaterReporting;
            //PlayerEvents.Banned -= PlayerHandler.OnBanned;
            //PlayerEvents.Kicked -= PlayerHandler.OnKicked;
            
            
            //PlayerEvents.ChangedRole -= PlayerHandler.OnPlayerChangeRole;
            
            
            //PlayerEvents.ChangedSpectator -= PlayerHandler.OnPlayerChangeSpectator;
            //PlayerEvents.Cuffed -= PlayerHandler.OnCuffed;
            //PlayerEvents.Uncuffed -= PlayerHandler.OnUnCuffed;
            //PlayerEvents.Hurting -= PlayerHandler.OnPlayerHurting;
            //PlayerEvents.Hurt -= PlayerHandler.OnPlayerHurt;
            //PlayerEvents.Death -= PlayerHandler.OnPlayerDeath;
            //PlayerEvents.Escaped -= PlayerHandler.OnEscaped;
            //PlayerEvents.Joined -= PlayerHandler.OnJoined;
            
            
            //PlayerEvents.Left -= PlayerHandler.OnPlayerLeft;
            
            
            //PlayerEvents.Muted -= PlayerHandler.OnMuted;
            //PlayerEvents.UsedItem -= PlayerHandler.OnPlayerUsedItem;
            //PlayerEvents.PickedUpItem -= PlayerHandler.PlayerEventsOnPickedUpItem;
            //PlayerEvents.PreAuthenticating -= PlayerHandler.OnPreAuthenticating;
            //PlayerEvents.InteractedElevator -= PlayerHandler.OnInteractedElevator;
            //PlayerEvents.PlacingBulletHole -= PlayerHandler.OnPlacingBulletHole;
            //PlayerEvents.ShootingWeapon -= PlayerHandler.OnPlayerShooting;
            //PlayerEvents.Jumped -= PlayerHandler.OnPlayerJumped;

            //// 914 Events
            //Scp914Events.ProcessedPlayer -= Scp914Handler.OnProcessedPlayer;
        }

    }
}
