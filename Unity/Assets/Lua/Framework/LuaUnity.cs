using UnityEngine;
using LuaInterface;
using System.Collections.Generic;
using LuaInterface;

public class LuaUnity : MonoBehaviour
{
    /// <summary>
    /// �Ƿ���debugģʽ
    /// </summary>
    public bool IsDebug = true;

    /// <summary>
    /// ��Ҫִ�нű��б�
    /// </summary>
    public string Script;

    //lua����ļ��Ļ���·��
    private const string BaseLuaScriptPath = "LuaBase/";

    //���ص���lua�ű���������������
    private static Dictionary<string, string> LoadLuaScript = new Dictionary<string, string>();

    #region RUN LUA

    Lua _lua;

    /// <summary>
    /// ������ù���Methond
    /// </summary>
    Dictionary<string, LuaFunction> Functions = new Dictionary<string, LuaFunction>();

    /// <summary>
    /// ����luaʵ��
    /// </summary>
    void Run()
    {
        _lua = new Lua();

        SetData(_lua);

        //����lua��ܴ���
        RequireBase("init");

        //ִ���û�����
        if (!string.IsNullOrEmpty(Script))
        {
            Require(Script);
        }
    }

    //�����õı���ע�뵽Lua������
    void SetData(Lua lua)
    {
        lua["LuaUnity"]     = this;
        lua["IsDebug"]      = IsDebug;
        lua["gameObject"]   = gameObject;
        lua["transform"]    = transform;
    }

    #endregion

    /// <summary>
    /// ִ��lua�ű�
    /// </summary>
    void RunScript(string text)
    {
        _lua.DoString(text);
    }

    /// <summary>
    /// ��resources�»�ȡ�ű�
    /// </summary>
    string GetTextFromUnity(string path)
    {
        TextAsset text = Resources.Load("Scripts/Lua/" + path+".lua") as TextAsset;
        if (text != null)
        {
            return text.text;
        }
        return null;
    }

    /// <summary>
    /// ���ļ��л�ȡ�ű�
    /// </summary>
    string GetTextFromFile(string path)
    {
#if !UNITY_WEBPLAYER
        path = Application.persistentDataPath + "/Scripts/Lua/" + path +".lua.txt";
        if(System.IO.File.Exists(path))
        {
            return System.IO.File.ReadAllText(path);
        }
        return null;
#endif
    }

    #region �ṩ��lua�ķ���

    /// <summary>
    /// ����lua�ļ�
    /// </summary>
    public bool Require(string filename)
    {
        //�Ѿ��������ļ���ֱ��ִ��
        if (LoadLuaScript.ContainsKey(filename))
        {
            RunScript(LoadLuaScript[filename]);
            return true;
        }

        //����lua����
        string text = string.Empty;
        if (Application.platform == RuntimePlatform.Android ||
            Application.platform == RuntimePlatform.IPhonePlayer)
        {
            text = GetTextFromFile(filename);
        }
        else
        {
            text = GetTextFromUnity(filename);
        }

        if (!string.IsNullOrEmpty(text))
        {
            RunScript(text);
            LoadLuaScript[filename] = text;
            return true;
        }

        //δ���ص��ļ�
        return false;
    }

    /// <summary>
    /// ����lua����ļ�
    /// </summary>
    public bool RequireBase(string filename)
    {
        //�Ѿ��������ļ���ֱ��ִ��
        if (LoadLuaScript.ContainsKey(filename))
        {
            RunScript(LoadLuaScript[filename]);
            return true;
        }

        //����lua����
        TextAsset text= Resources.Load(BaseLuaScriptPath + filename+".lua") as TextAsset;
        if (text != null)
        {
            RunScript(text.text);
            LoadLuaScript[filename] = text.text;
            return true;
        }

        //δ���ص��ļ�
        return false;
    }

    #endregion

    /// <summary>
    /// ִ��һ��LUA����
    /// </summary>
    public object[] CallMethond(string func)
    {
        if (!Functions.ContainsKey(func))
        {
            Functions[func] = _lua.GetFunction(func);
        }

        if (Functions[func] != null)
        {
            return Functions[func].Call();
        }
        return null;
    }

    #region Call Methond
    void Awake()
    {
        Run();

        CallMethond("Awake");
    }

	void Start()
	{
        CallMethond("Start");
	}

    void Update()
    {
        CallMethond("Update");
    }

    void LateUpdate()
    {
        CallMethond("LateUpdate");
    }

    void FixedUpdate()
    {
        CallMethond("FixedUpdate");
    }

    void OnEnable()
    {
        CallMethond("OnEnable");
    }

    void OnDisable()
    {
        CallMethond("OnDisable");
    }

    void OnDestroy()
    {
        CallMethond("OnDestroy");
    }

    void OnGUI()
    {
        CallMethond("OnGUI");
    }
#endregion
}