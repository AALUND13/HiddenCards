using BepInEx;
using HarmonyLib;
using System.Collections;
using UnboundLib.GameModes;
using UnityEngine;
using UnboundLib;
using BepInEx.Configuration;
using UnboundLib.Utils.UI;
using Photon.Pun;
using UnboundLib.Networking;
using System.Linq;

namespace HiddenCards
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class HC : BaseUnityPlugin
    {
        private const string ModId = "com.ancientkoala.rounds.hiddencards";
        private const string ModName = "Hidden Cards";
        public const string Version = "0.0.2";
        public const string ModInitials = "HC";

        public static HC instance;

        private bool gameGoing = false;

        public static ConfigEntry<bool> hiddenOn;
        public static bool tempEnable = true;

        private void EnableCardsAction(bool val)
        {
            hiddenOn.Value = val;
        }
        private void NewGUI(GameObject menu)
        {
            MenuHandler.CreateToggle(hiddenOn.Value, "Hidden Mode", menu, EnableCardsAction);
        }

        void Start()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
            instance = this;

            Unbound.RegisterMenu("Hidden Cards", () => { }, this.NewGUI, null);
            hiddenOn = base.Config.Bind<bool>("HC", "enabled hidden", true);

            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameStart);
            GameModeManager.AddHook(GameModeHooks.HookGameEnd, GameEnd);
        }
        void Update()
        {
            if (HC.tempEnable && gameGoing)
            {
                int myTeamId = PlayerManager.instance.players.Where((p) => p.data.view.IsMine).First().teamID;
                foreach (var p in PlayerManager.instance.players)
                {
                    if(p.teamID != myTeamId)
                    {
                        ModdingUtils.Utils.CardBarUtils.instance.PlayersCardBar(p.playerID).ClearBar();
                    }
                }
            }
            
        }
        IEnumerator GameStart(IGameModeHandler gm)
        {
            gameGoing = true;
            if (PhotonNetwork.IsMasterClient)
            {
                tempEnable = hiddenOn.Value;
                NetworkingManager.RPC_Others(typeof(HC), nameof(RPC_SyncSettings), hiddenOn.Value);
            }
            yield return null;
        }
        IEnumerator GameEnd(IGameModeHandler gm)
        {
            gameGoing = false;
            yield return null;
        }
        [UnboundRPC]
        private static void RPC_SyncSettings(bool isEnabled)
        {
            tempEnable = isEnabled;
        }
    }

}