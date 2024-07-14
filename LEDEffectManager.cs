using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using ChromaSDK;
using UnityEngine.SceneManagement;
using GameNetcodeStuff;
using UnityEngine.InputSystem.Controls;

namespace CompanyKeyLights
{
    public class LEDEffectManager : MonoBehaviour
    {
        private float time;
        private float redFade = 0f;
        private Color bgColor;
        private bool hasDamageListener;

        private ShipLights shipLights;

        private int[] lastKeyboardColors;

        void Awake()
        {
            if (!UniColor.Initialized)
            {
                Destroy(gameObject);
            }
        }

        void Update()
        {
            // Logic process
            time += Time.deltaTime;
            while (time > Mathf.PI * 4.0f)
                time -= Mathf.PI * 4.0f;
            bgColor = Color.Lerp(bgColor, GetBackgroundColor(), Time.deltaTime);

            string name = SceneManager.GetActiveScene().name;
            if (name == "InitScene" || name == "InitSceneLANMode" || name == "MainMenu")
                redFade = 0.6f + Mathf.Abs(Mathf.Sin(time)) * 0.4f;
            else
                redFade -= Time.deltaTime;

            redFade = Mathf.Clamp01(redFade);
            // Add damage hook
            if (StartOfRound.Instance != null && !hasDamageListener)
            {
                StartOfRound.Instance.LocalPlayerDamagedEvent.AddListener(new UnityAction(LocalPlayerDamaged));
                hasDamageListener = true;
            }
            if (StartOfRound.Instance == null && hasDamageListener)
            {
                hasDamageListener = false;
            }

            // Razer update
            if (UniColor.RazerInitialized)
            {
                // Keyboard
                int[] keyboardColors = new int[ChromaAnimationAPI.GetMaxRow(ChromaAnimationAPI.Device2D.Keyboard) * ChromaAnimationAPI.GetMaxColumn(ChromaAnimationAPI.Device2D.Keyboard)];

                keyboardColors = UniColor.RazerSetStaticColor(keyboardColors, GetShipLightsColor());
                keyboardColors = UniColor.RazerSetStaticColor(keyboardColors, bgColor);
                // keyboardColors = UniColor.RazerSetKeyColor(keyboardColors, Keyboard.RZKEY.RZKEY_W, new Color(1f, 0.54f, 0f));
                // keyboardColors = KeyboardPlayerActions(keyboardColors);
                keyboardColors = UniColor.RazerSetStaticColor(keyboardColors, GetFearColor());
                keyboardColors = UniColor.RazerSetStaticColor(keyboardColors, new Color(1f, 0f, 0f, redFade));

                ChromaAnimationAPI.SetEffectCustom2D((int)ChromaAnimationAPI.Device2D.Keyboard, keyboardColors);
            }
        }

        void LocalPlayerDamaged()
        {
            redFade = 0.75f;
        }

        /*
        int[] KeyboardPlayerActions(int[] colors)
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null) return colors;
            PlayerControllerB p = GameNetworkManager.Instance.localPlayerController;

            foreach (InputBinding binding in p.playerActions.Movement.Sprint.bindings)
            {
                Plugin.Logger.LogInfo(InputSystem.FindControl(binding.overridePath));
                KeyControl keyControl = InputSystem.FindControl(binding.path) as KeyControl;
                if (keyControl != null && keyControl.device is Keyboard)
                    UniColor.RazerSetKeyColor(colors, keyControl.keyCode, new Color(1f, 0.54f, 0f));
            }

            return colors;
        }
        */

        Color GetShipLightsColor()
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null || GameNetworkManager.Instance.localPlayerController.isInsideFactory) return new Color(0f, 0f, 0f, 0f);
            shipLights = FindObjectOfType<ShipLights>();
            if (shipLights == null || shipLights.areLightsOn)
                return new Color(1f, 1f, 1f, 1f);
             else
                return new Color(0f, 0f, 0f, 0f);
        }

        Color GetBackgroundColor()
        {
            if (GameNetworkManager.Instance == null || GameNetworkManager.Instance.localPlayerController == null) return new Color(0f, 0f, 0f, 0f);

            if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
            {
                return new Color(0f, 0f, 0f, 0f);
            }
            if (!GameNetworkManager.Instance.localPlayerController.isInsideFactory && TimeOfDay.Instance != null)
            {
                Gradient timeGradient = new Gradient();

                GradientColorKey[] colors = new GradientColorKey[4];
                colors[0] = new GradientColorKey(new Color(247f / 255f, 170f / 255f, 111f / 255f), 0f);
                colors[1] = new GradientColorKey(new Color(255f/255f, 255f/255f, 112f/255f), 1f/3f);
                colors[2] = new GradientColorKey(new Color(19f / 255f, 24f / 255f, 98f / 255f), 2f/3f);
                colors[3] = new GradientColorKey(new Color(0f, 0f, 0f), 1f);

                GradientAlphaKey[] alphas = new GradientAlphaKey[4];
                alphas[0] = new GradientAlphaKey(1f, 0f);
                alphas[1] = new GradientAlphaKey(1f, 1f/3f);
                alphas[2] = new GradientAlphaKey(0.75f, 2f/3f);
                alphas[3] = new GradientAlphaKey(0f, 1f);

                timeGradient.SetKeys(colors, alphas);

                return timeGradient.Evaluate(TimeOfDay.Instance.currentDayTime / TimeOfDay.Instance.totalTime);
            }
            if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
            {
                return new Color(0f, 0f, 0f, 1f);
            }

            return new Color(0f, 1f, 0f, 1f);
        }

        Color GetFearColor()
        {
            if (StartOfRound.Instance != null)
            {
                return new Color(0f, 0f, 1f, StartOfRound.Instance.fearLevel-0.1f);
            }
            return new Color(0f, 0f, 1f, 0f);
        }
    }
}
