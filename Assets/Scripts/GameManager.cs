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
    /// FAKEPLAY : 노트에 hit하면 일시정지.
    /// PHYTHMGAME : 노트에 hit 해도 PASS.
    /// </summary>
    public GameMode CurrentGameMode;

    /// <summary>
    ///  Music distance는 악보 상의 거리로 4분의 1 노트 하나당 1의 길이를 가진다.
    ///  Current Music Distance는 주어진 악보에 대해 현재 시점 플레이 중인 악보 상의 position을 나타낸다.
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
        // hasStarted이 true일 때만 게임 플레이에 필요한 작업 활성화
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

            // Note의 움직임을 관리
            BS.UpdateBeatScroller();
            // 키보드 입력을 받아 HitBar 동작
            HS.UpdateHitBar();
        }
    }

    /* 
     * 게임의 전체적인 플레이를 관리하는 코루틴
     * RoundSetup, RoundPlaying, RoundEnding 3개의 단계로 구성
     * RoundStarting: 한 라운드를 플레이 하는 데 필요한 정보를 로드하는 단계, 곡 하나를 플레이 하는 것을 한 라운드로 정의
     * RoundPlaying: 한 라운드를 플레이하는 단계
     * RoundEnding: 한 라운드를 끝내는 단계
     */
    private IEnumerator GameLoop()
    {
        // RoundStarting 코루틴을 작동시켜 한 라운드 플레이에 필요한 정도 로드, RoundStarting 코루틴이 완료된 후 return 된다.
        yield return StartCoroutine(RoundStarting());

        // RoundStarting 코루틴이 완료되면 RoundPlaying 코루틴을 작동시켜 게임 플레이, 한 라운드가 종료된 후 return 된다.
        yield return StartCoroutine(RoundPlaying());

        // RoundPlaying 코루틴이 완료되면 RoundEnding 코루틴을 작동시켜 한 라운드를 끝낸다, 필요한 작업을 마친 후 return 된다.
        yield return StartCoroutine(RoundEnding());

        // 게임이 완전히 끝났으면 게임 종료
        if (isGameEnd)
        {
            Debug.Log("Game End");
        }
        // 게임이 완전히 끝난게 아니면 다음 라운드(곡) 실행
        else 
        {
            StartCoroutine(GameLoop());
        }
    }


    /* 
     * 한 라운드를 플레이 하는 데 필요한 정보를 로드하는 코루틴
     */
    private IEnumerator RoundStarting()
    {
        Debug.Log("Round Starting...");

        /* To do
         * 게임 플레이를 할 음악을 고르는 단계가 추가될 예정
         */

        string musicNameWithoutExt = MusicName.Replace(".mp3", "").Replace(".wav", "");
        Track.clip = Resources.Load<AudioClip>($"Music/{musicNameWithoutExt}");
        Video.clip = Resources.Load<VideoClip>($"Videos/{musicNameWithoutExt}");

        // Python Server를 실행하여 해당 라운드에 플레이 되는 곡의 정보(템포, 비트 배열 등)를 받아옴
        Task<bool> pythonProcessingTask = PM.RunPythonServer();

        yield return new WaitUntil(() => pythonProcessingTask.IsCompleted);

        // Python Server에서 받아온 정보를 바탕으로 BeatScroller의 Tempo 및 BeatArray 변경
        /*
         * To do BeatScroller 내의 하나의 함수로 통합하기
         */

        BS.Tempo = Tempo / 60f;
        BS.ChordArray = ChordArray;
        BS.CreateLanes();

        Debug.Log("Round Setup Done");

        yield return null;
    }


    /* 
    * 한 라운드를 플레이하는 코루틴
    */
    private IEnumerator RoundPlaying()
    {
        Debug.Log("Round Playing...");
    
        bool isKeyDown = false;
        while(!isKeyDown)
        {
            // 키보드 입력이 있으면 플레이 시작
            if (Input.anyKeyDown)
            {
                Debug.Log("Key Down");

                isKeyDown = true;

                // IsPlaying을 true로 바꾸어서 HitBar입력, Note움직임이 활성화 되게 한다.
                hasStarted = true;
                IsPlaying = true;

                // 키보드 입력을 하자마자 바로 음악이 재생되는 것을 방지하기 위해 HitBar와 NoteHolder의 초기 위치가 일정 거리 만큼 떨어져 있다.
                // HitBar와 NoteHolder 사이의 간격이 0이 되면 음악 재생
                yield return new WaitForSeconds(-HS.transform.position.z * 60.0f / Tempo / Speed);

                //Track.Play();
                Video.Play();
            }

            // 키보드 입력이 없으면 다음 프레임에 이 위치로 다시 돌아온다.
            yield return null;
        }

        
        // 현재 노래가 끝나면 while 문을 벗어난다.
        while (true)
        {

            yield return null;
        }

        //yield return new WaitForSeconds(5.0f);
        //Track.Stop();
        
    }


    /* 
     * 한 라운드를 끝내는 코루틴
     */
    private IEnumerator RoundEnding()
    {
        Debug.Log("Round Ending");

        // IsPlaying을 true로 바꾸어서 HitBar입력, Note움직임이 비활성화 되게 한다.
        hasStarted = false;
        IsPlaying = false;

        // NoteHolder의 위치를 초기 위치로 되돌린다.
        BS.transform.position = new Vector3(0.0f, 0.0f, 0.0f);

        /* To do
         * BeatScroller내의 BeatArray를 초기화 하는 작업 추가 필요
         */

        bool isKeyDown = false;
        while (!isKeyDown)
        {
            // ESC를 누르면 게임 완전히 종료
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                isKeyDown = true;
                isGameEnd = true;
            }
            // ESC를 제외한 키를 누르면 다음 라운드 시작
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
