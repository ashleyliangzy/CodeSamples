using System;
using System.IO;
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

//records and saves the audio to persistentDataPath
public class RecordManager : MonoBehaviour
{
    [SerializeField]
    private ListOfStringsVariable soundItemsFilenames, audioLengthFile, recordingStates, joinedEvents;
    
    [SerializeField]
    private ListOfIntVariable soundLabelNumbers;

    [SerializeField]
    private StringVariable currentRecordingState, fileType;

    [SerializeField]
    public IntVariable randomFileNameNumber, updatedIndex, numberOfJoinedEvents;

    [SerializeField]
    private GameObject soundItem, homePage, RecordingPage, soundLabelDropDown, soundsUploadingPage, galleryPage;

    [SerializeField]
    private Text recordingStateText, recordingTimerText, joinEventText;

    [SerializeField]
    private InputField username;

    [SerializeField]
    private ListOfStringsVariable gamePieces;

    [SerializeField]
    private IntVariable chosenGamePieceIndex;
    
    [SerializeField]
    private StringVariable token;

    private string soundLabelText, customSoundLabelText;
    private int soundLabelIndex;
    private AudioClip audioClip;
    private float prepareStartTime = 0f;
    private FileInfo[] files;
    private DirectoryInfo dirInfo;
    private bool isCustomSoundLabel;
    private byte[] recordedAudioData;


    void Start()
    {
        Screen.orientation = ScreenOrientation.Portrait;
        isCustomSoundLabel = false;
        soundLabelText = "";
        customSoundLabelText = "";
        Init();
    }

    private void Init()
    {
        string lastState = currentRecordingState.Value;
        currentRecordingState.Value = recordingStates.Value[0];

        if (lastState != recordingStates.Value[1])
        {
            recordingTimerText.text = "00:00:00";
        }
    }

    //When start record button is clicked
    public void PrepareRecord()
    {
        if(joinedEvents.Value.Count == 0 || numberOfJoinedEvents.Value == 0)
        {
            joinEventText.gameObject.SetActive(true);
            return;
        }
        homePage.SetActive(false);
        RecordingPage.SetActive(true);
        currentRecordingState.Value = "Prepare";
        prepareStartTime = Time.realtimeSinceStartup;
        //soundLabelText = "";
        customSoundLabelText = "";

        if (!CheckPermission())
        {
            recordingStateText.text = "Requesting microphone permission...";
            StartCoroutine(TryGainPermission());
        }
        else
        {
            recordingStateText.text = "Recording";
            StartRecord();
        }
    }

    private bool CheckPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            return false;
        }
#elif UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            return false;
        }
