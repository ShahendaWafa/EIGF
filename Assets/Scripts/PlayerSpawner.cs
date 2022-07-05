using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] Transform[] spawnPoints;

    private void Start()
    {
        Vector3 spawnPos = spawnPoints[(PhotonNetwork.LocalPlayer.ActorNumber - 1)].position;
        PhotonNetwork.Instantiate("Hoverboard", spawnPos, Quaternion.identity);

    }
}
