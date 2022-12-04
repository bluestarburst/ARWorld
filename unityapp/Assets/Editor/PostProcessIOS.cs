using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
//add import for monobehaviour
using UnityEngine;


public class PostProcessIOS : MonoBehaviour
{
    [PostProcessBuildAttribute(0)]//must be between 40 and 50 to ensure that it's not overriden by Podfile generation (40) and that it's added before "pod install" (50)
    private static void PostProcessBuild_iOS(BuildTarget target, string buildPath)
    {
        if (target == BuildTarget.iOS)
        {

            using (StreamWriter sw = File.AppendText(buildPath + "/Podfile"))
            {
                //in this example I'm adding an app extension
                sw.WriteLine("source 'https://cdn.cocoapods.org/'\nplatform :ios, '12.0'\nworkspace 'unitysandbox'\nproject '../sandbox/sandbox.xcodeproj'\nproject 'Unity-Iphone.xcodeproj'\ndef sharedpod\nuse_frameworks! :linkage => :dynamic\npod 'Firebase/Analytics', '10.1.0'pod 'Firebase/Auth', '10.1.0'\npod 'Firebase/Core', '10.1.0'\n  pod 'Firebase/Firestore', '10.1.0'\nend\ntarget 'UnityFramework' do\n  project 'Unity-Iphone.xcodeproj'\n  sharedpod\nend\ntarget 'Unity-iPhone' do\nend\ntarget 'sandbox' do\n  project '../sandbox/sandbox.xcodeproj'\n  sharedpod\nend\nuse_frameworks! :linkage => :dynamic");
            }
        }
    }
}