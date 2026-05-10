using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace LLMValley.NPCChat
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class NPCAnimation : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NPCChatAgent chatAgent;
        [SerializeField] private NPCPersona personaOverride;
        [SerializeField] private SpriteRenderer targetRenderer;
        [SerializeField] private Animator targetAnimator;

        [Header("Idle Loop")]
        [SerializeField] private Sprite[] idleFrames = new Sprite[0];
        [SerializeField, Min(0.1f)] private float framesPerSecond = 4f;
        [SerializeField] private bool useAnimatorController = true;
        [SerializeField] private bool playOnEnable = true;

        private int frameIndex;
        private float frameTimer;
        private double lastEditorTime;

        public IReadOnlyList<Sprite> IdleFrames => idleFrames;

        private void Awake()
        {
            ResolveReferences();
            EnsureRuntimeFrames();
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorTick;
            EditorApplication.update += EditorTick;
#endif

            if (!playOnEnable)
            {
                return;
            }

            frameIndex = 0;
            frameTimer = 0f;
            lastEditorTime = GetCurrentTime();
            if (!CanUseAnimator())
            {
                ApplyCurrentFrame();
            }
        }

        private void OnDisable()
        {
#if UNITY_EDITOR
            EditorApplication.update -= EditorTick;
#endif
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (CanUseAnimator())
            {
                return;
            }

            StepAnimation(Time.unscaledDeltaTime);
        }

        private void LateUpdate()
        {
            if (Application.isPlaying && CanUseAnimator())
            {
                return;
            }

            ApplyCurrentFrame();
        }

        private void StepAnimation(float deltaTime)
        {
            if (CanUseAnimator())
            {
                return;
            }

            if (!playOnEnable || idleFrames == null || idleFrames.Length == 0 || targetRenderer == null)
            {
                return;
            }

            if (idleFrames.Length == 1)
            {
                ApplyCurrentFrame();
                return;
            }

            frameTimer += deltaTime;
            var frameDuration = 1f / Mathf.Max(0.1f, framesPerSecond);

            while (frameTimer >= frameDuration)
            {
                frameTimer -= frameDuration;
                frameIndex = (frameIndex + 1) % idleFrames.Length;
                ApplyCurrentFrame();
            }
        }

        private static double GetCurrentTime()
        {
#if UNITY_EDITOR
            return EditorApplication.timeSinceStartup;
#else
            return Time.realtimeSinceStartupAsDouble;
#endif
        }

#if UNITY_EDITOR
        private void EditorTick()
        {
            if (Application.isPlaying || this == null)
            {
                return;
            }

            var currentTime = GetCurrentTime();
            var delta = lastEditorTime <= 0d ? 0d : currentTime - lastEditorTime;
            lastEditorTime = currentTime;
            StepAnimation(Mathf.Clamp((float)delta, 0f, 0.1f));
        }
#endif

        private void OnValidate()
        {
            ResolveReferences();
#if UNITY_EDITOR
            RefreshIdleFramesFromPersona();
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    RebuildAnimatorController();
                    if (!CanUseAnimator())
                    {
                        ApplyCurrentFrame();
                    }
                }
            };
#else
            if (!CanUseAnimator())
            {
                ApplyCurrentFrame();
            }
