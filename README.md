#ELua简介

    ELua 是基于KopiLua和LuaInterface的一款Unity3d的lua脚本框架。可以在Unity3d中快速的加入Lua脚本支持。

![demo](http://git.oschina.net/ximu/ELua/raw/master/Src/Images/demo.png "demo")

#特性:

    1)使用简单，基于KopiLua全平台支持  
    2)基础类型定义，无需重复工作  
    3)自动释放Unity中Lua到指定平台，方便脚本更新  
    4)require脚本包含支持  
    5)MonoBehaviour流程Lua中自动调用  
    6)多层目录支持


#目录结构

    /ELua   		        #lua框架
	/ELua/KopiLua 		        #KopiLua主程序和Luainterface 等依赖Dll
	/ELua/Resources		        #框架依赖资源
	/ELua/Resources/LuaBase		#框架基础Lua脚本
	/Demo			        #框架使用demo



#简单使用:

1)ELua会从Resources/Scripts/Lua 中加载lua脚本

创建lua代码demo.lua.txt
	
	function Start()
		Print('Start')
	end

2)在Gameobjcet上添加组件Elua在Elua的Script的变量中填入demo

3)运行游戏即可在console中看到Lua脚本打印的日志


#其他说明:

    1）文件夹需要添加__pack__.txt 文件内容添加当前文件夹相对路径。ELua才可以按目录将Lua代码释放到指定平台中。  
    2）调用ELuaHelper.Release() 可以将Resources/Scripts/Lua 中脚本释放到文件中。    
    3）webplayer下会直接运行包含在Unity中的Lua代码，而不释放出来。