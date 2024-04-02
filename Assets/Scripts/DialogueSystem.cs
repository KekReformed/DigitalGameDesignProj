using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    [Header("Dialogue")]
    [Space(10)]
    public GameObject[] dialogue;
    public GameObject[] dialogueCont;
    public GameObject[] endDialogue;
    private int dialogueCounter = -1;
    private int talkedToCount = 0;
    public bool faceLeft;

    [Header("Fade Effect")]
    [Space(10)]
    [Range(0, 1)] public float fade;
    public DarknessFade darkness;
    private bool active = false;
    private bool movePlayer = false;

    [Header("Camera and Player")]
    [Space(10)]
    public GameObject virtualCamera;
    public GameObject objectToMoveTo;
    public GameObject circle;
    public GameObject vision;

    public PlayerMovementNew playerScript;

    private Vector2 positionToMoveTo;

    public void Start()
    {
        positionToMoveTo = new Vector2(objectToMoveTo.transform.position.x, objectToMoveTo.transform.position.y);
    }

    public void OnTriggerEnter2D()
    {
        active = true;
    }

    public void OnTriggerExit2D()
    {
        active = false;
        if (-1 < dialogueCounter && dialogueCounter < dialogue.Length) dialogue[dialogueCounter].SetActive(false);
        dialogueCounter = -1;
    }

    public void Update()
    {

        GameObject[] activeDialogue;

        if (talkedToCount == 0)
        {
            activeDialogue = dialogue;
        }
        else if (talkedToCount == 1)
        {
            activeDialogue = dialogueCont;
        }
        else activeDialogue = endDialogue;

        if (movePlayer && active)
        {
            playerScript.MoveTo(positionToMoveTo);
            playerScript.LockMovement = true;
            virtualCamera.SetActive(true);
            darkness.FadeTo(fade);
            circle.SetActive(false);
            vision.SetActive(false);
            if (playerScript.body.velocity.x < 0.1 && playerScript.body.velocity.x > -0.1) 
            {
                playerScript.renderer.flipX = faceLeft;
            }
            else if (playerScript.body.velocity.x > 0) playerScript.renderer.flipX = false;
            else playerScript.renderer.flipX = true;
        }

        if (Input.GetKeyDown(KeyCode.F) && active)
        {
            if (dialogueCounter == -1) movePlayer = true;
            //Close last dialogue box making sure that it actually exists
            if (dialogueCounter > -1) activeDialogue[dialogueCounter].SetActive(false);

            dialogueCounter += 1;

            //Open the next dialogue box making sure that it actually exists
            if (activeDialogue.Length != dialogueCounter) activeDialogue[dialogueCounter].SetActive(true);
            else
            {
                dialogueCounter = -1;
                movePlayer = false;
                playerScript.LockMovement = false;
                virtualCamera.SetActive(false);
                talkedToCount += 1;
                darkness.FadeTo(darkness.startingAlpha);
                circle.SetActive(true);
                vision.SetActive(true);
            }
        }
    }
}
