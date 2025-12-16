using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Settings
{
    //---------------------------------------- Animation Name ----------------------------------
    //Player
    public static readonly int PlayerRealAnimNameToHash_Idle = Animator.StringToHash("PlayerIdle");
    public static readonly int PlayerRealAnimNameToHash_Death = Animator.StringToHash("PlayerDeath");
    public static readonly int PlayerRealAnimNameToHash_Run = Animator.StringToHash("PlayerRun");
    public static readonly int PlayerRealAnimNameToHash_Gethit = Animator.StringToHash("PlayerGetHit");
    public static readonly int PlayerRealAnimNameToHash_Attack = Animator.StringToHash("PlayerAttack");
    public static readonly int PlayerRealAnimNameToHash_Jump = Animator.StringToHash("PlayerJump");
    public static readonly int PlayerRealAnimNameToHash_Fall = Animator.StringToHash("PlayerFall");
    public static readonly int PlayerRealAnimNameToHash_FlyEnter = Animator.StringToHash("PlayerFlyEnter");
    public static readonly int PlayerRealAnimNameToHash_Flying = Animator.StringToHash("PlayerFlying");
    public static readonly int PlayerRealAnimNameToHash_Transition = Animator.StringToHash("PlayerTransition");

    //Spider
    public static readonly int SpiderAnimNameToHash_Crawl = Animator.StringToHash("SpiderCrawl");
    public static readonly int SpiderAnimNameToHash_SkillHeal = Animator.StringToHash("SpiderSkillHeal");
    public static readonly int SpiderAnimNameToHash_SkillVenomBite = Animator.StringToHash("SpiderSkillVenomBite");
    public static readonly int SpiderAnimNameToHash_SkillShake = Animator.StringToHash("SpiderSkillShake");

    //FoxTwo
    public static readonly int FoxTwoAnimNameToHash_SkillDash = Animator.StringToHash("FoxTwoSkillDash");
    public static readonly int FoxTwoAnimNameToHash_SkillJump = Animator.StringToHash("FoxTwoSkillJump");
    public static readonly int FoxTwoAnimNameToHash_SkillJumpScratck = Animator.StringToHash("FoxTwoSkillJumpScratck");
    public static readonly int FoxTwoAnimNameToHash_SkillBoom = Animator.StringToHash("FoxTwoSkillBoom");
    public static readonly int FoxTwoAnimNameToHash_SkillBoomLoop = Animator.StringToHash("FoxTwoSkillBoomLoop");
    public static readonly int FoxTwoAnimNameToHash_SkillPetrify = Animator.StringToHash("FoxTwoSkillPetrify");

    //FoxOne
    public static readonly int FoxOneAnimNameToHash_CloneForm = Animator.StringToHash("FoxOneCloneForm");
    

    //---------------------------------------- Shader Parameter ----------------------------------
    public static readonly string DissolveAmountString = "_DissolveAmount";
    //---------------------------------------- Tag ----------------------------------
    public static readonly string PlayerTag = "Player";
    public static readonly string MonsterAmmoTag = "MonsterAmmo";

}
