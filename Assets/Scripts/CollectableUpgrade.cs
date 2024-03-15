using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CollectableUpgrade : MonoBehaviour
{
    [SerializeField] string upgradeName;
    [SerializeField] int upgradeIndex;
    void OnTriggerEnter2D(Collider2D player)
    {
        if (!player.gameObject.CompareTag("Player")) return;
        player.AddComponent<Upgrade>().createUpgrade(upgradeName, upgradeIndex);
    }
}
