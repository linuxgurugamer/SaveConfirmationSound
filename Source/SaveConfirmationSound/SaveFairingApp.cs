using System;
using System.Diagnostics;
using UnityEngine;

namespace SaveConfirmationSound
{
    [KSPAddon(KSPAddon.Startup.MainMenu, true)]
    public class SaveConfirmationSound : MonoBehaviour
    {
        public string settingsURL = "GameData/SaveConfirmationSound/settings.cfg";
        public bool DEBUG = false;
        public DateTime LastSaveTime;
        bool launcherButtonNeedsInitializing = true;
        int MinimumTimeBetweenSounds = 100;
        string SoundURL = "SaveConfirmationSound/SoundFile";
        Stopwatch updateStopwatch;

        void LoadSettings(string sSettingURL)
        {
            try
            {
                ConfigNode settings = ConfigNode.Load(KSPUtil.ApplicationRootPath + sSettingURL);

                foreach (ConfigNode node in settings.GetNodes("SaveConfirmationSoundSettings"))
                {
                    try
                    {
                        DEBUG = bool.Parse(node.GetValue("Debug"));
                        SoundURL = node.GetValue("SoundLocation");
                        MinimumTimeBetweenSounds = int.Parse(node.GetValue("MinimumTimeBetweenSounds"));
                    }
                    catch (Exception)
                    {
                        Log("Error loading from config (field)", true);
                        throw;
                    }
                }
            }
            catch (Exception)
            {
                Log("Error loading from config (file)", true);
                throw;
            }
        }

        void LateUpdate()
        {
            if (launcherButtonNeedsInitializing)
            {
                Log("LateUpdate()", false);
                Audio.initializeAudio();

                GameEvents.onGameStateSaved.Add(OnSave);

                launcherButtonNeedsInitializing = false;
                updateStopwatch = new Stopwatch();
                updateStopwatch.Start();

                LoadSettings(settingsURL);
            }
        }

        private void OnSave(Game data)
        {
            Log("onGameStateSaved", false);
            Log(updateStopwatch.ElapsedMilliseconds + "ms", false);

            if ((float)updateStopwatch.ElapsedMilliseconds <= MinimumTimeBetweenSounds)
            {
                Log("below timer threshold", false);
                return;
            }
            else
            {
                updateStopwatch.Reset();
                updateStopwatch.Start();
                if (Event.current.Equals(Event.KeyboardEvent(KeyCode.F5.ToString())))
                {
                    Log("And Quick Saving", false);

                    if (!MapView.MapIsEnabled)
                    {
                        Log("In flight", false);
                    }
                    else
                    {
                        Log("In map", false);
                        Audio.markerAudio.transform.SetParent(MapView.MapCamera.transform);
                    }
                    Log("Playing Sound " + SoundURL, false);
                    Audio.markerAudio.PlayOneShot(GameDatabase.Instance.GetAudioClip(SoundURL));
                    updateStopwatch.Reset();
                    updateStopwatch.Start();
                }
            }
        }

        public void Log(String message, bool warning)
        {
            if (DEBUG && !warning)
            {
                UnityEngine.Debug.Log("[SaveConfirmationSound] " + message);
            }
            else UnityEngine.Debug.LogWarning("[SaveConfirmationSound] " + message);
        }
    }

    public class Audio
    {
        private static Audio instance;
        private Audio() { }
        public static Audio Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Audio();
                }
                return instance;
            }
        }

        public static bool isLoaded = false;
        public static GameObject audioplayer;
        public static AudioSource markerAudio;

        public static void initializeAudio()
        {
            audioplayer = new GameObject();
            markerAudio = new AudioSource();
            try
            {
                markerAudio = audioplayer.AddComponent<AudioSource>();
                markerAudio.volume = GameSettings.UI_VOLUME;
                markerAudio.panStereo = 0;
                markerAudio.dopplerLevel = 0;
                markerAudio.bypassEffects = true;
                markerAudio.loop = true;
                markerAudio.rolloffMode = AudioRolloffMode.Linear;
                Audio.markerAudio.transform.SetParent(FlightCamera.fetch.mainCamera.transform);
            }
            catch (Exception)
            {
                throw;
            }
            isLoaded = true;
        }
    }
}
