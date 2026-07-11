using System;

using UnityEngine;

namespace Bonfire
{
    public class LevellingSystem : MonoBehaviour
    {
        public int StrengthIncrease = 0;
        public int DexterityIncrease = 0;
        public int IntelligenceIncrease = 0;
        public int ResilienceIncrease = 0;
        public int WisdomIncrease = 0;
        public int LuckIncrease = 0;

        public static LevellingSystem Instance
        {
            get => _instance;
            private set => _instance = value;
        }

        // stat formulas

        // strength: nail damage
        public int NailDamage(int totalStr) =>
            (int)Math.Round(
                    (5 + 4 * PlayerData.instance.nailSmithUpgrades) * Math.Pow(1.25, Math.Log(totalStr + 1, 2.0)));

        // dexterity: nail attack speed
        public float AttackSpeed(int totalDex) =>
            (float)Math.Round(2.7 / (1.0 + 1.82 * Math.Exp(-0.08 * (totalDex + 1))) - 0.01, 2);

        // dexterity: nail crit damage
        public int CritDamage(int totalDex, int nailDamage) => (int)(nailDamage * (1.2 + Math.Log(totalDex + 1)));

        // intelligence: spell damage
        public int SpellDamage(int baseDamage, int totalInt) =>
            (int)Math.Round(baseDamage * Math.Pow(1.25, Math.Log(totalInt + 1, 2.0)));

        // intelligence: focus cost
        public float FocusCost(int totalInt) => (float)Math.Round(34.0 * Math.Exp(-0.01 * (totalInt + 2.0)));

        // resilience: extra lifeblood masks
        public int ExtraMasks(int totalRes) => (int)Math.Round(-0.4 + 2.6 * Math.Log(totalRes + 1));

        // resilience: chance to block an enemy hit. 
        // This is the raw value: AverageBlockChance gives the practical average
        public float BlockChance(int totalRes) => (float)(0.9 / (1.0 + 8.0 * Math.Exp(-0.09 * totalRes)));

        // wisdom: extra soul gained on nail strikes
        public int ExtraSoul(int totalWsdm, int baseSoul) => (int)Math.Round(baseSoul + 5.0 * Math.Log(totalWsdm + 1));

        // wisdom: passive soul regen
        public int SoulRegen(int totalWsdm) => (int)Math.Round(0.32 + 0.68 * Math.Log(totalWsdm + 1));

        // luck: crit chance
        public int CritChance(int totalLck) => (int)Math.Round(6.5 * Math.Log(totalLck + 1));

        // luck: geo drop multiplier from enemies (5% per stat level)
        public int IncreaseGeo(int droppedGeo, int totalLck) => (int)(droppedGeo * (1f + totalLck / 20f));

        /// <summary>
        /// Increments the pending allocation for a single stat.
        /// </summary>
        public void IncreaseStat(string stat, PlayerStatus s, int geoToLvlUp)
        {
            if (stat == "Strength") StrengthIncrease++;
            else if (stat == "Dexterity") DexterityIncrease++;
            else if (stat == "Intelligence") IntelligenceIncrease++;
            else if (stat == "Resilience") ResilienceIncrease++;
            else if (stat == "Wisdom") WisdomIncrease++;
            else LuckIncrease++;

            int availableRelicLevels = s.AvailableKingsIdols + s.AvailableArcaneEggs
                         + s.RespecRelicLevels - s.PendingRelicLevels;

            if (availableRelicLevels > 0)
            {
                if (s.RespecRelicLevels - s.PendingRelicLevels > 0)
                {
                    // use respec pool first, no item to deduct
                }
                else if (s.AvailableKingsIdols > 0) s.AvailableKingsIdols--;
                else if (s.AvailableArcaneEggs > 0) s.AvailableArcaneEggs--;

                s.PendingRelicLevels++;
            }
            else
            {
                s.PendingGeo += geoToLvlUp;
                s.PendingGeoLevels++;
            }
        }

