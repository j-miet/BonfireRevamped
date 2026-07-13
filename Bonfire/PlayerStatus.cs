using System;

namespace BonfireRelit
{
    [Serializable]
    public class PlayerStatus
    {
        public int StrengthStat = 0;
        public int DexterityStat = 0;
        public int IntelligenceStat = 0;
        public int ResilienceStat = 0;
        public int WisdomStat = 0;
        public int LuckStat = 0;

        public int TotalGeoLevels = 1; // levelling starts from 1
        public int TotalSpentGeo = 0;
        public int PendingGeoLevels = 0;
        public int PendingGeo = 0;

        public int RespecRelicLevels = 0; // available free levels earned back from respecs
        public int PendingRelicLevels = 0;
        public int AvailableKingsIdols = 0;
        public int AvailableArcaneEggs = 0;
        public int RespecCost = 1;

        public bool VoidHeartSoulRegenEnabled = false;
        public float VoidHeartSoulRegenMultiplier = 1f;
        public float VoidHeartSoulBuffer = 0f;

        public bool EnemyHealthBarsEnabled = false;
        public bool EnemyHpBarColorProgression = false;

        public static PlayerStatus _instance;
    }
}
