using System.IO;
using UnityEditor;

namespace Unity.VisualScripting
{
    public static class EditorDebugity
    {
        internal static void DeleteDebugLogFile()
        {
            if (File.Exists(Debugity.logPath))
            {
                File.Delete(Debugity.logPath);
            }
        }
    }
}
