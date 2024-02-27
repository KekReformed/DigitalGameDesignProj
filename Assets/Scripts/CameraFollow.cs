using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{

    public GameObject player;

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(Round(player.transform.position.x),Round(player.transform.position.y+1f),transform.position.z);
    }


    //I think this helps with pixel perfectness but im really not sure ngl
    float Round(float numToRound)
    {
        numToRound *= 100;
        int numToRoundInt = (int) numToRound;
        numToRound = numToRoundInt;
        return numToRound / 100;
    }
}
