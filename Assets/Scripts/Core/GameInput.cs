using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// Lê o teclado/mouse/toque. Funciona tanto com o Input antigo (Input.GetAxis) quanto com o
/// novo Input System, dependendo da configuração do projeto.
/// </summary>
public static class GameInput
{
    /// <summary>-1 (esquerda) .. +1 (direita): setas ou A/D.</summary>
    public static float Horizontal()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        if (k == null) return 0f;
        float h = 0f;
        if (k.leftArrowKey.isPressed  || k.aKey.isPressed) h -= 1f;
        if (k.rightArrowKey.isPressed || k.dKey.isPressed) h += 1f;
        return h;
#else
        return Input.GetAxisRaw("Horizontal");
#endif
    }

    /// <summary>Espaço, clique do mouse ou toque.</summary>
    public static bool LaunchPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current; Mouse m = Mouse.current; Touchscreen t = Touchscreen.current;
        return (k != null && k.spaceKey.wasPressedThisFrame)
            || (m != null && m.leftButton.wasPressedThisFrame)
            || (t != null && t.primaryTouch.press.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
#endif
    }

    /// <summary>Enter ou R (usado nas telas de vitória e game over).</summary>
    public static bool ConfirmPressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        return k != null && (k.enterKey.wasPressedThisFrame || k.numpadEnterKey.wasPressedThisFrame || k.rKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.R);
#endif
    }

    public static bool EscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        return k != null && k.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    public static bool MutePressed()
    {
#if ENABLE_INPUT_SYSTEM
        Keyboard k = Keyboard.current;
        return k != null && k.mKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.M);
#endif
    }

    public static bool PointerHeld()
    {
#if ENABLE_INPUT_SYSTEM
        Mouse m = Mouse.current; Touchscreen t = Touchscreen.current;
        return (m != null && m.leftButton.isPressed) || (t != null && t.primaryTouch.press.isPressed);
#else
        return Input.GetMouseButton(0);
#endif
    }

    public static Vector2 PointerPosition()
    {
#if ENABLE_INPUT_SYSTEM
        Touchscreen t = Touchscreen.current;
        if (t != null && t.primaryTouch.press.isPressed) return t.primaryTouch.position.ReadValue();
        Mouse m = Mouse.current;
        return m != null ? m.position.ReadValue() : Vector2.zero;
#else
        return Input.mousePosition;
#endif
    }
}
