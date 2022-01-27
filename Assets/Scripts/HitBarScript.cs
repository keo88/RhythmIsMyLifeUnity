using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBarScript : MonoBehaviour
{
    private Renderer renderer;
    public Color ColorDefault;
    public Color ColorClicked;

    public KeyCode HitKey;

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
        }
        else if (Input.GetKeyUp(HitKey))
        {
            renderer.material.SetColor("_Color", ColorDefault);
        }
    }
}
