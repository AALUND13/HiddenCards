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

namespace HiddenCards
{
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class HC : BaseUnityPlugin
    {
        private const string ModId = "com.ancientkoala.rounds.hiddencards";
        private const string ModName = "Hidden Cards";
        public const string Version = "0.0.1";
        public const string ModInitials = "HC";

        public static HC instance;

        internal static AssetBundle ArtAssets;

        private bool gameGoing = false;

        public static ConfigEntry<bool> enabled;
        public static bool tempEnable = true;

        private void EnableCardsAction(bool val)
        {
            enabled.Value = val;
        }
        private void NewGUI(GameObject menu)
        {
            MenuHandler.CreateToggle(enabled.Value, "Hidden Mode", menu, EnableCardsAction);
        }

        void Start()
        {
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
            instance = this;

            Unbound.RegisterMenu("Hidden Cards", () => { }, this.NewGUI, null);
            enabled = base.Config.Bind<bool>("HC", "enabled hidden", true);

            GameModeManager.AddHook(GameModeHooks.HookGameStart, GameStart);
            GameModeManager.AddHook(GameModeHooks.HookGameEnd, GameEnd);
        }
        void Update()
        {
            if (enabled.Value)
            {
                if (gameGoing)
                {
                    foreach (var p in PlayerManager.instance.players)
                    {
                        if (!p.data.view.IsMine) ModdingUtils.Utils.CardBarUtils.instance.ClearCardBar(p);
                    }
                }
            }
            
        }
        IEnumerator GameStart(IGameModeHandler gm)
        {
            gameGoing = true;
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC_Others(typeof(HC), nameof(RPC_SyncSettings), enabled.Value);
                tempEnable = enabled.Value;
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