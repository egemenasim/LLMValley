using UnityEngine;

/// <summary>
/// Concrete Plantable implementation so plant prefabs can be created in the editor.
/// Plantable is abstract, so it cannot be added directly to a GameObject.
/// </summary>
[DisallowMultipleComponent]
public sealed class BasicPlant : Plantable
{
}
