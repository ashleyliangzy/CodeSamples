using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

#if UNITY_ANDROID
using UnityEngine.Android;
#elif UNITY_IOS
using UnityEngine.iOS;
#endif
public class TagManager : MonoBehaviour
{
    public CrossAudioList crossAudioList;


    public List<string> tagmanager;
    public List<string> chosentag = new List<string>();
    public string userchoosetag;
    public GameObject option1;
    public GameObject option2;
    public GameObject option3;
    private string target;
    int ClipAmountsDid = 1 ;

    public UnityWebRequest www;
    public Text audioTime;
    private AudioClip download_audioClip;

    public GameObject completepage;

    public GameObject playaudiobutton;
    public GameObject pauseaudiobutton;


    [SerializeField]
    private Button[] optionButtons;



    //public int audioindex = 0;
    // Start is called before the first frame update

    public void generateTagList()
    {

        string[] labelinput = {
            "Sink/Faucet","Disposer","Garbage bin","Microwave","Oven","Toaster","Cooktop","Kettle","Refrigerator","Cooking","Silverwave","Plates","Mopping floor",
            "Sink","Bathtub","Shower","Hairdryer","Mirror cabinet","Toothbrush","Toilet flush","Toilet paper","Hand wash","Electric trimmer","Soap dispenser","Deodorant","Extractor fan",
            "Door","Doorbell","TV","Stereo","Children playing","Phone Ringing","Typing Keyboard","Window","Chair/Coach","Air conditioner","Vacuum cleaner","Fireplace","Clock",
            "Door","Car","Bike","Motorbike","Tools","Washer","Dryer","Furnace","Water leaking","Repair trimmer","Wood work",
            "Footsteps","Drinking","Eating","Baby crying","Dog/Cat","Light switch","Walking","Running","Car sounds","Bird sounds","Rain",
            "Person falling","Snoring","Coughing","Sneezing","Call for help","Smoke detector","Security alarm","Glass break"
        };

        tagmanager = new List<string>(labelinput);

    }
    public void loadTagList(string t)
    {
        randomMnumber(3, tagmanager.Count);
        if (chosentag.Contains(t))
        {
                //NO PROBLEM
        }
        else
        {
            chosentag[Random.Range(0,3)] = t;
                //add right tag
        }
        option1.GetComponent<Text>().text = chosentag[0];
        option2.GetComponent<Text>().text = chosentag[1];
        option3.GetComponent<Text>().text = chosentag[2];

        target = t;
    } 

    void randomMnumber(int m, int n)
    {
        chosentag.Clear();
        System.Random rand = new System.Random();
        for(int i = 0; i < n; i++)
        {
            int temp = rand.Next() % (n - i);
            if (temp < m)
            {
                chosentag.Add(tagmanager[i]);
                m--;
                //Debug.LogError(i);
            }
        }
    }

    void OnClickButton0()
    {
        userchoosetag = chosentag[0];
        //if (chosentag[0] == target)
        //{
        //    Debug.Log("yeeeeee");
        //}
        //else
        //{
        //    Debug.Log("noooooooo");
        //}
        //update 
        //UpdateAudio();


    }
    void OnClickButton1()
    {
        userchoosetag = chosentag[1];

        //if (chosentag[1] == target)
        //{
        //    Debug.Log("yeeeeee");
        //}
        //else
        //{
        //    Debug.Log("noooooooo");
        //}
        //UpdateAudio();
    }
    void OnClickButton2()
    {
        userchoosetag = chosentag[2];
        //if (chosentag[2] == target)
        //{
        //    Debug.Log("yeeeeee");
        //}
        //else
        //{
        //    Debug.Log("noooooooo");
        //}
        //UpdateAudio();
    }

    void CompareTag()
    {
        if (userchoosetag == target)
        {
            Debug.Log("yeeeeee");
        }
        else
        {
            Debug.Log("noooooooo");
        }
    }
    
    void ReportQuestion()
    {
        SkipAudio();
        Debug.Log("questionnnnnnnnn");
        //UpdateAudio();
    }



