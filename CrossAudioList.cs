using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#elif UNITY_IOS
using UnityEngine.iOS;
#endif
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using UnityEngine.Networking;

public class CrossAudioList : MonoBehaviour
{


    public List<DownLoadPack> ClipDownLoaded = new List<DownLoadPack>();

    public bool notdownloading = true;
    public int ClipDownLoaded_currentindex = 0;

    public List<System.Collections.Generic.Dictionary<AudioClip, string>> dictionaries;
    public List<SoundObject> sounds;
    // Start is called before the first frame update
    void Start()
    {
        sounds = new List<SoundObject>();
        StartCoroutine(RequestSoundList("5e57fe18da56f97fcd2a0986"));






    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator RequestSoundList(string eventId)
    {
        string responseBody;
        using (UnityWebRequest req = UnityWebRequest.Get("https://echoes.etc.cmu.edu/api/viewer/events/"+ eventId+ "/sound"))
        {

            req.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("token"));

            yield return req.SendWebRequest();


            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log(req.error + " : " + req.downloadHandler.text);
            }

            responseBody = DownloadHandlerBuffer.GetContent(req);
            PlayerPrefs.SetString("body", responseBody);
        }
        StartCoroutine(GetSoundList());

    }

    


    IEnumerator GetSoundList()
    {
        string responseBody = PlayerPrefs.GetString("body");

        SoundFetchAPIResult result = JsonUtility.FromJson<SoundFetchAPIResult>(responseBody);
        if (result.result != null)
        {
            foreach (SoundData sound in result.result)
            {
                string path = "https://echoes.etc.cmu.edu" + sound.path;
                string displayName = sound.user.display_name;
                string labelName = sound.game_meta.sound_label;
                string id = sound.id;

                sounds.Add(new SoundObject(path, displayName, labelName, id));
            }
        }

        yield return new WaitForEndOfFrame();
        StartCoroutine(DownLoadClipRoutine());

        //foreach (SoundObject sound in sounds)
        //{

        //    StartCoroutine(DownloadAudioFromSoundList(sound.displayName, sound.labelName, sound.path, sound.id));

        //}

    }

    IEnumerator DownLoadClipRoutine()
    {
        bool notFinished = true;

        while (notFinished)
        {
            if (ClipDownLoaded.Count< 10 && notdownloading)
            {
                notdownloading = false;
                




                if (ClipDownLoaded_currentindex < sounds.Count)
                {
                    DownLoadClip(sounds[ClipDownLoaded_currentindex]);

                }
                else
                {
                    notFinished = false;
                }
                ClipDownLoaded_currentindex++;

            }
            yield return null;
        }
    }

    public void DownLoadClip(SoundObject Sound)
    {
        StartCoroutine(DownloadAudioFromSoundList(Sound.displayName, Sound.labelName, Sound.path, Sound.id));
    }


    IEnumerator DownloadAudioFromSoundList(string displayName, string labelName, string path, string id)
    {

       
        using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV))
        {
            yield return req.SendWebRequest();
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.LogError(req.error);
                yield break;
            }
            DownLoadPack newClip= new DownLoadPack();
            newClip.downloadclips = DownloadHandlerAudioClip.GetContent(req);
            newClip.labelnames = labelName;
            ClipDownLoaded.Add(newClip);

            notdownloading = true;
            //Debug.Log(ClipDownLoaded.Count);

            //clips.Add(DownloadHandlerAudioClip.GetContent(req));
            //labelnames.Add(labelName);
            //clips_labels.Add(DownloadHandlerAudioClip.GetContent(req), labelName);
            //dictionaries.Add(clips_labels);
            
        }
        



    }








}
