using UnityEngine;
using HarmonyLib;
using System.Linq;
using HiddenCards;
using UnboundLib.Networking;
using Photon.Pun;
using UnboundLib;

[HarmonyPatch(typeof(CardChoice))]
public class UniqueCardChoicePatch
{
    [HarmonyPatch("SpawnUniqueCard")]
    [HarmonyPostfix]
    public static void SpawnUniqueCard_Patch(ref GameObject __result, int ___pickrID, PickerType ___pickerType, Vector3 pos, Quaternion rot)
    {
        Player player;
        if (___pickerType == PickerType.Team)
        {
            player = PlayerManager.instance.GetPlayersInTeam(___pickrID)[0];
        }
        else
        {
            player = PlayerManager.instance.players[___pickrID];
        }
        if(HC.tempEnable)
        {
            NetworkingManager.RPC(typeof(UniqueCardChoicePatch), nameof(RPC_HideHiddenCards), __result.GetComponent<PhotonView>().ViewID, ___pickrID);
        }
    }

    [UnboundRPC]
    public static void RPC_HideHiddenCards(int targetCardID, int pickID)
    {
        var card = PhotonNetwork.GetPhotonView(targetCardID).gameObject;
        Player me = PlayerManager.instance.players.Where((p) => p.data.view.IsMine).First();
        Player picker = PlayerManager.instance.players.Where((p) => p.playerID == pickID).First();
        if (me.teamID != picker.teamID) 
        {
            // CardInfoDisplayer should be every "CardBase" object
            card.transform.GetComponentInChildren<CardInfoDisplayer>().transform.GetChild(0).GetChild(0).gameObject.SetActive(false);
            card.transform.GetComponentInChildren<CardInfoDisplayer>().transform.GetChild(0).GetChild(1).GetComponent<CanvasGroup>().enabled = false;
        }
    }
}