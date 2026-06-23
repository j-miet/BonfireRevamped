using System.Collections.Generic;
using System.Reflection;

using HutongGames.PlayMaker;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bonfire
{
    public class BonfireRevamped : Mod, ILocalSettings<PlayerStatus>
    {
        public PlayerStatus Status = new PlayerStatus();
        public void OnLoadLocal(PlayerStatus s) => Status = s;
        public PlayerStatus OnSaveLocal() => Status;

        public override string GetVersion() => "1.1.0";
        public int HitsSinceShielded { get; set; } = 0;
        public bool Crit { get; set; } = false;

        public static BonfireRevamped Instance;
        public static GameManager gm;
        public static PlayerData pd;

        public override void Initialize()
        {
            Instance = this;
            Instance.LogDebug("Bonfire Mod initializing!");

            ModHooks.NewGameHook += SetupGameRefs;
            ModHooks.SavegameLoadHook += SetupGameRefs;
            ModHooks.CharmUpdateHook += BenchApply;
            ModHooks.SoulGainHook += SoulGain;
            On.PlayerData.UpdateBlueHealth += UpdateBlueHealth;
            ModHooks.FocusCostHook += FocusCost;
            ModHooks.SlashHitHook += CritHit;
            ModHooks.CursorHook += ShowCursor;
            ModHooks.HitInstanceHook += SetDamages;
            ModHooks.AfterTakeDamageHook += ResShield;
            ModHooks.HeroUpdateHook += HeroUpdate;
            ModHooks.OnEnableEnemyHook += OnEnableEnemy;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += SceneLoaded;

            Instance.LogDebug("BonfireRevamped v." + GetVersion() + " initialized!");
        }

        private int Dreamers; // can remove this + SceneLoaded unless some other dreamer-reliant system gets added
        private int critRoll;
        private float manaRegenTime;
        private LevellingSystem ls;

        // inject levelling system and GUI into game
        private void SetupGameRefs(int id) => SetupGameRefs();
        private void SetupGameRefs()
        {
            if (gm == null)
            {
                gm = GameManager.instance;
                gm.gameObject.AddComponent<LevellingSystem>();
                gm.gameObject.AddComponent<BonfireGUI>();
            }
            if (ls == null)
                ls = LevellingSystem.Instance;
            if (pd == null && PlayerData.instance != null)
                pd = PlayerData.instance;
        }

        // calls SetupGameRefs after each scene transition
        // ORIGINAL FUNCTION was to apply dreamer kill status which was then used for scaling enemy hp based on amount
        // of dreamers killed. This has been disabled.
        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            SetupGameRefs();

            /*
            if (pd == null && PlayerData.instance != null)
                pd = PlayerData.instance;

            Dreamers = 0;
            if (pd.lurienDefeated) Dreamers++;
            if (pd.hegemolDefeated) Dreamers++;
            if (pd.monomonDefeated) Dreamers++;
            */
        }

        // add health bars for enemies and scale their geo drops by luck stat
        private bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
        {
            HealthManager hm = enemy.GetComponent<HealthManager>();

            if (hm != null)
            {
                var gui = BonfireGUI.Instance;
                if (!gui.EnemyMaxHp.ContainsKey(hm))
                {
                    gui.EnemyMaxHp.Add(hm, hm.hp);
                    gui.EnemyIsBoss.Add(hm, BossSceneController.Instance != null);
                }

                hm.SetGeoSmall(ls.IncreaseGeo(GetGeo("small", hm), Status.LuckStat));
                hm.SetGeoMedium(ls.IncreaseGeo(GetGeo("medium", hm), Status.LuckStat));
                hm.SetGeoLarge(ls.IncreaseGeo(GetGeo("large", hm), Status.LuckStat));
            }

            return isAlreadyDead;
        }

        // custom cursor display function to allow cursor use in levelling menu near benches
        private void ShowCursor()
        {
            if (HeroController.instance != null &&
                HeroController.instance.cState != null &&
                GameManager.instance != null)
            {
                Cursor.visible = PlayerData.instance.atBench || GameManager.instance.isPaused;
                return;
            }

            if (GameManager.instance != null)
                Cursor.visible = GameManager.instance.isPaused;
        }

        // handles damage blocking from resiliency stat
        private int ResShield(int hazardType, int damage)
        {
            if (Status.ResilienceStat > 1 && hazardType == 1)
            {
                if (HitsSinceShielded > 7)
                    HitsSinceShielded = 7;

                float num = Random.Range(1, 100);
                float iframes = ls.IFrames(Status.ResilienceStat);
                float multiplier = 0f;

                // apply weight multiplier based on how many hits taken since last blocked hit
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

        // get focus cost multiplier
        private float FocusCost() => (float)ls.FocusCost(Status.IntelligenceStat) / 33f;

        // get soul gain per nail hit based on current wisdom stat
        private int SoulGain(int num) => ls.ExtraSoul(Status.WisdomStat, num);

        // attack speed multiplier based on dexterity stat
        private void BenchApply(PlayerData pd, HeroController hc)
        {
            HeroController.instance.ATTACK_DURATION =
                0.35f / LevellingSystem.Instance.AttackSpeed(Status.DexterityStat);
            HeroController.instance.ATTACK_DURATION_CH =
                0.25f / LevellingSystem.Instance.AttackSpeed(Status.DexterityStat);
            HeroController.instance.ATTACK_COOLDOWN_TIME =
                0.41f / LevellingSystem.Instance.AttackSpeed(Status.DexterityStat);
            HeroController.instance.ATTACK_COOLDOWN_TIME_CH =
                0.25f / LevellingSystem.Instance.AttackSpeed(Status.DexterityStat);
        }

        // lifeblood scaling based on resiliency stat
        private int BlueHPRestored() => ls.ExtraMasks(Status.ResilienceStat);
        private void UpdateBlueHealth(On.PlayerData.orig_UpdateBlueHealth orig, PlayerData self)
        {
            orig(self);
            self.SetInt("healthBlue", self.GetInt("healthBlue") + BlueHPRestored());
        }

        // helper for geo value updates. Uses lookup via reflection
        private int GetGeo(string size, HealthManager enemy)
        {
            FieldInfo fi = enemy.GetType().GetField(size + "GeoDrops", BindingFlags.NonPublic | BindingFlags.Instance);
            object geo = fi.GetValue(enemy);
            return geo == null ? 0 : (int)geo;
        }

        // handles critical hit visual
        private void CritHit(Collider2D otherCollider, GameObject go)
        {
            if (Crit && otherCollider.gameObject.layer == 11)
            {
                HeroController.instance.shadowRingPrefab.transform.SetScaleX(0.5f);
                HeroController.instance.shadowRingPrefab.transform.SetScaleY(0.5f);

                Object.Instantiate(
                    HeroController.instance.shadowRingPrefab,
                    otherCollider.gameObject.transform.position,
                    go.transform.rotation
                );
            }
        }

        // checks if player has obtained the void heart
        private bool HasVoidHeart()
        {
            return PlayerData.instance != null && PlayerData.instance.GetBool("equippedCharm_36");
        }

        // called when the hero (player character) is updated each frame
        private void HeroUpdate()
        {
            // remove dead enemies from the HP-bar tracking dictionaries
            var gui = BonfireGUI.Instance;
            var toRemove = new List<HealthManager>();
            foreach (var e in gui.EnemyMaxHp.Keys)
            {
                if (e == null || e.hp <= 0)
                    toRemove.Add(e);
            }
            foreach (var e in toRemove)
            {
                gui.EnemyMaxHp.Remove(e);
                gui.EnemyIsBoss.Remove(e);
            }

            // crit chance roll
            if (GameManager.instance.inputHandler.inputActions.attack.WasPressed)
                critRoll = Random.Range(1, 100);

            if (HeroController.instance == null || PlayerData.instance == null)
                return;

            // passive soul regen from wisdom stat and optionally from void heart
            try
            {
                float dt = Time.deltaTime;

                if (manaRegenTime > 0)
                {
                    manaRegenTime -= dt;
                }
                else
                {
                    LogDebug("Recovering MP!");
                    manaRegenTime = 1.11f;
                    HeroController.instance.AddMPChargeSpa(ls.SoulRegen(Status.WisdomStat));
                }

                if (Status.VoidHeartSoulRegenEnabled && HasVoidHeart())
                {
                    float regenPerSecond = 1.5f * Status.VoidHeartSoulRegenMultiplier;
                    Status.VoidHeartSoulBuffer += regenPerSecond * dt;

                    int toGive = Mathf.FloorToInt(Status.VoidHeartSoulBuffer);
                    if (toGive > 0)
                    {
                        HeroController.instance.AddMPChargeSpa(toGive);
                        Status.VoidHeartSoulBuffer -= toGive;
                    }
                }
            }
            catch { }
        }

        // custom damage function
        private HitInstance SetDamages(Fsm owner, HitInstance hit)
        {
            bool isSpell;

            switch (hit.Source.name)
            {
                case "Fireball":        // Vengeful Spirit / Shade Soul
                case "Fireball(Clone)": // not sure what this refers to exactly (maybe this is actually Shade Soul?)
                case "Q Fall Damage":   // Desolate Dive / Descending Dark, direct hit
                case "Hit L":           // dive, left side
                case "Hit R":           // dive, right side
                case "Hit U":           // Howling Wraiths / Abyss Shriek
                    isSpell = true;
                    break;
                default:
                    isSpell = false;
                    break;
            }

            // spell damage scaling based on intelligence stat
            if (isSpell)
            {
                LogDebug($"[Vanilla] Spell: {hit.Source.name}. Damage: {hit.DamageDealt}");

                hit.DamageDealt = ls.SpellDamage(hit.DamageDealt, Status.IntelligenceStat);

                LogDebug($"[Bonfire] Spell: {hit.Source.name}. Damage: {hit.DamageDealt}");
            }

            // nail arts scaling based on strength stat
            if (hit.Source.name.Contains("lash"))
            {
                LogDebug($"[Vanilla] Damage for {hit.Source.name} = {hit.DamageDealt}");

                hit.DamageDealt = ls.NailDamage(Status.StrengthStat);

                LogDebug($"[Bonfire] Damage for {hit.Source.name} = {hit.DamageDealt}");
                LogDebug($"Crit chance: {ls.CritChance(Status.LuckStat)}. Rolled {critRoll}.");

                Crit = critRoll <= ls.CritChance(Status.LuckStat);
                if (Crit)
                {
                    hit.DamageDealt = ls.CritDamage(Status.DexterityStat, hit.DamageDealt);
                    LogDebug($"[Crit] Damage for {hit.Source.name} = {hit.DamageDealt}");

                    HeroController.instance.GetComponent<SpriteFlash>().FlashGrimmflame();
                    HeroController.instance.carefreeShield.SetActive(true);
                }
            }

            return hit;
        }
    }
}
