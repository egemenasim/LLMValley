using UnityEngine;
using LLMValley.Player;
using LLMValley.SaveSystem;
using UnityEngine.Events;
using TMPro;

namespace LLMValley.Interaction
{
    [AddComponentMenu("Interaction/Area Gate")]
    public class AreaGate : Interactable
    {
        [Header("Gate")]
        //[SerializeField] private string requiredKeyId;
        //[SerializeField] private string requiredKeyName;
        //[SerializeField] private string gateId;

        [SerializeField] private LLMValley.Items.KeyItemData requiredKey;
        [SerializeField] private string gateId;


        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour inventoryProvider;

        [Header("Events")]
        [SerializeField] private UnityEvent onGateOpened;

        [Header("State")]
        [SerializeField] private bool isOpen;

        [Header("Physical Collider")]
        [SerializeField] private Collider2D physicalCollider; // isTrigger kapalı collider

        [Header("Feedback")]
        [Tooltip("Kapı objesinin altındaki feedback GameObject (TMP içeren)")]
        [SerializeField] private GameObject feedbackObject; // Kapı alt objesi, prefab değil
        [Tooltip("Feedback yazısı için TMP_Text componenti")]
        [SerializeField] private TMP_Text feedbackText;


        private IInventoryKeyProvider _keyProvider;

        private void Awake()
        {
            _keyProvider = inventoryProvider as IInventoryKeyProvider;
            ApplySavedState();
            UpdateGateVisual();
            if (isOpen)
                onGateOpened?.Invoke();
        }



        public override void interact_event()
        {
            if (isOpen)
            {
                onGateOpened?.Invoke();
                base.interact_event();
                return;
            }

            if (!HasValidKey())
            {
                string keyDisplayName = requiredKey != null && !string.IsNullOrEmpty(requiredKey.keyName)
                    ? requiredKey.keyName
                    : (requiredKey != null ? requiredKey.keyId : "Key");
                ShowFeedback($"{keyDisplayName} Required!");
                return; // <-- Burası önemli!
            }

            OpenGate();
            base.interact_event();
        }

        private void ShowFeedback(string message)
        {
            if (feedbackObject == null || feedbackText == null)
                return;

            feedbackObject.SetActive(true);
            feedbackText.text = message;
            CancelInvoke(nameof(HideFeedback));
            Invoke(nameof(HideFeedback), 2f);
        }

        private void HideFeedback()
        {
            if (feedbackObject != null)
                feedbackObject.SetActive(false);
        }

        private bool HasValidKey()
        {
            string keyId = requiredKey != null ? requiredKey.keyId : "";
            if (string.IsNullOrWhiteSpace(keyId))
                return false;

            if (_keyProvider == null)
                _keyProvider = FindKeyProviderInScene();

            return _keyProvider != null && _keyProvider.HasKey(keyId);
        }

        private void OpenGate()
        {
            isOpen = true;
            RemoveKeyFromInventory();
            SaveGateState();
            UpdateGateVisual();
            onGateOpened?.Invoke();
        }

        private void RemoveKeyFromInventory()
        {
            if (_keyProvider is PlayerInventory inv && requiredKey != null)
            {
                foreach (var stack in inv.Items)
                {
                    if (stack.item == requiredKey)
                    {
                        inv.TryRemoveItem(stack.item, 1);
                        break;
                    }
                }
            }
        }

        private void UpdateGateVisual()
        {

            if (isOpen)
            {
                Destroy(gameObject); 
                return;
            }
            if (physicalCollider != null)
                physicalCollider.enabled = true;
        }

        private void ApplySavedState()
        {
            if (string.IsNullOrWhiteSpace(gateId))
                return;

            isOpen = GateStateStore.GetGateOpen(gateId, isOpen);
        }

        private void SaveGateState()
        {
            if (string.IsNullOrWhiteSpace(gateId))
                return;

            GateStateStore.SetGateOpen(gateId, isOpen);
        }

        private static IInventoryKeyProvider FindKeyProviderInScene()
        {
            var behaviours = Object.FindObjectsByType<MonoBehaviour>(
     FindObjectsInactive.Include,
     FindObjectsSortMode.None
 );
            foreach (var behaviour in behaviours)
            {
                if (behaviour is IInventoryKeyProvider provider)
                    return provider;
            }
            return null;
        }
    }
}