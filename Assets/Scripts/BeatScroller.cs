using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    public GameManager GM;

    public const int BARSIZE = 6;
    public int LANEGAP = 4;

    public KeyCode SwitchLaneLeftKey;
    public KeyCode SwitchLaneRightKey;
    public KeyCode DebugLaneSyncForwardKey;
    public KeyCode DebugLaneSyncBackwardKey;
    public GameObject Note;

    /// <summary>
    /// Tempo-indendent note falling speed.
    /// </summary>
    //public float Speed;

    /// <summary>
    ///  Defines how "far" the notes spawn from the current position, music distance-wise.
    /// </summary>
    public float SpawnNoteMusicDistance;
    public float SpawnCheckInterval;

    public float Tempo;
    public bool StartFlag;
    public int CurrentLane;

    /// <summary>
    /// lanes는 lane의 nested List로, 각 lane은 사용자가 switch 가능한 리듬이다. 
    /// 각 lane은 music distance의 배열이며 이 music distance element 하나당 해당 위치에 note 하나가 생성된다.
    /// </summary>
    public List<List<float>> lanes { get; set; }

    /// <summary>
    ///  lanes의 각 lane 마다 따로 관리되고 있는 지금까지 생성된 note의 index가 저장된 배열이다.
    /// </summary>
    private List<int> laneSpawnHeads;
    private int chordHead;
    private int spawnChordHead;
    private Chord spawnChord;

    private List<float> pattern1 = new List<float> { 0.5f };
    private List<float> pattern2 = new List<float> { 0.25f };
    private List<float> pattern3 = new List<float>{ 0.25f, 0.5f, 0.75f };

    private int targetLanePositionX;
    private float checkElapsedMusicDist;

    // Start is called before the first frame update
    void Start()
    {
        CurrentLane = 0;
        Tempo = 0;
        chordHead = 0;

        GM.CurrentMusicDistance = 0f;

        lanes = new List<List<float>>();
        laneSpawnHeads = new List<int>();
    }

    public void CreateLanes()
    {
        CreateLane(GM.ChordArray, 1);
        CreateLane(GM.BeatArray);
        CreateLane(GM.BeatArray, pattern1);
        CreateLane(GM.BeatArray, pattern2);
        CreateLane(GM.BeatArray, pattern3);
    }    

    public void UpdateBeatScroller()
    {
        if (Input.GetKeyDown(SwitchLaneLeftKey))
        {
            CurrentLane = (CurrentLane + lanes.Count - 1) % lanes.Count;
            targetLanePositionX = -CurrentLane * LANEGAP;
            Debug.Log(targetLanePositionX);
        }
        else if (Input.GetKeyDown(SwitchLaneRightKey))
        {
            CurrentLane = (CurrentLane + 1) % lanes.Count;
            targetLanePositionX = -CurrentLane * LANEGAP;
        }
        else if (Input.GetKeyDown(DebugLaneSyncForwardKey))
        {
            transform.position -= new Vector3(0f, 0f, 0.1f * GM.Speed);
        }
        else if (Input.GetKeyDown(DebugLaneSyncBackwardKey))
        {
            transform.position += new Vector3(0f, 0f, 0.1f * GM.Speed);
        }

        if (GM.IsPlaying)
        {
            transform.position += new Vector3((targetLanePositionX - transform.position.x) * 0.1f, 0f, -Tempo * Time.deltaTime * GM.Speed);

            float delta_music_dist = Tempo * Time.deltaTime;

            checkElapsedMusicDist += delta_music_dist;
            GM.CurrentMusicDistance += delta_music_dist;

            if (checkElapsedMusicDist >= SpawnCheckInterval)
            {
                CreateAdjacentNotes();
                checkElapsedMusicDist -= SpawnCheckInterval;
            }

            
            while (chordHead < GM.ChordArray.Count && GM.ChordArray[chordHead].offset < GM.CurrentMusicDistance)
            {
                chordHead++;
            }

            GM.CurrentChord = GM.ChordArray[chordHead];
            
        }
    }


    private void CreateLane(List<Chord> patterns, int loop=1)
    {
        List<float> lane = new List<float>();

        laneSpawnHeads.Add(0);
        lanes.Add(lane);

        for (int bar = 0; bar < loop * BARSIZE; bar += BARSIZE)
        {
            foreach (Chord chord in patterns)
            {
                lane.Add(bar + chord.offset);
            }
        }
    }

    private void CreateLane(List<float> beatTrack)
    {
        int noteInd;
        List<float> lane = new List<float>();

        laneSpawnHeads.Add(0);
        lanes.Add(lane);

        for (noteInd = 0; noteInd < beatTrack.Count; noteInd++)
        {
            lane.Add(beatTrack[noteInd]);
        }
    }

    private void CreateLane(List<float> beatTrack, List<float> patterns)
    {
        int noteInd;
        float beatTrackTempoInSec = GM.BeatTrackTempo / 60f;
        List<float> lane = new List<float>();

        laneSpawnHeads.Add(0);
        lanes.Add(lane);

        for(noteInd = 0; noteInd < beatTrack.Count - 1; noteInd++)
        {
            lane.Add(beatTrack[noteInd]);

            double maxInterval = Math.Floor((beatTrack[noteInd + 1] - beatTrack[noteInd]) * 10)/ 10;
            for (int pat = 0; pat < patterns.Count && patterns[pat] < maxInterval; pat++)
            {
                lane.Add(beatTrack[noteInd] + patterns[pat] * beatTrackTempoInSec);
            }
        }

        lane.Add(beatTrack[noteInd]);
    }

    /// <summary>
    /// Spawns "close-enough" notes to hitbar(adjacency distance is defined by SpawnNoteMusicDistance).
    /// </summary>
    private void CreateAdjacentNotes()
    {
        float spawn_cap = GM.CurrentMusicDistance + SpawnNoteMusicDistance;

        while (spawnChordHead < GM.ChordArray.Count && GM.ChordArray[spawnChordHead].offset < spawn_cap)
        {
            spawnChordHead++;
        }

        spawnChord = GM.ChordArray[spawnChordHead];

        for (int lane_index = 0; lane_index < lanes.Count; lane_index++)
        {
            while (laneSpawnHeads[lane_index] < lanes[lane_index].Count && lanes[lane_index][laneSpawnHeads[lane_index]] <= spawn_cap)
            {
                float spawn_note_z = (lanes[lane_index][laneSpawnHeads[lane_index]] - GM.CurrentMusicDistance) * GM.Speed;

                CreateNote(gameObject, new Vector3(lane_index * LANEGAP + gameObject.transform.position.x, 0, spawn_note_z), spawnChord.pitch);

                laneSpawnHeads[lane_index]++;
            }
        }
    }

    public NoteScript CreateNote(GameObject parent, Vector3 transform, float pitch)
    {
        GameObject newNote = Instantiate(Note, transform, Quaternion.identity);
        newNote.transform.parent = parent.transform;
        NoteScript nsComp = newNote.GetComponent<NoteScript>();
        nsComp.Pitch = pitch;

        Renderer noteRenderer = newNote.GetComponent<Renderer>();
        noteRenderer.material.SetColor("_EmissionColor", pitchToColor(pitch) * nsComp.Intensity);

        return nsComp;
    }

    private static Color pitchToColor(float pitch)
    {
        int intPitch = (int)(pitch * 2);
        int octave = intPitch / 24 - 4;
        float code = (intPitch % 24) / 2f;

        return Color.HSVToRGB(code / 12f, 0.5f - octave / 4f, 1.0f);
    }
}
