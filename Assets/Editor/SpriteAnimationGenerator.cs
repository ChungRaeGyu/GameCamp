using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[InitializeOnLoad]
public static class SpriteAnimationGenerator
{
    private const float FramesPerSecond = 8f;
    private static readonly string[] SpriteRoots =
    {
        "Assets/Images/Enemy",
        "Assets/Images/Heros"
    };

    static SpriteAnimationGenerator()
    {
        EditorApplication.delayCall += GenerateAll;
    }

    [MenuItem("GameCamp/Generate Sprite Animations")]
    public static void GenerateAll()
    {
        foreach (string spriteRoot in SpriteRoots)
        {
            GenerateForRoot(spriteRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Sprite animations and Animator Controllers generated.");
    }

    private static void GenerateForRoot(string spriteRoot)
    {
        string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { spriteRoot });
        Dictionary<string, List<Sprite>> spritesByFolder = new();

        foreach (string guid in spriteGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                continue;
            }

            string folder = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(folder))
            {
                continue;
            }

            if (!spritesByFolder.TryGetValue(folder, out List<Sprite> sprites))
            {
                sprites = new List<Sprite>();
                spritesByFolder.Add(folder, sprites);
            }

            sprites.Add(sprite);
        }

        foreach (IGrouping<string, KeyValuePair<string, List<Sprite>>> unitGroup in spritesByFolder
                     .GroupBy(entry => GetUnitFolder(spriteRoot, entry.Key)))
        {
            GenerateUnitController(spriteRoot, unitGroup.Key, unitGroup);
        }
    }

    private static void GenerateUnitController(
        string spriteRoot,
        string unitFolder,
        IEnumerable<KeyValuePair<string, List<Sprite>>> animationFolders)
    {
        string category = Path.GetFileName(spriteRoot);
        string unitName = Path.GetFileName(unitFolder);
        string outputFolder = $"Assets/Animation/{category}/{unitName}";
        EnsureFolder(outputFolder);

        List<AnimationClip> clips = new();
        foreach (KeyValuePair<string, List<Sprite>> animationFolder in animationFolders)
        {
            string animationName = animationFolder.Key
                .Substring(unitFolder.Length)
                .Trim('/')
                .Replace('/', '_');
            string clipPath = $"{outputFolder}/{animationName}.anim";
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, clipPath);
            }

            ConfigureClip(clip, animationFolder.Value, IsAttackAnimation(animationName));
            clips.Add(clip);
        }

        string controllerPath = $"{outputFolder}/{unitName}.controller";
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        }

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        foreach (ChildAnimatorState state in stateMachine.states)
        {
            stateMachine.RemoveState(state.state);
        }

        if (!controller.parameters.Any(parameter => parameter.name == "Attack"))
        {
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        }

        AnimatorState defaultState = null;
        AnimatorState attackState = null;
        foreach (AnimationClip clip in clips.OrderBy(clip => clip.name))
        {
            AnimatorState state = stateMachine.AddState(clip.name);
            state.motion = clip;
            if (defaultState == null || (!IsAttackAnimation(clip.name) && IsAttackAnimation(defaultState.name)))
            {
                defaultState = state;
            }

            if (attackState == null && IsAttackAnimation(clip.name))
            {
                attackState = state;
            }
        }

        stateMachine.defaultState = defaultState;
        if (attackState != null && attackState != defaultState)
        {
            AnimatorStateTransition attackTransition = stateMachine.AddAnyStateTransition(attackState);
            attackTransition.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            attackTransition.duration = 0f;
            attackTransition.canTransitionToSelf = false;

            AnimatorStateTransition returnTransition = attackState.AddTransition(defaultState);
            returnTransition.hasExitTime = true;
            returnTransition.exitTime = 1f;
            returnTransition.duration = 0f;
        }

        EditorUtility.SetDirty(controller);
    }

    private static void ConfigureClip(AnimationClip clip, List<Sprite> sprites, bool isAttackAnimation)
    {
        ObjectReferenceKeyframe[] keyframes = sprites
            .OrderBy(sprite => GetFrameNumber(sprite.name))
            .ThenBy(sprite => sprite.name)
            .Select((sprite, index) => new ObjectReferenceKeyframe
            {
                time = index / FramesPerSecond,
                value = sprite
            })
            .ToArray();

        AnimationUtility.SetObjectReferenceCurve(
            clip,
            new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = string.Empty,
                propertyName = "m_Sprite"
            },
            keyframes);

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = !isAttackAnimation;
        AnimationUtility.SetAnimationClipSettings(clip, settings);
        clip.frameRate = FramesPerSecond;
        EditorUtility.SetDirty(clip);
    }

    private static string GetUnitFolder(string spriteRoot, string animationFolder)
    {
        string relativePath = animationFolder.Substring(spriteRoot.Length).Trim('/');
        string unitName = relativePath.Split('/')[0];
        return $"{spriteRoot}/{unitName}";
    }

    private static int GetFrameNumber(string spriteName)
    {
        return int.TryParse(spriteName, out int frameNumber) ? frameNumber : int.MaxValue;
    }

    private static bool IsAttackAnimation(string animationName)
    {
        string lowercaseName = animationName.ToLowerInvariant();
        return lowercaseName.Contains("attack") ||
               lowercaseName.Contains("slash") ||
               lowercaseName.Contains("fire") ||
               lowercaseName.Contains("shot") ||
               lowercaseName.Contains("skill");
    }

    private static void EnsureFolder(string folderPath)
    {
        string[] segments = folderPath.Split('/');
        string currentPath = segments[0];
        for (int i = 1; i < segments.Length; i++)
        {
            string nextPath = $"{currentPath}/{segments[i]}";
            if (!AssetDatabase.IsValidFolder(nextPath))
            {
                AssetDatabase.CreateFolder(currentPath, segments[i]);
            }

            currentPath = nextPath;
        }
    }
}
