using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CmdType
{
    DIE = 0,    //  死亡
    MOVE,       //  移动
    IDLE,       //  空闲

    //攻击
    ATTACK1,
    ATTACK2,
    ATTACK3,
    ATTACK4,
    ATTACK5,
    ATTACK6,
    ATTACK7,
    //技能
    SPELL1,
    SPELL2,
    SPELL3,
    SPELL4,

    //  技能
    CMD_Skill,
    Skill_QiGong,

    Jump,
    Action_Defend,                    //防御
    Action_FakeDeath,                 //装死
    Action_CancelFakeDeath,           //取消装死
    Action_Scream,                    //尖叫
    Action_PickUp,                    //拾取/丢弃道具
    Action_ActionSelectOpen,          //开始动作选择
    Action_EmojiSelectOpen,           //开始表情选择
    Action_ActionSelectEnd,           //结束动作选择
    Action_EmojiSelectEnd,            //结束表情选择
    Action1_Jump,                     //跳跃
    Action_LiftUpBomb,                //举起炸弹
    Action_ThrowBomb,                 //扔出炸弹
    Action_PutAwayBomb,               //收起炸弹
    Action_UsePropTriggerDown,        //按下触发的道具
    Action_UsePropTriggerUp,          //弹起触发的道具
    Action_UsePropTriggerStay,        //按住触发的道具
    Action_FlashManMove,              //闪电侠移动
    Action2_Grab,                     //抓人
    Action2_ReleaseGrab,              //取消抓人
    Action2_Lift,                     //举起人
    Action_JumpTriggerDown,           //跳跃按下触发道具技能

    Action1_JetPackBoosting,
    Action1_QuickJump,//
    Action2_ReleaseProp,
    Action3_Headbutt,
    Action4_Dance,
    ActionMain_SpinHammer, // 代码注释了，好像废弃了
    ActionMain_ReleaseGrab,
    ActionMain_Punch,
    ActionMain_ThrowProp,
    ActionMain_KickBall, // 足球模式 废弃了
    Action6_ItemSlot1,//Item1
    Action7_ItemSlot2,//Item2
    Action8,//Skill          /// 释放技能，后续处理一下方向就可以了
    Action9_SmallSkill,         /// 释放小技能，后续处理一下方向就可以了

    Petrified,//石化

}