using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MainBattleSystem
{
    public class BattleManager : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            //占位防报错用的，不用管
            int a=0;
            if (a==0) a=0;
        }



        // Update is called once per frame
        void Update()
        {
            
        }
    }

    /*
        InitializeBattle();//- 战斗初始化
        StartBattleFromWorldMap();//- 从大地图进入战斗
        EndBattle();//- 结束战斗并返回结果
        AddUnit();//- 单位管理(添加)
        RemoveUnit();//- 单位管理(移除)
        CheckBattleEndCondition();//- 最终胜负判定，从隔壁turnmanager接收信息

    事件发布：
    OnBattleStart - 战斗开始

    OnBattleEnd - 战斗结束（含结果）

    OnUnitAdded/OnUnitRemoved - 单位变化

    依赖引用：
    PositionPlacementSystem（位置放置）

    BackgroundRenderer（背景管理）

    TurnManager（回合管理委托）
    */

}