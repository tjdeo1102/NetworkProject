using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ResultScoreEntry : MonoBehaviour
{
    [SerializeField] TMP_Text playerName;
    [SerializeField] TMP_Text blockScore;

    public void SetEntry(string name, int score)
    {
        playerName?.SetText(name);
        blockScore?.SetText(score.ToString());
    }
}
