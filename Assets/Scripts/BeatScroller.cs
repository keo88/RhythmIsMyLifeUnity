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
    public List<Chord> ChordArray;
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

    private List<float> pattern1 = new List<float>{ 0f, 2f, 4f, 6f };
    private List<float> pattern2 = new List<float>{ 0f, 2f, 1f, 3f, 4f, 5f, 6f, 7f };
    private List<float> pattern3 = new List<float>{ 1f, 1.5f, 2f, 3f, 3.5f, 4f, 4.5f, 5f, 5.5f, 6f, 6.5f, 7f, 7.5f, 8f };

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
        CreateLane(ChordArray, 1);
        CreateLane(GM.BeatArray, 1);
        CreateLane(pattern1, 100);
        CreateLane(pattern2, 100);
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

            while (chordHead < ChordArray.Count && ChordArray[chordHead].offset < GM.CurrentMusicDistance)
            {
                chordHead++;
            }

            GM.CurrentChord = ChordArray[chordHead];
        }
    }


    private void CreateLane(List<Chord> patterns, int loop)
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

    private void CreateLane(List<float> patterns, int loop)
    {
        List<float> lane = new List<float>();

        laneSpawnHeads.Add(0);
        lanes.Add(lane);

        for (int bar = 0; bar < loop * BARSIZE; bar += BARSIZE)
        {
            foreach (float note in patterns)
            {
                lane.Add(bar + note);
            }
        }
    }

    /// <summary>
    /// Spawns "close-enough" notes to hitbar(adjacency distance is defined by SpawnNoteMusicDistance).
    /// </summary>
    private void CreateAdjacentNotes()
    {
        float spawn_cap = GM.CurrentMusicDistance + SpawnNoteMusicDistance;

        for (int lane_index = 0; lane_index < lanes.Count; lane_index++)
        {
            while (laneSpawnHeads[lane_index] < lanes[lane_index].Count && lanes[lane_index][laneSpawnHeads[lane_index]] <= spawn_cap)
            {
                float spawn_note_z = (lanes[lane_index][laneSpawnHeads[lane_index]] - GM.CurrentMusicDistance) * GM.Speed;
                GameObject note = Instantiate(Note, new Vector3(lane_index * LANEGAP + gameObject.transform.position.x, 0, spawn_note_z), Quaternion.identity);
                note.transform.parent = gameObject.transform;
                // note.GetComponent<NoteScript>().pitch = ChordArray[laneSpawnHeads[lane_index]].pitch;

                laneSpawnHeads[lane_index]++;
            }
        }
    }
}
