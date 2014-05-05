using UnityEngine;
using LuaInterface;
using System.Collections.Generic;
using LuaInterface;

public class LuaUnity : MonoBehaviour
{
    /// <summary>
    /// 是否是debug模式
    /// </summary>
    public bool IsDebug = true;

    /// <summary>
    /// 需要执行脚本列表
    /// </summary>
    public string Script;

    //lua框架文件的基础路径
    private const string BaseLuaScriptPath = "LuaBase/";

    //加载到的lua脚本，避免重入载入
    private static Dictionary<string, string> LoadLuaScript = new Dictionary<string, string>();

    #region RUN LUA

    Lua _lua;

    /// <summary>
    /// 缓存调用过的Methond
    /// </summary>
    Dictionary<string, LuaFunction> Functions = new Dictionary<string, LuaFunction>();

    /// <summary>
    /// 启动lua实例
    /// </summary>
    void Run()
    {
        _lua = new Lua();

        SetData(_lua);

        //载入lua框架代码
        RequireBase("init");

        //执行用户代码
        if (!string.IsNullOrEmpty(Script))
        {
            Require(Script);
        }
    }

    //将常用的变量注入到Lua代码中
    void SetData(Lua lua)
    {
        lua["LuaUnity"]     = this;
        lua["IsDebug"]      = IsDebug;
        lua["gameObject"]   = gameObject;
        lua["transform"]    = transform;
    }

    #endregion

    /// <summary>
    /// 执行lua脚本
    /// </summary>
    void RunScript(string text)
    {
        _lua.DoString(text);
    }

    /// <summary>
    /// 从resources下获取脚本
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
    /// 从文件中获取脚本
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

    #region 提供给lua的方法

    /// <summary>
    /// 包含lua文件
    /// </summary>
    public bool Require(string filename)
    {
        //已经加载了文件，直接执行
        if (LoadLuaScript.ContainsKey(filename))
        {
            RunScript(LoadLuaScript[filename]);
            return true;
        }

        //加载lua代码
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

        //未加载到文件
        return false;
    }

    /// <summary>
    /// 加载lua框架文件
    /// </summary>
    public bool RequireBase(string filename)
    {
        //已经加载了文件，直接执行
        if (LoadLuaScript.ContainsKey(filename))
        {
            RunScript(LoadLuaScript[filename]);
            return true;
        }

        //加载lua代码
        TextAsset text= Resources.Load(BaseLuaScriptPath + filename+".lua") as TextAsset;
        if (text != null)
        {
            RunScript(text.text);
            LoadLuaScript[filename] = text.text;
            return true;
        }

        //未加载到文件
        return false;
    }

    #endregion

    /// <summary>
    /// 执行一个LUA方法
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