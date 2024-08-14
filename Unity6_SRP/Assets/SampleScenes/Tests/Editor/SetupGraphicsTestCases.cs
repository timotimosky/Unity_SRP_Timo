using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Linq;

public class SetupGraphicsTestCases : IPrebuildSetup
{
    public void Setup()
    {

    }

    // Disable ErrorMaterial scene in build settings. This method is called from the command line before running
    // the build job in Yamato. It is necessary to do so because the ErrorMaterial scene is using shaders with
    // errors that would fail the build job in Yamato. UTR allows to ignore compilation errors
    // but shader errors are error logs, not compilation errors.
    [MenuItem("Tools/Remove ErrorMaterial scene from Build")]
    public static void DisableErrorSceneInBuildProfile()
    {
        string sceneFilename = "ErrorMaterial.unity";

        var newScenes = EditorBuildSettings.scenes;
        var sceneToDisable = newScenes.FirstOrDefault(x => x.path.Contains(sceneFilename));

        if(sceneToDisable != null)
        {
            sceneToDisable.enabled = false;
            EditorBuildSettings.scenes = newScenes;
            UnityEngine.Debug.Log("Scene ErrorMaterial has been disabled in build settings.");

        }
        else
        {
            UnityEngine.Debug.Log("Attempted to disable scene ErrorMaterial but it's not in build settings.");
        }
    }
}
