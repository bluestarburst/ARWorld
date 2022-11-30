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

    var body: some View {
        ZStack {
            // PassthroughView()
            ColorPicker("", selection: $color)
                .frame(width: 50, height: 50, alignment: .center)
                .onChange(of: color) { newValue in
                    let colorString = "\(newValue)"
                    let arr = colorString.components(separatedBy: " ")
                    if arr.count > 1 {
                        let r = CGFloat(Float(arr[1]) ?? 1)
                        let g = CGFloat(Float(arr[2]) ?? 1)
                        let b = CGFloat(Float(arr[3]) ?? 1)
                        UnityBridge.getInstance().api.setColor(r: r, g: g, b: b)
                    }
                }
                .onAppear {
                    let api = UnityBridge.getInstance()
                    api.show()
                }
        }
    }
    
}

struct UnityView_Previews: PreviewProvider {
    static var previews: some View {
        UnityView()
    }
}
