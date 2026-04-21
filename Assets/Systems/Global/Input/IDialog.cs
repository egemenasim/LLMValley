/// <summary>
/// Contract for any modal dialog panel in LLMValley.
///
/// Implementing this interface on a MonoBehaviour allows the global
/// DialogInputManager to close it via the Escape key without knowing
/// what the dialog actually is.
///
/// Implementation responsibilities:
///   • Call PlayerInputLock.Lock()   when the dialog opens.
///   • Call PlayerInputLock.Unlock() when the dialog closes.
///   • Register/unregister with DialogInputManager in OnEnable/OnDisable.
/// </summary>
public interface IDialog
{
    /// <summary>Whether the dialog is currently visible and active.</summary>
    bool IsOpen { get; }

    /// <summary>
    /// Close the dialog. Called by DialogInputManager on Escape.
    /// Implementations must call PlayerInputLock.Unlock() here (or in
    /// the close logic they delegate to).
    /// </summary>
    void CloseDialog();
}
