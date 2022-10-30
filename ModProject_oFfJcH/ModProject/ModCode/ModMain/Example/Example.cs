using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace MOD_lssmsp
{
    /// <summary>
    /// 部分常用API接口，和简单的例子
    /// </summary>
    public class Example
    {
        public Example()
        {
        }

        #region TestTimer
        /// <summary>
        /// g.timer：延迟控制，游戏中大部分类都是不继承MonoBehaviour，且IL2CPP下不能动态载入Mono，在普通类想做延迟调用，或者每帧调用则可以利用g.timer
        /// </summary>
        private void TestTimer()
        {
            //循环10帧调用
            TimerCoroutine cor1 = null;
            int frameCount = 0;
            cor1 = g.timer.Frame(new Action(() => {
                frameCount++;

                //执行10帧后，把调用去除
                if (frameCount >= 10)
                {
                    g.timer.Stop(cor1);
                }
            }), 1, true);

            //必须先注册才能AddComponent
            ClassInjector.RegisterTypeInIl2Cpp<ExampleTestMono>();
            g.root.AddComponent<ExampleTestMono>();
        }
        #endregion

        #region TestEvents
        /// <summary>
        /// g.events：事件，游戏中所有的广播都是通过这个广播的。
        /// EGameType：游戏通用广播
        /// EMapType：在大地图时的广播
        /// EBattleType：在战斗场景时的广播
        /// </summary>
        private void TestEvents()
        {
            Il2CppSystem.Action<ETypeData> callUnitHit = (Il2CppSystem.Action<ETypeData>)OnUnitHitDynIntHandler;

            //监听广播，有单位被击打
            g.events.On(EBattleType.UnitHitDynIntHandler, callUnitHit);

            //取消监听
            g.events.Off(EBattleType.UnitHitDynIntHandler, callUnitHit);
        }

        /// <summary>
        /// 广播的参数只需要一个：ETypeData e
        /// </summary>
        private void OnUnitHitDynIntHandler(ETypeData e)
        {
            //广播分类Data.广播类型，通过这样获取广播数据
            EBattleTypeData.UnitHitDynIntHandler edata = e.Cast<EBattleTypeData.UnitHitDynIntHandler>();

            Debug.Log("战斗中被击的单位：" + edata.hitUnit.data.name);

            //设置本次伤害变成0
            edata.dynV.baseValue = 0;
            edata.dynV.ClearCall();
        }
        #endregion

        #region TestWorld
        /// <summary>
        /// g.world：管理游戏中世界实例，例如，宗门，城镇，事件，NPC，等等等等
        /// </summary>
        private void TestWorld()
        {
            Debug.Log("玩家名字：" + g.world.playerUnit.data.unitData.propertyData.GetName());

            //进入竹林副本，对应DungeonBase表，一个境界对应5级，5就是练气后期打的副本
            g.world.battle.IntoBattle(new DataMap.MonstData() { id = 1011, level = 5 });

            //创建一个山洞事件到玩家脚底，对应WorldFortuitousEventBase表
            g.world.mapEvent.AddGridEvent(g.world.playerUnit.data.unitData.GetPoint(), 6);

            //条件：判断玩家是练气境，对应配置对照表->条件说明表，什么时候都能调用
            bool b = UnitConditionTool.Condition("grade_0_1_1", new UnitConditionData(g.world.playerUnit, null));

            //剧情命令：打开玩家信息UI，对应配置对照表->剧情条件及影响，什么时候都能调用
            DramaFunctionTool.OptionsFunction("openUI_" + UIType.PlayerInfo.uiName);

            //副本命令：创建一个当康到副本中，对应配置对照表->副本作用功能，只有在副本中才能调用
            BattleFunctionTool.OptionsFunction("createMonst_7210_0_0_3");

            //获取一个NPC对象，unitNPC.data.unitData    NPC的存档信息类，一个NPC的全部数据都在此类
            WorldUnitBase unitNPC = g.world.unit.GetUnit("NPCID_XXXXXX");
            Debug.Log("" + unitNPC.data.unitData.propertyData.GetName());

            //Action对应的是人为行为（可以理解为接口），如移动、添加气运、添加任务、使用道具，装备武学等等等等
            unitNPC.CreateAction(new UnitActionLuckAdd(120));

            //UnitActionRole，是双人行为，如聊天、双修、切磋、论道，等等
            unitNPC.CreateAction(new UnitActionRoleTrains(g.world.playerUnit));

            //打开一个剧情610011，对应DramaDialogue表
            DramaTool.OpenDrama(610011, new DramaData() { unitLeft = g.world.playerUnit, unitRight = null });

            //第二种打开剧情方法，封装了更方便的方法
            UICustomDramaDyn dramaDyn = new UICustomDramaDyn(610011);
            //点击按钮回调
            Action onClick6100111 = () => { };
            dramaDyn.SetOptionCall(6100111, onClick6100111);
            dramaDyn.dramaData.dialogueOptionsAddText[1001] = "额外增加的按钮";
            dramaDyn.OpenUI();
        }
        #endregion

        #region TestBattle
        /// <summary>
        /// 当你在战斗中的时候，可以通过SceneType.battle访问管理器入口
        /// </summary>
        private void Battle()
        {
            //创建一个友方NPC在场景中心
            UnitCtrlHumanNPC unitNPC = SceneType.battle.unit.CreateUnitHuman<UnitCtrlHumanNPC>(g.world.unit.GetUnit("NPCID_XXXXXX").data, UnitType.PlayerNPC);
            unitNPC.move.SetPosition(SceneType.battle.battleMap.roomCenterPosi);

            //创建一个当康到战斗中，对应BattleUnitAttr表的ID
            SceneType.battle.unit.CreateUnitMonst(7210, SceneType.battle.battleMap.roomCenterPosi, UnitType.Monst);

            //给玩家施加一个冰冻效果，对应BattleEffect表，SceneType.battle.battleMap.playerUnitCtrl是玩家的引用
            SceneType.battle.battleMap.playerUnitCtrl.AddEffect(51612, SceneType.battle.battleMap.playerUnitCtrl, new SkillCreateData());

            //当你在大地图的时候，可以通过SceneType.map访问管理器入口
            //在大地图的时候大部分是通过g.world访问其他数据，SceneType.map通用接口不多
            //关闭所有UI
            SceneType.map.world.CloseAllUI();
        }
        #endregion

        #region TestRes
        /// <summary>
        /// g.res：加载资源，可以加载Resources的资源，也能加载自定义AB资源
        /// </summary>
        private void TestRes()
        {
            GameObject goEffect = g.res.Load<GameObject>("Effect/Battle/Skill/jueyingjian");
            Debug.Log("这是绝影剑特效：" + goEffect.name);
        }
        #endregion

        #region TestSounds
        /// <summary>
        /// g.sounds：播放声音，可以播放Resources/Sounds下的声音
        /// </summary>
        private void TestSounds()
        {
			//如果是PlayEffect，则不需要Effect这个文件夹名字前缀
			//如果是PlayBG，则不需要BG这个文件夹名字前缀
            g.sounds.PlayEffect("Battle/jineng/jian/jueyingjian", 1, null, null, true);
            Debug.Log("这是绝影剑的声音");
        }
        #endregion

        #region TestConf
        /// <summary>
        /// g.conf：游戏中所有的配置都存放在这里
        /// </summary>
        private void TestConf()
        {
            ConfRoleGradeItem item = g.conf.roleGrade.GetGradeItem(1, 1);
            Debug.Log("这是练气前期配置：" + GameTool.LS(item.gradeName));
        }
        #endregion

        #region TestData
        /// <summary>
        /// g.data：游戏中存放存档的数据
        /// </summary>
        private void TestData()
        {
            Debug.Log("这是玩家名字：" + g.data.unit.GetUnit(g.data.world.playerUnitID).propertyData.GetName());
        }
        #endregion

        #region TestUI
        /// <summary>
        /// g.ui：游戏中控制UI的打开关闭
        /// </summary>
        private void TestUI()
        {
            //打开玩家信息
            g.ui.OpenUI(UIType.PlayerInfo);

            //弹出一个提示框
            UITipItem.AddTip("提示信息");

            //弹出一个二次确定框
            g.ui.OpenUI<UICheckPopup>(UIType.CheckPopup).InitData("提示", "这是有两个按钮的提示框", 2, new Action(() => { Debug.Log("点击了确定"); }));

            //弹出一个文本信息UI
            g.ui.OpenUI<UITextInfo>(UIType.TextInfo).InitData("提示", "这是文本信息");
            g.ui.OpenUI<UITextInfoLong>(UIType.TextInfoLong).InitData("提示", "这是很长的文本信息");

            //打开一个自定义UI，参考修改资源功能
            g.ui.OpenUI(new UIType.UITypeBase("UI预制体名称", UILayer.UI));
        }
        #endregion

        #region Tool
        /// <summary>
        /// 游戏中常用工具类
        /// </summary>
        private void TestTool()
        {
            //CommonTool，通用的普通常用工具
            Debug.Log("随机一个数值0-9：" + CommonTool.Random(0, 10));

            //GameTool，游戏常用工具
            Debug.Log("读取国际化表文本“提示”：" + GameTool.LS("common_tishi"));
        }
        #endregion
    }

    public class ExampleTestMono : MonoBehaviour
    {
        //必须有这行代码，否则无法MonoBehaviour类无法AddComponent
        public ExampleTestMono(IntPtr ptr) : base(ptr) { }

        void Update()
        {
            Debug.Log(Time.frameCount + "，每帧打印");
            Console.WriteLine(Time.frameCount + "，每帧打印");
        }
    }
}
