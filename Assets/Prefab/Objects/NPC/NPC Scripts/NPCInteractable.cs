using UnityEngine;

namespace LLMValley.NPCChat
{
    [AddComponentMenu("Interaction/NPC Interactable")]
    [RequireComponent(typeof(Collider2D))]
    public class NPCInteractable : Interactable
    {
        [SerializeField] private NPCChatAgent chatAgent;

        private void Reset()
        {
            chatAgent = GetComponentInParent<NPCChatAgent>();

            var trigger = GetComponent<Collider2D>();
            if (trigger != null)
            {
                trigger.isTrigger = true;
            }
        }

        private void Awake()
        {
            if (chatAgent == null)
            {
                chatAgent = GetComponentInParent<NPCChatAgent>();
            }
        }

        public override void interact_event()
        {
            base.interact_event();

            if (chatAgent == null)
            {
                return;
            }

            chatAgent.BeginConversation();
        }
    }
}
