#一个易用的UnityLua框架

#已定义类型
UnityEngine		= luanet.UnityEngine
System			= luanet.System
Debug			= UnityEngine.Debug
GameObject		= UnityEngine.GameObject
Transfrom		= UnityEngine.Transfrom
Vector2			= UnityEngine.Vector2
Vector3			= UnityEngine.Vector3
Time			= UnityEngine.Time
GUI				= UnityEngine.GUI
Rect			= UnityEngine.Rect

#已定义函数
function Print(str)
function Error(str)
function require(filename)
function Find(str)

#demo

function Awake()
	Print('Awake Time:' .. Time.time)
end

function Start()
	Debug.Log("test2 lua Start");
	Print(gameObject.name);
	transform.localPosition = Vector3(-100,0,0)
end

function Update()
	--Print('Update:' .. Time.deltaTime)
	transform:RotateAround(Vector3.zero, Vector3.up,20 * Time.deltaTime)
end