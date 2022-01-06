using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    public float Tempo;
    public bool StartFlag;

    private float tempoInSec;

    // Start is called before the first frame update
    void Start()
    {
        tempoInSec = Tempo / 60f;
    }

    // Update is called once per frame
    void Update()
    {
        if (!StartFlag) 
        {
            if (Input.anyKeyDown) 
            {
                StartFlag = true;
            }
        }
        else
        {
            transform.position -= new Vector3(0f, 0f, tempoInSec * Time.deltaTime);
        }
    }
}
