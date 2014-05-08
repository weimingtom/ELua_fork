using UnityEngine;
using LuaInterface;
using System;
using System.Collections.Generic;

public class ELua : MonoBehaviour
{
    /// <summary>
    /// 是否是debug模式
    /// </summary>
    public bool IsDebug = false;

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

        SetBaseData(_lua);

        //载入lua框架代码
        RequireBase("init");

        //执行用户代码
        if (!string.IsNullOrEmpty(Script))
        {
            Require(Script);
        }
    }

    //将常用的变量注入到Lua代码中
    void SetBaseData(Lua lua)
    {
        lua["LuaUnity"]     = this;
        lua["IsDebug"]      = IsDebug;
        lua["gameObject"]   = gameObject;
        lua["transform"]    = transform;
    }

    #endregion

    /// <summary>
    /// 设置Lua总的变量
    /// </summary>
    public void SetData(string key, object data)
    {
        _lua[key] = data;
    }

    /// <summary>
    /// 在GameObject上增加一个脚本
    /// </summary>
    public static ELua Add(GameObject obj, string script)
    {
        ELua lua = obj.AddComponent<ELua>();
        lua.Script = script;
        return lua;
    }

    /// <summary>
    /// 获取lua中的变量值
    /// </summary>
    public object GetData(string name)
    {
        return _lua[name];
    }

    /// <summary>
    /// 执行lua脚本
    /// </summary>
    void RunScript(string text)
    {
        try
        {
            _lua.DoString(text);
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            this.enabled = false;
        }
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
        string text = ELuaHelper.GetScript(filename);
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
    public object[] CallLua(string func)
    {
        try
        {
            if (_lua == null)
            {
                return null;
            }
            if (!Functions.ContainsKey(func))
            {
                Functions[func] = _lua.GetFunction(func);
            }
            if (Functions[func] != null)
            {
                return Functions[func].Call();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Call Fcun Error:" + func +"\n"+ e.Message);
            this.enabled = false;
        }

        return null;
    }

    #region Call Methond
    void Awake()
    {
        Run();

        CallLua("Awake");
    }

	void Start()
	{
        CallLua("Start");
	}

    void Update()
    {
        CallLua("Update");
    }

    void LateUpdate()
    {
        CallLua("LateUpdate");
    }

    void FixedUpdate()
    {
        CallLua("FixedUpdate");
    }

    void OnEnable()
    {
        CallLua("OnEnable");
    }

    void OnDisable()
    {
        CallLua("OnDisable");
    }

    void OnDestroy()
    {
        CallLua("OnDestroy");
    }

    void OnGUI()
    {
        CallLua("OnGUI");
    }
#endregion
}