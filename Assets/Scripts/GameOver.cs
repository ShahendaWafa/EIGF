using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    
    [SerializeField] TMP_Text winner;
    void Start()
    {
        winner.text +=  Winner.winnerName;
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");

    }
}
