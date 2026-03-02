#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Reflection;

public class AudioPreviewTool : EditorWindow
{
    private AudioClip[] clips = new AudioClip[0];
    private Vector2 scroll;

    [MenuItem("Window/Audio Preview Tool")]
    public static void OpenWindow()
    {
        GetWindow<AudioPreviewTool>("Audio Preview Tool");
    }

    private void OnGUI()
    {
        SerializedObject so = new SerializedObject(this);
        SerializedProperty clipsProp = so.FindProperty("clips");
        EditorGUILayout.PropertyField(clipsProp, new GUIContent("Clips"), includeChildren: true);
        so.ApplyModifiedProperties();

        EditorGUILayout.Space(8f);

        scroll = EditorGUILayout.BeginScrollView(scroll);
        foreach (AudioClip clip in clips)
        {
            if (clip == null) continue;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(clip.name);
            if (GUILayout.Button("Play", GUILayout.Width(30f))) PlayClip(clip);
            if (GUILayout.Button("Stop", GUILayout.Width(30f))) StopClip();
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void PlayClip(AudioClip clip)
    {
        var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        audioUtil.GetMethod("PlayPreviewClip", BindingFlags.Static | BindingFlags.Public)
                 ?.Invoke(null, new object[] { clip, 0, false });
    }

    private void StopClip()
    {
        var audioUtil = typeof(AudioImporter).Assembly.GetType("UnityEditor.AudioUtil");
        audioUtil.GetMethod("StopAllPreviewClips", BindingFlags.Static | BindingFlags.Public)
                 ?.Invoke(null, null);
    }
}
#endif