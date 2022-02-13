using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBarScript : MonoBehaviour
{
    public GameManager GM;
    public MidiStreamManager MidiStreamManagerScript;

    public Color ColorDefault;
    public Color ColorClicked;
    public KeyCode HitKey;

    private Renderer renderer;
    private bool isNoteInHitBar = false;
    private GameObject note = null;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateHitBar()
    {
        if (Input.GetKeyDown(HitKey))
        {
            renderer.material.SetColor("_Color", ColorClicked);

            if (isNoteInHitBar)
            {
                note.SetActive(false);
                // int pitch = note.GetComponent<NoteScript>().pitch;
                // MidiStreamManagerScript.PlayNote(pitch, 1000);

                if (GM.CurrentGameMode == GameMode.FAKEPLAY)
                {
                    GM.IsPlaying = true;
                    isNoteInHitBar = false;
                }
            }
            if (GM.CurrentChord != null)
            {
                MidiStreamManagerScript.PlayNote(GM.CurrentChord.pitch, 1000);
            }
        }
        else if (Input.GetKeyUp(HitKey))
        {
            renderer.material.SetColor("_Color", ColorDefault);
            
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Note")
        {
            isNoteInHitBar = true;
            note = other.gameObject;   
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Note")
        {
            if (GM.CurrentGameMode == GameMode.FAKEPLAY)
            {
                GM.IsPlaying = false;
            }
            else if (GM.CurrentGameMode == GameMode.RHYTHMGAME)
            {
                isNoteInHitBar = false;
            }
        }
    }
}
