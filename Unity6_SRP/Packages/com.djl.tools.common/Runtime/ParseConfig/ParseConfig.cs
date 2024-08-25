using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
public class ParseConfig
{

    public static ParseConfig Instance;
    private static readonly Dictionary<string, ConfigInterface> ConfigInterfaces;
#if UNITY_EDITOR
    public static List<string> useDic = new List<string>();
#endif

    public static Dictionary<string, ConfigInterface> AllConfigs
    {
        get { return ConfigInterfaces; }
    }

    static ParseConfig()
    {
        Instance = new ParseConfig();
        ConfigInterfaces = new Dictionary<string, ConfigInterface>();

        InitConfig();
    }


    static void InitConfig()
    {
        var baseInterface = typeof(ConfigInterface);
        var types = Assembly.GetExecutingAssembly().GetTypes();
        int count = types.Length;
        for (int i = 0; i < count; i++)
        {
            var t = types[i];
            var isAssignableFrom = baseInterface.IsAssignableFrom(t);
            t = t.BaseType;
            if (isAssignableFrom && t != null)
            {
                var methodInfo = t.GetMethod("Instance");
                if (methodInfo != null)
                {
                    ConfigInterface configInterface = methodInfo.Invoke(null, null) as ConfigInterface;
                    if (configInterface != null)
                    {
                        ConfigInterfaces.Add(configInterface.ConfigPath(), configInterface);
#if UNITY_EDITOR
                        string name = t.FullName;
                        var setNameMethod = t.GetMethod("SetConfigName");
                        setNameMethod.Invoke(null, new object[1] { t.FullName });
                        if (!useDic.Contains(name))
                            useDic.Add(name);
#endif
                    }
                }
            }
        }
    }


    public string RealConfigPath(string configPath)
    {
        var of = configPath.IndexOf('_');
        if (of == -1)
        {
            return configPath;
        }
        return configPath.Remove(of, configPath.Length - of);
    }


