using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Video;

public class GameManager : MonoBehaviour
{
    public BeatScroller BS;
    public PythonManager PM;
    public HitBarScript HS;
    public AudioSource Track;
    public VideoPlayer Video;

    public string MusicName;

    public float Tempo;
    public List<float> BeatArray;
    public List<Chord> ChordArray;
    public Chord CurrentChord;
    public float Speed;
    public bool IsPlaying;

    /// <summary>
    /// Gamemodes.
    /// FAKEPLAY : ��Ʈ�� hit�ϸ� �Ͻ�����.
    /// PHYTHMGAME : ��Ʈ�� hit �ص� PASS.
    /// </summary>
    public GameMode CurrentGameMode;

    /// <summary>
    ///  Music distance�� �Ǻ� ���� �Ÿ��� 4���� 1 ��Ʈ �ϳ��� 1�� ���̸� ������.
    ///  Current Music Distance�� �־��� �Ǻ��� ���� ���� ���� �÷��� ���� �Ǻ� ���� position�� ��Ÿ����.
    /// </summary>
    public float CurrentMusicDistance;

    private bool isGameEnd;
    private bool hasStarted;

 
    // Start is called before the first frame update
    void Start()
    {
        hasStarted = false;
        IsPlaying = false;
        isGameEnd = false;

        Video.SetDirectAudioVolume(0, 0.5f);

        Debug.Log("Game Start!");
        StartCoroutine(GameLoop());
    }


    // Update is called once per frame
    void Update()
    {
        // hasStarted�� true�� ���� ���� �÷��̿� �ʿ��� �۾� Ȱ��ȭ
        if (hasStarted)
        {
            if (!Video.isPlaying && IsPlaying)
            {
                //Track.UnPause();
                Video.Play();
            }
            else if (Video.isPlaying && !IsPlaying)
            {
                //Track.Pause();
                Video.Pause();
            }

            // Note�� �������� ����
            BS.UpdateBeatScroller();
            // Ű���� �Է��� �޾� HitBar ����
            HS.UpdateHitBar();
        }
    }

    /* 
     * ������ ��ü���� �÷��̸� �����ϴ� �ڷ�ƾ
     * RoundSetup, RoundPlaying, RoundEnding 3���� �ܰ�� ����
     * RoundStarting: �� ���带 �÷��� �ϴ� �� �ʿ��� ������ �ε��ϴ� �ܰ�, �� �ϳ��� �÷��� �ϴ� ���� �� ����� ����
     * RoundPlaying: �� ���带 �÷����ϴ� �ܰ�
     * RoundEnding: �� ���带 ������ �ܰ�
     */
    private IEnumerator GameLoop()
    {
        // RoundStarting �ڷ�ƾ�� �۵����� �� ���� �÷��̿� �ʿ��� ���� �ε�, RoundStarting �ڷ�ƾ�� �Ϸ�� �� return �ȴ�.
        yield return StartCoroutine(RoundStarting());

        // RoundStarting �ڷ�ƾ�� �Ϸ�Ǹ� RoundPlaying �ڷ�ƾ�� �۵����� ���� �÷���, �� ���尡 ����� �� return �ȴ�.
        yield return StartCoroutine(RoundPlaying());

        // RoundPlaying �ڷ�ƾ�� �Ϸ�Ǹ� RoundEnding �ڷ�ƾ�� �۵����� �� ���带 ������, �ʿ��� �۾��� ��ģ �� return �ȴ�.
        yield return StartCoroutine(RoundEnding());

        // ������ ������ �������� ���� ����
        if (isGameEnd)
        {
            Debug.Log("Game End");
        }
        // ������ ������ ������ �ƴϸ� ���� ����(��) ����
        else 
        {
            StartCoroutine(GameLoop());
        }
    }


    /* 
     * �� ���带 �÷��� �ϴ� �� �ʿ��� ������ �ε��ϴ� �ڷ�ƾ
     */
    private IEnumerator RoundStarting()
    {
        Debug.Log("Round Starting...");

        /* To do
         * ���� �÷��̸� �� ������ ���� �ܰ谡 �߰��� ����
         */

        string musicNameWithoutExt = MusicName.Replace(".mp3", "").Replace(".wav", "");
        Track.clip = Resources.Load<AudioClip>($"Music/{musicNameWithoutExt}");
        Video.clip = Resources.Load<VideoClip>($"Videos/{musicNameWithoutExt}");

        // Python Server�� �����Ͽ� �ش� ���忡 �÷��� �Ǵ� ���� ����(����, ��Ʈ �迭 ��)�� �޾ƿ�
        Task<bool> pythonProcessingTask = PM.RunPythonServer();

        yield return new WaitUntil(() => pythonProcessingTask.IsCompleted);

        // Python Server���� �޾ƿ� ������ �������� BeatScroller�� Tempo �� BeatArray ����
        /*
         * To do BeatScroller ���� �ϳ��� �Լ��� �����ϱ�
         */

        BS.Tempo = Tempo / 60f;
        BS.ChordArray = ChordArray;
        BS.CreateLanes();

        Debug.Log("Round Setup Done");

        yield return null;
    }


    /* 
    * �� ���带 �÷����ϴ� �ڷ�ƾ
    */
    private IEnumerator RoundPlaying()
    {
        Debug.Log("Round Playing...");
    
        bool isKeyDown = false;
        while(!isKeyDown)
        {
            // Ű���� �Է��� ������ �÷��� ����
            if (Input.anyKeyDown)
            {
                Debug.Log("Key Down");

                isKeyDown = true;

                // IsPlaying�� true�� �ٲپ HitBar�Է�, Note�������� Ȱ��ȭ �ǰ� �Ѵ�.
                hasStarted = true;
                IsPlaying = true;

                // Ű���� �Է��� ���ڸ��� �ٷ� ������ ����Ǵ� ���� �����ϱ� ���� HitBar�� NoteHolder�� �ʱ� ��ġ�� ���� �Ÿ� ��ŭ ������ �ִ�.
                // HitBar�� NoteHolder ������ ������ 0�� �Ǹ� ���� ���
                yield return new WaitForSeconds(-HS.transform.position.z * 60.0f / Tempo / Speed);

                //Track.Play();
                Video.Play();
            }

            // Ű���� �Է��� ������ ���� �����ӿ� �� ��ġ�� �ٽ� ���ƿ´�.
            yield return null;
        }

        
        // ���� �뷡�� ������ while ���� �����.
        while (true)
        {

            yield return null;
        }

        //yield return new WaitForSeconds(5.0f);
        //Track.Stop();
        
    }


    /* 
     * �� ���带 ������ �ڷ�ƾ
     */
    private IEnumerator RoundEnding()
    {
        Debug.Log("Round Ending");

        // IsPlaying�� true�� �ٲپ HitBar�Է�, Note�������� ��Ȱ��ȭ �ǰ� �Ѵ�.
        hasStarted = false;
        IsPlaying = false;

        // NoteHolder�� ��ġ�� �ʱ� ��ġ�� �ǵ�����.
        BS.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        /* To do
         * BeatScroller���� BeatArray�� �ʱ�ȭ �ϴ� �۾� �߰� �ʿ�
         */

        bool isKeyDown = false;
        while (!isKeyDown)
        {
            // ESC�� ������ ���� ������ ����
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isKeyDown = true;
                isGameEnd = true;
            }
            // ESC�� ������ Ű�� ������ ���� ���� ����
            else if (Input.anyKeyDown)
            {
                isKeyDown = true;
            }

            yield return null;
        }

        Debug.Log("Round End");
    }
}

public enum GameMode
{
    RHYTHMGAME,
    FAKEPLAY,
}
