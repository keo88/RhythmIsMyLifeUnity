using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatScroller : MonoBehaviour
{
    public const int BARSIZE = 8;
    public int LANEGAP = 4;

    public KeyCode SwitchLaneLeftKey;
    public KeyCode SwitchLaneRightKey;
    public KeyCode DebugLaneSyncForwardKey;
    public KeyCode DebugLaneSyncBackwardKey;

    public float Speed;
    public float Tempo;
    public bool StartFlag;
    public int CurrentLane;
    public GameObject Note;

    public List<int> AvailableLanes;

    private float[] pattern1 = { 2f, 4f, 6f, 8f};
    private float[] pattern2 = { 1f, 2f, 3f, 4f, 5f, 6f, 7f, 8f};
    private float[] pattern3 = { 1f, 1.5f, 2f, 3f, 3.5f, 4f, 4.5f, 5f, 5.5f, 6f, 6.5f, 7f, 7.5f, 8f };

    private int target_lane_position_x;

    // Start is called before the first frame update
    void Start()
    {
        AvailableLanes = new List<int>();
        CurrentLane = 0;
        Tempo = 0;

        CreateLane(pattern1, 0, 10);
        CreateLane(pattern2, 1, 10);
        CreateLane(pattern3, 2, 10);

        gameObject.transform.localScale = new Vector3(1f, 1f, Speed);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(SwitchLaneLeftKey))
        {
            CurrentLane = AvailableLanes[(AvailableLanes.IndexOf(CurrentLane) + AvailableLanes.Count - 1) % AvailableLanes.Count];
            target_lane_position_x = -CurrentLane * LANEGAP;
            Debug.Log(target_lane_position_x);
        }
        else if (Input.GetKeyDown(SwitchLaneRightKey))
        {
            CurrentLane = AvailableLanes[(AvailableLanes.IndexOf(CurrentLane) + 1) % AvailableLanes.Count];
            target_lane_position_x = -CurrentLane * LANEGAP;
        }
        else if (Input.GetKeyDown(DebugLaneSyncForwardKey))
        {
            transform.position -= new Vector3(0f, 0f, 0.1f * Speed);
        }
        else if (Input.GetKeyDown(DebugLaneSyncBackwardKey))
        {
            transform.position += new Vector3(0f, 0f, 0.1f * Speed);
        }

            if (StartFlag) 
        {
            transform.position += new Vector3((target_lane_position_x - transform.position.x) * 0.1f, 0f, -Tempo * Time.deltaTime * Speed);
        }
    }


    private void CreateLane(float[] patterns, int lane, int loop)
    {
        AvailableLanes.Add(lane);
        AvailableLanes.Sort();
        // AvailableLanes.Reverse();

        for (int bar = 0; bar < loop * BARSIZE; bar += BARSIZE)
        {
            foreach (float note_pos in patterns)
            {
                GameObject note = Instantiate(Note, new Vector3(lane * LANEGAP, 0, (bar + note_pos)), Quaternion.identity);
                note.transform.parent = gameObject.transform;
            }
        }
    }
}
