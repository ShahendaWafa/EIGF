using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
public class GameOver : MonoBehaviour
{
    
    [SerializeField] TMP_Text winner;
    void Start()
    {
        winner.text = Winner.winnerName;
    }
}
