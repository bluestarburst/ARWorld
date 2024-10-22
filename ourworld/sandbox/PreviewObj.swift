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
    
    @State var displayed = false
    
    @State var url: URL? = nil
    @State var saveUrl: URL? = nil
    @State var user: String = ""
    @State var id: String = ""
    
    @State var type: String = "stickers"
    @State var change = false
    
    @State var title: String = "title"
    @State var creations: Int = 0
    
    @Binding var disabled: Bool
    
    @State var fav: Bool = false
    @State var canFav: Bool = true
    
    @State var showImg: Bool = true
    
    @State var isOwn: Bool = false
    
    @State var did: String = ""
    @State var chunkId: String = ""
    
    func updatePreview(type: String, user: String, id: String, url: URL) {
        isOwn = false
        self.showImg = true
        self.url = nil
        self.saveUrl = url
        self.user = user
        self.type = type
        self.id = id
        
        DataHandler.shared.getPrevData(type: type, user: user, id: id, { title, creations, favorite in
            self.title = title
            self.creations = creations
            self.fav = favorite
            self.canFav = true
        })
        
        withAnimation {
            displayed = true
            offset = minOffset
        }
    }
    
    func setElementOptions(_ type: String, _ id: String, _ chunkId: String, _ storageId: String, _ user: String, _ createdBy: String) {
        disabled = false
        self.did = id
        self.chunkId = chunkId
        
        if (createdBy == DataHandler.shared.uid!) {
            isOwn = true
        } else {
            isOwn = false
        }
        
        if (type == "spotlights" || type == "filters") {
            self.showImg = false
            self.url = nil
            self.saveUrl = url
            self.user = user
            self.type = type
            self.id = storageId
            self.canFav = false
            
            let index = type.index(type.endIndex, offsetBy: -1)
            self.title = String(type[..<index])
            
            withAnimation {
                displayed = true
                offset = minOffset
            }
        } else {
            DataHandler.shared.getURL(user: user, id: storageId, type: type, { url in
                self.showImg = true
                self.url = nil
                self.saveUrl = url
                self.user = user
                self.type = type
                self.id = storageId
                
                DataHandler.shared.getPrevData(type: type, user: user, id: storageId, { title, creations, favorite in
                    self.title = title
                    self.creations = creations
                    self.fav = favorite
                })
                
                withAnimation {
                    displayed = true
                    offset = minOffset
                }
            })
            
        }
        
        print("swifty options")
    }
    
    var body: some View {
        VStack {
            if (displayed) {
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
                            .background(Color(.black).opacity(0.6))
                            .frame(width: UIScreen.screenWidth/3, height: UIScreen.screenWidth/3)
                            .cornerRadius(16)
                        } else if (saveUrl != nil) {
                            Text("\(title)")
                                .onAppear {
                                    withAnimation {
                                        self.url = saveUrl
                                        self.saveUrl = nil
                                    }
                                }
                        }
                        VStack(alignment: .leading) {
                            Text("\(title)")
                                .font(.title)
                            //                            .frame(maxWidth: .infinity, alignment: .leading)
                                .multilineTextAlignment(.leading)
                            Text("Uploaded by \(user)")
                                .foregroundColor(Color(hex: "d6d6d6"))
                            //                            .frame(maxWidth: .infinity, alignment: .leading)
                                .multilineTextAlignment(.leading)
                            Text("Type: \(type)")
                                .foregroundColor(Color(hex: "d6d6d6"))
                            //                            .frame(maxWidth: .infinity, alignment: .leading)
                                .multilineTextAlignment(.leading)
                        }
                        .padding(.horizontal,5)
                        .frame(maxWidth: .infinity, alignment: .leading)
                    }.padding(.horizontal).transition(.opacity)
                    HStack {
                        if (isOwn) {
                            Button(action: {withAnimation {
                                UnityBridge.getInstance().api.deleteObj(type: type, id: did, chunkId: chunkId)
                                withAnimation {
                                    displayed = false
                                    offset = CGFloat(1000)
                                }
                            }}, label: {
                                Image(systemName: "trash.fill")
                                    .imageScale(.medium)
                                    .font(.title)
                                    .foregroundColor(Color(.red).opacity(0.5))
                                    .padding(10)
                                    .padding(.horizontal,5)
                            })
                            .background(Color(hex: "424242"))
                            .frame(width: 50, height: 50)
                            .cornerRadius(16)
                        }
                        
                        if (canFav) {
                            Button(action: {withAnimation {
                                if (!fav) {
                                    DataHandler.shared.favElem(type: type, id: id)
                                } else {
                                    DataHandler.shared.unfavElem(type: type, id: id)
                                }
                                fav = !fav;
                            }}, label: {
                                Image(systemName: fav ? "star.fill" : "star")
                                    .imageScale(.medium)
                                    .font(.title)
                                    .foregroundColor(fav ? .yellow : .gray)
                                    .padding(10)
                                    .padding(.horizontal,5)
                            })
                            .background(Color(hex: "424242"))
                            .frame(width: 50, height: 50)
                            .cornerRadius(16)
                        }
                        
                        Button(action: {UnityBridge.getInstance().api.addObject(type: type, user: user, id: id); DataHandler.shared.setAddingType(type); disabled = !disabled; withAnimation{displayed = false;offset = CGFloat(1000)}}, label: {
                            Spacer()
                            Text("Add \(type)")
                                .foregroundColor(.white)
                                .padding()
                            Spacer()
                        })
                        .background(.blue)
                        .cornerRadius(16)
                        .padding(.leading,5)
                    }.padding().transition(.opacity)
                        .padding(.bottom,80)
                    //                Spacer()
                }
                //            .frame(height: UIScreen.screenHeight/1.75)
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
                            if (offset > 200 || gesture.predictedEndTranslation.height > 200) {
                                withAnimation {
                                    displayed = false
                                    offset = CGFloat(1000)
                                }
                            } else {
                                withAnimation {
                                    offset = minOffset
                                }
                            }
                        }
                )
                .onTapGesture {}
                .transition(.move(edge: .bottom))
            }
        }
        .background(displayed ? Color(.black).opacity(0.5) : .clear)
        .transition(.opacity)
        .frame(width: UIScreen.screenWidth)
        .onAppear {
            DataHandler.shared.setPreview = updatePreview
            UnityBridge.getInstance().api.setElementOptions = self.setElementOptions
        }.onTapGesture {
            withAnimation{
                displayed = false
                offset = CGFloat(1000)
            }
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
