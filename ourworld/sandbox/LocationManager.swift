import MapKit
import CoreLocation
import CoreMotion

class LocationManager: NSObject,CLLocationManagerDelegate, ObservableObject {
    @Published var region = MKCoordinateRegion()
    @Published var altitude = 0.0
    @Published var latitude = 0.0
    @Published var longitude = 0.0
    
    var accuracy = 1000.0
    var altAcc = 1000.0
    
    @Published var sendDat: () -> Void = {}
    
    private let manager = CLLocationManager()
    
    private let altimeter = CMAltimeter()
    
    override init() {
        super.init()
        manager.delegate = self
        manager.desiredAccuracy = kCLLocationAccuracyBest
        manager.requestWhenInUseAuthorization()
        manager.startUpdatingLocation()
        
        if (CMAltimeter.isAbsoluteAltitudeAvailable()) {
            altimeter.startAbsoluteAltitudeUpdates(to: OperationQueue.main, withHandler: {(data, error) in
                self.altitude = data?.altitude ?? 0.0
                self.altAcc = data?.accuracy ?? 1000.0
            })
        }
    }
    
    func locationManager(_ manager: CLLocationManager, didUpdateLocations locations: [CLLocation]) {
            locations.last.map {
                accuracy = $0.horizontalAccuracy
                region = MKCoordinateRegion(
                    center: CLLocationCoordinate2D(latitude: $0.coordinate.latitude, longitude: $0.coordinate.longitude),
                    span: MKCoordinateSpan(latitudeDelta: 0.5, longitudeDelta: 0.5)
                )
                
                latitude = $0.coordinate.latitude
                longitude = $0.coordinate.longitude
                
                if ($0.verticalAccuracy < altAcc) {
                    altitude = $0.altitude
                    altAcc = $0.verticalAccuracy
                }
                
                if (accuracy < 35 && altAcc < 25) {
//                    print("Swifty up:" + String(latitude) + "   " + String(longitude))
                    sendDat()
                }
                    
//                print("Swifty down:" + String(accuracy) + "   " + String(altAcc))
                
            }
        }
}
