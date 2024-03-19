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
        player.GetComponent<PlayerMovementNew>().Flip();
        player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y - 4); ;
    }
}
