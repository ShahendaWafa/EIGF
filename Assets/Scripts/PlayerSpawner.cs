using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;
    [SerializeField] GameObject startCutscene;
    [SerializeField] GameObject endCutscene;

    private void Start()
    {
        if (Winner.gameWon)
        {
            endCutscene.SetActive(true);
            StartCoroutine(EndScene());
        }
        else
        { 
            startCutscene.SetActive(true);
            StartCoroutine(StartScene());
        }
        

    }

    void SpawnPlayers()
    {
        Vector3 spawnPos = spawnPoints[(PhotonNetwork.LocalPlayer.ActorNumber - 1)].position;
        PhotonNetwork.Instantiate("Hoverboard", spawnPos, Quaternion.identity);
    }

    IEnumerator StartScene()
    {
        yield return new WaitForSeconds(9.0f);
        startCutscene.SetActive(false);
        SpawnPlayers();
    }
    IEnumerator EndScene()
    {
        yield return new WaitForSeconds(5.0f);
        PhotonNetwork.LoadLevel("GameOver");

    }


}
