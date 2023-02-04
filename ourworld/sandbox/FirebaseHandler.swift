

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
//    let storage = Storage.storage(app: FirebaseApp.app(name: "swift")!)
    let storage = Storage.storage()
    
    @ObservedObject static var shared = DataHandler()
    
    var mapStatus = ""
    var addingObj = ""
    var setAddingObj: () -> Void = {}
    var setMapStatus: () -> Void = {}
    
    var setPreview: (_ type: String, _ user: String, _ id: String, _ url: URL) -> Void = {_,_,_,_ in }
    
    override init() {
        super.init()

        self.getUID()
        let api = UnityBridge.getInstance().api
        
        api.setMapStatus = { status in
            self.mapStatus = status
            self.setMapStatus()
        }
        api.setAddingObj = { status in
            self.addingObj = status
            self.setAddingObj()
        }
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
    
    func storeImg(img: UIImage, type: String = "posters") {
        var doc = db.collection(type).document()
        let id = doc.documentID
        
        doc.setData([
            "user": (self.uid ?? ""),
            "type": type,
            "creations": 0,
            "id": id,
            "timestamp": Timestamp()
        ])
        
        var userDoc = db.collection("users").document(self.uid ?? "").collection(type).document(id)
        
        userDoc.setData([
            "user": (self.uid ?? ""),
            "type": type,
            "creations": 0,
            "id": id,
            "timestamp": Timestamp()
        ])
        
        let storageRef = storage.reference().child("users/" + (self.uid ?? "") + "/" + type + "/" + id + ".jpg")
        
        let metadata = StorageMetadata()
        metadata.contentType = "image/jpg"
        
//        var temp = img.resized(toWidth: 500)
        
//        let data = img.compress(to: 300)
        let data = img.jpegData(compressionQuality: 0.15)
        
        if let data = data {
            storageRef.putData(data, metadata: metadata) { (metadata, error) in
                if let error = error {
                    print("Error while uploading file: ", error)
                }
                
                if let metadata = metadata {
                    print("Metadata: ", metadata)
                    self.getUserPosters(type: self.posterType)
                }
            }
        }
        
    }
    
    func documentDirectoryPath() -> URL? {
        let path = FileManager.default.urls(for: .documentDirectory,
                                            in: .userDomainMask)
        return path.first
    }
    
    var posterType = "stickers"
    
    var addNextUserPoster: (_ user: String, _ id: String, _ url: URL) -> Void = {_,_,_  in}
    
    func getUserPosters(type: String, fav: Bool = false) {
        self.posterType = type
        if (type != self.lastType) {
            lastDoc = nil
        }
        self.lastType = type
        print("swifty " + type)
        db.collection("users").document(self.uid ?? "").collection(fav ? "f" + type : type).limit(to: 25).getDocuments { (querySnapshot, err) in
            if let err = err {
                print("error getting documents: \(err)")
            } else {
                for document in querySnapshot!.documents {
                    self.lastDoc = document
                    print("\(document.documentID) => \(document.data())")
                    let usr = document.data()["user"] ?? ""
                    print(usr)
                    self.getFromStorage(user: usr as! String, id: document.documentID, type: type, self.addNextUserPoster)
                }
            }
        }
    }
    
    var addNextUserObj: (_ user: String, _ id: String, _ url: URL) -> Void = {_,_,_  in}
    
    func getUserObjects(type: String, fav: Bool = false) {
        if (type != self.lastType) {
            lastDoc = nil
        }
        self.lastType = type
        print("swifty " + type)
        db.collection("users").document(self.uid ?? "").collection(fav ? "f" + type : type).limit(to: 25).getDocuments { (querySnapshot, err) in
            if let err = err {
                print("error getting documents: \(err)")
            } else {
                for document in querySnapshot!.documents {
                    self.lastDoc = document
                    print("\(document.documentID) => \(document.data())")
                    let usr = document.data()["user"] ?? ""
                    print(usr)
                    self.getFromStorage(user: usr as! String, id: document.documentID, type: type, self.addNextUserObj)
                }
            }
        }
    }
    
    
    
    func readImage(id: String) -> Bool {
        if let path = documentDirectoryPath() {
            let jpgImageURL = path.appendingPathComponent(id + ".jpg")
            
            let jpgImage = FileManager.default.contents(atPath: jpgImageURL.path)
            
            if (jpgImage != nil) {
//                self.posters[id] = jpgImageURL
                return true
            }
            
            print(jpgImage)
        }
        return false
    }
    
    var lastDoc: DocumentSnapshot? = nil
    var lastType: String? = nil
    
    var addNextTop: (_ user: String, _ id: String, _ url: URL) -> Void = {_,_,_  in}
    
    func getTopThumbs(type: String) {
        if (type != self.lastType) {
            lastDoc = nil
        }
        self.lastType = type
        print("swifty " + type)
        db.collection(type).order(by: "creations").limit(to: 25).getDocuments { (querySnapshot, err) in
            if let err = err {
                print("error getting documents: \(err)")
            } else {
                for document in querySnapshot!.documents {
                    self.lastDoc = document
                    print("\(document.documentID) => \(document.data())")
                    let usr = document.data()["user"] ?? ""
                    print(usr)
                    self.getFromStorage(user: usr as! String, id: document.documentID, type: type, self.addNextTop)
                }
            }
        }
    }
    
    func getFromStorage(user: String, id: String, type: String, _ addToArr: @escaping ((String,String,URL) -> Void)) {
        let storageRef = self.storage.reference().child("users/" + user + "/" + type + "/" + id + ".jpg")
        storageRef.downloadURL(completion: { url, error in
            guard let url = url, error == nil else {
                let storageRef2 = self.storage.reference().child("users/" + user + "/" + type + "/" + id + ".png")
                storageRef2.downloadURL(completion: { url2, error2 in
                    guard let url2 = url2, error2 == nil else {
                        return
                    }
                    addToArr(user,id,url2)
                })
                return
            }
            addToArr(user,id,url)
        })
    }
    
    func getURL(user: String, id:String, type: String, _ setURL: @escaping ((URL) -> Void)) {
        let storageRef = self.storage.reference().child("users/" + user + "/" + type + "/" + id + ".jpg")
        storageRef.downloadURL(completion: { url, error in
            guard let url = url, error == nil else {
                let storageRef2 = self.storage.reference().child("users/" + user + "/" + type + "/" + id + ".png")
                storageRef2.downloadURL(completion: { url2, error2 in
                    guard let url2 = url2, error2 == nil else {
                        return
                    }
                    setURL(url2)
                })
                return
            }
            setURL(url)
        })
    }
    
    func getPrevData(type: String, user: String, id: String, _ completion: @escaping ((String,Int,Bool) -> Void)) {
        db.collection("users").document(self.uid!).collection("f" + type).document(id).getDocument { (documento, erroro) in
            var fav = false
            if let documento = documento, documento.exists {
                fav = true
            }
            
            self.db.collection(type).document(id).getDocument { (document, error) in
                if let document = document, document.exists {
                    let dat = document.data()
                    completion(dat!["name"] as? String ?? "",dat!["creations"] as? Int ?? 0,fav)
                }
            }
        }
    }
    
    func favElem(type: String, id: String) {
        db.collection(type).document(id).getDocument { (document, error) in
            if let document = document, document.exists {
                var dat = document.data()
                if ((dat) != nil) {
                    dat!["timestamp"] = Timestamp()
                    self.db.collection("users").document(self.uid!).collection("f" + type).document(id).setData(dat!)
                }
            }
        }
    }
    
    func unfavElem(type: String, id: String) {
        db.collection("users").document(self.uid!).collection("f" + type).document(id).getDocument { (document, error) in
            if let document = document, document.exists {
                self.db.collection("users").document(self.uid!).collection("f" + type).document(id).delete()
            }
        }
    }
    
    
}

extension UIImage {

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

    func resized(withPercentage percentage: CGFloat, isOpaque: Bool = true) -> UIImage? {
        let canvas = CGSize(width: size.width * percentage, height: size.height * percentage)
        let format = imageRendererFormat
        format.opaque = isOpaque
        return UIGraphicsImageRenderer(size: canvas, format: format).image {
            _ in draw(in: CGRect(origin: .zero, size: canvas))
        }
    }

    func compress(to kb: Int, allowedMargin: CGFloat = 0.2) -> Data {
        let bytes = kb * 1024
        var compression: CGFloat = 1.0
        let step: CGFloat = 0.05
        var holderImage = self
        var complete = false
        while(!complete) {
            if let data = holderImage.jpegData(compressionQuality: 1.0) {
                let ratio = data.count / bytes
                if data.count < Int(CGFloat(bytes) * (1 + allowedMargin)) {
                    complete = true
                    return data
                } else {
                    let multiplier:CGFloat = CGFloat((ratio / 5) + 1)
                    compression -= (step * multiplier)
                }
            }
            
            guard let newImage = holderImage.resized(withPercentage: compression) else { break }
            holderImage = newImage
        }
        return Data()
    }
}

