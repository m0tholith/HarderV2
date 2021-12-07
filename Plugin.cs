using BepInEx;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HarderV2
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, "HarderV2", "1.0.0")]
    [BepInProcess("ULTRAKILL.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private ConfigEntry<string> newV2Name;
        private ConfigEntry<float> v2HealthMultiplier;
        private ConfigEntry<float> v2DamageMultiplier;
        private bool guiEnabled = true;
        private bool assignedV2Values;

        private void Awake()
        {
            // Testing the Console.
            Logger.LogInfo($"If you see __instance then you're on the right track.");

            // Testing configuration values.
            newV2Name = Config.Bind("V2", // The section under which the option is shown
            "Name", // The key of the configuration option in the configuration file (What it's shown as)
            "V2.05", // The default value
            "What V2's new name will be"); // The description of the key

            v2HealthMultiplier = Config.Bind("V2", "Health_Multiplier", 2f, "What V2's health will be multiplied by.");
            v2DamageMultiplier = Config.Bind("V2", "Damage_Multiplier", 2f, "What V2's damage will be multiplied by.");
        }

        private void Start()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void Update()
        {
            if (SceneManager.GetActiveScene().name == "Level 4-4" && !assignedV2Values)
            {
                assignedV2Values = true;
                AssignV2Values();
            }
        }

        private void OnSceneChanged(Scene from, Scene to)
        {
            if (SceneManager.GetActiveScene().name == "Level 4-4")
            {
                foreach (CheckPoint c in Resources.FindObjectsOfTypeAll<CheckPoint>())
                {
                    c.onRestart.AddListener(AssignV2Values);
                }
            }
        }

        private void AssignV2Values()
        {
            if (v2HealthMultiplier.Value < 1 || v2DamageMultiplier.Value < 1)
            {
                MonoSingleton<AssistController>.Instance.cheatsEnabled = true;
                MonoSingleton<CheatsController>.Instance.cheatsEnabled = true;
            }

            V2 v2 = Resources.FindObjectsOfTypeAll<V2>()[0];
            Machine mac = v2.GetComponent<Machine>();
            EnemyIdentifier eid = v2.GetComponent<EnemyIdentifier>();
            BossHealthBar bhb = v2.GetComponent<BossHealthBar>();

            v2.knockOutHealth =  30 * v2HealthMultiplier.Value;
            eid.totalDamageMultiplier = v2DamageMultiplier.Value;
            mac.health = 80 * v2HealthMultiplier.Value;
            Debug.Log($"Boss health bar layers: {bhb.healthLayers.Length}");
            bhb.bossName = newV2Name.Value;
            bhb.healthLayers[0].health = 30 * v2HealthMultiplier.Value;
            bhb.healthLayers[1].health = 50 * v2HealthMultiplier.Value;

            Debug.Log($"Applied V2 values!\neid.health:{eid.health}\neid.totalDamageMultiplier:{eid.totalDamageMultiplier}\nv2.knockoutHealth:{v2.knockOutHealth}\nmac.health:{mac.health}\nbhb.bossName:{bhb.bossName}\nbhb.healthLayers[0].health:{bhb.healthLayers[0].health}\nbhb.healthLayers[1].health:{bhb.healthLayers[1].health}");
            return;
        }

        private void OnGUI()
        {
            if (SceneManager.GetActiveScene().name == "Main Menu" || SceneManager.GetActiveScene().name == "Level 4-4")
            {
                if (GUI.Button(new Rect(Screen.currentResolution.width - 123, 3, 120, 40), guiEnabled? "Hide V2 Settings" : "Show V2 Settings"))
                {
                    guiEnabled = !guiEnabled;
                }
                if (guiEnabled)
                {
                    GUI.skin.label.fontSize = 50;
                    GUI.Label(new Rect(0, -7, 400, 80), "V2 Settings");

                    GUI.skin.label.fontSize = 35;
                    GUI.skin.textField.fontSize = 25;
                    GUI.Label(new Rect(0, 45, 200, 80), "Boss Label:");
                    newV2Name.Value = GUI.TextField(new Rect(0, 82, 300, 30), newV2Name.Value);

                    GUI.Label(new Rect(0, 110, 400, 80), "Health:");
                    if (float.TryParse(GUI.TextField(new Rect(0, 147, 60, 30), $"{v2HealthMultiplier.Value * 80}"), out float x))
                    {
                        v2HealthMultiplier.Value = x / 80;
                    }

                    GUI.Label(new Rect(0, 175, 400, 80), "Damage Multiplier:");
                    if (float.TryParse(GUI.TextField(new Rect(0, 212, 60, 30), $"{v2DamageMultiplier.Value.ToString("0.0##")}"), out float y))
                    {
                        v2DamageMultiplier.Value = y;
                    }

                    if (SceneManager.GetActiveScene().name == "Level 4-4")
                    {
                        GUI.skin.label.fontSize = 25;
                        GUI.Label(new Rect(0, 252, 400, 80), "Remember to restart the level to apply the changes!");
                    }
                }
            }
        }
    }
}