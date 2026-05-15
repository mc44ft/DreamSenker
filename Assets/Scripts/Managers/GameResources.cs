using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace DreamSeeker.Managers
{
public class GameResources : MonoBehaviour
{
    private static GameResources _instance;
    public static GameResources Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = Resources.Load<GameResources>("GameResources");
            }
            return _instance;
        }
    }
    [field: Space(5)]
    [field: Header("-------------- VIDEO ASSETS ----------------")]
    [field: Header("VIDEO")]
    [field: SerializeField] public VideoClip BeginVideoClip { get; private set; }
    [field: SerializeField] public VideoClip EndVideoClip { get; private set; }
    [field:Header("-------------- AUDIO ASSETS ----------------")]
    [field:Header("AUDIO UI")]
    [field: SerializeField] public AudioClip BeginPanelClip {  get; private set; }
    [field: SerializeField] public AudioClip UiButtonClip {  get; private set; }
    [field: SerializeField] public AudioClip UiShowPanelClip {  get; private set; }
    [field: Header("AUDIO MAP")]
    [field: SerializeField] public AudioClip CommonMapClip { get; private set; }
    [field: SerializeField] public AudioClip MirrorMapClip { get; private set; }
    [field: Header("AUDIO MONSTER FOX")]
    [field: SerializeField] public AudioClip FoxFightingClip { get; private set; }
    [field: SerializeField] public AudioClip FoxFlameBurnClip { get; private set; }
    [field: SerializeField] public AudioClip FoxTwoRestoreHealthClip { get; private set; }
    [field: SerializeField] public AudioClip FoxTwoSkillCloneClip { get; private set; }
    [field: SerializeField] public AudioClip FoxTwoSkillDashClip { get; private set; }
    [field: SerializeField] public AudioClip FoxTwoSkillJumpScratchClip { get; private set; }
    [field: SerializeField] public AudioClip FoxOneSkillDeterClip { get; private set; }
    [field: SerializeField] public AudioClip FoxOneSkillTailClip { get; private set; }
    [field: Header("AUDIO MONSTER SPIDER")]
    [field: SerializeField] public AudioClip SpiderDeathClip { get; private set; }
    [field: SerializeField] public AudioClip SpiderFightingClip { get; private set; }
    [field: SerializeField] public AudioClip SpiderSkillShakeClip { get; private set; }
    [field: SerializeField] public AudioClip SpiderSkillVenomBiteClip { get; private set; }
    [field: SerializeField] public AudioClip SpiderSkillWebClip { get; private set; }
    [field: Header("AUDIO MONSTER MINION")]
    [field: SerializeField] public AudioClip SlimeAmmoFellToGroundClip { get; private set; }
    [field: Header("AUDIO PLAYER")]
    [field: SerializeField] public AudioClip PlayerMirrorFormAttackClip { get; private set; }
    [field: SerializeField] public AudioClip PlayerRealFormAttackClip { get; private set; }
    [field: SerializeField] public AudioClip PlayerRunClip { get; private set; }
    [field: SerializeField] public AudioClip PlayerSwitchFormClip { get; private set; }
    [field: Header("AUDIO OTHER")]
    [field: SerializeField] public AudioClip PickupItemClip { get; private set; }

}
}
