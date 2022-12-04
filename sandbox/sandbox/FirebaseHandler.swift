

import SwiftUI
import UIKit
//import Firebase
//import FirebaseCore
//import FirebaseFirestore

class DataHandler: NSObject, ObservableObject {
    
    var uid: String?
    
    @ObservedObject static var shared = DataHandler()
    
    override init() {
        super.init()
        self.getUID()
    }
    
    func getUID() {
//        guard let uid = Auth.auth().currentUser?.uid else { return }
//        self.uid = uid
    }
}
