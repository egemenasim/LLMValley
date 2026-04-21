using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Scene-level MonoBehaviour that listens for the Escape key and closes
/// the most-recently-opened dialog that implements <see cref="IDialog"/>.
///
/// HOW TO USE
/// ──────────
/// 1. Add a single DialogInputManager to any persistent GameObject
///    (e.g. your scene's "Game Manager" or "UI Root").
///
/// 2. In every dialog's MonoBehaviour, call:
///      DialogInputManager.Register(this);   // in Open / OnEnable
///      DialogInputManager.Unregister(this); // in Close / OnDisable
///
/// 3. That's it. Pressing Escape will always close the topmost dialog.
///
/// DESIGN NOTE
/// ───────────
/// Dialogs are tracked in a stack (List used as stack). The last one
/// registered is the first one closed — correct LIFO behaviour for
/// nested/layered UIs (e.g. confirm popup over inventory over chat).
/// </summary>
public class DialogInputManager : MonoBehaviour
{
    // ── Singleton ─────────────────────────────────────────────────────────
    public static DialogInputManager Instance { get; private set; }

    // ── State ─────────────────────────────────────────────────────────────
    // List used as a stack: last element = topmost open dialog.
    private static readonly List<IDialog> _dialogStack = new();

    // ── Unity lifecycle ───────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseTopmostDialog();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Register a dialog so it can receive Escape-to-close.
    /// Call this when the dialog opens.
    /// </summary>
    public static void Register(IDialog dialog)
    {
        if (dialog == null || _dialogStack.Contains(dialog))
        {
            return;
        }

        _dialogStack.Add(dialog);
    }

    /// <summary>
    /// Unregister a dialog. Call this when the dialog closes.
    /// </summary>
    public static void Unregister(IDialog dialog)
    {
        _dialogStack.Remove(dialog);
    }

    // ── Private helpers ───────────────────────────────────────────────────
    private static void CloseTopmostDialog()
    {
        // Walk from the top of the stack downward, find the first open dialog.
        for (int i = _dialogStack.Count - 1; i >= 0; i--)
        {
            var dialog = _dialogStack[i];
            if (dialog == null)
            {
                _dialogStack.RemoveAt(i);
                continue;
            }

            if (dialog.IsOpen)
            {
                dialog.CloseDialog();
                return; // Only close one dialog per Escape press.
            }
        }
    }
}
