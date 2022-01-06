using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript : MonoBehaviour
{
    public KeyCode KeyToPress;

    public bool CanBePressed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyToPress) && CanBePressed) 
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if (other.tag == "HitBar")
        {
            CanBePressed = true;
        }
    }

    private void OnTriggerExit(Collider other) 
    {
        if (other.tag == "HitBar")
        {
            CanBePressed = false;
        }
    }
}
