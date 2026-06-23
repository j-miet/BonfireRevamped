using System;

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
                    _instance = new LevellingSystem();
                return _instance;
            }
            set => _instance = value;
        }

        // delegate GUI rendering to BonfireGUI.OnGUI
        public void OnGUI() => BonfireGUI.Instance?.OnGUI();

        // all stat formulas

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
        public int SpellDamage(int baseDamage, int totalInt) =>
            (int)Math.Round(baseDamage * Math.Pow(1.25, Math.Log(totalInt, 2.0)));

        // extra soul gained with nail strikes based on wisdom stat
        public int ExtraSoul(int totalWsdm, int baseSoul) => (int)Math.Round(baseSoul + 5.0 * Math.Log(totalWsdm));

        // passive soul regen based on wisdom stat
        public int SoulRegen(int totalWsdm) => (int)Math.Round(0.32 + 0.68 * Math.Log(totalWsdm));

        // immunity frames based on resilience stat
        public float IFrames(int totalRes) => (float)(3.25 / (1.0 + 2.4 * Math.Exp(-0.07 * (totalRes - 1))));

        // calculates dodge chance percentage (used by BonfireGUI for preview label)
        public float ExpectedHits(int totalRes)
        {
            float num = 0f;
            for (int i = 1; i < 8; i++)
                num += (i + 1) * IFramesChance(totalRes, i);
            return (float)Math.Round(100f / num);
        }

        /// <summary>
        /// Increments the pending allocation for a single stat.
        /// </summary>
        public void IncreaseStat(string stat, PlayerStatus s)
        {
            if (stat == "Strength") s.StrengthIncrease++;
            else if (stat == "Dexterity") s.DexterityIncrease++;
            else if (stat == "Intelligence") s.IntelligenceIncrease++;
            else if (stat == "Resilience") s.ResilienceIncrease++;
            else if (stat == "Wisdom") s.WisdomIncrease++;
            else s.LuckIncrease++;

            if (s.RL3Levels <= 0)
            {
                if (s.RL4Levels <= 0)
                {
                    s.SpentGeo += s.GeoToLvUp;
                    s.SpentGeoLevels++;
                }
                else
                {
                    s.RL4Levels--;
                    s.SpentFreeLevels++;
                }
            }
            else
            {
                s.RL3Levels--;
                s.SpentFreeLevels++;
            }
        }

        /// <summary>
        /// Commits the pending stat allocations, deducting geo and relics.
        /// </summary>
        public void ApplyLevel()
        {
            var s = BonfireRevamped.Instance.Status;

            PlayerData.instance.TakeGeo(s.SpentGeo);
            PlayerData.instance.trinket3 = s.RL3Levels;
            PlayerData.instance.trinket4 = s.RL4Levels;
            HeroController.instance.geoCounter.TakeGeo(s.SpentGeo);

            s.TotalSpentGeo += s.SpentGeo;
            s.SpentGeo = 0;

            s.TotalGeoLevels += s.SpentGeoLevels;
            s.SpentGeoLevels = 0;

            s.TotalFreeLevels += s.SpentFreeLevels;
            s.SpentFreeLevels = 0;

            s.StrengthStat += s.StrengthIncrease;
            s.DexterityStat += s.DexterityIncrease;
            s.ResilienceStat += s.ResilienceIncrease;
            s.WisdomStat += s.WisdomIncrease;
            s.IntelligenceStat += s.IntelligenceIncrease;
            s.LuckStat += s.LuckIncrease;

            HeroController.instance.CharmUpdate();
            PlayerData.instance.UpdateBlueHealth();
            PlayMakerFSM.BroadcastEvent("UPDATE BLUE HEALTH");

            BonfireRevamped.Instance.Log(
                "Level up applied: "
                + s.StrengthIncrease + " Strength, "
                + s.DexterityIncrease + " Dexterity, "
                + s.IntelligenceIncrease + " Intelligence, "
                + s.ResilienceIncrease + " Resilience, "
                + s.WisdomIncrease + " Wisdom and "
                + s.LuckIncrease + " Luck."
            );

            s.StrengthIncrease = 0;
            s.DexterityIncrease = 0;
            s.WisdomIncrease = 0;
            s.ResilienceIncrease = 0;
            s.IntelligenceIncrease = 0;
            s.LuckIncrease = 0;

            PlayerData.UpdateBlueHealth();
        }

        /// <summary>
        /// Refunds all spent levels and resets stats to 1, consuming one Rancid Egg.
        /// </summary>
        public void Respec()
        {
            var s = BonfireRevamped.Instance.Status;

            PlayerData.instance.AddGeo(s.TotalSpentGeo);
            HeroController.instance.AddGeoToCounter(s.TotalSpentGeo);
            s.TotalSpentGeo = 0;

            PlayerData.instance.trinket3 += s.TotalFreeLevels;
            PlayMakerFSM.BroadcastEvent("TRINK 3");

            s.StrengthStat = 1; s.DexterityStat = 1;
            s.ResilienceStat = 1; s.WisdomStat = 1;
            s.IntelligenceStat = 1; s.LuckStat = 1;

            s.StrengthIncrease = 0; s.DexterityIncrease = 0;
            s.IntelligenceIncrease = 0; s.ResilienceIncrease = 0;
            s.WisdomIncrease = 0; s.LuckIncrease = 0;

            s.SpentGeo = 0;
            s.FreeLevels = 0;
            s.TotalFreeLevels = 0;
            s.RL3Levels = 0;
            s.GeoLevels = 0;
            s.TotalGeoLevels = 1;
            s.SpentGeoLevels = 0;
            s.RelicLevels = 0;
            s.CurrentLv = 1;

            PlayerData.instance.rancidEggs -= s.Respec;
            s.Respec += 1;

            PlayerData.UpdateBlueHealth();
        }

        // helper for ExpectedHits
        private float IFramesChance(int totalRes, int hitsTaken)
        {
            if (hitsTaken > 7) hitsTaken = 7;

            float f = IFrames(totalRes);
            switch (hitsTaken)
            {
                case 1:
                    return 0.1f * f;
                case 2:
                    return (1f - 0.1f * f) * 0.2f * f;
                case 3:
                    return (1f - 0.1f * f) * (1f - 0.2f * f) * 0.3f * f;
                case 4:
                    return (1f - 0.1f * f) * (1f - 0.2f * f) * (1f - 0.3f * f) * 0.5f * f;
                case 5:
                    return (1f - 0.1f * f) * (1f - 0.2f * f) * (1f - 0.3f * f) * (1f - 0.5f * f) * 0.7f * f;
                case 6:
                    return (1f - 0.1f * f)
                    * (1f - 0.2f * f)
                    * (1f - 0.3f * f)
                    * (1f - 0.5f * f)
                    * (1f - 0.7f * f)
                    * 0.8f * f;
                case 7:
                    return (1f - 0.1f * f)
                    * (1f - 0.2f * f)
                    * (1f - 0.3f * f)
                    * (1f - 0.5f * f)
                    * (1f - 0.7f * f)
                    * (1f - 0.8f * f)
                    * 0.9f * f;
                default:
                    return 0f;
            }
        }
    }
}
