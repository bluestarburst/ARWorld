/// Serialized structure sent to Unity.
///
/// This is used on the Unity side to decide what to do when a message
/// arrives.
struct MessageWithData<T: Encodable>: Encodable {
    var type: String
    var data: T
}

/// Swift API to handle Native <> Unity communication.
///
/// - Note:
///   - Message passing is done via serialized JSON
///   - Message passing is done via function pointer exchanged between Unity <> Native
public class UnityAPI: NativeCallsProtocol {

    // Name of the gameobject that receives the
    // messages from the native side.
    private static let API_GAMEOBJECT = "APIEntryPoint"
    // Name of the method to call when sending
    // messages from the native side.
    private static let API_MESSAGE_FUNCTION = "ReceiveMessage"

    public weak var communicator: UnityCommunicationProtocol!
    public var ready: () -> () = {}
    public var getPhoneResult: (String) -> Void = {_ in}

    /**
        Function pointers to static functions declared in Unity
     */

    private var testCallback: TestDelegate!

    public init() {}

    /**
     * Public API for developers.
     */

    /// Friendly wrapper arround the message passing system.
    ///
    /// - Note:
    /// This wrapper is used to get friendlier API for Swift developers.
    /// They shouldn't have to care about how the color is sent to Unity.
    public func setColor(r: CGFloat, g: CGFloat, b: CGFloat) {
        let data = [r, g, b]
        sendMessage(type: "change-color", data: data)
    }

    public func saveMap() {
        sendMessage(type: "save-map", data: "")
    }

    public func loadMap() {
        sendMessage(type: "load-map", data: "")
    }

    public func updateVars(lat: CGFloat, lon: CGFloat, alt: CGFloat) {
        sendMessage(type: "update-vars", data: [lat, lon, alt])
    }
    
    public func phoneLogin(cc: String, ph: String) {
        sendMessage(type: "phone-login", data: [cc,ph])
    }

    public func addObject(type: String, user: String, id: String) {
        sendMessage(type: "add-object", data: [type, user, id])
    }

    public func changeTransform(change: String) {
        sendMessage(type: "change-transform", data: change)
    }

    public func changeSettings(change: String) {
        sendMessage(type: "change-settings", data: change)
    }

    public func changeRadius(top: CGFloat, bottom: CGFloat, r: CGFloat = CGFloat(255), g: CGFloat = CGFloat(255), b: CGFloat = CGFloat(255)) {
        sendMessage(type: "change-radius", data: [top, bottom, r, g, b])
    }

    public func changeFilter(r: CGFloat, g: CGFloat, b: CGFloat, saturation: CGFloat, threshold: CGFloat, isColor: CGFloat, contrast: CGFloat = CGFloat(1), hue: CGFloat = CGFloat(0)) {
        sendMessage(type: "change-filter", data: [r,g,b,saturation,threshold,isColor,contrast,hue])
    }

    public func deleteObj(type: String, id: String, chunkId: String) {
        sendMessage(type: "delete-obj", data: [type, id, chunkId])
    }

    public func nextStepFilter() {
        sendMessage(type: "next-step-filter", data: "")
    }

    public func takePic() {
        sendMessage(type: "take-pic", data: "")
    }

    public func takeVideo(stop: Bool) {
        sendMessage(type: "take-video", data: stop ? "stop" : "start")
    }

    public func test(_ value: String) {
        self.testCallback(value)
    }

    /**
     * Internal API.
     */

    public func onUnityStateChange(_ state: String) {
        switch (state) {
        case "ready":
            self.ready()
        default:
            return
        }
    }

    public func onSetTestDelegate(_ delegate: TestDelegate!) {
        self.testCallback = delegate
    }

    public var setMapStatus: (String) -> Void = {_ in}
    
    public func onMapStatus(_ status: String) {
        print("onMapStatus: \(status)")
        setMapStatus(status)
    }
    
    public var setAddingObj: (String) -> Void = {_ in}

    public func onAddingObj(_ status: String) {
        print("onAddingObj: \(status)")
        setAddingObj(status)
    }

    public var setElementOptions: (String,String,String,String,String,String) -> Void = {_,_,_,_,_,_ in}

    public func onElementOptions(_ type: String, _ id: String, _ chunkId: String, _ storageId: String, _ user: String, _ createdBy: String) {
        // print("onElementOptions: \(type), \(user), \(id)")
        print("onElementOptions: \(type), \(user), \(id), \(chunkId), \(storageId), \(createdBy)")
        setElementOptions(type,id,chunkId,storageId,user,createdBy)
    }

    public var setDataPath: (String) -> Void = {_ in}

    public func onSetPersistentDataPath(_ path: String) {
        print("swfty path onSetPersistentDataPath: \(path)")
        setDataPath(path)
    }

    public var onSetLoadingMap: (String) -> Void = {_ in}

    public func onLoadingMap(_ mapID: String) {
        print("onLoadingMap: \(mapID)")
        onLoadingMap(mapID)
    }

    public var onSetScreenshot: (String) -> Void = {_ in}

    public func onScreenshot(_ status: String) {
        print("onScreenshot: \(status)")
        onSetScreenshot(status)
    }

    /**
     * Private  API.
     */

    /// Internal function sending message to Unity.
    private func sendMessage<T: Encodable>(type: String, data: T) {
        let message = MessageWithData(type: type, data: data)
        let encoder = JSONEncoder()
        let json = try! encoder.encode(message)
        communicator.sendMessageToGameObject(
            go: UnityAPI.API_GAMEOBJECT,
            function: UnityAPI.API_MESSAGE_FUNCTION,
            message: String(data: json, encoding: .utf8)!
        )
    }
}
