using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ComponentFieldViewer : EditorWindow
{
    private Vector2 scrollPosition;
    private GameObject targetObject; // アタッチする対象オブジェクト
    private SerializedObject serializedTarget; // シリアライズされたオブジェクトを管理

    [MenuItem("Window/Component Field Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<ComponentFieldViewer>("Component Field Viewer");
        window.Show();
    }

    private void OnEnable()
    {
        EditorApplication.update += MyUpdate;
        LoadTargetObject(); // 保存されたオブジェクトを復元
    }

    private void OnDisable()
    {
        EditorApplication.update -= MyUpdate;
        SaveTargetObject(); // オブジェクトを保存
    }

    void MyUpdate()
    {
        Repaint();
    }

    void OnGUI()
    {
        // オブジェクトを設定するフィールド
        EditorGUILayout.LabelField("Target GameObject", EditorStyles.boldLabel);
        targetObject = (GameObject)EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true);

        // 変更があればシリアライズ対象を更新
        if (targetObject != null && (serializedTarget == null || serializedTarget.targetObject != targetObject))
        {
            serializedTarget = new SerializedObject(targetObject);
        }

        if (targetObject == null)
        {
            GUILayout.Label("No GameObject selected.");
            return;
        }

        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        foreach (Component component in targetObject.GetComponents<Component>())
        {
            if (component == null)
                continue;

            GUILayout.Label(component.GetType().Name, EditorStyles.boldLabel);

            FieldInfo[] fields = component.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo field in fields)
            {
                object value = field.GetValue(component);
                string valueStr = value != null ? value.ToString() : "null";
                EditorGUILayout.LabelField(field.Name + " (" + field.FieldType.Name + ")", valueStr);
            }

            GUILayout.Space(10);
        }

        GUILayout.EndScrollView();
    }

    // オブジェクトの保存と復元
    private void SaveTargetObject()
    {
        if (targetObject != null)
        {
            EditorPrefs.SetString("ComponentFieldViewer.TargetObject", AssetDatabase.GetAssetPath(targetObject));
        }
        else
        {
            EditorPrefs.DeleteKey("ComponentFieldViewer.TargetObject");
        }
    }

    private void LoadTargetObject()
    {
        string path = EditorPrefs.GetString("ComponentFieldViewer.TargetObject", null);
        if (!string.IsNullOrEmpty(path))
        {
            targetObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}