    public void Parse(string path, byte[] bytes)
    {
        var realPath = RealConfigPath(path);
        //Debug.Log(string.Format("{0} | {1}", path, realPath));

        ConfigInterface config = null;
        if (ConfigInterfaces.TryGetValue(realPath, out config))
        {
            config.PushData(bytes);
        }
        else
        {
            Debug.LogFormat(string.Format("没有指定的配置文件 realPath == {0}, path == {1}", realPath, path));
        }

#if false
        //switch (path)
        //{
        //    case "Firstload/FirstWindowSetting":
        //        Config<FirstLoadWIndowSetting>.Instance().data = bytes;
        //        break;
        //    case "Firstload/LocalizationSetting":
        //        Config<LocalizationSetting>.Instance().firstdata = bytes;
        //        break;
        //    case "Firstload/ServerList":
        //        Config<ServerListSetting>.Instance().data = bytes;
        //        break;
        //    case "Firstload/AnnouncementSetting":
        //        Config<AnnouncementSetting>.Instance().data = bytes;
        //        break;
        //    case "Firstload/WindowSetting":
        //        Config<UguiDescription>.Instance().data = bytes;

        //        break;
        //    case "Localization/LocalizationSetting":
        //        Config<LocalizationSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/PassiveSetting":
        //        Config<PassiveSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/SkillSetting":
        //        Config<SkillSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/BuffSetting":
        //        Config<BuffSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/BossShowSetting":
        //        Config<BossShowSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitPerformanceSetting":
        //        Config<UnitPerformanceSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/ParticleSetting":
        //        Config<ParticleSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/SkillBranchSetting":
        //        Config<SkillBranchSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/SkillBranchViewSetting":
        //        Config<SkillBranchViewSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitDesSetting":
        //        Config<UnitDesSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/SkillDesSetting":
        //        Config<SkillDesSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/FighterSetting":
        //        Config<FighterSetting>.Instance().data = bytes;
        //        break;
        //    case "Mission/MissionSetting":
        //        Config<MissionSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/GuideSetting":
        //        Config<GuideSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/GuideWeakSetting":
        //        Config<GuideWeakSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/ModelSetting":
        //        Config<ModelSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/DialogSetting":
        //        Config<DialogSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/EventSetting":
        //        Config<EventSetting>.Instance().data = bytes;
        //        break;
        //    case "Error/ErrorSetting":
        //        Config<ErrorListSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/Formula":
        //        Config<Formula>.Instance().data = bytes;
        //        break;
        //    case "Mission/BoxSetting":
        //        Config<BoxSetting>.Instance().data = bytes;
        //        break;
        //    case "Mission/ChapterSetting":
        //        Config<ChapterSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/HeroSetting":
        //        Config<HeroSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/FetterSetting":
        //        Config<FetterSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/HeroExpSetting":
        //        Config<HeroExpSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/LevelSetting":
        //        Config<LevelSetting>.Instance().data = bytes;
        //        break;
        //    case "Pack/ItemSetting":
        //        Config<ItemSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/SkillAdditionSetting":
        //        Config<SkillAdditionSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/EquipRankSetting":
        //        Config<EquipRankSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/EquipSetting":
        //        Config<EquipSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/EquipStarSetting":
        //        Config<EquipStarSetting>.Instance().data = bytes;
        //        break;
        //    case "Lottery/SpecifySetting":
        //        Config<SpecifySetting>.Instance().data = bytes;
        //        break;
        //    case "Pack/BoxSetting":
        //        Config<ItemBoxSetting>.Instance().data = bytes;
        //        break;
        //    case "Lottery/LotterySetting":
        //        Config<LotterySetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/StateSetting":
        //        Config<StateSetting>.Instance().data = bytes;
        //        break;
        //    case "merger/RewardSetting":
        //        Config<RewardSetting>.Instance().data = bytes;
        //        break;
        //    case "Energy/EnergySetting":
        //        Config<EnergySetting>.Instance().data = bytes;
        //        break;
        //    case "merger/ConfigValue":
        //        Config<ConfigValueSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/HeroAllSkillSetting":
        //        Config<HeroAllSkillSetting>.Instance().data = bytes;
        //        break;
        //    case "Player/LevelSetting":
        //        Config<PlayerLevelSetting>.Instance().data = bytes;
        //        break;
        //    case "Goddess/GoddessSetting":
        //        Config<GoddessSetting>.Instance().data = bytes;
        //        break;
        //    case "Goddess/SkillSetting":
        //        Config<GoddessSkillSetting>.Instance().data = bytes;
        //        break;
        //    case "Goddess/SkinSetting":
        //        Config<GoddessSkinSetting>.Instance().data = bytes;
        //        break;
        //    case "Chat/ChatSetting":
        //        Config<ChatSetting>.Instance().data = bytes;
        //        break;
        //    case "Chat/PostSetting":
        //        Config<PostSetting>.Instance().data = bytes;
        //        break;
        //    case "Function/FunctionOpenSetting":
        //        Config<FunctionOpenSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/SkillLevelSetting":
        //        Config<SkillLevelSetting>.Instance().data = bytes;
        //        break;
        //    case "Tower/NpcSetting":
        //        Config<NpcSetting>.Instance().data = bytes;
        //        break;
        //    case "Tower/TowerSetting":
        //        Config<TowerSetting>.Instance().data = bytes;
        //        break;
        //    case "Tower/BoxSetting":
        //        Config<TowerBoxSetting>.Instance().data = bytes;
        //        break;
        //    case "Tower/PassRewardSetting":
        //        Config<PassRewardSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/HeroManualSetting":
        //        Config<HeroManualSetting>.Instance().data = bytes;
        //        break;
        //    case "Mall/MallSetting":
        //        Config<MallSetting>.Instance().data = bytes;
        //        break;
        //    case "Ladder/CreditSetting":
        //        Config<CreditSetting>.Instance().data = bytes;
        //        break;
        //    case "Ladder/PeriodNameSetting":
        //        Config<PeriodNameSetting>.Instance().data = bytes;
        //        break;
        //    case "Ladder/PeriodSetting":
        //        Config<PeriodSetting>.Instance().data = bytes;
        //        break;
        //    case "Union/UnionSetting":
        //        Config<UnionSetting>.Instance().data = bytes;
        //        break;
        //    case "Union/UnionModelSetting":
        //        Config<UnionModelSetting>.Instance().data = bytes;
        //        break;
        //    case "Union/UnionFunctionOpenSetting":
        //        Config<UnionFunctionOpenSetting>.Instance().data = bytes;
        //        break;
        //    case "Promotion/PromotionSetting":
        //        Config<PromotionSetting>.Instance().data = bytes;
        //        break;
        //    case "Promotion/PromotionRewardSetting":
        //        Config<PromotionRewardSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/BattleUnitValueDescribe":
        //        Config<BattleUnitValueDescribe>.Instance().data = bytes;
        //        break;
        //    case "Common/ProjectSetting":
        //        Config<ProjectSetting>.Instance().data = bytes;
        //        break;
        //    case "Union/DonateSetting":
        //        Config<DonateSetting>.Instance().data = bytes;
        //        break;
        //    case "Ladder/PeriodRewardSetting":
        //        Config<PeriodRewardSetting>.Instance().data = bytes;
        //        break;
        //    case "Compound/CompoundSetting":
        //        Config<CompoundSetting>.Instance().data = bytes;
        //        break;
        //    case "Getway/GetWaySetting":
        //        Config<GetwaySetting>.Instance().data = bytes;
        //        break;
        //    case "Lottery/LotteryTotalAwardSetting":
        //        Config<LotteryTotalAwardSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/EnchantingCostSetting":
        //        Config<EnchantingCostSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/EnchantingOpenSetting":
        //        Config<EnchantingOpenSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/EnchantingLevelSetting":
        //        Config<EnchantingLevelSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/EnchantingItemSetting":
        //        Config<EnchantingItemSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/EnchantingAdditionSetting":
        //        Config<EnchantingAdditionSetting>.Instance().data = bytes;
        //        break;
        //    case "Artifactbattle/ArtifactUnitSetting":
        //        Config<ArtifactUnitSetting>.Instance().data = bytes;
        //        break;
        //    case "Artifactbattle/ArtifactSetting":
        //        Config<ArtifactSetting>.Instance().data = bytes;
        //        break;
        //    case "Artifactbattle/ArtifactRewardSetting":
        //        Config<ArtifactRewardSetting>.Instance().data = bytes;
        //        break;
        //    case "Artifactbattle/RandomRewardSetting":
        //        Config<RandomRewardSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitReplaceSetting":
        //        Config<UnitReplaceSetting>.Instance().data = bytes;
        //        break;
        //    case "Mystery/GoodsSetting":
        //        Config<GoodsSetting>.Instance().data = bytes;
        //        break;
        //    case "Mystery/MallTypeSetting":
        //        Config<MallTypeSetting>.Instance().data = bytes;
        //        break;
        //    case "Vip/VipSetting":
        //        Config<VipSetting>.Instance().data = bytes;
        //        break;
        //    case "Emoji/EmojiSetting":
        //        Config<EmojiSetting>.Instance().data = bytes;
        //        break;
        //    case "Loading/LoadingHintSetting":
        //        Config<LoadingHintSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/SceneSetting":
        //        Config<SceneSetting>.Instance().data = bytes;
        //        break;
        //    case "Goddess/StarSetting":
        //        Config<StarSetting>.Instance().data = bytes;
        //        break;
        //    case "Sumlottery/SumlotterySetting":
        //        Config<SumlotterySetting>.Instance().data = bytes;
        //        break;
        //    case "Goddess/GoddessStarSetting":
        //        Config<GoddessStarSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/WalletBarSetting":
        //        Config<WalletBarSetting>.Instance().data = bytes;
        //        break;
        //    case "Gm/GmBox":
        //        Config<GmBox>.Instance().data = bytes;
        //        break;
        //    case "Email/EmailTemplate":
        //        Config<EmailTemplate>.Instance().data = bytes;
        //        break;
        //    case "Player/HeadSetting":
        //        Config<HeadSetting>.Instance().data = bytes;
        //        break;
        //    case "Apart/ApartSetting":
        //        Config<ApartSetting>.Instance().data = bytes;
        //        break;
        //    case "Apart/HeroCostSetting":
        //        Config<HeroCostSetting>.Instance().data = bytes;
        //        break;
        //    case "Apart/ArtifactApartSetting":
        //        Config<ArtifactApartSetting>.Instance().data = bytes;
        //        break;
        //    case "Apart/HeroSkillApartSetting":
        //        Config<HeroSkillApartSetting>.Instance().data = bytes;
        //        break;
        //    case "Apart/HeroQualityApartSetting":
        //        Config<HeroQualityApartSetting>.Instance().data = bytes;
        //        break;
        //    case "Lottery/CardShowSetting":
        //        Config<CardShowSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/BattleWeakSetting":
        //        Config<BattleWeakSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/BattleWeakTipSetting":
        //        Config<BattleWeakTipSetting>.Instance().data = bytes;
        //        break;
        //    case "Worldtask/TaskSetting":
        //        Config<TaskSetting>.Instance().data = bytes;
        //        break;
        //    case "Worldtask/BoxSetting":
        //        Config<WorldTaskBoxSetting>.Instance().data = bytes;
        //        break;
        //    case "Arena/RobotSetting":
        //        Config<ArenaRobotSetting>.Instance().data = bytes;
        //        break;
        //    case "College/LevelSetting":
        //        Config<CollegeLevelSetting>.Instance().data = bytes;
        //        break;
        //    case "College/TeamSetting":
        //        Config<CollegeTeamSetting>.Instance().data = bytes;
        //        break;
        //    case "College/CollegeHeroSetting":
        //        Config<CollegeHeroSetting>.Instance().data = bytes;
        //        break;
        //    case "College/TitleSetting":
        //        Config<CollegeTitleSetting>.Instance().data = bytes;
        //        break;
        //    case "College/TeamAdditionSetting":
        //        Config<CollegeTeamAdditionSetting>.Instance().data = bytes;
        //        break;
        //    case "College/CollegeUpSetting":
        //        Config<CollegeUpSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/BattleDialogSetting":
        //        Config<BattleDialogSetting>.Instance().data = bytes;
        //        break;
        //    case "Worldtask/WorldSetting":
        //        Config<WorldSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/PromoteSetting":
        //        Config<PromoteSetting>.Instance().data = bytes;
        //        break;
        //    case "Arena/AernaTalkSetting":
        //        Config<AernaTalkSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/IntitleSetting":
        //        Config<IntitleSetting>.Instance().data = bytes;
        //        break;
        //    case "Risk/TaskSetting":
        //        Config<CampTaskSetting>.Instance().data = bytes;
        //        break;
        //    case "Risk/RequireSetting":
        //        Config<RequireSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/BlinkSetting":
        //        Config<BlinkSetting>.Instance().data = bytes;
        //        break;
        //    case "Hero/DelayEquipSetting":
        //        Config<DelayEquipSetting>.Instance().data = bytes;
        //        break;
        //    case "Goddess/GoddessVoiceSetting":
        //        Config<GoddessVoiceSetting>.Instance().data = bytes;
        //        break;
        //    case "Gift/GlobalGiftSetting":
        //        Config<GlobalGiftSetting>.Instance().data = bytes;
        //        break;
        //    case "Guide/GuideCheckSetting":
        //        Config<GuideCheckSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/UIModelScaleSetting":
        //        Config<UIModelScaleSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/UIModelCameraSetting":
        //        Config<UIModelCameraSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/PopOrderSetting":
        //        Config<PopOrderSetting>.Instance().data = bytes;
        //        break;
        //    case "Arena/HistorySetting":
        //        Config<HistorySetting>.Instance().data = bytes;
        //        break;
        //    case "Vip/VipPrivilegeSetting":
        //        Config<VipPrivilegeSetting>.Instance().data = bytes;
        //        break;
        //    case "Vip/VipPrivilegeTypeSetting":
        //        Config<VipPrivilegeTypeSetting>.Instance().data = bytes;
        //        break;
        //    case "Vip/CardSetting":
        //        Config<CardSetting>.Instance().data = bytes;
        //        break;
        //    case "Common/SoundSetting":
        //        Config<SoundSetting>.Instance().data = bytes;
        //        break;
        //    case "Currency/CurrencySetting":
        //        Config<CurrencySetting>.Instance().data = bytes;
        //        break;
        //    case "MonsterTips/MonsterTipsSetting":
        //        Config<MonsterTipsSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitAbilitySetting":
        //        Config<UnitAbilitySetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitBubbleSetting":
        //        Config<UnitBubbleSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitAnimator":
        //        Config<UnitAnimator>.Instance().data = bytes;
        //        break;
        //    case "Language/LanguageSetting":
        //        Config<LanguageSetting>.Instance().data = bytes;
        //        break;
        //    case "Union/UnionPermissionSetting":
        //        Config<UnionPermissionSetting>.Instance().data = bytes;
        //        break;
        //    case "Promotion/FirstChargeSetting":
        //        Config<FirstChargeSetting>.Instance().data = bytes;
        //        break;
        //    case "Charge/ProductSetting":
        //        Config<ProductSetting>.Instance().data = bytes;
        //        break;
        //    case "Vip/VipPrivilegeContentSetting":
        //        Config<VipPrivilegeContentSetting>.Instance().data = bytes;
        //        break;
        //    case "Sign/SignSetting":
        //        Config<SignSetting>.Instance().data = bytes;
        //        break;
        //    case "Ad/AdSetting":
        //        Config<AdSetting>.Instance().data = bytes;
        //        break;
        //    case "Limit/LimitSetting":
        //        Config<LimitSetting>.Instance().data = bytes;
        //        break;
        //    case "Online/OnlineSetting":
        //        Config<OnlineSetting>.Instance().data = bytes;
        //        break;
        //    case "Battle/SubSceneSetting":
        //        Config<SubSceneSetting>.Instance().data = bytes;
        //        break;


        //    //  UnitSetting分表
        //    //case "Battle/UnitSetting":
        //    //    Config<UnitSetting>.Instance().data = bytes;
        //    //    break;

        //    case "Battle/UnitSetting_0":
        //        UnitSetting0.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_1":
        //        UnitSetting1.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_2":
        //        UnitSetting2.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_3":
        //        UnitSetting3.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_4":
        //        UnitSetting4.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_5":
        //        UnitSetting5.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_6":
        //        UnitSetting6.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_7":
        //        UnitSetting7.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_8":
        //        UnitSetting8.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_9":
        //        UnitSetting9.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_10":
        //        UnitSetting10.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_11":
        //        UnitSetting11.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_12":
        //        UnitSetting12.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_13":
        //        UnitSetting13.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_14":
        //        UnitSetting14.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_15":
        //        UnitSetting15.Instance().data = bytes;
        //        break;
        //    case "Battle/UnitSetting_16":
        //        UnitSetting16.Instance().data = bytes;
        //        break;
            case "Battle/UnitTransformSetting":
                UnitTransformSetting.Instance().data = bytes;
                break;
            case "Sound/SoundNameSetting":
                SoundName.Instance().data = bytes;
                break;
        //    //  学院分表
        //    //case "College/HeroAdditionSetting":
        //    //    Config<CollegeAdditionSetting>.Instance().data = bytes;
        //    //    break;
        //    case "College/HeroAdditionSetting_0":
        //        CollegeAdditionSetting0.Instance().data = bytes;
        //        break;
        //    case "College/HeroAdditionSetting_1":
        //        CollegeAdditionSetting1.Instance().data = bytes;
        //        break;
        //    case "College/HeroAdditionSetting_2":
        //        CollegeAdditionSetting2.Instance().data = bytes;
        //        break;
        //    case "College/HeroAdditionSetting_3":
        //        CollegeAdditionSetting3.Instance().data = bytes;
        //        break;
        //    case "College/HeroAdditionSetting_4":
        //        CollegeAdditionSetting4.Instance().data = bytes;
        //        break;

        //    default:
        //        Debug.LogWarningFormat("已下载的配置表{0}没有处理,如游戏不需要此表,请策划删除客户端字段", path);
        //        break;
        //}
#endif
    }

}