#endif
        return true;
    }

    IEnumerator TryGainPermission()
    {
#if UNITY_ANDROID
        if (!Permission.HasUserAuthorizedPermission(Permission.Microphone)) {
            Permission.RequestUserPermission(Permission.Microphone);
        }
#elif UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        }
#endif
        yield return new WaitForSeconds(1);

        if (Time.realtimeSinceStartup - prepareStartTime <= 15f)
        {
            PrepareRecord();
        }
        else
        {
            recordingStateText.text = "Failed to enable microphone";
            Init();
        }
    }

    private void StartRecord()
    {
        currentRecordingState.Value = recordingStates.Value[2];
        if (audioClip != null)
        {
            AudioClip.DestroyImmediate(audioClip);
        }

        // Maximum record length is one hour
        audioClip = Microphone.Start(null, false, 3599, 44100);

        recordingTimerText.text = "00:00:00";

        StartCoroutine(WhileRecording());
    }

    IEnumerator WhileRecording()
    {
        float recordedPos = Microphone.GetPosition(null);
        float recordedTime = recordedPos / (float)(audioClip.channels * audioClip.frequency);
        UpdateTime(recordedTime);
        yield return new WaitForSeconds(0.05f);
        if(currentRecordingState.Value == recordingStates.Value[2])
        {
            StartCoroutine(WhileRecording());
        }
    }

    public void StopRecord()
    {
        StopCoroutine(WhileRecording());
        currentRecordingState.Value = recordingStates.Value[3];
        int recordEndPos = Microphone.GetPosition(null);
        Microphone.End(null);

        // No audio recorded
        if (recordEndPos <= 0)
        {
            Init();
            return;
        }

        ClipAudio(ref audioClip, recordEndPos);

        // Force GC to save memory
        System.GC.Collect();

        UpdateTime(audioClip.length);
    }

    public void PlayAudio()
    {
        Camera.main.GetComponent<AudioSource>().clip = audioClip;
        Camera.main.GetComponent<AudioSource>().Play();
    }

    private void ClipAudio(ref AudioClip clip, int endPos)
    {
        // from https://answers.unity.com/questions/544264/record-dynamic-length-from-microphone.html
        // Capture the current clip data
        var soundData = new float[endPos * clip.channels];
        clip.GetData(soundData, 0);

        // One does not simply shorten an AudioClip,
        //    so we make a new one with the appropriate length
        var newClip = AudioClip.Create(clip.name,
            endPos,
            clip.channels,
            clip.frequency,
            false);

        newClip.SetData(soundData, 0);        // Give it the data from the old clip

        // Replace the old clip
        AudioClip.DestroyImmediate(clip);
        clip = newClip;
    }

    private void UpdateTime(float rawTime)
    {
        int millisecond = Mathf.FloorToInt(rawTime * 1000) % 1000;
        int time = Mathf.FloorToInt(rawTime);
        recordingTimerText.text = $"{time / 60:D2}:{time % 60:D2}:{millisecond / 15:D2}";
    }

    public void OnUploadBtnClick(StringVariable eventId)
    {
        string tempLabel = "Custom";
        string serverCategory = tempLabel;
        string soundCustomLabel = "";
        OnSoundLabelOptionChange(soundLabelDropDown);
        
        if(audioClip != null)
        {
            if(isCustomSoundLabel)
            {
                if(customSoundLabelText == "")
                {
                    tempLabel = randomFileNameNumber.Value.ToString();
                    randomFileNameNumber.Value++;
                }
                else
                {
                    tempLabel = customSoundLabelText;
                    soundCustomLabel = tempLabel;
                }
            }
            else
            {
                if(soundLabelText == "")
                {
                    tempLabel = randomFileNameNumber.Value.ToString();
                    randomFileNameNumber.Value++;
                }
                else
                {
                    serverCategory = soundLabelText;
                    tempLabel = soundLabelText + soundLabelNumbers.Value[soundLabelIndex]++;
                    //randomFileNameNumber.Value++;
                }
            }

            string saveLabel = username.text + "_" + gamePieces.Value[chosenGamePieceIndex.Value] + "_" + tempLabel;
            recordedAudioData = SaveAudio.Save(saveLabel, audioClip, fileType.Value);
            int countTemp;

            tempLabel = tempLabel + "." + fileType.Value;
            int indexInFile = CheckIfInList(tempLabel);

            if(indexInFile == -1)
            {
                Debug.Log("df");
                soundItemsFilenames.Value.Add(tempLabel);
                audioLengthFile.Value.Add(recordingTimerText.text);

                if(PlayerPrefs.GetInt("NumberOfAudioFiles") >= 0)
                {
                    countTemp = PlayerPrefs.GetInt("NumberOfAudioFiles");
                    PlayerPrefs.SetInt("NumberOfAudioFiles", countTemp + 1);
                }
                else
                {
                    countTemp = 1;
                    PlayerPrefs.SetInt("NumberOfAudioFiles", countTemp);
                }
            }
            else
            {
                countTemp = indexInFile;
                updatedIndex.Value = indexInFile;
                audioLengthFile.Value[indexInFile] = recordingTimerText.text;
            }

            PlayerPrefs.SetString(countTemp.ToString(), tempLabel);
            PlayerPrefs.SetString("Length_" + countTemp.ToString(), recordingTimerText.text);

            StartCoroutine(UploadAudioToServer(serverCategory, soundCustomLabel, eventId.Value));

            soundLabelText = "";
        }
    }

    IEnumerator UploadAudioToServer(string category, string soundLabel, string eventId) {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormFileSection("file", recordedAudioData, "myfile.wav", "audio/wav"));

        if(string.IsNullOrEmpty(soundLabel))
        {
            var soundData = new JsonSoundDataWithoutLabel();
            soundData.game_meta = new JsonSoundGameMeta();
            soundData.meta = new JsonSoundMetaWithoutLabel();
            soundData.game_meta.model = gamePieces.Value[chosenGamePieceIndex.Value];
            soundData.meta.category = category;
            formData.Add(new MultipartFormDataSection("sound", JsonUtility.ToJson(soundData)));
        }
        else
        {
            var soundData = new JsonSoundDataWithLabel();
            soundData.game_meta = new JsonSoundGameMeta();
            soundData.meta = new JsonSoundMeta();
            soundData.game_meta.model = gamePieces.Value[chosenGamePieceIndex.Value];
            soundData.meta.category = category;
            soundData.meta.label = soundLabel;
            formData.Add(new MultipartFormDataSection("sound", JsonUtility.ToJson(soundData)));
        }

        UnityWebRequest www = UnityWebRequest.Post("https://echoes.etc.cmu.edu/api/viewer/events/" + eventId + "/sound", formData);
        www.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("token"));
        yield return www.SendWebRequest();
 
        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error + " : " + www.downloadHandler.text);
            soundsUploadingPage.transform.GetChild(1).gameObject.GetComponent<Text>().text = "Server Trouble. Please close the application and try again";
        }
        else {
            Debug.Log("Upload complete!");
            soundsUploadingPage.SetActive(false);
            galleryPage.SetActive(true);
        }

        Init();
    }

    int CheckIfInList(string fileName)
    {
        for(int i = 0; i < soundItemsFilenames.Value.Count; i++)
        {
            if(fileName == soundItemsFilenames.Value[i])
            {
                return i;
            }
        }
        return -1;
    }

    public void OnCustomizeLabel(InputField soundLabel)
    {
        // if(soundLabelIndex != 12)
        // {
        isCustomSoundLabel = !isCustomSoundLabel;
        soundLabel.gameObject.SetActive(!soundLabel.gameObject.activeSelf);
        // }
        // else
        // {
        //     isCustomSoundLabel = true;
        //     soundLabel.gameObject.SetActive(true);
        //     soundLabel.transform.parent.GetChild(0).gameObject.GetComponent<Toggle>().isOn = true;
        // }
    }

    public void OnCustomLabelValueChange(Text t)
    {
        customSoundLabelText = t.text;
    }

    //when back home or save is clicked
    public void ResetValuesInSoundLabelling(GameObject dropDownObject)
    {
        isCustomSoundLabel = false;
        int index = dropDownObject.transform.parent.GetSiblingIndex() + 1;
        Transform customLabel = dropDownObject.transform.parent.parent.GetChild(index);
        // customLabel.GetChild(2).gameObject.SetActive(false);
        customLabel.GetChild(0).gameObject.GetComponent<Toggle>().isOn = false;
    }

    public void OnSoundLabelOptionChange(GameObject dropDownObject)
    {
        soundLabelIndex = dropDownObject.GetComponent<Dropdown>().value;
        soundLabelText = dropDownObject.transform.GetChild(0).gameObject.GetComponent<Text>().text;
        Debug.Log("came here : " + soundLabelText);
        //soundLabelNumbers.Value[soundLabelIndex]++;

        // if(soundLabelIndex == 12)
        // {
        //     isCustomSoundLabel = true;
        //     int index = dropDownObject.transform.parent.GetSiblingIndex() + 1;
        //     Transform customLabel = dropDownObject.transform.parent.parent.GetChild(index);
        //     customLabel.GetChild(2).gameObject.SetActive(true);
        //     customLabel.GetChild(0).gameObject.GetComponent<Toggle>().isOn = true;
        // }
        // else
        // {
        isCustomSoundLabel = false;
        int index = dropDownObject.transform.parent.GetSiblingIndex() + 1;
        Transform customLabel = dropDownObject.transform.parent.parent.GetChild(index);
        // customLabel.GetChild(2).gameObject.SetActive(false);
        customLabel.GetChild(0).gameObject.GetComponent<Toggle>().isOn = false;
        //}
    }

    public void GamepieceUpdate(ListOfStringsVariable gamePiecesList, int index)
    {
        StartCoroutine(UploadGamePieceInServer(gamePiecesList, index));
    }

    IEnumerator UploadGamePieceInServer(ListOfStringsVariable gamePiecesList, int index)
    {
   
        JsonGamePieceDataUpdate gamePieceData = new JsonGamePieceDataUpdate();
        gamePieceData.game_meta = new JsonGamePieceData();
        Debug.Log("index : " + index);
        gamePieceData.game_meta.model = gamePiecesList.Value[index];

        string jsonData = gamePieceData.SaveToString();

        var request = UnityWebRequest.Put("https://echoes.etc.cmu.edu/api/users/info/", jsonData);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("token"));
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if(request.isNetworkError || request.isHttpError) {
            Debug.Log("Request error");
            Debug.Log(request.error);
            Debug.Log(request.downloadHandler.text);
            yield break;
        }
        
        Debug.Log("success");
    }
}
