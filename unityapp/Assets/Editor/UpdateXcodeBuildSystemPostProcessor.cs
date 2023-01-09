using System.IO;

using UnityEngine;

using UnityEditor;

using UnityEditor.Callbacks;

using UnityEditor.iOS.Xcode;



#if UNITY_IOS || UNITY_TVOS

// Create specific aliases for iOS.Xcode imports.

// Unity Editor on macOS can report a conflict with other plugins

using PlistDocument = UnityEditor.iOS.Xcode.PlistDocument;

using PlistElementDict = UnityEditor.iOS.Xcode.PlistElementDict;

#endif



public class UpdateXcodeBuildSystemPostProcessor : MonoBehaviour

{

    [PostProcessBuild(999)]

    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)

    {

        if (buildTarget == BuildTarget.iOS || buildTarget == BuildTarget.tvOS)

        {

            UpdateXcodeBuildSystem(path);

        }

    }



    private static void UpdateXcodeBuildSystem(string projectPath)

    {

        string workspaceSettingsPath = Path.Combine(projectPath,

            "Unity-iPhone.xcodeproj/project.xcworkspace/xcshareddata/" +

            "WorkspaceSettings.xcsettings");



        if (File.Exists(workspaceSettingsPath))

        {

            // Read the plist document, and find the root element

            PlistDocument workspaceSettings = new PlistDocument();

            workspaceSettings.ReadFromFile(workspaceSettingsPath);

            PlistElementDict root = workspaceSettings.root;



            // Modify the document as necessary.

            bool workspaceSettingsChanged = false;

            // Remove the BuildSystemType entry because it specifies the

            // legacy Xcode build system, which is deprecated



            if (root.values.ContainsKey("BuildSystemType"))

            {

                root.values.Remove("BuildSystemType");

                workspaceSettingsChanged = true;

            }



            // If actual changes to the document occurred, write the result

            // back to disk.

            if (workspaceSettingsChanged)

            {

                Debug.Log("UpdateXcodeBuildSystem: Writing updated " + 

                    "workspace settings to disk.");



                try

                {

                    workspaceSettings.WriteToFile(workspaceSettingsPath);

                }

                catch (System.Exception e)

                {

                    Debug.LogError(string.Format("UpdateXcodeBuildSystem: " +

                        "Exception occurred writing workspace settings to " +

                        "disk: \n{0}",

                        e.Message));

                    throw;

                }

            }

            else

            {

                Debug.Log("UpdateXcodeBuildSystem: workspace settings did " +

                    "not require modifications.");

            }

        }

        else

        {

            Debug.LogWarningFormat("UpdateXcodeBuildSystem: could not find " +

                "workspace settings files [{0}]",

                workspaceSettingsPath);

        }



        // Get the path to the Xcode project

        string pbxProjectPath = PBXProject.GetPBXProjectPath(projectPath);

        var pbxProject = new PBXProject();



        // Open the Xcode project

        pbxProject.ReadFromFile(pbxProjectPath);



        // Get the UnityFramework target GUID

        string unityFrameworkTargetGuid =

            pbxProject.GetUnityFrameworkTargetGuid();



        // Modify the Swift version in the UnityFramework target to a

        // compatible string

        pbxProject.SetBuildProperty(unityFrameworkTargetGuid,

            "SWIFT_VERSION", "5.0");



        // Write out the Xcode project

        pbxProject.WriteToFile(pbxProjectPath);



        Debug.Log("UpdateXcodeBuildSystem: update Swift version in Xcode " +

            "project.");

    }

}