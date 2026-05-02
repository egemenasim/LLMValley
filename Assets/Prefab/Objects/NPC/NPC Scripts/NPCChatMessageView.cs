using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LLMValley.NPCChat
{
    public class NPCChatMessageView : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private LayoutElement leftSpacer;
        [SerializeField] private LayoutElement rightSpacer;
        [SerializeField] private LayoutElement bubbleLayoutElement;
        [SerializeField] private Image bubbleImage;
        [SerializeField] private TMP_Text senderLabel;
        [SerializeField] private TMP_Text contentLabel;

        [Header("Colors")]
        [SerializeField] private Color userBubbleColor = new(0.29f, 0.35f, 0.90f, 1.0f);
        [SerializeField] private Color assistantBubbleColor = new(1.0f, 1.0f, 1.0f, 1.0f);
        [SerializeField] private Color userTextColor = Color.white;
        [SerializeField] private Color assistantTextColor = new(0.14f, 0.16f, 0.20f, 1.0f);

        [Header("Layout")]
        [SerializeField] private bool showSenderLabel;
        [SerializeField] private float maxBubbleWidth = 680f;
        [SerializeField] private float userSidePadding = 1.15f;
        [SerializeField] private float assistantSidePadding = 1.35f;

        public void Bind(NPCChatMessage message, string npcDisplayName)
        {
            var isUser = string.Equals(message.role, "user");

            if (leftSpacer != null)
            {
                leftSpacer.flexibleWidth = isUser ? userSidePadding : 0.15f;
            }

            if (rightSpacer != null)
            {
                rightSpacer.flexibleWidth = isUser ? 0.15f : assistantSidePadding;
            }

            if (bubbleLayoutElement != null)
            {
                bubbleLayoutElement.preferredWidth = maxBubbleWidth;
            }

            if (bubbleImage != null)
            {
                bubbleImage.color = isUser ? userBubbleColor : assistantBubbleColor;
            }

            var textColor = isUser ? userTextColor : assistantTextColor;

            if (senderLabel != null)
            {
                senderLabel.text = isUser ? "You" : npcDisplayName;
                senderLabel.color = textColor;
                senderLabel.gameObject.SetActive(showSenderLabel);
            }

            if (contentLabel != null)
            {
                contentLabel.text = message.content;
                contentLabel.color = textColor;
                contentLabel.alignment = isUser ? TextAlignmentOptions.MidlineRight : TextAlignmentOptions.MidlineLeft;
            }
        }
    }
}
