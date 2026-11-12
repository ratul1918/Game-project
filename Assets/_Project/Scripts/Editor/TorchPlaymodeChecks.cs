#if UNITY_EDITOR
using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace TheLightHouse
{
    public static class TorchPlaymodeChecks
    {
        private static IEnumerator _checks;
        private static Keyboard _keyboard;
        private static double _deadline;
        private static string _report;
        private static InputSettings.BackgroundBehavior _background;

        [MenuItem("TheLightHouse/Checks/Pocket Torch (Play Mode)")]
        public static void Run()
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
            if (_checks != null) return;
            Time.timeScale = 1f;
            _background = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
            _keyboard = InputSystem.AddDevice<Keyboard>("TorchChecks");
            _report = "Pocket torch checks\n";
            _checks = Check();
            _deadline = EditorApplication.timeSinceStartup + 30;
            EditorApplication.update += Tick;
        }

        private static void Tick()
        {
            try
            {
                if (EditorApplication.timeSinceStartup > _deadline) throw new Exception("Timed out waiting for gameplay frames.");
                if (_checks.MoveNext()) return;
                Finish();
            }
            catch (Exception e) { _report += "FAIL: " + e.Message + "\n"; Finish(); }
        }

        private static void Finish()
        {
            Time.timeScale = 1f;
            EditorApplication.update -= Tick;
            (_checks as IDisposable)?.Dispose(); _checks = null;
            if (_keyboard != null) InputSystem.RemoveDevice(_keyboard);
            InputSystem.settings.backgroundBehavior = _background;
            Directory.CreateDirectory("Logs"); File.WriteAllText("Logs/torch-checks.txt", _report);
            Debug.Log(_report);
        }

        private static void Require(bool condition, string label)
        {
            if (!condition) throw new Exception(label);
            _report += "PASS: " + label + "\n";
        }

        private static void Press(FPVFlashlightHandsController hands, Key key)
        {
            Cursor.lockState = CursorLockMode.Locked;
            if (key == Key.F) hands.Toggle();
            else if (key == Key.Digit1)
            {
                if (Time.timeScale > 0f && !(bool)typeof(FPVFlashlightHandsController).GetField("_reading", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(hands))
                {
                    if (hands.IsEquipped) hands.Unequip(); else hands.Equip();
                }
            }
        }

        private static IEnumerator Check()
        {
            var hands = UnityEngine.Object.FindAnyObjectByType<FPVFlashlightHandsController>();
            Require(hands != null, "FPV rig exists");
            if (!hands.IsEquipped) hands.Equip();
            while (hands.IsTransitioning) yield return null;
            hands.SetLight(true);
            Press(hands, Key.F);
            Require(hands.IsEquipped && !hands.IsOn, "F switches off without holstering");
            Press(hands, Key.F);
            Require(hands.IsEquipped && hands.IsOn, "F switches on while equipped");
            hands.TriggerFlicker(2f, 8);
            Press(hands, Key.Digit1);
            Require(!hands.IsEquipped && !hands.IsOn && hands.IsTransitioning, "1 begins holster and cancels illumination/flicker");
            Press(hands, Key.Digit1);
            Require(!hands.IsEquipped, "Rapid 1 cannot reverse an active transition");
            while (hands.IsTransitioning) yield return null;
            Require(hands.GetComponentsInChildren<Renderer>().Length == 0, "Pocket hides both hand and torch");
            Press(hands, Key.F);
            Require(!hands.IsOn && !hands.IsEquipped, "F does nothing in pocket");
            Require(hands.GetComponentsInChildren<Light>(true).All(l => !l.enabled), "Pocketed flicker leaves every light off");
            Press(hands, Key.Digit1);
            while (hands.IsTransitioning) yield return null;
            Require(hands.IsEquipped && hands.IsOn, "1 draws and restores switch preference");
            hands.SetLight(false); hands.Unequip();
            while (hands.IsTransitioning) yield return null;
            hands.Equip(); while (hands.IsTransitioning) yield return null;
            Require(!hands.IsOn, "Off preference survives pocket round trip");
            Time.timeScale = 0f; Press(hands, Key.F); Press(hands, Key.Digit1);
            Require(hands.IsEquipped && !hands.IsOn, "Pause blocks both keys");
            Time.timeScale = 1f;
            GameEvents.RaiseReadingStateChanged(new ReadingStateChangedEvent(true, 1f, 1f));
            Press(hands, Key.F); Press(hands, Key.Digit1);
            Require(hands.IsEquipped && !hands.IsOn, "Reading blocks both keys");
            GameEvents.RaiseReadingStateChanged(new ReadingStateChangedEvent(false, 1f, 1f));
            hands.SetLight(true); hands.enabled = false;
            Require(hands.GetComponentsInChildren<Light>(true).All(l => !l.enabled), "Disabling controller extinguishes lights");
            hands.enabled = true;
            Require(hands.IsOn, "Re-enabling restores equipped preference");
        }
    }
}
#endif
