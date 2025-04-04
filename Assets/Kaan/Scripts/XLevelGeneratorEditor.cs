#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(XLevelGenerator))]
public class XLevelGeneratorEditor : Editor
{
    SerializedProperty seedProperty;
    SerializedProperty minRoomsProperty;
    SerializedProperty maxRoomsProperty;
    SerializedProperty minRoomSizeProperty;
    SerializedProperty maxRoomSizeProperty;
    SerializedProperty cellSizeProperty;
    SerializedProperty roomPrefabProperty;
    SerializedProperty generationModeProperty;
    SerializedProperty constrainedAreaSizeProperty;
    SerializedProperty themesProperty;
    SerializedProperty furnitureSetsProperty;

    private void OnEnable()
    {
        // Generation Settings
        seedProperty = serializedObject.FindProperty("seed");
        minRoomsProperty = serializedObject.FindProperty("minRooms");
        maxRoomsProperty = serializedObject.FindProperty("maxRooms");
        minRoomSizeProperty = serializedObject.FindProperty("minRoomSize");
        maxRoomSizeProperty = serializedObject.FindProperty("maxRoomSize");
        cellSizeProperty = serializedObject.FindProperty("cellSize");
        roomPrefabProperty = serializedObject.FindProperty("roomPrefab");

        // Generation Mode
        generationModeProperty = serializedObject.FindProperty("generationMode");
        constrainedAreaSizeProperty = serializedObject.FindProperty("constrainedAreaSize");

        // Themes
        themesProperty = serializedObject.FindProperty("themes");
        furnitureSetsProperty = serializedObject.FindProperty("furnitureSets");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        XLevelGenerator generator = (XLevelGenerator)target;

        // Generation Settings Header
        EditorGUILayout.LabelField("Generation Settings", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(seedProperty);
        EditorGUILayout.PropertyField(minRoomsProperty);
        EditorGUILayout.PropertyField(maxRoomsProperty);
        EditorGUILayout.PropertyField(minRoomSizeProperty);
        EditorGUILayout.PropertyField(maxRoomSizeProperty);
        EditorGUILayout.PropertyField(cellSizeProperty);
        EditorGUILayout.PropertyField(roomPrefabProperty);

        // Generation Mode Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Generation Mode", EditorStyles.boldLabel);

        // Generation Mode dropdown
        EditorGUILayout.PropertyField(generationModeProperty);

        // Dinamik olarak seçime göre alanlarý göster/gizle
        if ((GenerationMode)generationModeProperty.enumValueIndex == GenerationMode.ConstrainedArea)
        {
            // ConstrainedArea seçildiðinde, alan boyutu ayarlarýný göster
            EditorGUILayout.PropertyField(constrainedAreaSizeProperty, new GUIContent("Area Size"));
        }
        // CompactArrangement seçildiðinde, hiçbir ek ayar gösterme

        // Themes Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Themes", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(themesProperty, true);
        EditorGUILayout.PropertyField(furnitureSetsProperty, true);

        // Action Buttons
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Dungeon"))
        {
            generator.GenerateDungeon();
        }

        // Vizualizasyon seçeneði eklenebilir
        if ((GenerationMode)generationModeProperty.enumValueIndex == GenerationMode.ConstrainedArea)
        {
            if (GUILayout.Button("Show Area Boundaries"))
            {
                // Burada alan sýnýrlarýný görselleþtirmek için özel bir iþlev çaðrýlabilir
                SceneView.RepaintAll();
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI()
    {
        XLevelGenerator generator = (XLevelGenerator)target;

        // ConstrainedArea modundaysa ve alanda görselleþtirme gerekiyorsa
        if (generator.generationMode == GenerationMode.ConstrainedArea)
        {
            // Alan sýnýrlarýný görselleþtir
            Vector3 center = generator.transform.position;
            Vector2 size = generator.constrainedAreaSize;

            // Alan sýnýrlarýný yeþil çizgilerle çiz
            Handles.color = Color.green;

            Vector3 topLeft = center + new Vector3(-size.x / 2, 0, size.y / 2);
            Vector3 topRight = center + new Vector3(size.x / 2, 0, size.y / 2);
            Vector3 bottomLeft = center + new Vector3(-size.x / 2, 0, -size.y / 2);
            Vector3 bottomRight = center + new Vector3(size.x / 2, 0, -size.y / 2);

            Handles.DrawLine(topLeft, topRight);
            Handles.DrawLine(topRight, bottomRight);
            Handles.DrawLine(bottomRight, bottomLeft);
            Handles.DrawLine(bottomLeft, topLeft);
        }
    }
}
#endif