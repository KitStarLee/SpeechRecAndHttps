using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Net.Security;

public class SGMWPETH5 : MonoBehaviour {
    
    string filePath = null;


    X509Certificate2 adminClient;
    string clientCertPath = "";

    //服务器URL
    static string baseUrl = "https://192.168.1.69:10010";


    // Use this for initialization
    void Start () {

        filePath =
#if UNITY_ANDROID
 Application.persistentDataPath;
#elif UNITY_EDITOR
 Application.dataPath + "/StreamingAssets" ;
#endif

       
      //  StartCoroutine(GETHWPETHSession());

        clientCertPath = filePath + "/client.p12";
        
        Debug.Log(clientCertPath);
        StartCoroutine(GetCert());

    }
	
	// Update is called once per frame
	void Update () {
	
	}
  
    public void OnButtonClientToServer()
    {
        StartCoroutine(GetCert());
    }

    /// <summary>
    /// 检查client.p12文件是否存在。大概由于U3D打包压缩什么的。如果在打包的时候连同这个文件一起打包。在运行软件的时候是没法访问到这个文件的。所以不存在还需要先下载一边。
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetCert()
    {
        if (File.Exists(clientCertPath))
        {
            //如果文件存在。那么直接跳到高潮吧
            MakeRestCall();
        }
        else
        {
            //此处由于服务器还没有相应的Get这个文件的请求。所以使用了HFS这个网络文件服务器来下载client.p12文件。
            WWW download = new WWW("http://192.168.1.69/client.p12");
            yield return download;
            
            if (download.error != null)
            {
                print("Error downloading: " + download.error);
            }
            else
            {
                File.WriteAllBytes(clientCertPath, download.bytes);

            }
        }
    }

    public XmlDocument MakeRestCall()
    {
        try
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(ValidateServerCertificate);

            adminClient = new X509Certificate2(clientCertPath, "123456");

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(baseUrl);
            

            request.AuthenticationLevel = AuthenticationLevel.MutualAuthRequested;
            
            request.ClientCertificates.Add(adminClient);
            
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            
            Debug.Log(response.StatusDescription);
            
            XmlDocument xmlDoc = new XmlDocument();


            return xmlDoc;
        }
        catch(WebException ex)
        {
            XmlDocument xmlDoc = new XmlDocument();
            return xmlDoc;
        }
       
    }

    public static bool ValidateServerCertificate(
        object sender,
        X509Certificate certificate,
        X509Chain chain,
        SslPolicyErrors sslPolicyErrors)
        {
            //TODO Make this more secure
            return true;
        }





}
