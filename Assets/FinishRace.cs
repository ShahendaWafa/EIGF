using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class FinishRace : MonoBehaviour
{
    [SerializeField] TMP_Text winnerName;
    private void OnTriggerEnter(Collider other)
    {
        winnerName.text = other.name;
        SceneManager.LoadScene("GameOver");
    }
}
