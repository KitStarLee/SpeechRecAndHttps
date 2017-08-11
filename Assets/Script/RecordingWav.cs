using UnityEngine;
using System.IO;
using System;
using UnityEngine.UI;
using System.Text;
using UnityEngine.EventSystems;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class RecordingWav : MonoBehaviour,IPointerDownHandler, IPointerUpHandler
{
    public Image imageButton;
    public Button recordingButton;
    public Text show_text;

    string filePath = null;
    int audioLength_time;

    private AudioSource m_audioSource;
    private AudioClip m_audioClip;

    public const int SamplingRate = 8000;
    private const int HEADER_SIZE = 44;

    public SpeechRecognition speechRecognition;

    [HideInInspector]
    public bool isRecording = false;

    [HideInInspector]
    public Byte[] speech_Byte;
    

    // Use this for initialization  


    void Start()
    {

        m_audioSource = GetComponent<AudioSource>();

        filePath =
#if UNITY_ANDROID
 Application.persistentDataPath + "/Microphone.wav";
#elif UNITY_EDITOR
 Application.dataPath + "/StreamingAssets" + "/Microphone.wav";
#endif

        
        

    }

    public void StartRecording( bool isRecording)
    {
        if (isRecording)
        {
            Microphone.End(null);
            foreach (string d in Microphone.devices)
            {
                Debug.Log("Devid :" + d);
            }

            m_audioClip = Microphone.Start(null, false, 60, SamplingRate);

            imageButton.color = Color.red;
        }
        else
        {
            imageButton.color = Color.blue;

            audioLength_time = 0;
            int lastPos = Microphone.GetPosition(null);

            if (Microphone.IsRecording(null))
            {
                audioLength_time = lastPos / SamplingRate;
            }
            else
            {
                audioLength_time = 0;
                Debug.Log("error : 录音时间太短");
            }
            Microphone.End(null);

            if (audioLength_time <= 1.0f) return;

            SaveWav(filePath, m_audioClip);

        }
    }
    public void PlayAudioClip()
    {
        if (m_audioClip.length > 5 && m_audioClip != null)
        {
            if (m_audioSource.isPlaying)
            {
                m_audioSource.Stop();
            }
            Debug.Log("Channel :" + m_audioClip.channels + " ;Samle :" + m_audioClip.samples + " ;frequency :" + m_audioClip.frequency + " ;length :" + m_audioClip.length);
            m_audioSource.clip = m_audioClip;
            m_audioSource.Play();
        }
    }

    bool SaveWav(string filename, AudioClip clip)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Debug.Log("please save the file and delete is later");
        }

        try
        {
            FileStream fileStream;

            FileInfo info = new FileInfo(filePath);
            if (!info.Exists)
            {
                fileStream = info.Create();
            }
            else
            {
                fileStream = info.OpenWrite();
            }
         
           // using (FileStream fileStream = CreateEmpty(filePath))
         //   {
                ConvertAndWrite(fileStream, clip);

          //  }
            return true;
        }
        catch (Exception ex)
        {
            Debug.Log("error : " + ex);
          //  show_text.text = "error : " + ex;
            return false;
        }

    }

    FileStream CreateEmpty(string filePath)
    {
        FileStream fileStream = new FileStream(filePath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < HEADER_SIZE; i++)
        {
            fileStream.WriteByte(emptyByte);
        }
        return fileStream;
    }
    void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        int actual_Length = (audioLength_time + 1) * SamplingRate * 2;
        //防止数据丢失，多加一秒的时间

        float[] samples = new float[actual_Length];

        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        //converting in 2 float[] steps to Int16[], //then Int16[] to Byte[]  

        Byte[] bytesData = new Byte[samples.Length * 2];
        //bytesData array is twice the size of  
        //dataSource array because a float converted in Int16 is 2 bytes.  


        int rescaleFactor = 32767; //to convert float to Int16  

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);

            // bytesData = BitConverter.GetBytes(intData[i]);

            Byte[] byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        speech_Byte = null;


        Stream stream = new MemoryStream(bytesData, true);

        BinaryReader br = new BinaryReader(stream);

        byte[] buffer = new byte[actual_Length];
        br.Read(buffer, 0, buffer.Length);

        string speech = Convert.ToBase64String(buffer);



        //Debug.Log("speech_Byte str1：" + speech);

        Debug.Log("录音时间 ：" + audioLength_time);
        Debug.Log("Byte大小 ：" + bytesData.Length);
        Debug.Log("samples lenght : " + samples.Length);
        Debug.Log("实际录取长度: " + actual_Length);


        StartCoroutine(WriteFileStream(fileStream, bytesData , actual_Length));

        //WriteHeader(fileStream, clip, actual_Length);
        
    }

    IEnumerator WriteFileStream(FileStream fileStream, Byte[] bytesData, int actual_Length)
    {
        fileStream.Write(bytesData, 0, actual_Length);
        show_text.text = " ... ";
        yield return new WaitForSeconds(audioLength_time);

        fileStream.Close();
        fileStream.Dispose();

        Debug.Log(" OK ");

        speechRecognition.UploadAudio();
    }


    void WriteHeader(FileStream fileStream, AudioClip clip, int samplesLen)
    {

        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = samplesLen;

        fileStream.Seek(0, SeekOrigin.Begin);

        Byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        Byte[] chunkSize = BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        Byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        Byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        Byte[] subChunk1 = BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 two = 2;
        UInt16 one = 1;

        Byte[] audioFormat = BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        Byte[] numChannels = BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        Byte[] sampleRate = BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        Byte[] byteRate = BitConverter.GetBytes(hz * channels * 2); // sampleRate * bytesPerSample*number of channels, here 44100*2*2    
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        Byte[] bitsPerSample = BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        Byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        Byte[] subChunk2 = BitConverter.GetBytes(samples * 2 * channels);
        fileStream.Write(subChunk2, 0, 4);




        fileStream.Close();
        fileStream.Dispose();
        Debug.Log(" OK ");

    }

   

    public void OnPointerDown(PointerEventData eventData)
    {
        StartRecording(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        StartRecording(false);
    }
}