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

    public func saveMap(lat: CGFloat, lon: CGFloat, alt: CGFloat) {
        sendMessage(type: "save-map", data: [lat, lon, alt]])
    }

    public func loadMap() {
        sendMessage(type: "load-map", data: true)
    }
    
    public func phoneLogin(cc: String, ph: String) {
        sendMessage(type: "phone-login", data: [cc,ph])
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

    public func onSaveMap(_ map: String) {
        // self.communicator.saveMap(map)
        print("onSaveMap: \(map)")
    }

    public func onPhoneResponse(_ response: String) {
        print("onPhoneResponse: \(response)")
        self.getPhoneResult(response)
    }

    // public func saveARWorldMap(_ data: T) {
    //     // let message = MessageWithData(type: "save-ar-world-map", data: data)
    //     // let encoder = JSONEncoder()
    //     // let jsonData = try! encoder.encode(message)
    //     // let jsonString = String(data: jsonData, encoding: .utf8)!
    //     // self.communicator.sendMessage(jsonString)
    // }

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
