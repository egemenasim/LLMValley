using System;
using UnityEngine;

/// <summary>
/// Global, static event bus that tracks how many dialog/UI panels are
/// currently requesting player-input suppression.
///
/// Pattern: Mediator + Reference-Counted Lock Stack
///
/// Usage — any dialog that should block player movement:
///   void OnOpen()  => PlayerInputLock.Lock();
///   void OnClose() => PlayerInputLock.Unlock();
///
/// Subscribers (e.g. Move.CanMove) check IsLocked; they never need to
/// know which specific dialog is open.
/// </summary>
public static class PlayerInputLock
{
    // ── State ─────────────────────────────────────────────────────────────
    private static int _lockCount;

    /// <summary>True when at least one dialog has locked player input.</summary>
    public static bool IsLocked => _lockCount > 0;

    // ── Events ────────────────────────────────────────────────────────────
    /// <summary>Fired the first time the lock counter goes from 0 to 1.</summary>
    public static event Action OnLocked;

    /// <summary>Fired when the lock counter returns from N to 0.</summary>
    public static event Action OnUnlocked;

    // ── API ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Increment the lock counter. Player-input consumers (e.g. PlayerMove)
    /// will start returning false from CanMove() after this call.
    /// Call once per dialog when it opens.
    /// </summary>
    public static void Lock()
    {
        _lockCount++;

        if (_lockCount == 1)
        {
            ApplyCursorState(visible: true, lockState: CursorLockMode.None);
            OnLocked?.Invoke();
            Debug.Log("[PlayerInputLock] Input locked. Active locks: 1");
        }
        else
        {
            Debug.Log($"[PlayerInputLock] Lock stacked. Active locks: {_lockCount}");
        }
    }

    /// <summary>
    /// Decrement the lock counter. When it reaches zero, player-input
    /// consumers are unblocked and OnUnlocked is fired.
    /// Call once per dialog when it closes.
    /// </summary>
    public static void Unlock()
    {
        if (_lockCount <= 0)
        {
            Debug.LogWarning("[PlayerInputLock] Unlock() called when lock count is already 0. Ignored.");
            return;
        }

        _lockCount--;

        if (_lockCount == 0)
        {
            // Restore to default game cursor state (unlocked + visible for 2D top-down).
            // Adjust CursorLockMode if the game later needs a locked cursor in normal gameplay.
            ApplyCursorState(visible: true, lockState: CursorLockMode.None);
            OnUnlocked?.Invoke();
            Debug.Log("[PlayerInputLock] Input unlocked. All dialogs closed.");
        }
        else
        {
            Debug.Log($"[PlayerInputLock] Lock decremented. Remaining locks: {_lockCount}");
        }
    }

    /// <summary>
    /// Force-resets the lock counter to zero and fires OnUnlocked if it was locked.
    /// Use only in edge cases like scene unload or emergency cleanup.
    /// </summary>
    public static void ForceReset()
    {
        if (_lockCount > 0)
        {
            _lockCount = 0;
            ApplyCursorState(visible: true, lockState: CursorLockMode.None);
            OnUnlocked?.Invoke();
            Debug.LogWarning("[PlayerInputLock] ForceReset() called. Lock counter cleared.");
        }
    }

    // ── Cursor helper ─────────────────────────────────────────────────────
    private static void ApplyCursorState(bool visible, CursorLockMode lockState)
    {
        Cursor.visible   = visible;
        Cursor.lockState = lockState;
    }
}
