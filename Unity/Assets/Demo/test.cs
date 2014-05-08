using UnityEngine;
using System.Collections;

public class test : MonoBehaviour 
{

	void Start () 
    {
        if (PlayerPrefs.GetInt("LuaRelease", 0) == 0)
        {
            ELuaHelper.Release();
            PlayerPrefs.SetInt("LuaRelease", 1);
        }

        //GameObject.GameObject.CreatePrimitive(PrimitiveType.Cube);
	}
}
