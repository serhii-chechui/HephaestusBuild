using System.IO;
using System.Xml;
using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Callbacks;

#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

public class XCodePostProcess {
    
    #if UNITY_IOS
    [PostProcessBuild(150)]
    public static void OnPostproccesBuild(BuildTarget buildTarget, string path) {
        if (buildTarget == BuildTarget.iOS) {
            
            var projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";

            var proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));

            var target = proj.GetUnityMainTargetGuid();
            
            proj.SetBuildProperty(target,"CODE_SIGN_STYLE", "Manual");
            proj.SetBuildProperty(target,"CODE_SIGN_IDENTITY", "Apple Distribution: Serhii Chechui (FG899F43PQ)");
            proj.SetBuildProperty(target,"CODE_SIGN_IDENTITY[sdk=iphoneos*]", "Apple Distribution: Serhii Chechui (FG899F43PQ)");
            proj.SetBuildProperty(target,"DEVELOPMENT_TEAM", "FG899F43PQ");
            
            #if DEVELOPMENT_BUILD
            proj.SetBuildProperty(target,"PROVISIONING_PROFILE_SPECIFIER", "Merge Odyssey AdHoc");
            #else
            proj.SetBuildProperty(target,"PROVISIONING_PROFILE_SPECIFIER", "Merge Odyssey Distribution");
            #endif
            
            // proj.SetBuildProperty(target, "USYM_UPLOAD_AUTH_TOKEN", "FakeToken");
            //
            // var unityFrameworkTarget = proj.TargetGuidByName("UnityFramework");
            // proj.SetBuildProperty(unityFrameworkTarget, "USYM_UPLOAD_AUTH_TOKEN", "FakeToken");

            // var targetFramework = proj.GetUnityFrameworkTargetGuid();
            // proj.AddFrameworkToProject(target, "CFNetwork.framework", false);
            // proj.AddFrameworkToProject(target, "MobileCoreServices.framework", false);
            // proj.AddFrameworkToProject(target, "QuartzCore.framework", false);
            // proj.AddFrameworkToProject(target, "Security.framework", false);
            // proj.AddFrameworkToProject(target, "Accounts.framework", false);
            // proj.AddFrameworkToProject(targetFramework, "AuthenticationServices.framework", true);
            
            // proj.SetBuildProperty(target, "COPY_PHASE_STRIP", "YES");
            // proj.SetBuildProperty(target, "DEPLOYMENT_POSTPROCESSING", "YES");
            // proj.SetBuildProperty(target, "DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            //todo update frameworks        
            proj.SetBuildProperty(target, "ENABLE_BITCODE", "NO");

            File.WriteAllText(projPath, proj.WriteToString());
            UpdatePlistFile(buildTarget, path);

            // Dynamic links
            var entitlements = new ProjectCapabilityManager(projPath, "app.entitlements", targetGuid: target);
            // entitlements.AddAssociatedDomains(new string[] { "applinks:gamepoint.page.link", "applinks:wps.onelink.me" });
            // entitlements.AddSignInWithApple();
            
            #if DEVELOPMENT_BUILD
            entitlements.AddPushNotifications(true);
            #else
            entitlements.AddPushNotifications(false);
            #endif
            entitlements.WriteToFile();
        }
    }
    
    private static void UpdatePlistFile(BuildTarget buildTarget, string pathToBuiltProject) {
        
        if (buildTarget == BuildTarget.iOS) {
            
            // Get plist
            var plistPath = pathToBuiltProject + "/Info.plist";
            var plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));
       
            // Get root
            var rootDict = plist.root;
       
            // Change value of CFBundleVersion in Xcode plist
            // var buildKey = "CFBundleVersion";
            // rootDict.SetString(buildKey,"2.3.4");
            
            rootDict.SetBoolean("UIRequiresFullScreen", true);
            
            // Set Background Modes
            var bgModes = rootDict.CreateArray("UIBackgroundModes");
            bgModes.AddString("remote-notification");
            bgModes.AddString("fetch");
            
            // AppUsesNonExemptEncryption
            rootDict.SetString("ITSAppUsesNonExemptEncryption", "NO");
            
            // Device capabilities

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());

            // PListDict ts;
            // if (!xmlDict.ContainsKey("NSAppTransportSecurity")) {
            //     ts = new PListDict();
            //     xmlDict.Add("NSAppTransportSecurity", ts);
            // } else {
            //     ts = (PListDict) xmlDict["NSAppTransportSecurity"];
            // }
            // if (!ts.ContainsKey("NSAllowsArbitraryLoads")) {
            //     ts.Add("NSAllowsArbitraryLoads", true);
            // }

            

            // if (!xmlDict.ContainsKey("CFBundleLocalizations")) {
            //     //Add localizations
            //     string[] localizations = { "en", "fr", "es", "it", "nl", "de", "pt" };
            //     xmlDict.Add("CFBundleLocalizations", localizations);
            // }

            // if (!xmlDict.ContainsKey("View controller-based status bar appearance")) {
            //     xmlDict.Add("View controller-based status bar appearance", "NO");
            // }

            //Add DeviceCapabilities

            

            // if (!xmlDict.ContainsKey("NSCameraUsageDescription")) {
            //     xmlDict.Add("NSCameraUsageDescription", "This lets you take a photo to set as your avatar via your Profile.");
            // }
            //
            // if (!xmlDict.ContainsKey("NSPhotoLibraryUsageDescription")) {
            //     xmlDict.Add("NSPhotoLibraryUsageDescription", "This lets you choose an existing photo as your avatar via your Profile.");
            // }
            //
            // if (!xmlDict.ContainsKey("NSLocationAlwaysUsageDescription")) {
            //     xmlDict.Add("NSLocationAlwaysUsageDescription", "This ensures that your preferred bingo rooms and chats are set to your correct local language.");
            // }
            //
            // if (!xmlDict.ContainsKey("NSLocationWhenInUseUsageDescription")) {
            //     xmlDict.Add("NSLocationWhenInUseUsageDescription", "This ensures that your preferred bingo rooms and chats are set to your correct local language.");
            // }

            // // Corrected header of the plists
            // string publicId = "-//Apple//DTD PLIST 1.0//EN";
            // string stringId = "http://www.apple.com/DTDs/PropertyList-1.0.dtd";
            // string internalSubset = null;
            // XDeclaration declaration = new XDeclaration("1.0", "UTF-8", null);
            // XDocumentType docType = new XDocumentType("plist", publicId, stringId, internalSubset);
            //
            // xmlDict.Save(fullPath, declaration, docType);
        }
    }

#endif
}