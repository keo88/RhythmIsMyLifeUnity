using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyBarScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {      
        if (other.tag == "Note")
        {
            Destroy(other.gameObject);
        }
    }
}
