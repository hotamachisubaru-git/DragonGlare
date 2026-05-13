using UnityEngine;
using System.Collections.Generic;

namespace DragonGlare
{
    public class InputManager : MonoBehaviour
    {
        private readonly HashSet<KeyCode> heldKeys = new();
        private readonly HashSet<KeyCode> pressedKeys = new();

        public IReadOnlySet<KeyCode> HeldKeys => heldKeys;
        public IReadOnlySet<KeyCode> PressedKeys => pressedKeys;

        public void PollInput()
        {
            pressedKeys.Clear();
            var nextHeld = new HashSet<KeyCode>();

            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (UnityEngine.Input.GetKey(key))
                {
                    nextHeld.Add(key);
                    if (!heldKeys.Contains(key))
                    {
                        pressedKeys.Add(key);
                    }
                }
            }

            heldKeys.Clear();
            foreach (var key in nextHeld)
            {
                heldKeys.Add(key);
            }

            PollGamepad();
        }

        private void PollGamepad()
        {
            float ly = UnityEngine.Input.GetAxis("Vertical");
            float lx = UnityEngine.Input.GetAxis("Horizontal");

            MapGamepadToKey(ly > GameConstants.GamepadThumbStickThreshold || UnityEngine.Input.GetKey(KeyCode.UpArrow), KeyCode.UpArrow);
            MapGamepadToKey(ly < -GameConstants.GamepadThumbStickThreshold || UnityEngine.Input.GetKey(KeyCode.DownArrow), KeyCode.DownArrow);
            MapGamepadToKey(lx < -GameConstants.GamepadThumbStickThreshold || UnityEngine.Input.GetKey(KeyCode.LeftArrow), KeyCode.LeftArrow);
            MapGamepadToKey(lx > GameConstants.GamepadThumbStickThreshold || UnityEngine.Input.GetKey(KeyCode.RightArrow), KeyCode.RightArrow);

            MapGamepadToKey(UnityEngine.Input.GetButtonDown("Submit") || UnityEngine.Input.GetKeyDown(KeyCode.Z), KeyCode.Return);
            MapGamepadToKey(UnityEngine.Input.GetButtonDown("Cancel") || UnityEngine.Input.GetKeyDown(KeyCode.X), KeyCode.Escape);
        }

        private void MapGamepadToKey(bool isPressed, KeyCode mappedKey)
        {
            if (isPressed)
            {
                if (!heldKeys.Contains(mappedKey))
                {
                    pressedKeys.Add(mappedKey);
                }
                heldKeys.Add(mappedKey);
            }
        }

        public bool WasPressed(KeyCode key) => pressedKeys.Contains(key);

        public bool WasPrimaryConfirmPressed() => WasPressed(KeyCode.Return) || WasPressed(KeyCode.Z);

        public bool WasConfirmPressed() => WasPrimaryConfirmPressed() || WasPressed(KeyCode.X);

        public bool WasShopConfirmPressed() => WasPrimaryConfirmPressed();

        public bool WasShopBackPressed() => WasPressed(KeyCode.Escape) || WasPressed(KeyCode.X);

        public bool WasBattleSubmenuConfirmPressed() => WasPrimaryConfirmPressed();

        public bool WasBattleSubmenuBackPressed() => WasPressed(KeyCode.Escape) || WasPressed(KeyCode.X);

        public bool WasFieldInteractPressed() => WasPrimaryConfirmPressed();
    }
}
