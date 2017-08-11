using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class test : MonoBehaviour
{
    //文本中每行的内容
    ArrayList infoall;

    public Text show_text;
    //皮肤资源，这里用于显示中文
    public GUISkin skin;
    void Start()
    {

        //删除文件
        DeleteFile(Application.persistentDataPath, "FileName.txt");

        //创建文件，共写入3次数据
        CreateFile(Application.persistentDataPath, "FileName.txt", "宣雨松MOMO");
        CreateFile(Application.persistentDataPath, "FileName.txt", "宣雨松MOMO");
        CreateFile(Application.persistentDataPath, "FileName.txt", "宣雨松MOMO");
        //得到文本中每一行的内容
        infoall = LoadFile(Application.persistentDataPath, "FileName.txt");

    }

    /**
    * path：文件创建目录
    * name：文件的名称
    *  info：写入的内容
    */
    void CreateFile(string path, string name, string info)
    {
        //文件流信息
        StreamWriter sw;
        FileInfo t = new FileInfo(path + "//" + name);

        show_text.text = path + "//" + name;
        if (!t.Exists)
        {
            //如果此文件不存在则创建
            sw = t.CreateText();
        }
        else
        {
            //如果此文件存在则打开
            sw = t.AppendText();
        }
        //以行的形式写入信息
        sw.WriteLine(info);
        //关闭流
        sw.Close();
        //销毁流
        sw.Dispose();
    }

    /**
     * path：读取文件的路径
     * name：读取文件的名称
     */
    ArrayList LoadFile(string path, string name)
    {
        //使用流的形式读取
        StreamReader sr = null;
        try
        {
            sr = File.OpenText(path + "//" + name);
        }
        catch (Exception e)
        {
            //路径与名称未找到文件则直接返回空
            return null;
        }
        string line;
        ArrayList arrlist = new ArrayList();
        while ((line = sr.ReadLine()) != null)
        {
            //一行一行的读取
            //将每一行的内容存入数组链表容器中
            arrlist.Add(line);
        }
        //关闭流
        sr.Close();
        //销毁流
        sr.Dispose();
        //将数组链表容器返回
        return arrlist;
    }

    /**
     * path：删除文件的路径
     * name：删除文件的名称
     */

    void DeleteFile(string path, string name)
    {
        File.Delete(path + "//" + name);

    }

    void OnGUI()
    {
        //用新的皮肤资源，显示中文
        GUI.skin = skin;
        //读取文件中的所有内容
        foreach (string str in infoall)
        {
            //绘制在屏幕当中，哇咔咔！
            GUILayout.Label(str);
        }

    }

}