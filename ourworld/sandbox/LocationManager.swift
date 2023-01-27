import MapKit
import CoreLocation

class LocationManager: NSObject,CLLocationManagerDelegate, ObservableObject {
    @Published var region = MKCoordinateRegion()
    @Published var altitude = 0.0
    @Published var latitude = 0.0
    @Published var longitude = 0.0
    
    var accuracy = 0.0
    
    @Published var sendDat: () -> Void = {}
    
    private let manager = CLLocationManager()
    
    override init() {
            super.init()
            manager.delegate = self
            manager.desiredAccuracy = kCLLocationAccuracyBest
            manager.requestWhenInUseAuthorization()
            manager.startUpdatingLocation()
        }
    
    func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation]) {
            locations.last.map {
                accuracy = $0.horizontalAccuracy
                region = MKCoordinateRegion(
                    center: CLLocationCoordinate2D(latitude: $0.coordinate.latitude, longitude: $0.coordinate.longitude),
                    span: MKCoordinateSpan(latitudeDelta: 0.5, longitudeDelta: 0.5)
                )
                altitude = $0.altitude
                latitude = $0.coordinate.latitude
                longitude = $0.coordinate.longitude
                
                if (accuracy < 25) {
                    sendDat()
                }
                print("Swifty acc:" + String(accuracy))
            }
        }
}
