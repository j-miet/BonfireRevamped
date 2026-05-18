using System;
using System.Collections.Generic;
using HutongGames.PlayMaker;
using UnityEngine;
using System.Reflection;
using Modding;
using UnityEngine.SceneManagement;


namespace Bonfire
{
    public class BonfireMod : Mod, ILocalSettings<PlayerStatus>
    {
        public static BonfireMod Instance;

        public PlayerStatus Status = new PlayerStatus();
        public void OnLoadLocal(PlayerStatus s) => Status = s;
        public PlayerStatus OnSaveLocal() => Status;

        public override void Initialize()
        {
            Instance = this;
            Instance.LogDebug("Bonfire Mod initializing!");

            BonfyBench.Load();

            ModHooks.NewGameHook += SetupGameRefs;
            ModHooks.SavegameLoadHook += SetupGameRefs;
            ModHooks.CharmUpdateHook += BenchApply;
            ModHooks.SoulGainHook += SoulGain;
            ModHooks.HeroUpdateHook += MpRegen;
            On.PlayerData.UpdateBlueHealth += UpdateBlueHealth;
            ModHooks.FocusCostHook += FocusCost;
            ModHooks.SlashHitHook += CritHit;
            ModHooks.CursorHook += ShowCursor;
            ModHooks.HitInstanceHook += SetDamages;
            ModHooks.AfterTakeDamageHook += ResShield;
            ModHooks.HeroUpdateHook += HeroUpdate;
            ModHooks.OnEnableEnemyHook += OnEnableEnemy;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;

            Instance.LogDebug("Bonfire Mod v." + GetVersion() + " initialized!");
        }

        private void UpdateBlueHealth(On.PlayerData.orig_UpdateBlueHealth orig, PlayerData self)
        {
            orig(self);
            self.SetInt("healthBlue", self.GetInt("healthBlue") + ls.ExtraMasks(Status.ResilienceStat));
        }

        private bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
        {
            HealthManager hm = enemy.GetComponent<HealthManager>();
            if (hm != null && hm.hp < 5000 && !isAlreadyDead)
            {
                LogDebug($@"Vanilla HP for {enemy.name} = {hm.hp}");
                hm.hp *= (int)((1.25 + (double)Dreamers / 3) * (2.5 / (1.0 + Math.Exp(-0.05 * Status.CurrentLv))));
                LogDebug($@"Bonfire HP for {enemy.name} = {hm.hp}");

                hm.SetGeoSmall(ls.IncreaseGeo(GetGeo("small", hm), Status.LuckStat));
                hm.SetGeoMedium(ls.IncreaseGeo(GetGeo("medium", hm), Status.LuckStat));
                hm.SetGeoLarge(ls.IncreaseGeo(GetGeo("large", hm), Status.LuckStat));
            }
            return isAlreadyDead;
        }

        private void HeroUpdate()
        {
            if (GameManager.instance.inputHandler.inputActions.attack.WasPressed)
                critRoll = UnityEngine.Random.Range(1, 100);
        }

        private HitInstance SetDamages(Fsm owner, HitInstance hit)
        {
            bool isSpell;

            switch (hit.Source.name)
            {
                case "Fireball(Clone)":
                case "Fireball":
                case "Hit L": // Dive
                case "Hit R": // Dive
                case "Q Fall Damage": // Dive
                case "Hit U": // Wraiths
                    isSpell = true;
                    break;
                default:
                    isSpell = false;
                    break;
            }

            if (isSpell)
            {
                LogDebug($"[Vanilla] Spell name: {hit.Source.name} - {hit.Source}. Damage: {hit.DamageDealt}");
                hit.DamageDealt = ls.SpellDamage(hit.DamageDealt, Status.IntelligenceStat);
                LogDebug($"[Bonfire] Spell name: {hit.Source.name} - {hit.Source}. Damage: {hit.DamageDealt}");
            }

            if (hit.Source.name.Contains("lash"))
            {
                LogDebug($@"[Vanilla] Damage for {hit.Source.name} = {hit.DamageDealt}");
                hit.DamageDealt = ls.NailDamage(Status.StrengthStat);
                LogDebug($@"[Bonfire] Damage for {hit.Source.name} = {hit.DamageDealt}");
                LogDebug($@"Crit chance: {ls.CritChance(Status.LuckStat)}. Rolled {critRoll}.");
                Crit = critRoll <= ls.CritChance(Status.LuckStat);
                if (Crit)
                {
                    hit.DamageDealt = ls.CritDamage(Status.DexterityStat, hit.DamageDealt);
                    LogDebug($@"[Crit] Damage for {hit.Source.name} = {hit.DamageDealt}");
                    HeroController.instance.GetComponent<SpriteFlash>().FlashGrimmflame();
                    HeroController.instance.carefreeShield.SetActive(true);
                }
            }

            return hit;
        }

        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            BonfyBench.Replace(arg0);

            if (pd == null && PlayerData.instance != null)
                pd = PlayerData.instance;

            Dreamers = 0;
            if (pd.lurienDefeated)
                Dreamers++;
            if (pd.hegemolDefeated)
                Dreamers++;
            if (pd.monomonDefeated)
                Dreamers++;
        }

