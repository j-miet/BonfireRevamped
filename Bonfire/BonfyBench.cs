using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Bonfire
{
    public class BonfyBench : MonoBehaviour
    {
        private static Dictionary<string, Sprite> sprites;
        public SpriteRenderer bonfy;
        public float frameTime = 0.03f;
        private float animationTime;
        private int currentFrame;

        public static void Load()
        {
            sprites = new Dictionary<string, Sprite>();
            Assembly asm = Assembly.GetExecutingAssembly();
            foreach (string res in asm.GetManifestResourceNames())
            {
                if (!res.EndsWith(".png"))
                    continue;

                using (Stream stream = asm.GetManifestResourceStream(res))
                {
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);

                    var texture = new Texture2D(1, 1);
                    texture.LoadImage(buffer);

                    var name = res.Substring(18, res.Length - 22); // Substring is to cut off the Bonfire.Resources. and the .png
                    var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    sprites.Add(name, sprite);
                }
            }
        }

        public static void Replace(Scene scene)
        {
            var debris = new List<string>
            {
                "outskirts__0003_camp", // Bench background in Kingdom's Edge
                "outskirts__0004_camp", // Floating rock in Kingdom's Edge
                "guardian_bench", // Bench in Queen's Gardens
                "spider_bench_states_0000_2", // Beast's Den webbed bench
                "GG_bench_0001_3", // Bench shadow in Godhome
                "GG_bench_metal_0001_1" // Bench metal railing in Godhome
            };
            foreach (var name in debris)
            {
                var trash = GameObject.Find(name);
                if (trash) Destroy(trash);
            }

            // Replace hive bench blob
            var hiveBench = GameObject.Find("Hive Bench");
            if (hiveBench)
            {
                // Normal blob
                var idleBlob = hiveBench.transform.Find("Spr Idle");
                if (idleBlob && idleBlob.TryGetComponent<SpriteRenderer>(out var idle))
                {
                    idle.transform.localScale = 1.5f * Vector3.one; // No idea why this is necessary
                    idle.sprite = sprites["Hive.Idle"];
                }
                // Cracked blob
                var crackedBlob = hiveBench.transform.Find("Spr Cracked");
                if (crackedBlob && crackedBlob.TryGetComponent<SpriteRenderer>(out var cracked))
                {
                    cracked.transform.localScale = 1.5f * Vector3.one; // Nor this
                    cracked.sprite = sprites["Hive.Cracked"];
                }
                // Falling bench
                var fallingBench = hiveBench.transform.Find("Bench Fall");
                if (fallingBench && fallingBench.gameObject.scene == scene)
                    fallingBench.gameObject.AddComponent<BonfyBench>();
            }

            // Replace dummy Crystal Guardian bench
            var dummyBench = GameObject.Find("Dummy Bench");
            if (dummyBench && dummyBench.scene == scene)
                dummyBench.AddComponent<BonfyBench>();

            // Replace trap Beast's Den bench
            var spiderBench = GameObject.Find("RestBench Spider");
            if (spiderBench && spiderBench.scene == scene)
                spiderBench.AddComponent<BonfyBench>();

            var brettaBench = GameObject.Find("Bretta Bench");
            if (brettaBench && brettaBench.scene == scene)
            {
                // Move Bretta down
                brettaBench.transform.position += new Vector3(0, -0.6f, 0);
                // Remove Bretta's bench
                var bench = brettaBench.transform.Find("Bench");
                if (bench) Destroy(bench.gameObject);
            }

            // Replace standard benches
            foreach (var bench in FindObjectsOfType<RestBench>())
            {
                if (bench.gameObject.scene == scene)
                    bench.gameObject.AddComponent<BonfyBench>();
            }
        }

        private void Start()
        {
            // Removing bench
            if (TryGetComponent<SpriteRenderer>(out var renderer))
                Destroy(renderer);

            var debris = new List<string>
            {
                "Lit", // Bench outline
                "Detect Range", // Range detector for outline
                "Particle B", // Particles when passing by
                "Particle F", // Particles when passing by
                "Particle Rest", // Particles when resting
                "Webbed", // Beast's Den webbing bench
                "rest_bench", // Toll bench
            };
            foreach (var name in debris)
            {
                var child = transform.Find(name);
                if (child) Destroy(child.gameObject);
            }

            // Remove tilt in Waterways bench
            if (TryGetComponent<RestBenchTilt>(out var tilt))
                Destroy(tilt);
            transform.rotation = Quaternion.identity;

            // Reset z position
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);

            // Add bonfy as child
            bonfy = new GameObject("Bonfy").AddComponent<SpriteRenderer>();
            bonfy.transform.parent = transform;
            bonfy.transform.localPosition = new Vector3(0, 1.7f, -0.3f);
            bonfy.transform.localScale = 0.9f * Vector3.one;

            // Ancestral Mound bench should be closer to the camera
            if (name == "BoneBench")
                bonfy.transform.localPosition = new Vector3(0, 1.7f, -0.7f);

            // Leg Eater bench should be higher up
            if (gameObject.scene.name == "Fungus2_26")
                bonfy.transform.localPosition = new Vector3(0, 2.0f, -0.3f);

            // Archives bench should be farther back and down
            if (gameObject.scene.name == "Fungus3_archive")
                bonfy.transform.localPosition = new Vector3(0, 1.6f, -0.1f);

            // Same for Colosseum bench
            if (gameObject.scene.name == "Room_Colosseum_02")
                bonfy.transform.localPosition = new Vector3(0, 1.6f, -0.2f);

            // And Pantheon bench
            if (gameObject.scene.name == "GG_Spa")
                bonfy.transform.localPosition = new Vector3(0, 1.6f, -0.1f);

            // White Palace benches should be further down
            if (name == "WhiteBench" || gameObject.scene.name == "White_Palace_06")
                bonfy.transform.localPosition = new Vector3(0, 1.1f, -0.3f);

            // Toll benches...
            if (transform.parent?.name == "Toll Machine Bench")
            {
                // ...should be even further down,
                bonfy.transform.localPosition = new Vector3(0, 0.55f, -0.3f);
                // deprived of their base
                var front = transform.parent.Find("pay_bench_front");
                if (front) Destroy(front.gameObject);
                // and their bench animation
                var anim = transform.parent.Find("Bench Anim");
                if (anim) Destroy(anim.gameObject);
                // Add bonfire spawn animation
                if (transform.parent.TryGetComponent<tk2dSpriteAnimator>(out var animator))
                    animator.AnimationCompleted += Spawn;
            }

            // Normalizing sitting height
            if (TryGetComponent<PlayMakerFSM>(out var fsm))
            {
                foreach (var variable in fsm.FsmVariables.Vector3Variables)
                {
                    if (variable.Name == "Adjust Vector")
                        variable.Value = new Vector2(0, 0.4f);
                }
            }
        }

        private void Update()
        {
            animationTime += Time.deltaTime;

            if (animationTime >= frameTime)
            {
                bonfy.sprite = sprites[$"Bonfy.{currentFrame}"];
                currentFrame = (currentFrame + 1) % 30;

                animationTime = 0f;
            }
        }

        private void Spawn(tk2dSpriteAnimator _, tk2dSpriteAnimationClip __)
        {
            gameObject.SetActive(true);
            StartCoroutine(Spawn(0.5f));
        }

        private IEnumerator Spawn(float duration)
        {
            float spawnTime = 0f;

            while (spawnTime < duration)
            {
                bonfy.transform.localPosition = new Vector3(0, Mathf.Lerp(-1.7f, 0.55f, spawnTime / duration), -0.3f);
                bonfy.transform.localScale = Vector3.Lerp(Vector3.zero, 0.9f * Vector3.one, spawnTime / duration);
                spawnTime += Time.deltaTime;
                yield return null;
            }
        }
    }
}
