using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;


	public static class UnityEditorCache
	{
		public static readonly Assembly UnityEditorAssembly;

		public static readonly dynamic EditorUtility;

		public static readonly dynamic EditorGUI;

		public static readonly dynamic EditorGUIUtility;

		public static readonly dynamic EditorWindow;

		public static  Type _AddComponentWindow;
	public static  dynamic AddComponentWindow
	{
		get {
            if (_AddComponentWindow == null)
                _AddComponentWindow = UnityEditorAssembly.GetType("UnityEditor.AddComponent.AddComponentWindow", true);

            return _AddComponentWindow;
        }
	}

		public static readonly dynamic SceneHierarchyWindow;

		public static readonly dynamic ProjectBrowser;

		public static readonly dynamic ConsoleWindow;

		public static readonly dynamic GameView;

		public static readonly dynamic PrefabUtility;

		public static readonly dynamic PrefabStageUtility;

		public static readonly dynamic WindowLayout;

		public static readonly dynamic AssetDatabase;

		public static readonly dynamic SearchFilter;

		public static readonly dynamic SearchUtility;

		public static readonly dynamic SceneView;

		public static readonly dynamic AssetPreview;

		public static readonly dynamic ScriptAttributeUtility;



    public static readonly Type ConsoleWindowType;

    public static readonly FieldInfo ConsoleWindow_m_ActiveText;

    public static readonly FieldInfo ConsoleWindow_m_ActiveInstanceID;

    public static readonly FieldInfo consoleWindowFiledInfo;

    static UnityEditorCache()
		{
			UnityEditorAssembly = typeof(UnityEditor.Editor).Assembly;
			EditorUtility = typeof(EditorUtility);
			EditorGUI = typeof(EditorGUI);
			EditorGUIUtility = typeof(EditorGUIUtility);
			EditorWindow = typeof(EditorWindow);


        ConsoleWindowType = UnityEditorCache.UnityEditorAssembly.GetType("UnityEditor.ConsoleWindow", true);
        ConsoleWindow_m_ActiveText = ConsoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
        ConsoleWindow_m_ActiveInstanceID = ConsoleWindowType.GetField("m_ActiveInstanceID", BindingFlags.Instance | BindingFlags.NonPublic);

         consoleWindowFiledInfo = UnityEditorCache.ConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);


        SceneHierarchyWindow = UnityEditorAssembly.GetType("UnityEditor.SceneHierarchyWindow", true);
			ProjectBrowser = UnityEditorAssembly.GetType("UnityEditor.ProjectBrowser", true);
			ConsoleWindow = UnityEditorAssembly.GetType("UnityEditor.ConsoleWindow", true);
			GameView = UnityEditorAssembly.GetType("UnityEditor.GameView", true);
			PrefabUtility = typeof(PrefabUtility);
			PrefabStageUtility = typeof(PrefabStageUtility);
			WindowLayout = UnityEditorAssembly.GetType("UnityEditor.WindowLayout", true);
			AssetDatabase = typeof(AssetDatabase);
			SearchFilter = UnityEditorAssembly.GetType("UnityEditor.SearchFilter", true);
			SearchUtility = UnityEditorAssembly.GetType("UnityEditor.SearchUtility", true);
			SceneView = typeof(SceneView);
			AssetPreview = typeof(AssetPreview);
			ScriptAttributeUtility = UnityEditorAssembly.GetType("UnityEditor.ScriptAttributeUtility", true);
		}

#if UNITY_EDITOR
    //callback function of oepnAssey
    [UnityEditor.Callbacks.OnOpenAssetAttribute(-1)]
    static bool OnOpenAsset(int instance, int line)
    {
        if (DebugTool.m_hasForceMono)
            return false;
        // 自定义函数，用来获取log中的stacktrace，定义在后面。
        string stack_trace = GetStackTrace();

        if (!string.IsNullOrEmpty(stack_trace) && stack_trace.Contains(DebugTool.WriteKeyString))
        {
            // 正则匹配at xxx，在第几行
            Match matches = Regex.Match(stack_trace, @"\(at (.+)\)", RegexOptions.IgnoreCase);
            string pathline = "";
            while (matches.Success)
            {
                pathline = matches.Groups[1].Value;

                // 找到不是我们自定义log文件的那行，重新整理文件路径，手动打开
                if (!pathline.Contains(typeof(DebugTool).Name + ".cs") && !string.IsNullOrEmpty(pathline))
                {
                    int split_index = pathline.LastIndexOf(":");
                    string path = pathline.Substring(0, split_index);
                    line = Convert.ToInt32(pathline.Substring(split_index + 1));
                    DebugTool.m_hasForceMono = true;
                    //方式一
                    UnityEditor.AssetDatabase.OpenAsset(UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path), line);
                    DebugTool.m_hasForceMono = false;
                    //方式二
                    //string fullpath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("Assets"));
                    // fullpath = fullpath + path;
                    //  UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(fullpath.Replace('/', '\\'), line);
                    return true;
                }
                matches = matches.NextMatch();
            }
            return true;
        }
        return false;
    }


    static string GetStackTrace()
    {
        // 找到UnityEditor.ConsoleWindow中的成员ms_ConsoleWindow
        // var filedInfo = UnityEditorCache.ConsoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
        // 获取ms_ConsoleWindow的值
        var ConsoleWindowInstance = UnityEditorCache.consoleWindowFiledInfo.GetValue(null);
        if (ConsoleWindowInstance != null)
        {
            if ((object)UnityEditor.EditorWindow.focusedWindow == ConsoleWindowInstance) //为了防止点击任何资源 都触发检查
            {
                // 找到类UnityEditor.ConsoleWindow中的成员m_ActiveText
                // filedInfo = type_console_window.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                string activeText = UnityEditorCache.ConsoleWindow_m_ActiveText.GetValue(ConsoleWindowInstance).ToString();
                return activeText;
            }
        }
        return null;
    }
#endif
}
