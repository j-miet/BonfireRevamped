using System;
using System.Collections.Generic;

using GlobalEnums;
using HutongGames.PlayMaker;
using UnityEngine;

namespace Bonfire
{
    public class BonfireGUI : MonoBehaviour
    {
        public static BonfireGUI Instance;

        // enemy HP bar data, populated by BonfireMod.OnEnableEnemy
        public readonly Dictionary<HealthManager, float> EnemyMaxHp = new Dictionary<HealthManager, float>();
        public readonly Dictionary<HealthManager, bool> EnemyIsBoss = new Dictionary<HealthManager, bool>();

        // Unity's GUI rendering loop
        public void OnGUI()
        {
            if (GameManager.instance == null) return;

            if (!(GameManager.instance.gameState == GameState.PLAYING ||
                  GameManager.instance.gameState == GameState.PAUSED) ||
                InInventory())
                return;

            EnsureFonts();

            if (PlayerData.instance.atBench && !GameManager.instance.isPaused)
            {
                GUI.enabled = true;
                EnsureStyles();

                GUI.backgroundColor = Color.white;
                GUI.contentColor = Color.white;
                GUI.color = Color.white;

                DrawBenchUI();
            }

            DrawEnemyHealthBars();   // shown outside the bench check

            // reset pending changes when the player leaves the bench
            if (!PlayerData.instance.atBench)
                ResetPendingLevelUp();
        }

        // fonts & styles
        private Font trajanBold;
        private Font trajanNormal;
        private GUIStyle labelStyle;
        private GUIStyle buttonStyle;

        // solid-texture helper for health bar drawing
        private static Texture2D _tex;
        private static Texture2D SolidTex()
        {
            if (_tex == null)
            {
                _tex = new Texture2D(1, 1);
                _tex.SetPixel(0, 0, Color.white);
                _tex.Apply();
            }
            return _tex;
        }

