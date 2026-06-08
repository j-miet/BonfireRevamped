using System.Collections.Generic;
using System.Reflection;

using HutongGames.PlayMaker;
using Modding;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bonfire
{
    public class BonfireMod : Mod, ILocalSettings<PlayerStatus>
    {
        public PlayerStatus Status = new PlayerStatus();
        public void OnLoadLocal(PlayerStatus s) => Status = s;
        public PlayerStatus OnSaveLocal() => Status;

        public override string GetVersion() => "4.0.1"; // keep incrementing the original mod version
        public int HitsSinceShielded { get; set; } = 0;
        public bool Crit { get; set; } = false;

        public static BonfireMod Instance;
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

        // Ui methods are currently tied to LevellingSystem.OnGUI call and should be only called there

        /// <summary>
        /// Create UI component for void heart soul regen controls
        /// </summary>
        public void CreateVoidHeartSettingsUI()
        {
            GUILayout.BeginVertical("box");
            GUILayout.Label("Void Heart Soul Regen");

            GUI.backgroundColor = Color.white;
            Status.VoidHeartSoulRegenEnabled = GUILayout.Toggle(Status.VoidHeartSoulRegenEnabled, "Enabled");

            GUILayout.BeginHorizontal();

            GUILayout.Label("Multiplier: " + Status.VoidHeartSoulRegenMultiplier.ToString("0.0"));

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("-"))
                Status.VoidHeartSoulRegenMultiplier = Mathf.Max(0f, Status.VoidHeartSoulRegenMultiplier - 0.1f);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("+"))
                Status.VoidHeartSoulRegenMultiplier += 0.1f;

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Create UI component toggle button to enable/disable enemy hp bars
        /// </summary>
        public void CreateEnemyHPToggleUI()
        {
            GUILayout.BeginVertical("box");

            GUI.backgroundColor = Color.white;
            Instance.Status.EnemyHealthBarsEnabled =
                GUILayout.Toggle(
                    Instance.Status.EnemyHealthBarsEnabled,
                    "Enemy Health Bars"
                );

            GUILayout.EndVertical();
        }

        /// <summary>
        /// Create health bars from each stored enemy HealthManager object
        /// </summary>
        public void DrawEnemyHealthBars()
        {
            if (!Status.EnemyHealthBarsEnabled) return;

            foreach (var kvp in enemyMaxHp)
            {
                HealthManager hm = kvp.Key;
                if (hm == null) continue;

                Vector3 screenPos = Camera.main.WorldToScreenPoint(hm.transform.position);
                if (screenPos.z < 0) continue;

                float maxHp = kvp.Value;
                float currentHp = hm.hp;
                bool isBoss = enemyIsBoss.TryGetValue(hm, out bool b) && b;

                if (currentHp == maxHp && !isBoss) continue; // if full hp, don't display bar for default enemies

                float pct = Mathf.Clamp01(currentHp / maxHp);

                float width = isBoss ? 300f : 120f;
                float height = isBoss ? 20f : 10f;

                float x = screenPos.x - width / 2f;
                float y = Screen.height - screenPos.y - 20f;

                Rect bg = new Rect(x, y, width, height);
                Rect fill = new Rect(x, y, width * pct, height);

                DrawRect(bg, new Color(0f, 0f, 0f, 0.7f));

                Color fillColor = isBoss ? new Color(1f, 0.5f, 0f) : Color.red;
                // for progressive color changes based on enemy hp e.g. green -> yellow -> red
                /*
                Color fillColor =
                    pct > 0.6f ? Color.green :
                    pct > 0.3f ? Color.yellow :
                                 Color.red;
                */

                DrawRect(fill, fillColor);

                GUI.color = Color.white;
            }
        }


        private int Dreamers; // can remove this + SceneLoaded unless some other dreamer-reliant system get added
        private int critRoll;
        private float manaRegenTime;
        private LevellingSystem ls;
        private static Texture2D _tex;

        // for enemy hp bars
        private readonly Dictionary<HealthManager, float> enemyMaxHp = new Dictionary<HealthManager, float>();
        private readonly Dictionary<HealthManager, bool> enemyIsBoss = new Dictionary<HealthManager, bool>();

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

        // add health bars for enemies
        private bool OnEnableEnemy(GameObject enemy, bool isAlreadyDead)
        {
            HealthManager hm = enemy.GetComponent<HealthManager>();


            if (hm != null)
            {
                if (!enemyMaxHp.ContainsKey(hm))
                {
                    enemyMaxHp.Add(hm, hm.hp);
                    enemyIsBoss.Add(hm, BossSceneController.Instance != null);
                }

                hm.SetGeoSmall(ls.IncreaseGeo(GetGeo("small", hm), Status.LuckStat));
                hm.SetGeoMedium(ls.IncreaseGeo(GetGeo("medium", hm), Status.LuckStat));
                hm.SetGeoLarge(ls.IncreaseGeo(GetGeo("large", hm), Status.LuckStat));
            }

            return isAlreadyDead;
        }

        // custom cursor display function to allow cursor use in leveling menu near benches
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

        // inject levelling system into game
        private void SetupGameRefs(int id) => SetupGameRefs();
        private void SetupGameRefs()
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

        // applies dreamer kill status
        // UNUSED old system would scale hp based on dreamers killed, but that's disabled for now
        private void SceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
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

        // handles damage blocking from resiliency stat.
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

        // Get focus cost multiplier
        private float FocusCost() => (float)ls.FocusCost(Status.IntelligenceStat) / 33f;

        // Get soul gain per nail hit based on current wisdow stat
        private int SoulGain(int num) => ls.ExtraSoul(Status.WisdomStat, num);

        // Attack speed multiplier based on dexterity stat
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

        // helper for geo value updates
        private int GetGeo(string size, HealthManager enemy)
        {
            FieldInfo fi = enemy.GetType().GetField(size + "GeoDrops", BindingFlags.NonPublic | BindingFlags.Instance);
            object geo = fi.GetValue(enemy);
            int ret = geo == null ? 0 : (int)geo;
            return ret;
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

        // called when hero (= player character) gets updated
        private void HeroUpdate()
        {
            var toRemove = new List<HealthManager>();
            foreach (var e in enemyMaxHp.Keys) // actively track hp bars for removal
            {
                if (e == null || e.hp <= 0)
                    toRemove.Add(e);
            }

            foreach (var e in toRemove)
            {
                enemyMaxHp.Remove(e);
                enemyIsBoss.Remove(e);
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
                    LogDebug($@"Recovering MP!");
                    manaRegenTime = 1.11f;
                    HeroController.instance.AddMPChargeSpa(ls.SoulRegen(Status.WisdomStat));
                }

                if (Status.VoidHeartSoulRegenEnabled && HasVoidHeart())
                {
                    float baseRegen = 1.5f;
                    float regenPerSecond = baseRegen;

                    regenPerSecond *= Status.VoidHeartSoulRegenMultiplier;

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
                case "Fireball": // vengeful spirit / shade soul
                    isSpell = true;
                    break;
                case "Fireball(Clone)": // not sure what this refers to exactly
                    isSpell = true;
                    break;
                case "Q Fall Damage": // desolate dive / descending dark, direct hit damage
                    isSpell = true;
                    break;
                case "Hit L": // dive, left side
                    isSpell = true;
                    break;
                case "Hit R": // dive, right side
                    isSpell = true;
                    break;
                case "Hit U": // howling wraiths / abyss shriek
                    isSpell = true;
                    break;
                default:
                    isSpell = false;
                    break;
            }

            // add spell damage scaling based on intelligence stat
            // previously only wraith/shriek got scaling, but it's now applied to all spells
            if (isSpell)
            {
                LogDebug($"[Vanilla] Spell name: {hit.Source.name} - {hit.Source}. Damage: {hit.DamageDealt}");

                hit.DamageDealt = ls.SpellDamage(hit.DamageDealt, Status.IntelligenceStat);

                LogDebug($"[Bonfire] Spell name: {hit.Source.name} - {hit.Source}. Damage: {hit.DamageDealt}");
            }

            // nail arts scaling based on strength stat
            if (hit.Source.name.Contains("lash"))
            {
                LogDebug($@"[Vanilla] Damage for {hit.Source.name} = {hit.DamageDealt}");

                hit.DamageDealt = ls.NailDamage(Status.StrengthStat);

                LogDebug($@"[Bonfire] Damage for {hit.Source.name} = {hit.DamageDealt}");
                LogDebug($@"Crit chance: {ls.CritChance(Status.LuckStat)}. Rolled {critRoll}.");

                Crit = critRoll <= ls.CritChance(Status.LuckStat);
                if (Crit) // nail art crit
                {
                    hit.DamageDealt = ls.CritDamage(Status.DexterityStat, hit.DamageDealt);

                    LogDebug($@"[Crit] Damage for {hit.Source.name} = {hit.DamageDealt}");

                    HeroController.instance.GetComponent<SpriteFlash>().FlashGrimmflame();
                    HeroController.instance.carefreeShield.SetActive(true);
                }
            }

            return hit;
        }
    }
}
