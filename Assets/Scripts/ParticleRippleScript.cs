using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleRippleScript : MonoBehaviour
{
    public float TimeLeft;

    void Start()
    {
        Destroy(this.gameObject, TimeLeft);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
