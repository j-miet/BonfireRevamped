using System;

namespace Bonfire
{
    [Serializable]
    public class PlayerStatus
    {
        public int StrengthStat = 1;
        public int DexterityStat = 1;
        public int IntelligenceStat = 1;
        public int ResilienceStat = 1;
        public int WisdomStat = 1;
        public int LuckStat = 1;
        public int StrengthIncrease = 0;
        public int DexterityIncrease = 0;
        public int IntelligenceIncrease = 0;
        public int ResilienceIncrease = 0;
        public int WisdomIncrease = 0;
        public int LuckIncrease = 0;

        public int GeoToLvUp = 0;
        public int CurrentLv = 1;
        public int SpentGeo = 0;
        public int Respec = 1;
        public int FreeLevels = 0;
        public int GeoLevels = 0;
        public int RL3Levels = 0; // available free levels from King's Idols
        public int RL4Levels = 0; // available free levels from Arcane Eggs
        public int TotalFreeLevels = 0;
        public int RelicLevels = 0;
        public int SpentFreeLevels = 0;
        public int TotalGeoLevels = 1;
        public int TotalSpentGeo = 0;
        public int SpentGeoLevels = 0;

        public bool VoidHeartSoulRegenEnabled = true;
        public float VoidHeartSoulRegenMultiplier = 1f;
        public bool VoidHeartSmoothRegen = false;
        public float VoidHeartSoulBuffer = 0f;

        public bool EnemyHealthBarsEnabled = true;

        public static PlayerStatus _instance;
    }
}
