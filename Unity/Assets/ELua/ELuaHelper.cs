using UnityEngine;
using System.IO;
using System.Collections.Generic;

/// <summary>
/// 根据平台加载lua代码
/// </summary>
public class ELuaHelper
{
    //脚本在Resources下的路径
    public const string ScriptUnityPath = "Scripts/Lua/";

    /// <summary>
    /// 获取指定文件内容
    /// </summary>
    public static string GetScript(string filename)
    {
        string text = string.Empty;
        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer ||
            //Application.platform == RuntimePlatform.WindowsEditor ||
            Application.platform == RuntimePlatform.WindowsPlayer)
        {
            text = GetTextFromFile(filename);
        }
        else
        {
            text = GetTextFromUnity(filename);
        }
        return text;
    }

    /// <summary>
    /// 从Unity中是否lua脚本到目标平台中
    /// </summary>
    public static bool Release()
    {
        try
        { 
            List<string>[] floder = new List<string>[10];
            TextAsset[] AllList = Resources.LoadAll<TextAsset>(ScriptUnityPath);

            foreach (TextAsset t in AllList)
            {
                if (t.name == "__pack__")
                {
                    int deep = t.text.Split('/').Length;
                    if (floder[deep] == null)
                    {
                        floder[deep] = new List<string>();
                    }
                    floder[deep].Add(t.text);
                }
            }
            floder[0] = new List<string>();
            floder[0].Add("");

            List<int> SaveList = new List<int>();

            for (int i = 10 - 1; i >= 0; i--)
            {
                if (floder[i] == null) continue;
                foreach (string f in floder[i])
                {
                    TextAsset[] flist = Resources.LoadAll<TextAsset>(ScriptUnityPath+f);
                    foreach (TextAsset lua in flist)
                    {
                        if(!SaveList.Contains(lua.GetHashCode()))
                        {
                            SaveFile(f,lua.name, lua.text);
                            SaveList.Add(lua.GetHashCode());
                        }
                    }
                }
            }
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }
        finally
        {
            Resources.UnloadUnusedAssets();
        }
        return true;
    }

    /// <summary>
    /// 获取在当前环境脚本的存放位置
    /// </summary>
    private static string GetScriptsSavePath()
    {
        string path = Application.persistentDataPath + "/Scripts/Lua/";
        switch(Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
                path = Application.dataPath + "/../Scripts/Lua/";
                break;
        }
        return path;
    }

    /// <summary>
    /// 保存内容到文件
    /// </summary>
    private static void SaveFile(string path,string name,string text)
    {
#if !UNITY_WEBPLAYER
        path = path.Trim('/');
        path = GetScriptsSavePath() + path;
        CreateDirctory(path);
        File.WriteAllText(path+"/"+name, text);
#endif
    }


    /// <summary>
    /// 创建多层次文件夹
    /// </summary>
    private static void CreateDirctory(string path)
    {
#if !UNITY_WEBPLAYER
        path = path.Replace('\\', '/');
        bool isFull = path[0] != '/';
        string[] dirs = path.Split('/');
        string dir = "";
        for (int i = 0; i < dirs.Length; i++)
        {
            if (i != 0 || !isFull)
            {
                dir += "/";
            }
            dir += dirs[i];

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
#endif
    }

    /// <summary>
    /// 从resources下获取脚本
    /// </summary>
    private static string GetTextFromUnity(string path)
    {
        TextAsset text = Resources.Load(ScriptUnityPath + path + ".lua") as TextAsset;
        if (text != null)
        {
            return text.text;
        }
        return null;
    }

    /// <summary>
    /// 从文件中获取脚本
    /// </summary>
    private static string GetTextFromFile(string path)
    {
#if !UNITY_WEBPLAYER
        string file = GetScriptsSavePath() + path + ".lua";
        if(System.IO.File.Exists(file))
        {
            return System.IO.File.ReadAllText(file);
        }
#endif
        return null;
    }
}