        private void DrawRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, SolidTex());
        }

        // internal Unity methods for startup and GUI updates
        private void Awake()
        {
            Instance = this;
        }

        // bench ui

        /// <summary>
        /// Main bench-side levelling panel (stat buttons, preview labels, apply/cancel/respec).
        /// Called from OnGUI when the player is on a bench.
        /// </summary>
        private void DrawBenchUI()
        {
            var s = BonfireRevamped.Instance.Status;
            var ls = LevellingSystem.Instance;

            if (s.PendingRelicLevels == 0 && s.PendingGeoLevels == 0)
            {
                s.AvailableKingsIdols = PlayerData.instance.trinket3;
                s.AvailableArcaneEggs = PlayerData.instance.trinket4;
            }

            int availableRelicLevels = s.AvailableKingsIdols + s.AvailableArcaneEggs
                                     + s.RespecRelicLevels - s.PendingRelicLevels;
            bool gotFreeLevel = availableRelicLevels > 0;

            int geoLevels = s.TotalGeoLevels + s.PendingGeoLevels;
            int geoToLvlUp = (int)(Math.Pow(geoLevels, 2.0) + (10 * geoLevels) + 50.0);

            int currentLvl = s.StrengthStat + s.DexterityStat + s.LuckStat
                          + s.ResilienceStat + s.WisdomStat + s.IntelligenceStat
                          + s.PendingRelicLevels
                          + s.PendingGeoLevels
                          + 1;

            // preview strings
            string totalStr = (s.StrengthStat + ls.StrengthIncrease).ToString();
            string totalDex = (s.DexterityStat + ls.DexterityIncrease).ToString();
            string totalInt = (s.IntelligenceStat + ls.IntelligenceIncrease).ToString();
            string totalRes = (s.ResilienceStat + ls.ResilienceIncrease).ToString();
            string totalWsdm = (s.WisdomStat + ls.WisdomIncrease).ToString();
            string totalLck = (s.LuckStat + ls.LuckIncrease).ToString();

            string nailDamage = ls.NailDamage(s.StrengthStat + ls.StrengthIncrease).ToString();
            string attackSpeed = ls.AttackSpeed(s.DexterityStat + ls.DexterityIncrease).ToString();
            string extraMasks = ls.ExtraMasks(s.ResilienceStat + ls.ResilienceIncrease).ToString();
            string extraSoul = ls.ExtraSoul(s.WisdomStat + ls.WisdomIncrease, 11).ToString();
            string critChance = ls.CritChance(s.LuckStat + ls.LuckIncrease).ToString();
            string critDamage = ls.CritDamage(s.DexterityStat + ls.DexterityIncrease, 100).ToString();
            string geoIncrease = (5 * (s.LuckStat + ls.LuckIncrease)).ToString();
            string focusCost = ls.FocusCost(s.IntelligenceStat + ls.IntelligenceIncrease).ToString();
            string soulRegen = ls.SoulRegen(s.WisdomStat + ls.WisdomIncrease).ToString();
            string spellDamage = ls.SpellDamage(100, s.IntelligenceStat + ls.IntelligenceIncrease).ToString();

            string expectedHits = s.ResilienceStat + ls.ResilienceIncrease > 0
                ? ls.ExpectedHits(s.ResilienceStat + ls.ResilienceIncrease).ToString()
                : "0";

            // apply button label
            string applyText;
            if (s.PendingRelicLevels > 0 && s.PendingGeo > 0)
            {
                applyText = $"Apply ({s.PendingGeo} geo and {s.PendingRelicLevels} relics)";
            }
            else if (s.PendingRelicLevels > 0)
            {
                applyText = $"Apply ({s.PendingRelicLevels} relic levels)";
            }
            else if (gotFreeLevel)
            {
                applyText = $"{availableRelicLevels} Free Levels available!";
            }
            else
            {
                applyText = $"Apply ({s.PendingGeo} geo)";
            }

            // layout
            GUILayout.BeginArea(new Rect(20f, Screen.height / 4.0f, 530f, 700f));
            GUILayout.Label("Level Up", labelStyle);

            // stats
            GUILayout.BeginHorizontal("box");

            GUILayout.BeginVertical("box");
            DrawStatButton("Strength", totalStr, gotFreeLevel, s, geoToLvlUp);
            DrawStatButton("Dexterity", totalDex, gotFreeLevel, s, geoToLvlUp);
            DrawStatButton("Intelligence", totalInt, gotFreeLevel, s, geoToLvlUp);
            DrawStatButton("Resilience", totalRes, gotFreeLevel, s, geoToLvlUp);
            DrawStatButton("Wisdom", totalWsdm, gotFreeLevel, s, geoToLvlUp);
            DrawStatButton("Luck", totalLck, gotFreeLevel, s, geoToLvlUp);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            DrawStatLabel("Nail Damage: " + nailDamage);
            DrawStatLabel("Slash Speed: " + attackSpeed);
            DrawStatLabel("Spell Damage: " + spellDamage + "%");
            DrawStatLabel("Extra Masks: " + extraMasks);
            DrawStatLabel("SOUL Gained: " + extraSoul);
            DrawStatLabel("Crit Chance: " + critChance + "%");
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            DrawStatLabel("");   // spacer to align with column 1
            DrawStatLabel("Crit Damage: " + critDamage + "%");
            DrawStatLabel("Focus Cost: " + focusCost);
            DrawStatLabel("Hit Resistance: " + expectedHits + "%");
            DrawStatLabel("SOUL Regen: " + soulRegen);
            DrawStatLabel("Geo Increase: " + geoIncrease + "%");
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.Label("Current Level: " + currentLvl, labelStyle);
            GUILayout.Label("Geo to Level Up: " + geoToLvlUp, labelStyle);

            // apply and cancel
            GUILayout.BeginHorizontal("box");

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button(applyText, buttonStyle, GUILayout.Height(40f), GUILayout.Width(258f))
                && PlayerData.instance.atBench)
            {
                LevellingSystem.Instance.ApplyLevel();
            }

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.white;
            if (GUILayout.Button("Cancel", buttonStyle, GUILayout.Height(40f), GUILayout.Width(258f))
                && PlayerData.instance.atBench)
            {
                ResetPendingLevelUp();
            }

            GUILayout.EndHorizontal();

            // respec
            GUILayout.BeginHorizontal("box");
            GUILayout.FlexibleSpace();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button(
                    $"Respec ({s.RespecCost}  Rancid Egg)",
                    buttonStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(522f))
                && PlayerData.instance.rancidEggs >= s.RespecCost
                && PlayerData.instance.atBench)
            {
                LevellingSystem.Instance.Respec();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // extra settings panels: void heart soul regen and enemy hp bar toggling
            DrawVoidHeartSettingsUI();
            DrawEnemyHPToggleUI();

            GUILayout.EndArea();
        }


        // ui panel for Void Heart soul regen controls
        private void DrawVoidHeartSettingsUI()
        {
            var s = BonfireRevamped.Instance.Status;

            GUILayout.BeginVertical("box");
            GUILayout.Label("Void Heart Soul Regen");

            GUI.backgroundColor = Color.white;
            s.VoidHeartSoulRegenEnabled = GUILayout.Toggle(s.VoidHeartSoulRegenEnabled, "Enabled");

            GUILayout.BeginHorizontal();
            GUILayout.Label("Multiplier: " + s.VoidHeartSoulRegenMultiplier.ToString("0.0"));

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("-"))
                s.VoidHeartSoulRegenMultiplier = Mathf.Max(0f, s.VoidHeartSoulRegenMultiplier - 0.1f);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+"))
                s.VoidHeartSoulRegenMultiplier += 0.1f;

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        // toggle button to enable/disable enemy HP bars
        private void DrawEnemyHPToggleUI()
        {
            var s = BonfireRevamped.Instance.Status;

            GUILayout.BeginVertical("box");

            GUI.backgroundColor = Color.white;
            s.EnemyHealthBarsEnabled = GUILayout.Toggle(s.EnemyHealthBarsEnabled, "Enemy Health Bars");

            GUI.enabled = s.EnemyHealthBarsEnabled;
            s.EnemyHpBarColorProgression = GUILayout.Toggle(
                s.EnemyHpBarColorProgression,
                "Toggle HP progression (green -> yellow -> red)");
            GUI.enabled = true;

            GUILayout.EndVertical();
        }

        /// draws floating health bars above tracked enemies
        private void DrawEnemyHealthBars()
        {
            if (!BonfireRevamped.Instance.Status.EnemyHealthBarsEnabled) return;

            foreach (var kvp in EnemyMaxHp)
            {
                HealthManager hm = kvp.Key;
                if (hm == null) continue;

                Vector3 screenPos = Camera.main.WorldToScreenPoint(hm.transform.position);
                if (screenPos.z < 0) continue;

                float maxHp = kvp.Value;
                float currentHp = hm.hp;
                bool isBoss = EnemyIsBoss.TryGetValue(hm, out bool b) && b;

                if (currentHp == maxHp && !isBoss) continue; // full HP = hide bars for normal enemies

                float pct = Mathf.Clamp01(currentHp / maxHp);
                float width = isBoss ? 300f : 120f;
                float height = isBoss ? 20f : 10f;
                float x = screenPos.x - width / 2f;
                float y = Screen.height - screenPos.y - 20f;

                DrawRect(new Rect(x, y, width, height), new Color(0f, 0f, 0f, 0.7f));

                Color fillColor;
                if (BonfireRevamped.Instance.Status.EnemyHpBarColorProgression)
                    fillColor = pct > 0.6f ? Color.green : pct > 0.3f ? Color.yellow : Color.red;
                else
                    fillColor = isBoss ? new Color(1f, 0.5f, 0f) : Color.red;

                DrawRect(new Rect(x, y, width * pct, height), fillColor);

                GUI.color = Color.white;
            }
        }

        // helper functions
        private void DrawStatButton(string statName, string displayValue, bool gotFreeLevel, PlayerStatus s,
                                    int geoToLvlUp)
        {
            if (GUILayout.Button(
                    $"{statName}: {displayValue}",
                    buttonStyle,
                    GUILayout.Height(40f),
                    GUILayout.Width(160f))
                && CanLevelUp(gotFreeLevel, geoToLvlUp)
                && PlayerData.instance.atBench)
            {
                LevellingSystem.Instance.IncreaseStat(statName, s, geoToLvlUp);
            }
        }

        private void DrawStatLabel(string text)
        {
            GUILayout.Label(text, labelStyle, GUILayout.Height(40f), GUILayout.Width(160f));
        }

        private bool CanLevelUp(bool gotFreeLevel, int geoToLvlUp)
        {
            var s = BonfireRevamped.Instance.Status;
            return PlayerData.instance.atBench &&
                   (gotFreeLevel || (geoToLvlUp + s.PendingGeo <= PlayerData.instance.geo));
        }

        // resets all pending (unapplied) stat allocations
        private void ResetPendingLevelUp()
        {
            var s = BonfireRevamped.Instance.Status;
            var ls = LevellingSystem.Instance;

            ls.StrengthIncrease = 0;
            ls.DexterityIncrease = 0;
            ls.WisdomIncrease = 0;
            ls.ResilienceIncrease = 0;
            ls.IntelligenceIncrease = 0;
            ls.LuckIncrease = 0;

            s.PendingGeo = 0;
            s.PendingGeoLevels = 0;
            s.PendingRelicLevels = 0;

            // reset relic values too. This way the relic snapshot in DrawBenchUI always
            // re-reads from inventory the next time the player sits on a bench.
            s.AvailableKingsIdols = 0;
            s.AvailableArcaneEggs = 0;
        }

        private void EnsureFonts()
        {
            if (trajanBold != null && trajanNormal != null) return;

            foreach (Font font in Resources.FindObjectsOfTypeAll<Font>())
            {
                if (font == null) continue;
                if (font.name == "TrajanPro-Bold") trajanBold = font;
                if (font.name == "TrajanPro-Regular") trajanNormal = font;
            }
        }

        private void EnsureStyles()
        {
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
        }

        private static bool InInventory()
        {
            GameObject go = GameObject.FindGameObjectWithTag("Inventory Top");
            if (go == null) return false;

            PlayMakerFSM fsm = FSMUtility.LocateFSM(go, "Inventory Control");
            if (fsm == null) return false;

            FsmBool open = fsm.FsmVariables.GetFsmBool("Open");
            return open != null && open.Value;
        }
    }
}
