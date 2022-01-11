using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public AudioSource Track;
    public bool IsPlaying;
    public BeatScroller BS;
    public PythonLibrosaManager PM;

    private bool has_clicked;

    // Start is called before the first frame update
    void Start()
    {
        IsPlaying = false;

        has_clicked = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!has_clicked && Input.anyKeyDown)
        {
            // 게임이 시작한 후 최소 1회의 클릭이 있어야 게임 시작.
            has_clicked = true;
        }

        if (!IsPlaying && has_clicked && PM.TempoIsAnalyzed)
        {
            IsPlaying = true;

            BS.Tempo = PM.Tempo / 60f;
            BS.StartFlag = true;

            Track.Play();
        }
    }
}
