using System;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 当你手动修改了此命名空间，需要去模组编辑器修改对应的新命名空间，程序集也需要修改命名空间，否则DLL将加载失败！！！
/// </summary>
namespace MOD_lssmsp
{
    /// <summary>
    /// 此类是模组的主类
    /// </summary>
    public class ModMain
    {
        private TimerCoroutine corUpdate;
		private static HarmonyLib.Harmony harmony;

        /// <summary>
        /// MOD初始化，进入游戏时会调用此函数
        /// </summary>
        public void Init()
        {
			//使用了Harmony补丁功能的，需要手动启用补丁。
			//启动当前程序集的所有补丁
			if (harmony != null)
			{
				harmony.UnpatchSelf();
				harmony = null;
			}
			if (harmony == null)
			{
				harmony = new HarmonyLib.Harmony("MOD_lssmsp");
			}
			harmony.PatchAll(Assembly.GetExecutingAssembly());

            corUpdate = g.timer.Frame(new Action(OnUpdate), 1, true);
        }

        /// <summary>
        /// MOD销毁，回到主界面，会调用此函数并重新初始化MOD
        /// </summary>
        public void Destroy()
        {
            g.timer.Stop(corUpdate);
        }

        /// <summary>
        /// 每帧调用的函数
        /// </summary>
        private void OnUpdate()
        {

        }
    }
}
