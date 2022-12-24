//
//  UnityView.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 11/29/22.
//

import SwiftUI

struct UnityView: View {
    @State private var color = Color(
        .sRGB,
        red: 0.98, green: 0.9, blue: 0.2)
    
    @Binding var isLoaded: Bool
    @StateObject var manager = LocationManager()

    var body: some View {
        ZStack {
            // PassthroughView()
            VStack {
                Spacer()
                Button(action: {
                    print(manager.altitude)
                }, label: {
                    Text("get region")
                        .fontWeight(.bold)
                        .foregroundColor(.white)
                        .padding(.vertical)
                        .frame(maxWidth: .infinity)
                        .background(Color.pink)
                        .cornerRadius(8)
                })
                Button(action: {
                    UnityBridge.getInstance().api.loadMap(lat: manager.latitude, lon: manager.longitude, alt: manager.altitude)
                }, label: {
                    Text("load")
                        .fontWeight(.bold)
                        .foregroundColor(.white)
                        .padding(.vertical)
                        .frame(maxWidth: .infinity)
                        .background(Color.pink)
                        .cornerRadius(8)
                })
                Button(action: {
                    UnityBridge.getInstance().api.saveMap(lat: manager.latitude, lon: manager.longitude, alt: manager.altitude)
                }, label: {
                    Text("save")
                        .fontWeight(.bold)
                        .foregroundColor(.white)
                        .padding(.vertical)
                        .frame(maxWidth: .infinity)
                        .background(Color.pink)
                        .cornerRadius(8)
                })
            }
        
//            ColorPicker("", selection: $color)
//                .frame(width: 50, height: 50, alignment: .center)
//                .onChange(of: color) { newValue in
//                    let colorString = "\(newValue)"
//                    let arr = colorString.components(separatedBy: " ")
//                    if arr.count > 1 {
//                        let r = CGFloat(Float(arr[1]) ?? 1)
//                        let g = CGFloat(Float(arr[2]) ?? 1)
//                        let b = CGFloat(Float(arr[3]) ?? 1)
//                        UnityBridge.getInstance().api.setColor(r: r, g: g, b: b)
//                    }
            
        }
        .padding()
        .onAppear {
            let api = UnityBridge.getInstance()
            api.show()
            api.updateVars(lat: manager.latitude, lon: manager.longitude, alt: manager.altitude)
        }
        .onChange(of: isLoaded) {
            let api = UnityBridge.getInstance()
            if ($0 == false) {
                api.unload()
            } else {
                api.show()
            }
        }
    }
    
}

//struct UnityView_Previews: PreviewProvider {
//    static var previews: some View {
//        UnityView()
//    }
//}