        /// <summary>
        /// Commits the pending stat allocations, deducting geo and relics + relic levels
        /// </summary>
        public void ApplyLevel()
        {
            var s = BonfireRevamped.Instance.Status;

            int idolsSpent = PlayerData.instance.trinket3 - s.AvailableKingsIdols;
            int eggsSpent = PlayerData.instance.trinket4 - s.AvailableArcaneEggs;
            PlayerData.instance.trinket3 -= idolsSpent;
            PlayerData.instance.trinket4 -= eggsSpent;

            int respecLevelsConsumed = s.PendingRelicLevels - (idolsSpent + eggsSpent);
            s.RespecRelicLevels -= respecLevelsConsumed;

            s.PendingRelicLevels = 0;
            s.AvailableKingsIdols = 0;
            s.AvailableArcaneEggs = 0;

            PlayerData.instance.TakeGeo(s.PendingGeo);
            HeroController.instance.geoCounter.TakeGeo(s.PendingGeo);
            s.TotalSpentGeo += s.PendingGeo;
            s.PendingGeo = 0;

            s.TotalGeoLevels += s.PendingGeoLevels;
            s.PendingGeoLevels = 0;

            s.StrengthStat += StrengthIncrease;
            s.DexterityStat += DexterityIncrease;
            s.ResilienceStat += ResilienceIncrease;
            s.WisdomStat += WisdomIncrease;
            s.IntelligenceStat += IntelligenceIncrease;
            s.LuckStat += LuckIncrease;

            BonfireRevamped.Instance.Log(
                "Level up applied: "
                + StrengthIncrease + " Strength, "
                + DexterityIncrease + " Dexterity, "
                + IntelligenceIncrease + " Intelligence, "
                + ResilienceIncrease + " Resilience, "
                + WisdomIncrease + " Wisdom and "
                + LuckIncrease + " Luck."
            );

            StrengthIncrease = 0;
            DexterityIncrease = 0;
            ResilienceIncrease = 0;
            WisdomIncrease = 0;
            IntelligenceIncrease = 0;
            LuckIncrease = 0;

            HeroController.instance.CharmUpdate();
            PlayMakerFSM.BroadcastEvent("UPDATE BLUE HEALTH");
        }

        /// <summary>
        /// Refunds all spent levels and resets stats to 1, consuming rancid eggs.
        /// </summary>
        public void Respec()
        {
            var s = BonfireRevamped.Instance.Status;
            int relicLevelsSpent = s.StrengthStat + s.DexterityStat + s.IntelligenceStat
                                 + s.ResilienceStat + s.WisdomStat + s.LuckStat
                                 - (s.TotalGeoLevels - 1);

            if (s.TotalGeoLevels == 1 && relicLevelsSpent == 0) return;

            // reset to 0 first so repeated respecs don't accumulate previously-refunded levels
            s.RespecRelicLevels = 0;
            s.RespecRelicLevels += relicLevelsSpent;

            PlayerData.instance.AddGeo(s.TotalSpentGeo);
            HeroController.instance.AddGeoToCounter(s.TotalSpentGeo);
            s.TotalSpentGeo = 0;
            s.TotalGeoLevels = 1;

            s.StrengthStat = 0;
            s.DexterityStat = 0;
            s.ResilienceStat = 0;
            s.WisdomStat = 0;
            s.IntelligenceStat = 0;
            s.LuckStat = 0;

            StrengthIncrease = 0;
            DexterityIncrease = 0;
            IntelligenceIncrease = 0;
            ResilienceIncrease = 0;
            WisdomIncrease = 0;
            LuckIncrease = 0;

            s.PendingGeo = 0;
            s.PendingGeoLevels = 0;
            s.PendingRelicLevels = 0;
            s.AvailableKingsIdols = 0;
            s.AvailableArcaneEggs = 0;

            PlayerData.instance.rancidEggs -= s.RespecCost;
            s.RespecCost++;

            HeroController.instance.CharmUpdate();
            PlayMakerFSM.BroadcastEvent("UPDATE BLUE HEALTH");
        }

        /// <summary>
        /// Returns average damage blocking chance from current resiliency stat value
        /// </summary>
        public float AverageBlockChance(int totalRes)
        {
            float f = BlockChance(totalRes);
            float[] multipliers = { 10f, 20f, 30f, 50f, 70f, 80f, 90f };
            float sum = 0f;
            foreach (var m in multipliers)
                sum += m * f;
            return sum / multipliers.Length;
        }


        private static LevellingSystem _instance;

        private void Awake()
        {
            _instance = this;
        }
    }
}
