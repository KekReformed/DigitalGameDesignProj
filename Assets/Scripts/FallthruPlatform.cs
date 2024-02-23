using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FallthruPlatform : MonoBehaviour
{


    [SerializeField] private float platformLockoutDuration;
    private TilemapCollider2D collider;
    private GameObject player;
    private Rigidbody2D playerBody;
    private bool playerInPlatform;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<TilemapCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player");
        playerBody = player.GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Jump") && Input.GetAxisRaw("Vertical") < 0) {
            collider.enabled = false;
            StartCoroutine("enableCollider");
        }
    }

    private IEnumerator enableCollider (){
        yield return new WaitForSeconds(platformLockoutDuration);
        collider.enabled = true;
    }
}
