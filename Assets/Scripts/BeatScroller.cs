using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    public GameManager GM;

    public const int BARSIZE = 8;
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
    public GameManager.Chord[] ChordArray;
    public bool StartFlag;
    public int CurrentLane;

    /// <summary>
    ///  Music distance�� �Ǻ� ���� �Ÿ��� 4���� 1 ��Ʈ �ϳ��� 1�� ���̸� ������.
    ///  Current Music Distance�� �־��� �Ǻ��� ���� ���� ���� �÷��� ���� �Ǻ� ���� position�� ��Ÿ����.
    /// </summary>
    public float CurrentMusicDistance { get; set; }

    /// <summary>
    /// lanes�� lane�� nested List��, �� lane�� ����ڰ� switch ������ �����̴�. 
    /// �� lane�� music distance�� �迭�̸� �� music distance element �ϳ��� �ش� ��ġ�� note �ϳ��� �����ȴ�.
    /// </summary>
    public List<List<float>> lanes { get; set; }

    /// <summary>
    ///  lanes�� �� lane ���� ���� �����ǰ� �ִ� ���ݱ��� ������ note�� index�� ����� �迭�̴�.
    /// </summary>
    private List<int> lane_spawn_heads;

    private float[] pattern1 = { 2f, 4f, 6f, 8f };
    private float[] pattern2 = { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f };
    private float[] pattern3 = { 1f, 1.5f, 2f, 3f, 3.5f, 4f, 4.5f, 5f, 5.5f, 6f, 6.5f, 7f, 7.5f, 8f };

    private int target_lane_position_x;
    private float check_elapsed_music_dist;

    // Start is called before the first frame update
    void Start()
    {
        CurrentLane = 0;
        Tempo = 0;
        CurrentMusicDistance = 0f;

        lanes = new List<List<float>>();
        lane_spawn_heads = new List<int>();

        // gameObject.transform.localScale = new Vector3(1f, 1f, Speed);
    }


    private void Update()
    {
    }
    public void CreateLanes()
    {
        CreateLane(ChordArray, 1);
        CreateLane(ChordArray, 10);
        CreateLane(ChordArray, 10);
    }    

    public void UpdateBeatScroller()
    {
        if (Input.GetKeyDown(SwitchLaneLeftKey))
        {
            CurrentLane = (CurrentLane + lanes.Count - 1) % lanes.Count;
            target_lane_position_x = -CurrentLane * LANEGAP;
            Debug.Log(target_lane_position_x);
        }
        else if (Input.GetKeyDown(SwitchLaneRightKey))
        {
            CurrentLane = (CurrentLane + 1) % lanes.Count;
            target_lane_position_x = -CurrentLane * LANEGAP;
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
            transform.position += new Vector3((target_lane_position_x - transform.position.x) * 0.1f, 0f, -Tempo * Time.deltaTime * GM.Speed);

            float delta_music_dist = Tempo * Time.deltaTime;
            check_elapsed_music_dist += delta_music_dist;

            if (check_elapsed_music_dist >= SpawnCheckInterval)
            {
                CreateAdjacentNotes();
                check_elapsed_music_dist -= SpawnCheckInterval;
            }
            CurrentMusicDistance += Tempo * Time.deltaTime;
        }
    }


    private void CreateLane(GameManager.Chord[] patterns, int loop)
    {
        List<float> lane = new List<float>();

        lane_spawn_heads.Add(0);
        lanes.Add(lane);

        for (int bar = 0; bar < loop * BARSIZE; bar += BARSIZE)
        {
            foreach (GameManager.Chord chord in patterns)
            {
                lane.Add(bar + chord.offset);
            }
        }
    }

    /// <summary>
    /// Spawns "close-enough" notes to hitbar(adjacency distance is defined by SpawnNoteMusicDistance).
    /// </summary>
    private void CreateAdjacentNotes()
    {
        float spawn_cap = CurrentMusicDistance + SpawnNoteMusicDistance;

        for (int lane_index = 0; lane_index < lanes.Count; lane_index++)
        {
            while (lane_spawn_heads[lane_index] < lanes[lane_index].Count && lanes[lane_index][lane_spawn_heads[lane_index]] <= spawn_cap)
            {
                float spawn_note_z = (lanes[lane_index][lane_spawn_heads[lane_index]] - CurrentMusicDistance) * GM.Speed;
                GameObject note = Instantiate(Note, new Vector3(lane_index * LANEGAP + gameObject.transform.position.x, 0, spawn_note_z), Quaternion.identity);
                note.transform.parent = gameObject.transform;
                note.GetComponent<NoteScript>().pitch = ChordArray[lane_spawn_heads[lane_index]].pitch;

                lane_spawn_heads[lane_index]++;
            }
        }
    }
}
