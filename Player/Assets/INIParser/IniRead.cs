using UnityEngine;
using System;

public class IniRead : MonoBehaviour
{
    public string INIPath;
    public static IniRead Instance;
    public float Aa;
    public float Bb;
    //public float SetPrimaryPlayerPosXOffset;
    //public float SetPrimaryPlayerPosZOffset;
    void Awake()
    {
        Instance = this;
        INIPath = Application.streamingAssetsPath + "/分辨率配置.ini";
        IniReadFile(INIPath);
    }
    void IniReadFile(string path)
    {
        INIParser iniParser = new INIParser();
        iniParser.Open(path);
        Aa = Convert.ToSingle(iniParser.ReadValue("AA", "a", 0d));
        Bb = Convert.ToSingle(iniParser.ReadValue("Bb", "b", 0d));
        //SetPrimaryPlayerPosXOffset = Convert.ToSingle(iniParser.ReadValue("SetPrimaryPlayerPos", "xOffset", 0.15d));
        //SetPrimaryPlayerPosZOffset = Convert.ToSingle(iniParser.ReadValue("SetPrimaryPlayerPos", "zOffset", 0.15d));
        Debug.Log("Aa="+Aa);
        Debug.Log("Bb=" + Bb);
        iniParser.Close();
    }
}