

import SwiftUI
import UIKit
import Firebase
import FirebaseStorage
import FirebaseAuth
import FirebaseCore
import FirebaseFirestore

class DataHandler: NSObject, ObservableObject {
    
    var uid: String?
    let db = Firestore.firestore(app: FirebaseApp.app(name: "swift")!)
    let storage = Storage.storage(app: FirebaseApp.app(name: "swift")!)
    
    @ObservedObject static var shared = DataHandler()
    
    override init() {
        super.init()

        self.getUID()
        
        
    }
    
    func getUID() {
        guard let uid = Auth.auth().currentUser?.uid else { return }
        
        self.uid = uid
        tryStore()
    }
    
    func tryStore() {
        let docRef = db.collection("users").document(self.uid ?? "")
        
        print("SWIFT THING")
        
        docRef.getDocument{ (document, error) in
            
            if let document = document, document.exists {
                let dataDescription = document.data().map(String.init(describing:)) ?? "nil"
                print("Document Data: \(dataDescription)")
            } else {
                print("Document does not exist")
                docRef.setData([
                    "username": "test",
                    "created": Timestamp()
                ], merge: true)
            }
        }
    }
    
    func signOut() {
        do {
            try Auth.auth().signOut()
        } catch let error as NSError {
            print("SIGNED OUT")
        }
    }
    
    func storeImg(img: UIImage) {
        var doc = db.collection("posters").document()
        let id = doc.documentID
        
        doc.setData([
            "user": (self.uid ?? ""),
            "type": "poster"
        ])
        
        var userDoc = db.collection("users").document(self.uid ?? "").collection("posters").document(id)
        
        userDoc.setData([
            "user": (self.uid ?? ""),
            "type": "poster"
        ])
        
        let storageRef = storage.reference().child("users/" + (self.uid ?? "") + "/posters/" + id + ".jpg")
        
        let metadata = StorageMetadata()
        metadata.contentType = "image/jpg"
        
        var temp = img.resized(toWidth: 200)
        
        let data = temp!.jpegData(compressionQuality: 0.1)
//        self.saveReusableImg(data: data, id: id)
        
        if let data = data {
            storageRef.putData(data, metadata: metadata) { (metadata, error) in
                if let error = error {
                    print("Error while uploading file: ", error)
                }
                
                if let metadata = metadata {
                    print("Metadata: ", metadata)
                    self.getUserPosters()
                }
            }
        }
        
    }
    
    func saveReusableImg(data: Data?, id: String) {
        let path = documentDirectoryPath()?.appendingPathComponent(id + ".jpg")
        try? data!.write(to: path!)
    }
    
    func documentDirectoryPath() -> URL? {
        let path = FileManager.default.urls(for: .documentDirectory,
                                            in: .userDomainMask)
        return path.first
    }
    
    var updatePosters: () -> Void = {}
    var posters: [String: URL] = [:]
    var posterData: [String: String] = [:]
    
    func getUserPosters() {
        db.collection("users").document(self.uid ?? "").collection("posters").getDocuments { (querySnapshot, err) in
            if let err = err {
                print("error getting documents: \(err)")
            } else {
                self.posters = [:]
                self.posterData = [:]
                for document in querySnapshot!.documents {
                    print("\(document.documentID) => \(document.data())")
                    
//                    if (self.readImage(id: document.documentID) == true) {
//
//                        continue
//                    }
                    
                    self.posterData[document.documentID] = self.uid ?? ""
                    
                    let storageRef = self.storage.reference().child("users/" + (self.uid ?? "") + "/posters/" + document.documentID + ".jpg")
                    storageRef.downloadURL(completion: { url, error in
                        guard let url = url, error == nil else {
                            let storageRef2 = self.storage.reference().child("users/" + (self.uid ?? "") + "/posters/" + document.documentID + ".png")
                            storageRef2.downloadURL(completion: { url2, error2 in
                                guard let url2 = url2, error2 == nil else {
                                    return
                                }
                                self.posters[document.documentID] = url2
                                self.updatePosters()
                            })
                            return
                        }
                        self.posters[document.documentID] = url
                        self.updatePosters()
                    })
                    
                }
            }
        }
    }
    
    func readImage(id: String) -> Bool {
        if let path = documentDirectoryPath() {
            let jpgImageURL = path.appendingPathComponent(id + ".jpg")
            
            let jpgImage = FileManager.default.contents(atPath: jpgImageURL.path)
            
            if (jpgImage != nil) {
                self.posters[id] = jpgImageURL
                return true
            }
            
            print(jpgImage)
        }
        return false
    }
    
    
}

extension UIImage {
    func resized(withPercentage percentage: CGFloat, isOpaque: Bool = true) -> UIImage? {
        let canvas = CGSize(width: size.width * percentage, height: size.height * percentage)
        let format = imageRendererFormat
        format.opaque = isOpaque
        return UIGraphicsImageRenderer(size: canvas, format: format).image {
            _ in draw(in: CGRect(origin: .zero, size: canvas))
        }
    }
    func resized(toWidth width: CGFloat, isOpaque: Bool = true) -> UIImage? {
        let canvas = CGSize(width: width, height: CGFloat(ceil(width/size.width * size.height)))
        let format = imageRendererFormat
        format.opaque = isOpaque
        return UIGraphicsImageRenderer(size: canvas, format: format).image {
            _ in draw(in: CGRect(origin: .zero, size: canvas))
        }
    }
    
    func aspectFittedToHeight(_ newHeight: CGFloat) -> UIImage {
        let scale = newHeight / self.size.height
        let newWidth = self.size.width * scale
        let newSize = CGSize(width: newWidth, height: newHeight)
        let renderer = UIGraphicsImageRenderer(size: newSize)

        return renderer.image { _ in
            self.draw(in: CGRect(origin: .zero, size: newSize))
        }
    }
}

