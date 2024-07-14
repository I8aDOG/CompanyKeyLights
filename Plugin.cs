using BepInEx;
using BepInEx.Logging;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CompanyKeyLights;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
    private static GameObject effectManager;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        UniColor.Init();
        EffectManager.AddDefaultEffects();

        SceneManager.sceneLoaded += OnSceneLoaded;

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Create effect manager on init scene, has natural delay so its the perfect buffer between initialization and API calls.
        if (scene.name == "InitScene" || scene.name == "InitSceneLANMode")
        {
            if (effectManager != null) return;

            Logger.LogInfo("Create Effect Manager");
            effectManager = new GameObject();
            effectManager.name = "LEDEffectManager";
            effectManager.AddComponent<LEDEffectManager>();
            DontDestroyOnLoad(effectManager);
        }
    }
}
