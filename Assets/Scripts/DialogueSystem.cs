using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{

    public GameObject[] dialogue;
    public GameObject[] dialogueCont;
    public int dialogueCounter = -1;
    public bool active = false;

    public void OnTriggerEnter2D()
    {
        active = true;
    }

    public void OnTriggerExit2D()
    {
        active = false;
        if (-1 < dialogueCounter && dialogueCounter < dialogue.Length)dialogue[dialogueCounter].SetActive(false);
        dialogueCounter = -1;
    }

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            //Close last dialogue box making sure that it actually exists
            if (dialogueCounter > -1) dialogue[dialogueCounter].SetActive(false);

            dialogueCounter += 1;

            //Open the next dialogue box making sure that it actually exists
            if (dialogue.Length != dialogueCounter) dialogue[dialogueCounter].SetActive(true);
            else dialogueCounter = -1;
        }
    }
}
