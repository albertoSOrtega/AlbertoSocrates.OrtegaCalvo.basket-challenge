#if UNITY_EDITOR
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class AudioPreviewTool : EditorWindow
{
    private List<AudioClip> clips = new List<AudioClip>();
    private ReorderableList reorderableList;
    private Vector2 scroll;

    [MenuItem("Window/Audio Preview Tool")]
    public static void OpenWindow()
    {
        GetWindow<AudioPreviewTool>("Audio Preview Tool");
    }

    private void OnEnable()
    {
        reorderableList = new ReorderableList(clips, typeof(AudioClip), true, true, true, true);

        reorderableList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Clips");

        reorderableList.drawElementCallback = (rect, index, isActive, isFocused) =>
        {
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;
            clips[index] = (AudioClip)EditorGUI.ObjectField(rect, clips[index], typeof(AudioClip), false);
        };

        reorderableList.onAddCallback = list => clips.Add(null);
        reorderableList.onRemoveCallback = list => clips.RemoveAt(list.index);
    }

    private void OnGUI()
    {
        reorderableList.DoLayoutList();

        EditorGUILayout.Space(8f);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (AudioClip clip in clips)
        {
            if (clip == null) continue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(clip.name);
            if (GUILayout.Button("Play", GUILayout.Width(50f))) PlayClip(clip);
            if (GUILayout.Button("Stop", GUILayout.Width(50f))) StopAllClips();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void PlayClip(AudioClip clip)
    {
        var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        MethodInfo playMethod = audioUtil?.GetMethod(
            "PlayPreviewClip",
            BindingFlags.Static | BindingFlags.Public,
            null,
            new[] { typeof(AudioClip), typeof(int), typeof(bool) },
            null
        );
        playMethod?.Invoke(null, new object[] { clip, 0, false });
    }

    private void StopAllClips()
    {
        var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        audioUtil?.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public)
                 ?.Invoke(null, null);
    }
}
#endif