using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
#endif

namespace ImmortalOrgan
{
    /// <summary>
    /// Supports Unity 6's recommended Input System package, with a legacy fallback.
    /// Touch gameplay controls use uGUI pointer events and don't depend on this class.
    /// </summary>
    public static class GameInput
    {
        public static bool Held(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && NewKey(key)?.isPressed == true) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKey(key);
#else
            return false;
#endif
        }

        public static bool Pressed(KeyCode key)
        {
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && NewKey(key)?.wasPressedThisFrame == true) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(key);
#else
            return false;
#endif
        }

        public static bool MouseLeftPressed()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButtonDown(0);
#else
            return false;
#endif
        }

        public static bool MouseRightHeld()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null && Mouse.current.rightButton.isPressed) return true;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetMouseButton(1);
#else
            return false;
#endif
        }

        public static Vector2 MouseDelta()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.delta.ReadValue() * .1f;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#else
            return Vector2.zero;
#endif
        }

        public static float Scroll()
        {
#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null) return Mouse.current.scroll.ReadValue().y / 120f;
#endif
#if ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetAxis("Mouse ScrollWheel");
#else
            return 0f;
#endif
        }

#if ENABLE_INPUT_SYSTEM
        private static KeyControl NewKey(KeyCode code)
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return null;
            switch (code)
            {
                case KeyCode.W: return keyboard.wKey;
                case KeyCode.A: return keyboard.aKey;
                case KeyCode.S: return keyboard.sKey;
                case KeyCode.D: return keyboard.dKey;
                case KeyCode.J: return keyboard.jKey;
                case KeyCode.Q: return keyboard.qKey;
                case KeyCode.R: return keyboard.rKey;
                case KeyCode.E: return keyboard.eKey;
                case KeyCode.Space: return keyboard.spaceKey;
                case KeyCode.LeftShift: return keyboard.leftShiftKey;
                case KeyCode.LeftControl: return keyboard.leftCtrlKey;
                case KeyCode.RightArrow: return keyboard.rightArrowKey;
                case KeyCode.LeftArrow: return keyboard.leftArrowKey;
                case KeyCode.UpArrow: return keyboard.upArrowKey;
                case KeyCode.DownArrow: return keyboard.downArrowKey;
                case KeyCode.Escape: return keyboard.escapeKey;
                default: return null;
            }
        }
#endif
    }
}
