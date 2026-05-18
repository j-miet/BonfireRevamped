using System;
using GlobalEnums;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Bonfire
{
    public class LevellingSystem : MonoBehaviour
    {
        public LevellingSystem() { }

        public static LevellingSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LevellingSystem();
                }
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        private static GUIStyle labelStyle;

        private static GUIStyle buttonStyle;

        public bool CanLevelUp()
        {
            return HeroController.instance.cState.nearBench && (gotFreeLevel || BonfireMod.Instance.Status.GeoToLvUp + BonfireMod.Instance.Status.SpentGeo <= PlayerData.instance.geo);
        }
        public void OnGUI()
        {
            if (GameManager.instance == null) return;
            if (!(GameManager.instance.gameState == GameState.PLAYING || GameManager.instance.gameState == GameState.PAUSED) || InInventory()) return;

            if (trajanBold == null || trajanNormal == null)
            {
                foreach (Font font in Resources.FindObjectsOfTypeAll<Font>())
                {
                    if (font != null && font.name == "TrajanPro-Bold")
                    {
                        trajanBold = font;
                    }
                    if (font != null && font.name == "TrajanPro-Regular")
                    {
                        trajanNormal = font;
                    }
                }
            }
            if (HeroController.instance.cState.nearBench && !GameManager.instance.isPaused)
            {
                GUI.enabled = true;
                if (labelStyle == null)
                    labelStyle = new GUIStyle(GUI.skin.label)
                    {
                        font = trajanNormal,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 15
                    };
                if (buttonStyle == null)
                    buttonStyle = new GUIStyle(GUI.skin.button)
                    {
                        font = trajanBold,
                        fontStyle = FontStyle.Normal,
                        fontSize = 15,
                        alignment = TextAnchor.MiddleCenter
                    };
                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
                GUI.color = Color.white;
                BonfireMod.Instance.Status.RelicLevels = BonfireMod.Instance.Status.TotalFreeLevels + BonfireMod.Instance.Status.SpentFreeLevels;
                BonfireMod.Instance.Status.CurrentLv = BonfireMod.Instance.Status.StrengthStat + BonfireMod.Instance.Status.DexterityStat + BonfireMod.Instance.Status.LuckStat + BonfireMod.Instance.Status.ResilienceStat + BonfireMod.Instance.Status.WisdomStat + BonfireMod.Instance.Status.IntelligenceStat + BonfireMod.Instance.Status.SpentGeoLevels + BonfireMod.Instance.Status.SpentFreeLevels - 5;
                if (BonfireMod.Instance.Status.CurrentLv == 1)
                    BonfireMod.Instance.Status.TotalGeoLevels = 1;
                BonfireMod.Instance.Status.GeoLevels = BonfireMod.Instance.Status.TotalGeoLevels + BonfireMod.Instance.Status.SpentGeoLevels;
                BonfireMod.Instance.Status.GeoToLvUp = (int)(Math.Pow(BonfireMod.Instance.Status.GeoLevels, 2.0) + (10 * BonfireMod.Instance.Status.GeoLevels) + 50.0);
                BonfireMod.Instance.Status.FreeLevels = BonfireMod.Instance.Status.RL3Levels + BonfireMod.Instance.Status.RL4Levels;
                gotFreeLevel = !(BonfireMod.Instance.Status.FreeLevels == 0);
                string geoToLevelUp = BonfireMod.Instance.Status.GeoToLvUp.ToString();
                string totalInt = (BonfireMod.Instance.Status.IntelligenceStat + BonfireMod.Instance.Status.IntelligenceIncrease).ToString();
                string totalStr = (BonfireMod.Instance.Status.StrengthStat + BonfireMod.Instance.Status.StrengthIncrease).ToString();
                string totalDex = (BonfireMod.Instance.Status.DexterityStat + BonfireMod.Instance.Status.DexterityIncrease).ToString();
                string totalLck = (BonfireMod.Instance.Status.LuckStat + BonfireMod.Instance.Status.LuckIncrease).ToString();
                string totalRes = (BonfireMod.Instance.Status.ResilienceStat + BonfireMod.Instance.Status.ResilienceIncrease).ToString();
                string totalWsdm = (BonfireMod.Instance.Status.WisdomStat + BonfireMod.Instance.Status.WisdomIncrease).ToString();
                string nailDamage = NailDamage(BonfireMod.Instance.Status.StrengthIncrease + BonfireMod.Instance.Status.StrengthStat).ToString();
                string attackSpeed = AttackSpeed(BonfireMod.Instance.Status.DexterityIncrease + BonfireMod.Instance.Status.DexterityStat).ToString();
                string extraMasks = ExtraMasks(BonfireMod.Instance.Status.ResilienceStat + BonfireMod.Instance.Status.ResilienceIncrease).ToString();
                string extraSoul = ExtraSoul(BonfireMod.Instance.Status.WisdomStat + BonfireMod.Instance.Status.WisdomIncrease, 11).ToString();
                string critChance = CritChance(BonfireMod.Instance.Status.LuckStat + BonfireMod.Instance.Status.LuckIncrease).ToString();
                string critDamage = CritDamage(BonfireMod.Instance.Status.DexterityIncrease + BonfireMod.Instance.Status.DexterityStat, 100).ToString();
                string geoIncrease = (5 * (BonfireMod.Instance.Status.LuckStat + BonfireMod.Instance.Status.LuckIncrease - 1)).ToString();
                string focusCost = FocusCost(BonfireMod.Instance.Status.IntelligenceStat + BonfireMod.Instance.Status.IntelligenceIncrease).ToString();
                string soulRegen = SoulRegen(BonfireMod.Instance.Status.WisdomStat + BonfireMod.Instance.Status.WisdomIncrease).ToString();
                string spellDamage = SpellDamage(100, BonfireMod.Instance.Status.IntelligenceStat + BonfireMod.Instance.Status.IntelligenceIncrease).ToString();
                string expectedHits;
                if (BonfireMod.Instance.Status.ResilienceStat + BonfireMod.Instance.Status.ResilienceIncrease > 1)
                {
                    expectedHits = ExpectedHits(BonfireMod.Instance.Status.ResilienceStat + BonfireMod.Instance.Status.ResilienceIncrease).ToString();
                }
                else
                {
                    expectedHits = "0";
                }
                string applyText;
                if (BonfireMod.Instance.Status.RL3Levels <= 0)
                {
                    if (BonfireMod.Instance.Status.RL4Levels <= 0)
                    {
                        if (BonfireMod.Instance.Status.SpentFreeLevels > 0)
                        {
                            applyText = string.Concat("Apply (", BonfireMod.Instance.Status.SpentGeo, " geo and ", BonfireMod.Instance.Status.SpentFreeLevels, " relics)");
                        }
                        else
                        {
                            applyText = "Apply (" + BonfireMod.Instance.Status.SpentGeo + " geo)";
                        }
                    }
                    else
                    {
                        applyText = BonfireMod.Instance.Status.RL4Levels + " Free Levels!\n(Arcane Egg)";
                    }
                }
                else
                {
                    applyText = BonfireMod.Instance.Status.RL3Levels + " Free Levels!\n(King's Idol)";
                }
                GUILayout.BeginArea(new Rect(20f, (float)(Screen.height / 4), 530f, 500f));
                GUILayout.Label("Level Up", labelStyle);
                GUILayout.BeginHorizontal("box");
                GUILayout.BeginVertical("box");
                if (GUILayout.Button(new GUIContent("Strength: " + totalStr), buttonStyle, GUILayout.Height(40f), GUILayout.Width(160f)) && CanLevelUp() && HeroController.instance.cState.nearBench)
                {
                    IncreaseStat("Strength");
                }
                if (GUILayout.Button(new GUIContent("Dexterity: " + totalDex), buttonStyle, GUILayout.Height(40f), GUILayout.Width(160f)) && CanLevelUp() && HeroController.instance.cState.nearBench)
                {
                    IncreaseStat("Dexterity");
                }
                if (GUILayout.Button(new GUIContent("Intelligence: " + totalInt), buttonStyle, GUILayout.Height(40f), GUILayout.Width(160f)) && CanLevelUp() && HeroController.instance.cState.nearBench)
                {
                    IncreaseStat("Intelligence");
                }
                if (GUILayout.Button(new GUIContent("Resilience: " + totalRes), buttonStyle, GUILayout.Height(40f), GUILayout.Width(160f)) && CanLevelUp() && HeroController.instance.cState.nearBench)
                {
                    IncreaseStat("Resilience");
                }
                if (GUILayout.Button(new GUIContent("Wisdom: " + totalWsdm), buttonStyle, GUILayout.Height(40f), GUILayout.Width(160f)) && CanLevelUp() && HeroController.instance.cState.nearBench)
                {
                    IncreaseStat("Wisdom");
                }
                if (GUILayout.Button(new GUIContent("Luck: " + totalLck), buttonStyle, GUILayout.Height(40f), GUILayout.Width(160f)) && CanLevelUp() && HeroController.instance.cState.nearBench)
                {
                    IncreaseStat("Luck");
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical("box");
                GUILayout.Label(new GUIContent("Nail Damage: " + nailDamage), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("Slash Speed: " + attackSpeed), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("Spell Damage: " + spellDamage + "%"), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("Extra Masks: " + extraMasks), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("SOUL Gained: " + extraSoul), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("Crit Chance: " + critChance + "%"), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.EndVertical();
                GUILayout.BeginVertical("box");
                GUILayout.Label(new GUIContent(""), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("Crit Damage: " + critDamage + "%"), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("Focus Cost: " + focusCost), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("Hit Resistance: " + expectedHits + "%"), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("SOUL Regen: " + soulRegen), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(new GUIContent("Geo Increase: " + geoIncrease + "%"), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
                GUILayout.Label(new GUIContent("Current Level: " + BonfireMod.Instance.Status.CurrentLv.ToString()), labelStyle);
                GUILayout.Label(new GUIContent("Geo to Level Up: " + geoToLevelUp), labelStyle);
                GUILayout.BeginHorizontal("box");
                GUI.backgroundColor = Color.green;
                if (GUILayout.Button(new GUIContent(applyText), buttonStyle, GUILayout.Height(40f), GUILayout.Width(258f)) && HeroController.instance.cState.nearBench)
                {
                    ApplyLevel();
                }
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button(new GUIContent("Cancel"), buttonStyle, GUILayout.Height(40f), GUILayout.Width(258f)) && HeroController.instance.cState.nearBench)
                {
                    BonfireMod.Instance.Status.SpentGeo = 0;
                    BonfireMod.Instance.Status.StrengthIncrease = 0;
                    BonfireMod.Instance.Status.DexterityIncrease = 0;
                    BonfireMod.Instance.Status.WisdomIncrease = 0;
                    BonfireMod.Instance.Status.ResilienceIncrease = 0;
                    BonfireMod.Instance.Status.IntelligenceIncrease = 0;
                    BonfireMod.Instance.Status.LuckIncrease = 0;
                    BonfireMod.Instance.Status.RL3Levels = PlayerData.instance.trinket3;
                    BonfireMod.Instance.Status.RL4Levels = PlayerData.instance.trinket4;
                    BonfireMod.Instance.Status.FreeLevels = BonfireMod.Instance.Status.RL3Levels + BonfireMod.Instance.Status.RL4Levels;
                    BonfireMod.Instance.Status.SpentFreeLevels = 0;
                    BonfireMod.Instance.Status.SpentGeoLevels = 0;
                }
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal("box");
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button(new GUIContent("Respec (" + BonfireMod.Instance.Status.Respec.ToString() + "  Rancid Egg)"), buttonStyle, GUILayout.Height(40f), GUILayout.Width(522f)) && PlayerData.instance.rancidEggs >= BonfireMod.Instance.Status.Respec && HeroController.instance.cState.nearBench)
                {
                    Respec();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.EndArea();
            }
            if (!HeroController.instance.cState.nearBench)
            {
                BonfireMod.Instance.Status.SpentGeo = 0;
                BonfireMod.Instance.Status.StrengthIncrease = 0;
                BonfireMod.Instance.Status.DexterityIncrease = 0;
                BonfireMod.Instance.Status.WisdomIncrease = 0;
                BonfireMod.Instance.Status.ResilienceIncrease = 0;
                BonfireMod.Instance.Status.IntelligenceIncrease = 0;
                BonfireMod.Instance.Status.LuckIncrease = 0;
                BonfireMod.Instance.Status.RL3Levels = PlayerData.instance.trinket3;
                BonfireMod.Instance.Status.RL4Levels = PlayerData.instance.trinket4;
                BonfireMod.Instance.Status.FreeLevels = BonfireMod.Instance.Status.RL3Levels + BonfireMod.Instance.Status.RL4Levels;
                BonfireMod.Instance.Status.SpentFreeLevels = 0;
                BonfireMod.Instance.Status.SpentGeoLevels = 0;
            }
        }

        public static bool InInventory()
        {
            GameObject gameObject = GameObject.FindGameObjectWithTag("Inventory Top");
            if (gameObject == null) return false;
            PlayMakerFSM component = FSMUtility.LocateFSM(gameObject, "Inventory Control");
            if (component == null) return false;
            FsmBool fsmBool = component.FsmVariables.GetFsmBool("Open");
            return fsmBool != null && fsmBool.Value;
        }


        public int NailDamage(int totalStr) => (int)Math.Round((5 + 4 * PlayerData.instance.nailSmithUpgrades) * Math.Pow(1.25, Math.Log(totalStr, 2.0)));


        public int ExtraMasks(int totalRes) => (int)Math.Round(-0.4 + 2.6 * Math.Log(totalRes));


        public float FocusCost(int totalInt) => (float)Math.Round(34.0 * Math.Exp(-0.01 * (totalInt + 1.0)));


        public int CritChance(int totalLck) => (int)Math.Round(6.5 * Math.Log(totalLck));


        public int DroppedGeo(int totalLck) => 1 + totalLck / 20;


        public float AttackSpeed(int totalDex) => (float)Math.Round(2.7 / (1.0 + 1.82 * Math.Exp(-0.08 * totalDex)) - 0.01, 2);


        public void Respec()
        {
            PlayerData.instance.AddGeo(BonfireMod.Instance.Status.TotalSpentGeo);
            HeroController.instance.AddGeoToCounter(BonfireMod.Instance.Status.TotalSpentGeo);
            PlayerData.instance.trinket3 += BonfireMod.Instance.Status.TotalFreeLevels;
            PlayMakerFSM.BroadcastEvent("TRINK 3");
            BonfireMod.Instance.Status.StrengthStat = 1;
            BonfireMod.Instance.Status.DexterityStat = 1;
            BonfireMod.Instance.Status.ResilienceStat = 1;
            BonfireMod.Instance.Status.WisdomStat = 1;
            BonfireMod.Instance.Status.IntelligenceStat = 1;
            BonfireMod.Instance.Status.LuckStat = 1;
            BonfireMod.Instance.Status.StrengthIncrease = 0;
            BonfireMod.Instance.Status.DexterityIncrease = 0;
            BonfireMod.Instance.Status.IntelligenceIncrease = 0;
            BonfireMod.Instance.Status.ResilienceIncrease = 0;
            BonfireMod.Instance.Status.WisdomIncrease = 0;
            BonfireMod.Instance.Status.LuckIncrease = 0;
            BonfireMod.Instance.Status.SpentGeo = 0;
            BonfireMod.Instance.Status.TotalSpentGeo = 0;
            BonfireMod.Instance.Status.FreeLevels = 0;
            BonfireMod.Instance.Status.TotalFreeLevels = 0;
            BonfireMod.Instance.Status.RL3Levels = 0;
            BonfireMod.Instance.Status.GeoLevels = 0;
            BonfireMod.Instance.Status.TotalGeoLevels = 1;
            BonfireMod.Instance.Status.SpentGeoLevels = 0;
            BonfireMod.Instance.Status.RelicLevels = 0;
            BonfireMod.Instance.Status.CurrentLv = 1;
            PlayerData.instance.rancidEggs -= BonfireMod.Instance.Status.Respec;
            BonfireMod.Instance.Status.Respec += 1;
            PlayerData.UpdateBlueHealth();
        }


        public void IncreaseStat(string stat)
        {
            if (stat == "Strength")
            {
                BonfireMod.Instance.Status.StrengthIncrease++;
            }
            else if (stat == "Dexterity")
            {
                BonfireMod.Instance.Status.DexterityIncrease++;
            }
            else if (stat == "Intelligence")
            {
                BonfireMod.Instance.Status.IntelligenceIncrease++;
            }
            else if (stat == "Resilience")
            {
                BonfireMod.Instance.Status.ResilienceIncrease++;
            }
            else if (stat == "Wisdom")
            {
                BonfireMod.Instance.Status.WisdomIncrease++;
            }
            else
            {
                BonfireMod.Instance.Status.LuckIncrease++;
            }
            if (BonfireMod.Instance.Status.RL3Levels <= 0)
            {
                if (BonfireMod.Instance.Status.RL4Levels <= 0)
                {
                    BonfireMod.Instance.Status.SpentGeo += BonfireMod.Instance.Status.GeoToLvUp;
                    BonfireMod.Instance.Status.SpentGeoLevels++;
                }
                else
                {
                    BonfireMod.Instance.Status.RL4Levels--;
                    BonfireMod.Instance.Status.SpentFreeLevels++;
                }
            }
            else
            {
                BonfireMod.Instance.Status.RL3Levels--;
                BonfireMod.Instance.Status.SpentFreeLevels++;
            }
        }
        public void ApplyLevel()
        {
            PlayerData.instance.TakeGeo(BonfireMod.Instance.Status.SpentGeo);
            PlayerData.instance.trinket3 = BonfireMod.Instance.Status.RL3Levels;
            PlayerData.instance.trinket4 = BonfireMod.Instance.Status.RL4Levels;
            HeroController.instance.geoCounter.TakeGeo(BonfireMod.Instance.Status.SpentGeo);
            BonfireMod.Instance.Status.TotalSpentGeo += BonfireMod.Instance.Status.SpentGeo;
            BonfireMod.Instance.Status.SpentGeo = 0;
            BonfireMod.Instance.Status.TotalGeoLevels += BonfireMod.Instance.Status.SpentGeoLevels;
            BonfireMod.Instance.Status.SpentGeoLevels = 0;
            BonfireMod.Instance.Status.TotalFreeLevels += BonfireMod.Instance.Status.SpentFreeLevels;
            BonfireMod.Instance.Status.SpentFreeLevels = 0;
            BonfireMod.Instance.Status.StrengthStat += BonfireMod.Instance.Status.StrengthIncrease;
            BonfireMod.Instance.Status.DexterityStat += BonfireMod.Instance.Status.DexterityIncrease;
            BonfireMod.Instance.Status.ResilienceStat += BonfireMod.Instance.Status.ResilienceIncrease;
            BonfireMod.Instance.Status.WisdomStat += BonfireMod.Instance.Status.WisdomIncrease;
            BonfireMod.Instance.Status.IntelligenceStat += BonfireMod.Instance.Status.IntelligenceIncrease;
            BonfireMod.Instance.Status.LuckStat += BonfireMod.Instance.Status.LuckIncrease;
            HeroController.instance.CharmUpdate();
            PlayerData.instance.UpdateBlueHealth();
            PlayMakerFSM.BroadcastEvent("UPDATE BLUE HEALTH");
            BonfireMod.Instance.Log("Level up applied: " + BonfireMod.Instance.Status.StrengthIncrease + " Strength, " + BonfireMod.Instance.Status.DexterityIncrease +
                " Dexterity, " + BonfireMod.Instance.Status.IntelligenceIncrease + " Intelligence, " + BonfireMod.Instance.Status.ResilienceIncrease + " Resilience, " +
                BonfireMod.Instance.Status.WisdomIncrease + " Wisdom and " + BonfireMod.Instance.Status.LuckIncrease + " Luck.");
            BonfireMod.Instance.Status.StrengthIncrease = 0;
            BonfireMod.Instance.Status.DexterityIncrease = 0;
            BonfireMod.Instance.Status.WisdomIncrease = 0;
            BonfireMod.Instance.Status.ResilienceIncrease = 0;
            BonfireMod.Instance.Status.IntelligenceIncrease = 0;
            BonfireMod.Instance.Status.LuckIncrease = 0;
            PlayerData.UpdateBlueHealth();
        }


        public int IncreaseGeo(int droppedGeo, int totalLck) => (int)(droppedGeo * (1f + (totalLck - 1) / 20f));


        public int CritDamage(int totalDex, int nailDamage) => (int)(nailDamage * (1.2 + Math.Log(totalDex)));


        public int SpellDamage(int baseDamage, int totalInt) => (int)Math.Round(baseDamage * Math.Pow(1.25, Math.Log(totalInt, 2.0)));


        public int ExtraSoul(int totalWsdm, int baseSoul) => (int)Math.Round(baseSoul + 5.0 * Math.Log(totalWsdm));


        public int SoulRegen(int totalWsdm) => (int)Math.Round(0.32 + 0.68 * Math.Log(totalWsdm));


        public float IFrames(int totalRes) => (float)(3.25 / (1.0 + 2.4 * Math.Exp(-0.07 * (totalRes - 1))));


        public float IFramesChance(int totalRes, int hitsTaken)
        {
            if (hitsTaken > 7)
            {
                hitsTaken = 7;
            }
            switch (hitsTaken)
            {
                case 1:
                    return 0.1f * IFrames(totalRes);
                case 2:
                    return (1f - 0.1f * IFrames(totalRes)) * 0.2f * IFrames(totalRes);
                case 3:
                    return (1f - 0.1f * IFrames(totalRes)) * (1f - 0.2f * IFrames(totalRes)) * 0.3f * IFrames(totalRes);
                case 4:
                    return (1f - 0.1f * IFrames(totalRes)) * (1f - 0.2f * IFrames(totalRes)) * (1f - 0.3f * IFrames(totalRes)) * 0.5f * IFrames(totalRes);
                case 5:
                    return (1f - 0.1f * IFrames(totalRes)) * (1f - 0.2f * IFrames(totalRes)) * (1f - 0.3f * IFrames(totalRes)) * (1f - 0.5f * IFrames(totalRes)) * 0.7f * IFrames(totalRes);
                case 6:
                    return (1f - 0.1f * IFrames(totalRes)) * (1f - 0.2f * IFrames(totalRes)) * (1f - 0.3f * IFrames(totalRes)) * (1f - 0.5f * IFrames(totalRes)) * (1f - 0.7f * IFrames(totalRes)) * 0.8f * IFrames(totalRes);
                case 7:
                    return (1f - 0.1f * IFrames(totalRes)) * (1f - 0.2f * IFrames(totalRes)) * (1f - 0.3f * IFrames(totalRes)) * (1f - 0.5f * IFrames(totalRes)) * (1f - 0.7f * IFrames(totalRes)) * (1f - 0.8f * IFrames(totalRes)) * 0.9f * IFrames(totalRes);
                default:
                    return 0f;
            }
        }


        public float ExpectedHits(int totalRes)
        {
            float num = 0f;
            for (int i = 1; i < 8; i++)
            {
                num += (i + 1) * IFramesChance(totalRes, i);
            }
            return (float)Math.Round(100f / num);
        }


        public static LevellingSystem _instance;


        public PlayerData PlayerData;


        public GameManager GameManager;


        public bool levellingUp;


        public HeroController HeroController;


        public PlayMakerFSM PlayMakerFSM;


        public Font trajanBold;


        public Font trajanNormal;


        public bool gotFreeLevel;

        public int freeLevelsSpent;

        public int geoToLvUp;

        public GameObject inventory;
    }

}
