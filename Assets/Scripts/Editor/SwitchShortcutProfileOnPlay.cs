using System;
using System.Linq;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace StlVault.Editor
{
    [InitializeOnLoad]
    public class SwitchShortcutsProfileOnPlay
    {
        private const string PlayingProfileId = "Playing"; //Make you don't already have a profile named like this
        private static string _previousProfileId;
        private static bool _switched;

        static SwitchShortcutsProfileOnPlay()
        {
            EditorApplication.playModeStateChanged += DetectPlayModeState;
        }

        private static void SetActiveProfile(string profileId)
        {
            Debug.Log($"Activating Shortcut profile \"{profileId}\"");
            ShortcutManager.instance.activeProfileId = profileId;
        }

        private static void DetectPlayModeState(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    OnEnteredPlayMode();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    OnExitingPlayMode();
                    break;
            }
        }

        private static void OnExitingPlayMode()
        {
            if (!_switched)  return;
            SetActiveProfile("Default");
            _switched = false;
        }

        private static void CreateEmptyProfile()
        {
            try
            {
                ShortcutManager.instance.CreateProfile(PlayingProfileId);
                ShortcutManager.instance.activeProfileId = PlayingProfileId;
                foreach (var pid in ShortcutManager.instance.GetAvailableShortcutIds())
                    ShortcutManager.instance.RebindShortcut(pid, ShortcutBinding.empty);
                ShortcutManager.instance.activeProfileId = ShortcutManager.defaultProfileId;
            }
            catch (Exception)
            {
                Debug.LogWarning("Couldn't create profile");
            }
        }

        private static void OnEnteredPlayMode()
        {
            _previousProfileId = ShortcutManager.instance.activeProfileId;
            var allProfiles = ShortcutManager.instance.GetAvailableProfileIds().ToList();

            if (!allProfiles.Contains(PlayingProfileId))
            {
                CreateEmptyProfile();
            }

            if (_previousProfileId.Equals(PlayingProfileId))
                return; // Same as active

            _switched = true;
            SetActiveProfile(PlayingProfileId);
        }
    }
}