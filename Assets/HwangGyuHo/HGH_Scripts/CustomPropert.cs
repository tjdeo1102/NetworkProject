using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using PhotonHashtable = ExitGames.Client.Photon.Hashtable;

public static class CustomPropert
{
    public const string READY = "Ready";

    private static PhotonHashtable customProperty = new PhotonHashtable();
    // 확장메소드
    public static void SetReady(this Player player, bool ready)
    {
        customProperty.Clear();
        customProperty[READY] = ready;
        player.SetCustomProperties(customProperty);
    }

    // 가져오는 방법
    public static bool GetReady(this Player player)
    {
        PhotonHashtable customProperty = player.CustomProperties;
        
        if (customProperty.ContainsKey(READY))
        {
            return (bool)customProperty[READY];
        }
        else
        {
            return false;
        }

    }
}
