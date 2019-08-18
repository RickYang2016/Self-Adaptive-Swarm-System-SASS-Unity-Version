using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using UnityEngine;

[Serializable]
class Data
{
    public List<Frame> infolist;

    public override string ToString()
    {
        string tmp = "";
        foreach(Frame one in infolist)
        {
            tmp += one+"\n";
        }

        return tmp;
    }

}

[Serializable]
class Frame
{
    public string name;
    public string age;        

    public override string ToString()
    {
        return "name: " + name + " age: " + age;
    }
}

public class GetSocket : MonoBehaviour
{
    private TextMesh text;
    private string message;
    private Socket client;
    private string host = "127.0.0.1";
    private int port = 10086;
    private byte[] messTmp;

    // Start is called before the first frame update
    void Start()
    {
        text = GameObject.FindGameObjectWithTag("text").GetComponent<TextMesh>();
        messTmp = new byte[1024];

        // 构建一个Socket实例，并连接指定的服务端。这里需要使用IPEndPoint类(ip和端口号的封装)
        client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            client.Connect(new IPEndPoint(IPAddress.Parse(host), port));
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return;
        }

        //client.Close();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Data ReadToObject(string json)
    {
        Data deserializedUser = new Data();               
        deserializedUser = (Data)JsonUtility.FromJson(json, deserializedUser.GetType());
        return deserializedUser;
    }

    void GetMessage()
    {
        var count = client.Receive(messTmp);

        if (count != 0)
        {
            Data frame = ReadToObject(Encoding.UTF8.GetString(messTmp, 1, count - 2));
            message = frame.ToString();
            Array.Clear(messTmp, 0, count);
        }
    }

    void FixedUpdate()
    {
        GetMessage();
        text.text = message;
    }
}
