using ChromaSDK;
using UnityEngine;
using UnityEngine.InputSystem;
using BepInEx;

namespace CompanyKeyLights
{
    // This class unifies RGB color apis into one interface.
    public class UniColor
    {
        private static bool initialized;
        public static bool Initialized {  get { return initialized; } }

        private static int razerResult;

        private static bool razerInitialized;
        public static bool RazerInitialized { get { return razerInitialized; } }

        public static void Init()
        {
            // Initialize Razer Chroma

            ChromaSDK.APPINFOTYPE appInfo = new APPINFOTYPE();
            appInfo.Title = "Lethal Company";
            appInfo.Description = "A co-op horror about scavenging at abandoned moons to sell scrap to the Company.";
            appInfo.Author_Name = "Zeekerss";
            appInfo.Author_Contact = "https://twitter.com/ZeekerssRBLX";

            // Keyboards, Mice, Headsets, Mousepads, Keypads, ChromaLink supported devices
            appInfo.SupportedDevice = (0x01 | 0x02 | 0x04 | 0x08 | 0x10 | 0x20);
            appInfo.Category = 2;

            razerResult = ChromaAnimationAPI.InitSDK(ref appInfo);
            switch (razerResult)
            {
                case RazerErrors.RZRESULT_DLL_NOT_FOUND:
                    Plugin.Logger.LogError(string.Format("Chroma DLL is not found! {0}", RazerErrors.GetResultString(razerResult)));
                    break;
                case RazerErrors.RZRESULT_DLL_INVALID_SIGNATURE:
                    Plugin.Logger.LogError(string.Format("Chroma DLL has an invalid signature! {0}", RazerErrors.GetResultString(razerResult)));
                    break;
                case RazerErrors.RZRESULT_SUCCESS:
                    Plugin.Logger.LogInfo("Successfully initialized Chroma.");
                    razerInitialized = true;
                    initialized = true;
                    break;
                default:
                    Plugin.Logger.LogError(string.Format("Failed to initialize Chroma! {0}", RazerErrors.GetResultString(razerResult)));
                    break;
            }
        }

        public static void Uninit()
        {
            if (razerInitialized)
            {
                ChromaAnimationAPI.StopAll();
                ChromaAnimationAPI.CloseAll();
                int result = ChromaAnimationAPI.Uninit();
                ChromaAnimationAPI.UninitAPI();

                if (result != RazerErrors.RZRESULT_SUCCESS)
                {
                    Plugin.Logger.LogError("Failed to uninitialize Chroma.");
                }
                else
                {
                    razerInitialized = false;
                }
            }

            initialized = false;
        }

        #region "Unified Functions"
        public static void KeyboardApplyStaticColor(Color color)
        {
            
        }
        #endregion

        #region "Razer Specific"
        public static int[] RazerSetKeyColor(int[] colors, Key key, Color keyColor)
        {
            return RazerSetKeyColor(colors, (RazerKeyboard.RZKEY)ChromaAnimationAPI.GetKeyboardRazerKey(key), keyColor);
        }

        public static int[] RazerSetKeyColor(int[] colors, RazerKeyboard.RZKEY key, Color keyColor)
        {
            int row = ((int)key >> 8) & 0xff; //high bit
            int column = (int)key & 0xff; // low bit

            int columns = ChromaAnimationAPI.GetMaxColumn(ChromaAnimationAPI.Device2D.Keyboard);
            colors[(row * columns) + column] = ColorToChromaInt(Color.Lerp(ChromaIntToColor(colors[(row * columns) + column]), keyColor, keyColor.a));
            return colors;
        }

        public static int[] RazerSetStaticColor(int[] colors, Color color)
        {
            for (int i = 0; i < colors.Length; i++)
                colors[i] = ColorToChromaInt(Color.Lerp(ChromaIntToColor(colors[i]), color, color.a));
                // colors[i] = ColorToChromaInt(color);

            return colors;
        }

        public static int[] RazerDeviceStaticColor(int[] colors, ChromaAnimationAPI.Device1D device, Color color)
        {
            if (!razerInitialized) return new int[0];

            colors = new int[ChromaAnimationAPI.GetMaxLeds(device)];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = ColorToChromaInt(color);

            return colors;
        }

        public static int[] RazerDeviceStaticColor(int[] colors, ChromaAnimationAPI.Device2D device, Color color)
        {
            if (!razerInitialized) return new int[0];

            colors = new int[ChromaAnimationAPI.GetMaxRow(device) * ChromaAnimationAPI.GetMaxColumn(device)];
            for (int i = 0; i < colors.Length; i++)
                colors[i] = ColorToChromaInt(color);

            return colors;
        }

        public static int ColorToChromaInt(Color color)
        {
            return ChromaAnimationAPI.GetRGB(Mathf.FloorToInt(color.r * color.a * 255f), Mathf.FloorToInt(color.g * color.a * 255f), Mathf.FloorToInt(color.b * color.a * 255f));
        }

        public static Color ChromaIntToColor(int color)
        {
            return new Color((color & 0xFF) / 255f, ((color >> 8) & 0xFF) / 255f, ((color >> 16) & 0xFF) / 255f);
        }
        #endregion
    }
}
