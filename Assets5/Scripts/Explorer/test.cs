using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.IO;

public class test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RunPythonInCs();
    }

    private void RunPythonInCs()
    {
        string tmp = "python /home/herobot/Documents/research/code/SGDT_version_2.py";

        Process p = new Process();
        p.StartInfo.FileName = "sh";
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        p.StandardInput.WriteLine(tmp);
        p.StandardInput.WriteLine("exit");
        string strResult = p.StandardOutput.ReadToEnd();
        print(strResult);
        p.Close();
    }
}
