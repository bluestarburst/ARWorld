import UIKit
import Firebase
import FirebaseCore

@main
class AppDelegate: NSObject, UIApplicationDelegate {
    
    func application(_ application: UIApplication, didFinishLaunchingWithOptions launchOptions: [UIApplication.LaunchOptionsKey: Any]?) -> Bool {
        // Override point for customization after application launch.
        FirebaseApp.configure()
        FirebaseApp.configure(name: "swift", options: FirebaseApp.app()!.options)
        
        do {
//            try Auth.auth().useUserAccessGroup("group.inkobako.ourworld")
            try Auth.auth().useUserAccessGroup("6254VA6855.com.inkobako.ourworld")
        } catch let error as NSError {
            print("ERROR ACCESS GROUP: %@", error)
        }
        
        do {
//            try Auth.auth(app: FirebaseApp.app(name: "unity")!).useUserAccessGroup("group.inkobako.ourworld")
            try Auth.auth(app: FirebaseApp.app(name: "swift")!).useUserAccessGroup("6254VA6855.com.inkobako.ourworld")
        } catch let error as NSError {
            print("ERROR ACCESS GROUP: %@", error)
        }
        
        return true
    }
    
    func application(_ application: UIApplication, configurationForConnecting connectingSceneSession: UISceneSession, options: UIScene.ConnectionOptions) -> UISceneConfiguration {
        let sceneConfig = UISceneConfiguration(name: nil, sessionRole: connectingSceneSession.role)
        sceneConfig.delegateClass = SceneDelegate.self
        return sceneConfig
    }
}