    void UpdateAudio()
    {
        CompareTag();
        StopAudio();

        DownLoadPack downLoadPackToDelete = crossAudioList.ClipDownLoaded[0];
        //AudioClip clipToDelete = crossAudioList.clips[audioindex];
        //string lableToDelete = crossAudioList.labelnames[audioindex];
        //crossAudioList.clips.Remove(clipToDelete);
        //crossAudioList.labelnames.Remove(lableToDelete);

        crossAudioList.ClipDownLoaded.Remove(downLoadPackToDelete);
        ClipAmountsDid++;


        Debug.Log(crossAudioList.sounds.Count);
        if (ClipAmountsDid <= crossAudioList.sounds.Count)
        {
            //completepage.SetActive(false);
            loadTagList(crossAudioList.ClipDownLoaded[0].labelnames);
            UpdateTime(crossAudioList.ClipDownLoaded[0].downloadclips.length - Camera.main.GetComponent<AudioSource>().time);

        }
        else
        {
            completepage.SetActive(true);
            Debug.Log("finished");
        }

  
    }



    public void PlayAudio()
    {

        Camera.main.GetComponent<AudioSource>().clip = crossAudioList.ClipDownLoaded[0].downloadclips;//audioclip from server
        Camera.main.GetComponent<AudioSource>().Play();

    }

    public void PauseAudio()
    {
        Camera.main.GetComponent<AudioSource>().Pause();
        //Debug.Log("Pause!");
    }

    public void StopAudio()
    {
        Camera.main.GetComponent<AudioSource>().Stop();
        //Debug.Log("Stop!");
    }
    
    private void UpdateTime(float rawTime)
    {
        int millisecond = Mathf.FloorToInt(rawTime * 1000) % 1000;
        int time = Mathf.FloorToInt(rawTime);
        audioTime.text = $"{time / 60:D2}:{time % 60:D2}:{millisecond / 15:D2}";
    }

    public void SkipAudio()
    {
        StopAudio();

        DownLoadPack downLoadPackToDelete = crossAudioList.ClipDownLoaded[0];

        crossAudioList.ClipDownLoaded.Remove(downLoadPackToDelete);
        //Debug.Log(" YI gong duoshao ge"+ crossAudioList.sounds.Count + " zuole duoshao ge  " + ClipAmountsDid + " haishng duoshao ge " + crossAudioList.ClipDownLoaded.Count);
        ClipAmountsDid++;


        if (ClipAmountsDid <= crossAudioList.sounds.Count)
        {
            
            loadTagList(crossAudioList.ClipDownLoaded[0].labelnames);
            //StartCoroutine("DownloadAudioFromServer");
            UpdateTime(crossAudioList.ClipDownLoaded[0].downloadclips.length - Camera.main.GetComponent<AudioSource>().time);

        }
        else
        {
            completepage.SetActive(true);
            Debug.Log("finished");
        }

    }



    void Start()
    {
        //clear
        //generateTaglist
        //
        generateTagList();

        loadTagList(crossAudioList.ClipDownLoaded[0].labelnames);
        UpdateTime(crossAudioList.ClipDownLoaded[0].downloadclips.length - Camera.main.GetComponent<AudioSource>().time);





        optionButtons[0].onClick.AddListener(OnClickButton0);
        optionButtons[1].onClick.AddListener(OnClickButton1);
        optionButtons[2].onClick.AddListener(OnClickButton2);
        optionButtons[3].onClick.AddListener(ReportQuestion);
        optionButtons[4].onClick.AddListener(PlayAudio);
        optionButtons[5].onClick.AddListener(PauseAudio);
        optionButtons[6].onClick.AddListener(UpdateAudio);
        optionButtons[7].onClick.AddListener(SkipAudio);


    }

    // Update is called once per frame
    void Update()
    {


        if (Camera.main.GetComponent<AudioSource>().isPlaying)
        {
            UpdateTime(crossAudioList.ClipDownLoaded[0].downloadclips.length - Camera.main.GetComponent<AudioSource>().time);
        }
        else
        {
            playaudiobutton.SetActive(true);
            pauseaudiobutton.SetActive(false);
            UpdateTime(crossAudioList.ClipDownLoaded[0].downloadclips.length - Camera.main.GetComponent<AudioSource>().time);
        }
        
    }
}
