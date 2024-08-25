using UnityEditor;
using UnityEditor.Build;

public class EnableLogWindow : EditorWindow
{
    private static string def = "EnableLog";

    [MenuItem("自动检测工具/EnableLog")]
    static void EnableLog()
    {
        string symbol = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
        if (symbol.Equals(string.Empty))
        {
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, def);
        }
        else if (!symbol.Contains(def))
        {
            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, symbol + ";" + def);
        }
        AssetDatabase.Refresh();
    }

    [MenuItem("自动检测工具/DisableLog")]
    static void DisableLog()
    {
        string symbol = PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone);
        int _index = symbol.IndexOf(def);
        if (_index < 0)
        {
            return;
        }
        if (_index > 0)//if not first,  delete "；"
        {
            _index -= 1;
        }
        int _length = def.Length;
        if (symbol.Length > _length)//if > cur  _length，reserve "；"
        {
            _length += 1;
        }
        symbol = symbol.Remove(_index, _length);
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, symbol);
        AssetDatabase.Refresh();
    }

}