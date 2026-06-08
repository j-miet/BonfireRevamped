using System;

using GlobalEnums;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Bonfire
{
    public class LevellingSystem : MonoBehaviour
    {
        public static LevellingSystem _instance;
        public PlayerData PlayerData;

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

        // override MonoBehavior.OnGui with custom leveling UI
        public void OnGUI()
        {
            if (GameManager.instance == null) return;

            if (!(GameManager.instance.gameState == GameState.PLAYING ||
                GameManager.instance.gameState == GameState.PAUSED) ||
                InInventory())
                return;

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

            if (PlayerData.instance.atBench && !GameManager.instance.isPaused)
            {
                GUI.enabled = true;

                // leveling system ui
                if (labelStyle == null)
                {
                    labelStyle = new GUIStyle(GUI.skin.label)
                    {
                        font = trajanNormal,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = 15
                    };
                }

                if (buttonStyle == null)
                {
                    buttonStyle = new GUIStyle(GUI.skin.button)
                    {
                        font = trajanBold,
                        fontStyle = FontStyle.Normal,
                        fontSize = 15,
                        alignment = TextAnchor.MiddleCenter
                    };
                }

                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
                GUI.color = Color.white;

                BonfireRevamped.Instance.Status.RelicLevels =
                    BonfireRevamped.Instance.Status.TotalFreeLevels + BonfireRevamped.Instance.Status.SpentFreeLevels;
                BonfireRevamped.Instance.Status.CurrentLv =
                    BonfireRevamped.Instance.Status.StrengthStat
                    + BonfireRevamped.Instance.Status.DexterityStat
                    + BonfireRevamped.Instance.Status.LuckStat
                    + BonfireRevamped.Instance.Status.ResilienceStat
                    + BonfireRevamped.Instance.Status.WisdomStat
                    + BonfireRevamped.Instance.Status.IntelligenceStat
                    + BonfireRevamped.Instance.Status.SpentGeoLevels
                    + BonfireRevamped.Instance.Status.SpentFreeLevels - 5; //
                if (BonfireRevamped.Instance.Status.CurrentLv == 1)
                    BonfireRevamped.Instance.Status.TotalGeoLevels = 1;
                BonfireRevamped.Instance.Status.GeoLevels =
                    BonfireRevamped.Instance.Status.TotalGeoLevels
                    + BonfireRevamped.Instance.Status.SpentGeoLevels;
                BonfireRevamped.Instance.Status.GeoToLvUp =
                    (int)(Math.Pow(BonfireRevamped.Instance.Status.GeoLevels, 2.0)
                    + (10 * BonfireRevamped.Instance.Status.GeoLevels) + 50.0);
                BonfireRevamped.Instance.Status.FreeLevels =
                    BonfireRevamped.Instance.Status.RL3Levels
                    + BonfireRevamped.Instance.Status.RL4Levels;
                gotFreeLevel = !(BonfireRevamped.Instance.Status.FreeLevels == 0);

                string geoToLevelUp = BonfireRevamped.Instance.Status.GeoToLvUp.ToString();

                string totalInt = (
                    BonfireRevamped.Instance.Status.IntelligenceStat
                    + BonfireRevamped.Instance.Status.IntelligenceIncrease
                ).ToString();
                string totalStr = (
                    BonfireRevamped.Instance.Status.StrengthStat
                    + BonfireRevamped.Instance.Status.StrengthIncrease
                ).ToString();
                string totalDex = (
                    BonfireRevamped.Instance.Status.DexterityStat
                    + BonfireRevamped.Instance.Status.DexterityIncrease
                ).ToString();
                string totalLck = (
                    BonfireRevamped.Instance.Status.LuckStat
                    + BonfireRevamped.Instance.Status.LuckIncrease
                ).ToString();
                string totalRes = (
                    BonfireRevamped.Instance.Status.ResilienceStat
                    + BonfireRevamped.Instance.Status.ResilienceIncrease
                ).ToString();
                string totalWsdm = (
                    BonfireRevamped.Instance.Status.WisdomStat
                    + BonfireRevamped.Instance.Status.WisdomIncrease
                ).ToString();

                string nailDamage = NailDamage(
                    BonfireRevamped.Instance.Status.StrengthIncrease
                    + BonfireRevamped.Instance.Status.StrengthStat
                ).ToString();
                string attackSpeed = AttackSpeed(
                    BonfireRevamped.Instance.Status.DexterityIncrease
                    + BonfireRevamped.Instance.Status.DexterityStat
                ).ToString();
                string extraMasks = ExtraMasks(
                    BonfireRevamped.Instance.Status.ResilienceStat
                    + BonfireRevamped.Instance.Status.ResilienceIncrease
                ).ToString();
                string extraSoul = ExtraSoul(
                    BonfireRevamped.Instance.Status.WisdomStat + BonfireRevamped.Instance.Status.WisdomIncrease,
                    11
                ).ToString();
                string critChance = CritChance(
                    BonfireRevamped.Instance.Status.LuckStat
                    + BonfireRevamped.Instance.Status.LuckIncrease
                ).ToString();
                string critDamage = CritDamage(
                    BonfireRevamped.Instance.Status.DexterityIncrease + BonfireRevamped.Instance.Status.DexterityStat,
                    100
                ).ToString();
                string geoIncrease = (
                    5 * (BonfireRevamped.Instance.Status.LuckStat + BonfireRevamped.Instance.Status.LuckIncrease - 1)
                ).ToString();
                string focusCost = FocusCost(
                    BonfireRevamped.Instance.Status.IntelligenceStat
                    + BonfireRevamped.Instance.Status.IntelligenceIncrease
                ).ToString();
                string soulRegen = SoulRegen(
                    BonfireRevamped.Instance.Status.WisdomStat
                    + BonfireRevamped.Instance.Status.WisdomIncrease
                ).ToString();
                string spellDamage = SpellDamage(
                    100,
                    BonfireRevamped.Instance.Status.IntelligenceStat
                    + BonfireRevamped.Instance.Status.IntelligenceIncrease
                ).ToString();

                string expectedHits;
                if (
                    BonfireRevamped.Instance.Status.ResilienceStat
                    + BonfireRevamped.Instance.Status.ResilienceIncrease > 1
                )
                {
                    expectedHits = ExpectedHits(
                        BonfireRevamped.Instance.Status.ResilienceStat
                        + BonfireRevamped.Instance.Status.ResilienceIncrease
                    ).ToString();
                }
                else
                {
                    expectedHits = "0";
                }

                string applyText;
                if (BonfireRevamped.Instance.Status.RL3Levels <= 0)
                {
                    if (BonfireRevamped.Instance.Status.RL4Levels <= 0)
                    {
                        if (BonfireRevamped.Instance.Status.SpentFreeLevels > 0)
                        {
                            applyText = string.Concat(
                                "Apply (", BonfireRevamped.Instance.Status.SpentGeo,
                                " geo and ",
                                BonfireRevamped.Instance.Status.SpentFreeLevels, " relics)"
                            );
                        }
                        else
                        {
                            applyText = "Apply (" + BonfireRevamped.Instance.Status.SpentGeo + " geo)";
                        }
                    }
                    else
                    {
                        applyText = BonfireRevamped.Instance.Status.RL4Levels + " Free Levels!\n(Arcane Egg)";
                    }
                }
                else
                {
                    applyText = BonfireRevamped.Instance.Status.RL3Levels + " Free Levels!\n(King's Idol)";
                }

                // layout creation begins here
                GUILayout.BeginArea(new Rect(20f, Screen.height / 4.0f, 530f, 700f));
                GUILayout.Label("Level Up", labelStyle);

                GUILayout.BeginHorizontal("box");
                GUILayout.BeginVertical("box");

                if (
                    GUILayout.Button(
                        new GUIContent("Strength: " + totalStr),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(160f)
                    )
                    && CanLevelUp() && PlayerData.instance.atBench
                )
                {
                    IncreaseStat("Strength");
                }
                if (
                    GUILayout.Button(
                        new GUIContent("Dexterity: " + totalDex),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(160f)
                    )
                    && CanLevelUp() && PlayerData.instance.atBench
                )
                {
                    IncreaseStat("Dexterity");
                }
                if (
                    GUILayout.Button(
                        new GUIContent("Intelligence: " + totalInt),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(160f)
                    )
                    && CanLevelUp() && PlayerData.instance.atBench
                )
                {
                    IncreaseStat("Intelligence");
                }
                if (
                    GUILayout.Button(
                        new GUIContent("Resilience: " + totalRes),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(160f)
                    )
                    && CanLevelUp() && PlayerData.instance.atBench
                )
                {
                    IncreaseStat("Resilience");
                }
                if (
                    GUILayout.Button(
                        new GUIContent("Wisdom: " + totalWsdm),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(160f)
                    )
                    && CanLevelUp() && PlayerData.instance.atBench
                )
                {
                    IncreaseStat("Wisdom");
                }
                if (
                    GUILayout.Button(
                        new GUIContent("Luck: " + totalLck),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(160f)
                    )
                    && CanLevelUp() && PlayerData.instance.atBench
                )
                {
                    IncreaseStat("Luck");
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                GUILayout.Label(
                    new GUIContent("Nail Damage: " + nailDamage),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("Slash Speed: " + attackSpeed),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("Spell Damage: " + spellDamage + "%"),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("Extra Masks: " + extraMasks),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("SOUL Gained: " + extraSoul),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("Crit Chance: " + critChance + "%"),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.EndVertical();

                GUILayout.BeginVertical("box");
                GUILayout.Label(new GUIContent(""), labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
                GUILayout.Label(
                    new GUIContent("Crit Damage: " + critDamage + "%"),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("Focus Cost: " + focusCost),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("Hit Resistance: " + expectedHits + "%"),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("SOUL Regen: " + soulRegen),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.Label(
                    new GUIContent("Geo Increase: " + geoIncrease + "%"),
                    labelStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f)
                );
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();

                GUILayout.Label(
                    new GUIContent("Current Level: " + BonfireRevamped.Instance.Status.CurrentLv.ToString()),
                    labelStyle
                );
                GUILayout.Label(new GUIContent("Geo to Level Up: " + geoToLevelUp), labelStyle);

                GUILayout.BeginHorizontal("box");

                GUI.backgroundColor = Color.green;
                if (
                    GUILayout.Button(
                        new GUIContent(applyText),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(258f)
                    )
                    && PlayerData.instance.atBench
                )
                {
                    ApplyLevel();
                }
                GUILayout.FlexibleSpace();

                GUI.backgroundColor = Color.white;
                if (
                    GUILayout.Button(
                        new GUIContent("Cancel"),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(258f)
                    )
                    && PlayerData.instance.atBench
                )
                {
                    BonfireRevamped.Instance.Status.SpentGeo = 0;
                    BonfireRevamped.Instance.Status.StrengthIncrease = 0;
                    BonfireRevamped.Instance.Status.DexterityIncrease = 0;
                    BonfireRevamped.Instance.Status.WisdomIncrease = 0;
                    BonfireRevamped.Instance.Status.ResilienceIncrease = 0;
                    BonfireRevamped.Instance.Status.IntelligenceIncrease = 0;
                    BonfireRevamped.Instance.Status.LuckIncrease = 0;
                    BonfireRevamped.Instance.Status.RL3Levels = PlayerData.instance.trinket3;
                    BonfireRevamped.Instance.Status.RL4Levels = PlayerData.instance.trinket4;
                    BonfireRevamped.Instance.Status.FreeLevels =
                        BonfireRevamped.Instance.Status.RL3Levels
                        + BonfireRevamped.Instance.Status.RL4Levels;
                    BonfireRevamped.Instance.Status.SpentFreeLevels = 0;
                    BonfireRevamped.Instance.Status.SpentGeoLevels = 0;
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal("box");
                GUILayout.FlexibleSpace();
                GUI.backgroundColor = Color.red;
                if (
                    GUILayout.Button(
                        new GUIContent(
                            "Respec (" + BonfireRevamped.Instance.Status.Respec.ToString()
                            + "  Rancid Egg)"
                        ),
                        buttonStyle,
                        GUILayout.Height(40f),
                        GUILayout.Width(522f)
                        )
                        && PlayerData.instance.rancidEggs >= BonfireRevamped.Instance.Status.Respec
                        && PlayerData.instance.atBench
                )
                {
                    Respec();
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                // ui extensions
                BonfireRevamped.Instance.CreateVoidHeartSettingsUI();
                BonfireRevamped.Instance.CreateEnemyHPToggleUI();

                GUILayout.EndArea();
            }

            BonfireRevamped.Instance.DrawEnemyHealthBars();  // call this outside of bench check

            if (!PlayerData.instance.atBench)
            {
                BonfireRevamped.Instance.Status.SpentGeo = 0;
                BonfireRevamped.Instance.Status.StrengthIncrease = 0;
                BonfireRevamped.Instance.Status.DexterityIncrease = 0;
                BonfireRevamped.Instance.Status.WisdomIncrease = 0;
                BonfireRevamped.Instance.Status.ResilienceIncrease = 0;
                BonfireRevamped.Instance.Status.IntelligenceIncrease = 0;
                BonfireRevamped.Instance.Status.LuckIncrease = 0;
                BonfireRevamped.Instance.Status.RL3Levels = PlayerData.instance.trinket3;
                BonfireRevamped.Instance.Status.RL4Levels = PlayerData.instance.trinket4;
                BonfireRevamped.Instance.Status.FreeLevels =
                    BonfireRevamped.Instance.Status.RL3Levels
                    + BonfireRevamped.Instance.Status.RL4Levels;
                BonfireRevamped.Instance.Status.SpentFreeLevels = 0;
                BonfireRevamped.Instance.Status.SpentGeoLevels = 0;
            }
        }

        // nail damage based on strength stat
        public int NailDamage(int totalStr) =>
            (int)Math.Round((5 + 4 * PlayerData.instance.nailSmithUpgrades) * Math.Pow(1.25, Math.Log(totalStr, 2.0)));

        // extra masks based on resilience stat
        public int ExtraMasks(int totalRes) => (int)Math.Round(-0.4 + 2.6 * Math.Log(totalRes));

        // focus cost based on intelligence stat
        public float FocusCost(int totalInt) => (float)Math.Round(34.0 * Math.Exp(-0.01 * (totalInt + 1.0)));

        // crit chance based on luck stat
        public int CritChance(int totalLck) => (int)Math.Round(6.5 * Math.Log(totalLck));

        // nail attack speed based on dexterity stat
        public float AttackSpeed(int totalDex) =>
            (float)Math.Round(2.7 / (1.0 + 1.82 * Math.Exp(-0.08 * totalDex)) - 0.01, 2);

        // geo drop multiplier from enemies based on luck stat (5% per stat level)
        public int IncreaseGeo(int droppedGeo, int totalLck) => (int)(droppedGeo * (1f + (totalLck - 1) / 20f));

        // nail crit damage based on dexterity stat
        public int CritDamage(int totalDex, int nailDamage) => (int)(nailDamage * (1.2 + Math.Log(totalDex)));

        // spell damage based on intelligence stat
        // this only increases wraith/shriek damage, could be updated in the future for others too
        public int SpellDamage(int baseDamage, int totalInt) =>
            (int)Math.Round(baseDamage * Math.Pow(1.25, Math.Log(totalInt, 2.0)));

        // extra soul gained with nail strikes based on wisdom stat
        public int ExtraSoul(int totalWsdm, int baseSoul) => (int)Math.Round(baseSoul + 5.0 * Math.Log(totalWsdm));

        // passive soul regen based on wisdom stat
        public int SoulRegen(int totalWsdm) => (int)Math.Round(0.32 + 0.68 * Math.Log(totalWsdm));

        // immunity frames based on resilience stat
        public float IFrames(int totalRes) => (float)(3.25 / (1.0 + 2.4 * Math.Exp(-0.07 * (totalRes - 1))));


        private Font trajanBold;
        private Font trajanNormal;
        private bool gotFreeLevel;
        private static GUIStyle labelStyle;
        private static GUIStyle buttonStyle;

        // checks if player can level up (be near bench and have free levels or enough geo)
        private bool CanLevelUp()
        {
            return PlayerData.instance.atBench &&
                (
                    gotFreeLevel ||
                    (BonfireRevamped.Instance.Status.GeoToLvUp
                    + BonfireRevamped.Instance.Status.SpentGeo <= PlayerData.instance.geo)
                );
        }

        // checks if inventory is open
        private static bool InInventory()
        {
            GameObject gameObject = GameObject.FindGameObjectWithTag("Inventory Top");
            if (gameObject == null) return false;

            PlayMakerFSM component = FSMUtility.LocateFSM(gameObject, "Inventory Control");
            if (component == null) return false;

            FsmBool fsmBool = component.FsmVariables.GetFsmBool("Open");
            return fsmBool != null && fsmBool.Value;
        }

        // respect levels
        private void Respec()
        {
            PlayerData.instance.AddGeo(BonfireRevamped.Instance.Status.TotalSpentGeo);
            HeroController.instance.AddGeoToCounter(BonfireRevamped.Instance.Status.TotalSpentGeo);
            PlayerData.instance.trinket3 += BonfireRevamped.Instance.Status.TotalFreeLevels;
            PlayMakerFSM.BroadcastEvent("TRINK 3");
            BonfireRevamped.Instance.Status.StrengthStat = 1;
            BonfireRevamped.Instance.Status.DexterityStat = 1;
            BonfireRevamped.Instance.Status.ResilienceStat = 1;
            BonfireRevamped.Instance.Status.WisdomStat = 1;
            BonfireRevamped.Instance.Status.IntelligenceStat = 1;
            BonfireRevamped.Instance.Status.LuckStat = 1;
            BonfireRevamped.Instance.Status.StrengthIncrease = 0;
            BonfireRevamped.Instance.Status.DexterityIncrease = 0;
            BonfireRevamped.Instance.Status.IntelligenceIncrease = 0;
            BonfireRevamped.Instance.Status.ResilienceIncrease = 0;
            BonfireRevamped.Instance.Status.WisdomIncrease = 0;
            BonfireRevamped.Instance.Status.LuckIncrease = 0;
            BonfireRevamped.Instance.Status.SpentGeo = 0;
            BonfireRevamped.Instance.Status.TotalSpentGeo = 0;
            BonfireRevamped.Instance.Status.FreeLevels = 0;
            BonfireRevamped.Instance.Status.TotalFreeLevels = 0;
            BonfireRevamped.Instance.Status.RL3Levels = 0;
            BonfireRevamped.Instance.Status.GeoLevels = 0;
            BonfireRevamped.Instance.Status.TotalGeoLevels = 1;
            BonfireRevamped.Instance.Status.SpentGeoLevels = 0;
            BonfireRevamped.Instance.Status.RelicLevels = 0;
            BonfireRevamped.Instance.Status.CurrentLv = 1;
            PlayerData.instance.rancidEggs -= BonfireRevamped.Instance.Status.Respec;
            BonfireRevamped.Instance.Status.Respec += 1;
            PlayerData.UpdateBlueHealth();
        }

        // level up a specific stat
        private void IncreaseStat(string stat)
        {
            if (stat == "Strength")
                BonfireRevamped.Instance.Status.StrengthIncrease++;
            else if (stat == "Dexterity")
                BonfireRevamped.Instance.Status.DexterityIncrease++;
            else if (stat == "Intelligence")
                BonfireRevamped.Instance.Status.IntelligenceIncrease++;
            else if (stat == "Resilience")
                BonfireRevamped.Instance.Status.ResilienceIncrease++;
            else if (stat == "Wisdom")
                BonfireRevamped.Instance.Status.WisdomIncrease++;
            else
                BonfireRevamped.Instance.Status.LuckIncrease++;

            if (BonfireRevamped.Instance.Status.RL3Levels <= 0)
            {
                if (BonfireRevamped.Instance.Status.RL4Levels <= 0)
                {
                    BonfireRevamped.Instance.Status.SpentGeo += BonfireRevamped.Instance.Status.GeoToLvUp;
                    BonfireRevamped.Instance.Status.SpentGeoLevels++;
                }
                else
                {
                    BonfireRevamped.Instance.Status.RL4Levels--;
                    BonfireRevamped.Instance.Status.SpentFreeLevels++;
                }
            }
            else
            {
                BonfireRevamped.Instance.Status.RL3Levels--;
                BonfireRevamped.Instance.Status.SpentFreeLevels++;
            }
        }

        // apply level by updating player status
        private void ApplyLevel()
        {
            PlayerData.instance.TakeGeo(BonfireRevamped.Instance.Status.SpentGeo);
            PlayerData.instance.trinket3 = BonfireRevamped.Instance.Status.RL3Levels;
            PlayerData.instance.trinket4 = BonfireRevamped.Instance.Status.RL4Levels;
            HeroController.instance.geoCounter.TakeGeo(BonfireRevamped.Instance.Status.SpentGeo);
            BonfireRevamped.Instance.Status.TotalSpentGeo += BonfireRevamped.Instance.Status.SpentGeo;
            BonfireRevamped.Instance.Status.SpentGeo = 0;
            BonfireRevamped.Instance.Status.TotalGeoLevels += BonfireRevamped.Instance.Status.SpentGeoLevels;
            BonfireRevamped.Instance.Status.SpentGeoLevels = 0;
            BonfireRevamped.Instance.Status.TotalFreeLevels += BonfireRevamped.Instance.Status.SpentFreeLevels;
            BonfireRevamped.Instance.Status.SpentFreeLevels = 0;
            BonfireRevamped.Instance.Status.StrengthStat += BonfireRevamped.Instance.Status.StrengthIncrease;
            BonfireRevamped.Instance.Status.DexterityStat += BonfireRevamped.Instance.Status.DexterityIncrease;
            BonfireRevamped.Instance.Status.ResilienceStat += BonfireRevamped.Instance.Status.ResilienceIncrease;
            BonfireRevamped.Instance.Status.WisdomStat += BonfireRevamped.Instance.Status.WisdomIncrease;
            BonfireRevamped.Instance.Status.IntelligenceStat += BonfireRevamped.Instance.Status.IntelligenceIncrease;
            BonfireRevamped.Instance.Status.LuckStat += BonfireRevamped.Instance.Status.LuckIncrease;
            HeroController.instance.CharmUpdate();
            PlayerData.instance.UpdateBlueHealth();
            PlayMakerFSM.BroadcastEvent("UPDATE BLUE HEALTH");

            BonfireRevamped.Instance.Log(
                "Level up applied: "
                + BonfireRevamped.Instance.Status.StrengthIncrease + " Strength, "
                + BonfireRevamped.Instance.Status.DexterityIncrease + " Dexterity, "
                + BonfireRevamped.Instance.Status.IntelligenceIncrease + " Intelligence, "
                + BonfireRevamped.Instance.Status.ResilienceIncrease + " Resilience, "
                + BonfireRevamped.Instance.Status.WisdomIncrease + " Wisdom and "
                + BonfireRevamped.Instance.Status.LuckIncrease + " Luck."
            );
            BonfireRevamped.Instance.Status.StrengthIncrease = 0;
            BonfireRevamped.Instance.Status.DexterityIncrease = 0;
            BonfireRevamped.Instance.Status.WisdomIncrease = 0;
            BonfireRevamped.Instance.Status.ResilienceIncrease = 0;
            BonfireRevamped.Instance.Status.IntelligenceIncrease = 0;
            BonfireRevamped.Instance.Status.LuckIncrease = 0;
            PlayerData.UpdateBlueHealth();
        }

        // helper function for ExpectedHits
        private float IFramesChance(int totalRes, int hitsTaken)
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
                    return
                    (1f - 0.1f * IFrames(totalRes))
                    * (1f - 0.2f * IFrames(totalRes))
                    * (1f - 0.3f * IFrames(totalRes))
                    * 0.5f * IFrames(totalRes);
                case 5:
                    return
                    (1f - 0.1f * IFrames(totalRes))
                    * (1f - 0.2f * IFrames(totalRes))
                    * (1f - 0.3f * IFrames(totalRes))
                    * (1f - 0.5f * IFrames(totalRes))
                    * 0.7f * IFrames(totalRes);
                case 6:
                    return
                    (1f - 0.1f * IFrames(totalRes))
                    * (1f - 0.2f * IFrames(totalRes))
                    * (1f - 0.3f * IFrames(totalRes))
                    * (1f - 0.5f * IFrames(totalRes))
                    * (1f - 0.7f * IFrames(totalRes))
                    * 0.8f * IFrames(totalRes);
                case 7:
                    return
                    (1f - 0.1f * IFrames(totalRes))
                    * (1f - 0.2f * IFrames(totalRes))
                    * (1f - 0.3f * IFrames(totalRes))
                    * (1f - 0.5f * IFrames(totalRes))
                    * (1f - 0.7f * IFrames(totalRes))
                    * (1f - 0.8f * IFrames(totalRes))
                    * 0.9f * IFrames(totalRes);
                default:
                    return 0f;
            }
        }

        // calculates dodge chance percentage
        private float ExpectedHits(int totalRes)
        {
            float num = 0f;
            for (int i = 1; i < 8; i++)
            {
                num += (i + 1) * IFramesChance(totalRes, i);
            }
            return (float)Math.Round(100f / num);
        }
    }
}
