using UnityEngine;
using HarmonyLib;
using UnboundLib;
using UnboundLib.Networking;
using Photon.Pun;
using UnityEngine.Assertions;
using System.Linq;
using UnboundLib.Utils;
using HiddenCards;

[HarmonyPatch(typeof(CardChoice))]
public class UniqueCardChoicePatch
{
    [HarmonyPatch("SpawnUniqueCard")]
    [HarmonyPostfix]
    public static void SpawnUniqueCard_Patch(ref GameObject __result, int ___pickrID, PickerType ___pickerType, Vector3 pos, Quaternion rot)
    {
        // get currently picking player
        Player player;
        if (___pickerType == PickerType.Team)
        {
            player = PlayerManager.instance.GetPlayersInTeam(___pickrID)[0];
        }
        else
        {
            player = PlayerManager.instance.players[___pickrID];
        }
        var hide = true;
        foreach(var p in PlayerManager.instance.players.Where((p) => p.data.view.IsMine)) if (p.teamID == player.teamID) hide = false;
        if(hide && HC.tempEnable)
        {
            __result.transform.GetChild(0).GetChild(0).GetChild(0).gameObject.SetActive(false);
            __result.transform.GetChild(0).GetChild(0).GetChild(1).GetComponent<CanvasGroup>().enabled = false;
        }
    }
}