        private int GetGeo(string size, HealthManager enemy)
        {
            FieldInfo fi = enemy.GetType().GetField(size + "GeoDrops", BindingFlags.NonPublic | BindingFlags.Instance);
            object geo = fi.GetValue(enemy);
            int ret = geo == null ? 0 : (int)geo;
            return ret;
        }

        

        public int ResShield(int hazardType, int damage)
        {
            if (Status.ResilienceStat > 1 && hazardType == 1)
            {
                if (HitsSinceShielded > 7)
                    HitsSinceShielded = 7;
                float num = UnityEngine.Random.Range(1, 100);
                float iframes = ls.IFrames(Status.ResilienceStat);
                float multiplier = 0f;
                switch (HitsSinceShielded)
                {
                    case 1:
                        multiplier = 10f;
                        break;
                    case 2:
                        multiplier = 20f;
                        break;
                    case 3:
                        multiplier = 30f;
                        break;
                    case 4:
                        multiplier = 50f;
                        break;
                    case 5:
                        multiplier = 70f;
                        break;
                    case 6:
                        multiplier = 80f;
                        break;
                    case 7:
                        multiplier = 90f;
                        break;
                }
                LogDebug($"{num} <= {multiplier} * {iframes}");
                if (multiplier > 0 && num <= multiplier * iframes)
                {
                    HitsSinceShielded = 0;
                    HeroController.instance.carefreeShield.SetActive(true);
                    damage = 0;
                }
                else
                {
                    HitsSinceShielded++;
                }
            }
            return damage;
        }

        public void ShowCursor()
        {
            if (HeroController.instance != null && HeroController.instance.cState != null && GameManager.instance != null)
            {
                Cursor.visible = HeroController.instance.cState.nearBench || GameManager.instance.isPaused;
                return;
            }

            if (GameManager.instance != null)
                Cursor.visible = GameManager.instance.isPaused;

        }

        public void CritHit(Collider2D otherCollider, GameObject go)
        {
            if (Crit && otherCollider.gameObject.layer == 11)
            {
                HeroController.instance.shadowRingPrefab.transform.SetScaleX(0.5f);
                HeroController.instance.shadowRingPrefab.transform.SetScaleY(0.5f);
                UnityEngine.Object.Instantiate(HeroController.instance.shadowRingPrefab, otherCollider.gameObject.transform.position, go.transform.rotation);
            }
        }

        public float FocusCost() => (float)ls.FocusCost(Status.IntelligenceStat) / 33f;

        public int BlueHPRestored() => ls.ExtraMasks(Status.ResilienceStat);
        
        public void MpRegen()
        {
            if (HeroController.instance != null && PlayerData.instance != null)
            {
                try
                {
                    if (manaRegenTime > 0)
                    {
                        manaRegenTime -= Time.deltaTime;
                    }
                    else
                    {
                        LogDebug($@"Recovering MP!");
                        manaRegenTime = 1.11f;
                        HeroController.instance.AddMPChargeSpa(ls.SoulRegen(Status.WisdomStat));
                    }
                }
                catch { }
            }
        }

        public int SoulGain(int num) => ls.ExtraSoul(Status.WisdomStat, num);

        public void SetupGameRefs(int id) => SetupGameRefs();

        public void SetupGameRefs()
        {
            if (gm == null)
            {
                gm = GameManager.instance;
                gm.gameObject.AddComponent<LevellingSystem>();
            }
            if (ls == null)
                ls = LevellingSystem.Instance;
            if (pd == null && PlayerData.instance != null)
                pd = PlayerData.instance;
        }

        public void BenchApply(PlayerData pd, HeroController hc)
        {
            HeroController.instance.ATTACK_DURATION = 0.35f / LevellingSystem.Instance.AttackSpeed(Status.DexterityStat);
            HeroController.instance.ATTACK_DURATION_CH = 0.25f / LevellingSystem.Instance.AttackSpeed(Status.DexterityStat);
            HeroController.instance.ATTACK_COOLDOWN_TIME = 0.41f / LevellingSystem.Instance.AttackSpeed(Status.DexterityStat);
            HeroController.instance.ATTACK_COOLDOWN_TIME_CH = 0.25f / LevellingSystem.Instance.AttackSpeed(Status.DexterityStat);
        }

        public void SetupNewModData()
        {
            Status.CurrentLv = 1;
            Status.StrengthIncrease = 0;
            Status.DexterityIncrease = 0;
            Status.IntelligenceIncrease = 0;
            Status.ResilienceIncrease = 0;
            Status.WisdomIncrease = 0;
            Status.LuckIncrease = 0;
            Status.SpentGeo = 0;
            Status.TotalSpentGeo = 0;
            Status.Respec = 1;
            Status.GeoLevels = 0;
            Status.TotalGeoLevels = 1;
            Status.SpentGeoLevels = 0;
            Status.StrengthStat = 1;
            Status.DexterityStat = 1;
            Status.IntelligenceStat = 1;
            Status.ResilienceStat = 1;
            Status.WisdomStat = 1;
            Status.LuckStat = 1;
            LogDebug("Set up new player data.");
        }
        

        public override string GetVersion() => "3.0.1";
        public int HitsSinceShielded { get; set; } = 0;
        public int Dreamers;
        public bool Crit { get; set; } = false;
        public int critRoll;
        public float manaRegenTime;
        public static GameManager gm;
        public LevellingSystem ls;
        public static PlayerData pd;
    }
}
