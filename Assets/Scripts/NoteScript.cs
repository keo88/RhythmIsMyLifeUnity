using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteScript : MonoBehaviour
{
    //public KeyCode KeyToPress;
    //public GameManager GM;

    //public bool CanBePressed;
    public int pitch;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyToPress) && CanBePressed)
    //    {
    //        gameObject.SetActive(false);

    //        if (GM.CurrentGameMode == GameMode.FAKEPLAY)
    //        {
    //            GM.IsPlaying = true;
    //        }
    //    }
    //}

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.tag == "HitBar")
    //    {
    //        CanBePressed = true;
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (GM.CurrentGameMode == GameMode.RHYTHMGAME)
    //    {
    //        if (other.tag == "HitBar")
    //        {
    //            CanBePressed = false;
    //        }
    //    }
    //    else if (GM.CurrentGameMode == GameMode.FAKEPLAY)
    //    {
    //        if (other.tag == "HitBar")
    //        {
    //            GM.IsPlaying = false;
    //        }
    //    }

    //    if (other.tag == "DestroyBar")
    //    {
    //        Destroy(gameObject);
    //    }
    //}
}
