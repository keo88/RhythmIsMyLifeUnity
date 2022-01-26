using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public AudioSource Track;
    public bool HasStarted;
    public BeatScroller BeatScrollerObject;
    public PythonLibrosaManager PythonManagerObject;
    public HitBarScript HitBarObject;

    public GameMode CurrentGameMode;

    public float Tempo;
    public bool IsPlaying;

    private bool has_clicked;

    // Start is called before the first frame update
    void Start()
    {
        HasStarted = false;

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

        if (!HasStarted && has_clicked && PythonManagerObject.TempoIsAnalyzed)
        {
            HasStarted = true;

            IsPlaying = true;
            Tempo = PythonManagerObject.Tempo / 60f;

            Track.Play();
        }

        if (!Track.isPlaying && IsPlaying)
        {
            Track.UnPause();
        }
        else if (Track.isPlaying && !IsPlaying)
        {
            Track.Pause();
        }

        BeatScrollerObject.UpdateBeatScroller();
        HitBarObject.UpdateHitBar();
    }
}

public enum GameMode
{
    RHYTHMGAME,
    FAKEPLAY,
}