#endif
        }

        public void RefreshFromPersona()
        {
            ResolveReferences();
#if UNITY_EDITOR
            RefreshIdleFramesFromPersona();
            RebuildAnimatorController();
#endif
            EnsureRuntimeFrames();
            frameIndex = 0;
            if (!CanUseAnimator())
            {
                ApplyCurrentFrame();
            }
        }

        private void ResolveReferences()
        {
            chatAgent ??= GetComponent<NPCChatAgent>();
            targetRenderer ??= chatAgent != null
                ? chatAgent.WorldSpriteRenderer
                : GetComponentInChildren<SpriteRenderer>(true);
            targetAnimator ??= targetRenderer != null ? targetRenderer.GetComponent<Animator>() : null;
        }

        private NPCPersona ResolvePersona()
        {
            if (personaOverride != null)
            {
                return personaOverride;
            }

            return chatAgent != null ? chatAgent.Persona : null;
        }

        private void ApplyCurrentFrame()
        {
            if (targetRenderer == null || idleFrames == null || idleFrames.Length == 0)
            {
                return;
            }

            frameIndex = Mathf.Clamp(frameIndex, 0, idleFrames.Length - 1);
            targetRenderer.sprite = idleFrames[frameIndex];
            targetRenderer.enabled = idleFrames[frameIndex] != null;
        }

        private bool CanUseAnimator()
        {
            return useAnimatorController &&
                   targetAnimator != null &&
                   targetAnimator.runtimeAnimatorController != null &&
                   targetAnimator.enabled;
        }

        private void EnsureRuntimeFrames()
        {
            if (idleFrames != null && idleFrames.Length > 0)
            {
                return;
            }

            var worldSprite = ResolvePersona()?.WorldSprite;
            if (worldSprite == null)
            {
                return;
            }

            idleFrames = FindLoadedSiblingSprites(worldSprite)
                .Take(3)
                .ToArray();

            if (idleFrames.Length == 0)
            {
                idleFrames = new[] { worldSprite };
            }
        }

        private static IEnumerable<Sprite> FindLoadedSiblingSprites(Sprite worldSprite)
        {
            if (worldSprite == null || worldSprite.texture == null)
            {
                return Enumerable.Empty<Sprite>();
            }

            return Resources.FindObjectsOfTypeAll<Sprite>()
                .Where(sprite => sprite != null && sprite.texture == worldSprite.texture)
                .OrderBy(sprite => sprite.name, System.StringComparer.OrdinalIgnoreCase)
                .ThenBy(sprite => sprite.textureRect.y)
                .ThenBy(sprite => sprite.textureRect.x);
        }

#if UNITY_EDITOR
        [ContextMenu("Rebuild Animator Controller")]
        public void RebuildAnimatorController()
        {
            ResolveReferences();

            if (!useAnimatorController || targetRenderer == null || idleFrames == null || idleFrames.Length == 0)
            {
                return;
            }

            if (targetAnimator == null)
            {
                targetAnimator = targetRenderer.GetComponent<Animator>();
                if (targetAnimator == null)
                {
                    targetAnimator = Undo.AddComponent<Animator>(targetRenderer.gameObject);
                }
            }

            const string folder = "Assets/Art/Animation/NPCGenerated";
            EnsureFolder(folder);

            var assetName = ResolvePersona()?.name;
            if (string.IsNullOrWhiteSpace(assetName))
            {
                assetName = targetRenderer.sprite != null ? targetRenderer.sprite.texture.name : gameObject.name;
            }

            assetName = SanitizeAssetName(assetName);
            var clipPath = $"{folder}/{assetName}_Idle.anim";
            var controllerPath = $"{folder}/{assetName}_Idle.controller";

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            clip.frameRate = Mathf.Max(1f, framesPerSecond);
            var frameDuration = 1f / Mathf.Max(0.1f, framesPerSecond);
            var keyframes = new ObjectReferenceKeyframe[idleFrames.Length + 1];
            for (var i = 0; i < idleFrames.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i * frameDuration,
                    value = idleFrames[i]
                };
            }

            keyframes[idleFrames.Length] = new ObjectReferenceKeyframe
            {
                time = idleFrames.Length * frameDuration,
                value = idleFrames[0]
            };

            var binding = EditorCurveBinding.PPtrCurve(string.Empty, typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            EditorUtility.SetDirty(clip);

            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            }

            var stateMachine = controller.layers[0].stateMachine;
            var state = stateMachine.states.FirstOrDefault(child => child.state.name == "Idle").state;
            if (state == null)
            {
                state = stateMachine.AddState("Idle");
            }

            state.motion = clip;
            stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);

            targetAnimator.runtimeAnimatorController = controller;
            targetAnimator.enabled = true;
            EditorUtility.SetDirty(targetAnimator);
            AssetDatabase.SaveAssets();
        }

        private void RefreshIdleFramesFromPersona()
        {
            var worldSprite = ResolvePersona()?.WorldSprite;
            if (worldSprite == null)
            {
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(worldSprite);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            var sprites = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<Sprite>()
                .Take(3)
                .ToArray();

            idleFrames = sprites.Length > 0 ? sprites : new[] { worldSprite };
        }

        private static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parts = folder.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        private static string SanitizeAssetName(string value)
        {
            foreach (var invalidChar in System.IO.Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalidChar, '_');
            }

            return value.Trim();
        }
#endif
    }
}
