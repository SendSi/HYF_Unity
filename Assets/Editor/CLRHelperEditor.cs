using System.IO;
using HybridCLR.Editor.Settings;
using UnityEditor;

/// <summary>
/// 
/// </summary>
public class CLRHelperEditor
{
    [MenuItem("HybridCLR/Generate/All_Copy_replace_dlls_to_bytes",false,5600)] //打了热更后  替换
    public static void CopyDllBytes()
    {
        var target = EditorUserBuildSettings.activeBuildTarget;
        var fromDir = Path.Combine(HybridCLRSettings.Instance.strippedAOTDllOutputRootDir, target.ToString());
        var toDir = "Assets/GameResHotFix";

        if (Directory.Exists(toDir))
        {
            Directory.Delete(toDir, true);
        }

        Directory.CreateDirectory(toDir);
        AssetDatabase.Refresh();
        //aot
        var list = AOTGenericReferences.PatchedAOTAssemblyList;
        foreach (var ite in list)
        {
            File.Copy(Path.Combine(fromDir,ite),Path.Combine(toDir, $"{ite}.bytes"), true);
        }
        //热更
        var hotUpdate = "HotUpdate.dll";
        var fromDirHot=Path.Combine(HybridCLRSettings.Instance.hotUpdateDllCompileOutputRootDir,target.ToString());
        File.Copy(Path.Combine(fromDirHot,hotUpdate),Path.Combine(toDir,$"{hotUpdate}.bytes"),true);

        AssetDatabase.Refresh();
    }
}