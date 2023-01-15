//
//  PreviewObj.swift
//  ourworld
//
//  Created by Bryant Hargreaves on 1/15/23.
//

import SwiftUI

struct PreviewObj: View {
    
    @State var minOffset = CGFloat(80)
    @State var offset = CGFloat(1000)
    @State var prevOffset = CGFloat(1000)
    
    @State var url: URL? = nil
    @State var saveUrl: URL? = nil
    @State var user: String = ""
    @State var id: String = ""
    
    @State var type: String = "stickers"
    @State var change = false
    
    func updatePreview(type: String, user: String, id: String, url: URL) {
        self.url = nil
        self.saveUrl = url
        self.user = user
        self.type = type
        self.id = id
        
        withAnimation {
            offset = minOffset
        }
    }
    
    var body: some View {
        VStack {
            Spacer()
            
            VStack {
                HStack {
                    RoundedRectangle(cornerRadius: 10)
                        .frame(width: 30, height: 10)
                        .foregroundColor(.white)
                }
                .padding()
                HStack {
                    if (url != nil) {
                        AsyncImage(
                            url: url!,
                            placeholder: {
                                ProgressView()
                            }
                        )
                        .frame(width: UIScreen.screenWidth/3, height: UIScreen.screenWidth/3)
                        .cornerRadius(16)
                    } else if (saveUrl != nil) {
                        Text("")
                            .onAppear {
                                withAnimation {
                                    self.url = saveUrl
                                    self.saveUrl = nil
                                }
                            }
                    }
                    VStack {
                        Text("title")
                        Text("Uploaded by \(user)")
                        Text("Type: \(type)")
                    }
                }.padding()
                HStack {
                    Button(action: {UnityBridge.getInstance().api.addObject(type: type, user: user, id: id)}, label: {
                        Text("Add \(type)")
                    })
                    .background(.blue)
                }
                Spacer()
            }
            .frame(minHeight: UIScreen.screenHeight/1.25)
            .background(Color(hex: "262626"))
            .cornerRadius(16)
            .offset(x: 0, y: offset)
            .gesture(
                DragGesture()
                    .onChanged { gesture in
                        print(gesture)
                        offset = max(gesture.translation.height + minOffset, 0)
                    }
                    .onEnded { gesture in
                        if (offset > 400 || gesture.predictedEndTranslation.height > 400) {
                            withAnimation {
                                offset = CGFloat(1000)
                            }
                        } else {
                            withAnimation {
                                offset = minOffset
                            }
                        }
                    }
            )
            .transition(.move(edge: .bottom))
        }
        .frame(width: UIScreen.screenWidth)
        .onAppear {
            DataHandler.shared.setPreview = updatePreview
        }
    }
                    
}


extension Color {
    init(hex: String) {
        let hex = hex.trimmingCharacters(in: CharacterSet.alphanumerics.inverted)
        var int: UInt64 = 0
        Scanner(string: hex).scanHexInt64(&int)
        let a, r, g, b: UInt64
        switch hex.count {
        case 3: // RGB (12-bit)
            (a, r, g, b) = (255, (int >> 8) * 17, (int >> 4 & 0xF) * 17, (int & 0xF) * 17)
        case 6: // RGB (24-bit)
            (a, r, g, b) = (255, int >> 16, int >> 8 & 0xFF, int & 0xFF)
        case 8: // ARGB (32-bit)
            (a, r, g, b) = (int >> 24, int >> 16 & 0xFF, int >> 8 & 0xFF, int & 0xFF)
        default:
            (a, r, g, b) = (1, 1, 1, 0)
        }

        self.init(
            .sRGB,
            red: Double(r) / 255,
            green: Double(g) / 255,
            blue:  Double(b) / 255,
            opacity: Double(a) / 255
        )
    }
}
