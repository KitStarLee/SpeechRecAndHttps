using UnityEngine;
using System.Collections;
using LitJson;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class SpeechRecognition : MonoBehaviour {

    public HorizontalLayoutGroup text_Group_layout;
    private Text[] textgroup;
    public Text show_text;

    public RecordingWav recordingWav;

    string filePath = null;

    string format = "wav";
    int rate = 8000;
    int channel = 1;
    string cuid = "yxh5274";
    string lan = "zh";
    string token;
    string speech;
    int len;

   
    // Use this for initialization
    void Start () {

        filePath =
#if UNITY_ANDROID
 Application.persistentDataPath + "/Microphone.wav";
#elif UNITY_EDITOR
 Application.dataPath + "/StreamingAssets" + "/Microphone.wav";
#endif


        if (text_Group_layout == null)
            return;

        textgroup = text_Group_layout.gameObject.GetComponentsInChildren<Text>();

    }
    public void UploadAudio()
    {
        Debug.Log("Star Upload.....");
        StartCoroutine(GetIdentifyWords());
    }

    void MatchTheWords(string needTest)
    {
        Debug.Log(needTest);
        for (int i = 0;i < textgroup.Length;i++)
        {
            if(needTest.Contains(textgroup[i].text))
            {
                textgroup[i].color = Color.green;
            }
            else
            {
                textgroup[i].color = Color.red;
            }
        }
    }


    string client_id = "cy57gvVAsEpxHpwtBB9zI9np";
    string client_secret = "T0QiUe8DGXUIS0mIABodTXGhVViTtGYr";
    string url_token = "https://openapi.baidu.com/oauth/2.0/token";


    string url_api = "http://vop.baidu.com/server_api";
   
    string post_string;
    IEnumerator GetIdentifyWords()
    {
        FileStream audio = new FileStream(filePath, FileMode.Open);
  
        BinaryReader br = new BinaryReader(audio);

        len = (int)audio.Length;

        Debug.Log(" 文件大小 ： " + len);

        byte[] buffer = new byte[len];
        br.Read(buffer, 0, buffer.Length);

        speech = Convert.ToBase64String(buffer);

       // Debug.Log("likai :  " + speech);

        if (recordingWav.speech_Byte != null)
        {
           // speech = Convert.ToBase64String(recordingWav.speech_Byte);
          //  Debug.Log("likai :  " + speech);
        }else
        {
           // Debug.Log(" 你所要取的信息为空 ");
        }
       


        audio.Close();
        br.Close();
        
        

        #region GetToken
        WWWForm form1 = new WWWForm();
        form1.AddField("grant_type", "client_credentials");
        form1.AddField("client_id", client_id);
        form1.AddField("client_secret", client_secret);

        WWW w1 = new WWW(url_token, form1);
        yield return w1;

        JsonData jd1 = JsonMapper.ToObject(w1.text);
        token = (string)jd1["access_token"];

        #endregion


        if (token == null || speech == null)
            Debug.Log("参数信息不够");
        yield return 0;
        
        var request = new UnityWebRequest(url_api, "POST");


        JsonData data = new JsonData();
        data["format"] = format;
        data["rate"] = rate;
        data["channel"] = channel;
        data["cuid"] = cuid;
        data["token"] = token;
        data["speech"] = speech;
        data["len"] = len;
        data["lan"] = lan;
        Byte[] post_byte = Encoding.Default.GetBytes(data.ToJson());

        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(post_byte);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.Send();

       // Debug.Log(request.downloadHandler.text);

        JsonData result = JsonMapper.ToObject(request.downloadHandler.text);

        MatchTheWords(result["result"][0].ToString());

        if(show_text != null)
        {
            show_text.text = result["result"][0].ToString();
        }

    }



}
