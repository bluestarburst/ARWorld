//
//  UnityView.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 11/29/22.
//

import SwiftUI
#if canImport(UIKit)
import UIKit
#elseif canImport(AppKit)
import AppKit
#endif

extension Color {
    var components: (red: CGFloat, green: CGFloat, blue: CGFloat, opacity: CGFloat) {
        
#if canImport(UIKit)
        typealias NativeColor = UIColor
#elseif canImport(AppKit)
        typealias NativeColor = NSColor
#endif
        
        var r: CGFloat = 0
        var g: CGFloat = 0
        var b: CGFloat = 0
        var o: CGFloat = 0
        
        guard NativeColor(self).getRed(&r, green: &g, blue: &b, alpha: &o) else {
            // You can handle the failure here as you want
            return (0, 0, 0, 0)
        }
        
        return (r, g, b, o)
    }
}

extension AnyTransition {
    static var bottomAndFade: AnyTransition {
        AnyTransition.move(edge: .bottom).combined(with: .opacity)
    }
}

struct UnityView: View {
    
    
    //    @State private var color = Color(.sRGB,red: 0.98, green: 0.9, blue: 0.2)
    
    @Binding var isLoaded: Bool
    var changePage: (Int) -> Void
    
    @StateObject var manager = LocationManager()
    
    @State private var showElementOptions = false
    @State private var showElementSelection = false
    @State private var showButtons = false
    
    @State private var addingObj = ""
    @State private var mapStatus = ""
    @State private var change = "move"
    
    @State private var elementType = "image"
    @State private var type = "spotlights"
    
    @State private var mapOffset = CGFloat(-100)
    
    @State private var showSettings = false
    @State private var showSettingsButton = true
    @State private var showMesh = false
    @State private var showChunks = false
    @State private var showLogs = false
    @State private var showPreview = false
    
    @State private var isAdding = false
    
    @State private var topRadius = 0.5
    @State private var botRadius = 1.5
    
    @State private var inColor =
    Color(.sRGB, red: 0.98, green: 0.9, blue: 0.2)
    
    @State private var saturation = CGFloat(1)
    @State private var threshold = CGFloat(0.25)
    
    func changeSelection(changeType: String) {
        withAnimation {
            elementType = changeType
        }
    }
    
    @State private var addSetoffset = CGFloat.zero
    @State private var prevOffsetHight = CGFloat.zero
    @State private var minOffset = CGFloat(5)
    
    @State private var geo = CGSize.zero
    
    @State private var isColor = false
    @State private var isCam = false
    
    
    var body: some View {
        ZStack {
            
            VStack {
                Spacer()
                HStack {
                    if (showSettingsButton) {
                        VStack {
                            Button( action: {withAnimation{showSettings=true}}, label: {
                                Image(systemName: "gearshape")
                                    .imageScale(.medium)
                                    .font(.title2)
                                    .foregroundColor(.white)
                                    .padding(10)
                                    .background(Color(.white).opacity(0.1))
                                    .clipShape(Circle())
                                    .padding(.vertical,15)
                            })
                            Spacer()
                        }
                    }
                    Spacer()
                    if (showButtons) {
                        VStack {
                            Button( action: {}, label: {
                                if (mapStatus == "saving") {
                                    ProgressView()
                                        .imageScale(.medium)
                                        .font(.title2)
                                        .foregroundColor(.white)
                                        .padding(10)
                                        .background(Color(.white).opacity(0.1))
                                        .clipShape(Circle())
                                        .padding(.vertical,15)
                                        .disabled(true)
                                } else if (mapStatus == "mapped") {
                                    Image(systemName: "checkmark")
                                        .imageScale(.medium)
                                        .font(.title2)
                                        .foregroundColor(.green)
                                        .padding(10)
                                        .background(Color(.white).opacity(0.1))
                                        .clipShape(Circle())
                                        .padding(.vertical,15)
                                }
                                
                            })
                            .offset(y: mapOffset)
                            Spacer()
                            Button( action: {changePage(1)}, label: {
                                Image(systemName: "photo")
                                    .imageScale(.medium)
                                    .font(.title2)
                                    .foregroundColor(.white)
                                    .padding(10)
                                    .background(Color(.white).opacity(0.1))
                                    .clipShape(Circle())
                                    .padding(.vertical,5)
                            })
                            Button( action: {withAnimation {
                                isCam = true
                                showSettings = false
                                showElementSelection = false
                                isAdding = false
                                showElementSelection = false
                                showButtons = false
                                showPreview = false
                                showSettingsButton = false
                            }}, label: {
                                Image(systemName: "camera")
                                    .imageScale(.medium)
                                    .font(.title2)
                                    .foregroundColor(.white)
                                    .padding(10)
                                    .background(Color(.white).opacity(0.1))
                                    .clipShape(Circle())
                                    .padding(.vertical,5)
                            })
                            Button( action: {withAnimation {showElementSelection = true;elementType="object"}}, label: {
                                Image(systemName: "plus")
                                    .imageScale(.large)
                                    .font(.title2)
                                    .foregroundColor(.white)
                                    .padding(10)
                                    .background(Color(.white).opacity(0.1))
                                    .clipShape(Circle())
                                    .padding(.vertical,5)
                            })
                        }
                        .padding(.bottom, 10)
                        .transition(.bottomAndFade)
                    }
                }
            }
            .padding()
            
            if (mapStatus == "failed") {
                VStack {
                    Spacer()
                    VStack{
                        Text("Could not find any nearby maps")
                        Button(action: {UnityBridge.getInstance().api.loadMap();withAnimation{mapStatus = ""}}, label: {
                            Spacer()
                            Text("Retry")
                                .foregroundColor(.white)
                                .padding()
                            Spacer()
                        })
                        .background(Color(.gray).opacity(0.5))
                        .cornerRadius(100)
                        .padding(.leading,5)
                        Text("or")
                        Button(action: {UnityBridge.getInstance().api.saveMap();withAnimation{mapStatus = ""}}, label: {
                            Spacer()
                            Text("Create New Map")
                                .foregroundColor(.black)
                                .padding()
                            Spacer()
                        })
                        .background(.white)
                        .cornerRadius(100)
                        .padding(.leading,5)
                    }.padding(50)
                    Spacer()
                }.background(Color(.black).opacity(0.65))
                    .transition(.opacity)
            }
            
            
            //            if (showElementOptions) {
            //                VStack {
            //
            //                    Spacer()
            //                    Text("Choose an Element to Create")
            //                        .font(.title)
            //                        .multilineTextAlignment(.center)
            //                        .padding(.horizontal, 20)
            //                    HStack {
            //                        Spacer()
            //                        VStack {
            //                            Button( action: {withAnimation {showElementSelection = true;showElementOptions = false;elementType="image"}}, label: {
            //                                Image(systemName: "photo")
            //                                    .imageScale(.medium)
            //                                    .font(.title)
            //                                    .foregroundColor(.white)
            //                                    .padding(10)
            //                                    .background(Color(.white).opacity(0.1))
            //                                    .clipShape(Circle())
            //                                    .padding(.horizontal,5)
            //                            })
            //                            Text("Photo")
            //                                .font(.title2)
            //                        }
            //                        VStack {
            //                            Button( action: {withAnimation {showElementSelection = true;showElementOptions = false;elementType="object"}}, label: {
            //                                Image(systemName: "cube")
            //                                    .imageScale(.medium)
            //                                    .font(.title)
            //                                    .foregroundColor(.white)
            //                                    .padding(10)
            //                                    .background(Color(.white).opacity(0.1))
            //                                    .clipShape(Circle())
            //                                    .padding(.horizontal,5)
            //                            })
            //                            Text("3D Object")
            //                                .font(.title2)
            //                        }
            //                        VStack {
            //                            Button( action: {print("add")}, label: {
            //                                Image(systemName: "sparkle")
            //                                    .imageScale(.medium)
            //                                    .font(.title)
            //                                    .foregroundColor(.white)
            //                                    .padding(10)
            //                                    .background(Color(.white).opacity(0.1))
            //                                    .clipShape(Circle())
            //                                    .padding(.horizontal,5)
            //                            })
            //                            Text("Effect")
            //                                .font(.title2)
            //                        }
            //                        Spacer()
            //                    }
            //                    Spacer()
            //
            //                }
            //                .background(
            //                    Color(.black)
            //                        .opacity(0.5)
            //                        .onTapGesture {
            //                            withAnimation {
            //                                showElementOptions = false
            //                            }
            //                        }
            //                        .transition(.opacity)
            //                )
            //
            //            }
            
            
            
            if (showElementSelection) {
                VStack{
                    Spacer()
                    HStack {
                        Spacer()
                    }
                }
                .background(.black.opacity(0.5))
                .transition(.opacity)
                .onTapGesture {
                    withAnimation {
                        showElementSelection = false
                    }
                }
                
                ZStack {
                    if (elementType == "image") {
                        ImageSelection(disabled: $showElementSelection, changeSelection: self.changeSelection)
                    } else if (elementType == "object") {
                        ObjSelection(disabled: $showElementSelection, changeSelection: self.changeSelection)
                    } else if (elementType == "effect") {
                        EffectSelection(disabled: $showElementSelection, changeSelection: self.changeSelection, type: $type)
                    }
                }
                .transition(.bottomAndFade)
            }
            
            if (addingObj == "adding") {
                VStack {
                    HStack {
                        Button (action: {withAnimation{change="move";showButtons = true; addingObj = ""; UnityBridge.getInstance().api.changeTransform(change: "delete")}}, label: {
                            Image(systemName: "xmark")
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(.white)
                                .padding(10)
                                .background(Color(.white).opacity(0.1))
                                .clipShape(Circle())
                                .padding(.horizontal,5)
                        })
                        Spacer()
                    }
                    .padding(30)
                    .padding(.top,45)
                    Spacer()
                    
                    
                    HStack {
                        Button (action: {withAnimation {UnityBridge.getInstance().api.changeTransform(change: "move");change = "move"}}, label: {
                            Image(systemName: "move.3d")
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(change == "move" ? .pink : .white)
                                .padding(10)
                                .background(Color(.white).opacity(0.1))
                                .clipShape(Circle())
                                .padding(.horizontal,5)
                        })
                        Button (action: {withAnimation {UnityBridge.getInstance().api.changeTransform(change: "rotate"); change = "rotate"}}, label: {
                            Image(systemName: "rotate.3d")
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(change == "rotate" ? .pink : .white)
                                .padding(10)
                                .background(Color(.white).opacity(0.1))
                                .clipShape(Circle())
                                .padding(.horizontal,5)
                        })
                        Button (action: {withAnimation {UnityBridge.getInstance().api.changeTransform(change: "scale"); change = "scale"}}, label: {
                            Image(systemName: "scale.3d")
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(change == "scale" ? .pink : .white)
                                .padding(10)
                                .background(Color(.white).opacity(0.1))
                                .clipShape(Circle())
                                .padding(.horizontal,5)
                        })
                    }
                    .padding(.bottom, type == "spotlights" ? 80 : 30)
                }
                .transition(.bottomAndFade)
            }
            
            if (addingObj == "adding") {
                VStack {
                    Spacer()
                    HStack {
                        Spacer()
                        Button (action: {withAnimation{change="move";showButtons = true; addingObj = ""; UnityBridge.getInstance().api.changeTransform(change: "save")}}, label: {
                            Image(systemName: "plus")
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(.white)
                                .padding(10)
                                .background(Color.pink)
                                .clipShape(Circle())
                                .padding(.horizontal,25)
                        })
                    }
                    .padding(.bottom, type == "spotlights" ? 80 : 30)
                }.transition(.bottomAndFade)
            } else if (addingObj == "preview") {
                VStack {
                    HStack {
                        Button (action: {withAnimation{change="move";showButtons = true; addingObj = "";showPreview=false; UnityBridge.getInstance().api.changeTransform(change: "delete")}}, label: {
                            Image(systemName: "xmark")
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(.white)
                                .padding(10)
                                .background(Color(.white).opacity(0.1))
                                .clipShape(Circle())
                                .padding(.horizontal,5)
                        })
                        Spacer()
                        Button (action: {withAnimation{change="move"; UnityBridge.getInstance().api.nextStepFilter()}}, label: {
                            Image(systemName: "chevron.right")
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(.white)
                                .padding(10)
                                .background(Color.pink)
                                .clipShape(Circle())
                                .padding(.horizontal,25)
                        })
                    }
                    .padding(30)
                    .padding(.top,45)
                    Spacer()
                }.transition(.bottomAndFade)
            } else if (isCam) {
                VStack {
                    HStack {
                        Button (action: {withAnimation{
                            isCam = false
                            showSettings = false
                            showElementSelection = false
                            isAdding = false
                            showElementSelection = false
                            showButtons = true
                            showPreview = false
                            showSettingsButton = true
                        }}, label: {
                            Image(systemName: "xmark")
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(.white)
                                .padding(10)
                                .background(Color(.white).opacity(0.1))
                                .clipShape(Circle())
                                .padding(.horizontal,5)
                        })
                        Spacer()
                    }
                    .padding(30)
                    .padding(.top,45)
                    Spacer()
                }.transition(.bottomAndFade)
            }
            
            if (showPreview || (addingObj == "adding" && type == "spotlights")) {
                // this is the extra options section
                VStack {
                    Spacer()
                    
                    VStack {
                        
                        HStack {
                            Spacer()
                            RoundedRectangle(cornerRadius: 10)
                                .frame(width: 30, height: 10)
                                .foregroundColor(.white)
                            Spacer()
                        }
                        .padding()
                        
                        VStack {
                            if (type == "spotlights") {
                                VStack {
                                    Text("top radius")
                                    Slider(value: $topRadius, in: 0...1.5) {
                                        Text("top radius")
                                    }.onChange(of: topRadius) { _ in
                                        UnityBridge.getInstance().api.changeRadius(top: topRadius, bottom: botRadius)
                                    }
                                    Text("bottom radius")
                                    Slider(value: $botRadius, in: 0.1...2) {
                                        Text("bottom radius")
                                    }.onChange(of: botRadius) { _ in
                                        UnityBridge.getInstance().api.changeRadius(top: topRadius, bottom: botRadius)
                                    }
                                }
                                    .onAppear {
                                        UnityBridge.getInstance().api.changeRadius(top: 0.50, bottom: 1.50)
//                                        withAnimation {
//                                            addSetoffset = CGFloat(geo.height - 65)
//                                            prevOffsetHight = addSetoffset
//                                        }
                                    }
                            } else if (showPreview) {
                                
                                Text("saturation")
                                Slider(value: $saturation, in: 0...1) {
                                    Text("saturation")
                                }.onChange(of: saturation) { _ in
                                    UnityBridge.getInstance().api.changeFilter(r: inColor.components.red, g: inColor.components.green, b: inColor.components.blue, saturation: saturation, threshold: threshold, isColor: (isColor ? CGFloat(1) : CGFloat.zero))
                                }.onAppear {
                                    UnityBridge.getInstance().api.changeFilter(r: inColor.components.red, g: inColor.components.green, b: inColor.components.blue, saturation: saturation, threshold: threshold, isColor: (isColor ? CGFloat(1) : CGFloat.zero))
                                }
                                
//                                Text("show color mask")
                                Toggle("color mask", isOn: $isColor).padding(.top, 20)
                                
                                if (isColor) {
                                    ColorPicker(selection: $inColor, supportsOpacity: false, label: {
                                        Text("color")
                                    }).onChange(of: inColor) { _ in
                                        UnityBridge.getInstance().api.changeFilter(r: inColor.components.red, g: inColor.components.green, b: inColor.components.blue, saturation: saturation, threshold: threshold, isColor: (isColor ? CGFloat(1) : CGFloat.zero))
                                    }
                                    
                                    Text("threshold")
                                    Slider(value: $threshold, in: 0...1) {
                                        Text("threshold")
                                    }.onChange(of: threshold) { _ in
                                        UnityBridge.getInstance().api.changeFilter(r: inColor.components.red, g: inColor.components.green, b: inColor.components.blue, saturation: saturation, threshold: threshold, isColor: (isColor ? CGFloat(1) : CGFloat.zero))
                                        print("isColor: \(isColor ? CGFloat(1) : CGFloat.zero)")
                                    }
                                    
                                }
                            }
                        }
                        .padding()
                        .padding(.horizontal, 30)
                        .gesture(
                            DragGesture()
                                .onChanged {gesture in
                                    addSetoffset = prevOffsetHight
                                }
                        )
                        
                    }
                    
                    
                    .padding(.bottom, 65)
                    .background(GeometryReader{ geom -> Color in
                        DispatchQueue.main.async {
                            self.geo = geom.size
                        }
                        return Color.black
                    })
                    .cornerRadius(16)
                    
                    .offset(x: 0, y: addSetoffset)
                    .gesture(
                        DragGesture()
                            .onChanged { gesture in
                                print(gesture)
                                addSetoffset = max(gesture.translation.height + prevOffsetHight, 0)
                            }
                            .onEnded { gesture in
                                if (addSetoffset > geo.height/2 || gesture.predictedEndTranslation.height > geo.height/2) {
                                    withAnimation {
                                        addSetoffset = CGFloat(geo.height - 65)
                                        prevOffsetHight = addSetoffset
                                    }
                                } else {
                                    withAnimation {
                                        addSetoffset = minOffset
                                        prevOffsetHight = addSetoffset
                                    }
                                }
                            }
                    )
                    .onChange(of: geo) {_ in
                        if (showPreview) {
                            withAnimation {
                                addSetoffset = minOffset
                                prevOffsetHight = addSetoffset
                            }
                        } else if (type == "spotlights") {
                            withAnimation {
                                addSetoffset = CGFloat(geo.height - 65)
                                prevOffsetHight = addSetoffset
                            }
                        }
                    }
                    .transition(.move(edge: .bottom))
                }
            }
            
            
            if (showSettings) {
                VStack{
                    Spacer()
                    HStack {
                        Spacer()
                    }
                }
                .background(.black.opacity(0.5))
                .transition(.opacity)
                .onTapGesture {
                    withAnimation {
                        showSettings = false
                    }
                }
                
                ZStack {
                    SettingsSelection(disabled: $showSettings, showMesh: $showMesh, showChunks: $showChunks, showLogs: $showLogs, changePage: self.changePage)
                }
                .transition(.bottomAndFade)
            }
            
            if (isCam) {
                VStack {
                    Spacer()
                    VStack {
                        HStack {
                            Spacer()
                            Button(action: {
                                UnityBridge.getInstance().api.takePic()
                            }) {
                                Circle()
                                    .strokeBorder(.white, lineWidth: 2)
                                    .background(Circle().fill(.clear))
                                    .frame(width: 75, height: 75)
                            }
                            Spacer()
                        }.padding()
                            .padding(.bottom, 18)
                    }.background(Color(.black).opacity(0.5))
                }
            }
            
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
        
        .onAppear {
            let api = UnityBridge.getInstance()
            api.show()
            manager.sendDat = {
                api.api.updateVars(lat: manager.latitude, lon: manager.longitude, alt: manager.altitude)
                api.api.changeSettings(change: (showLogs ? "logs-on" : "logs-off"))
            }
            DataHandler.shared.setAddingObj = {
                addingObj = DataHandler.shared.addingObj
                withAnimation {
                    if (addingObj == "adding") {
                        showSettings = false
                        showElementSelection = false
                        
                        isAdding = true
                        showElementSelection = false
                        showButtons = false
                        showPreview = false
                        showSettingsButton = false
                    }
                    else if (addingObj == "preview") {
                        addSetoffset = minOffset
                        prevOffsetHight = addSetoffset
                        showSettings = false
                        showElementSelection = false
                        showButtons = false
                        isAdding = false
                        showPreview = true
                        showSettingsButton = false
                    } else {
                        isAdding = false
                        showButtons = true
                        showPreview = false
                        showSettingsButton = true
                        showSettings = true
                    }
                }
            }
            DataHandler.shared.setMapStatus = {
                
                withAnimation {
                    mapStatus = DataHandler.shared.mapStatus
                    if (mapStatus == "mapped") {
                        mapOffset = -100
                        if (!isAdding && !showPreview && !isCam) {
                            showButtons = true
                        }
                    } else if(mapStatus == "saving") {
                        mapOffset = 0
                    } else {
                        showButtons = false
                    }
                }
            }
            
            DataHandler.shared.setAddingType = { type in
                self.type = type
            }
            
        }
        .onChange(of: isLoaded) {
            let api = UnityBridge.getInstance()
            if ($0 == false) {
                api.unload()
            } else {
                api.show()
            }
        }
        
        .ignoresSafeArea()
        
    }
}



//struct UnityView_Previews: PreviewProvider {
//    static var previews: some View {
//        UnityView()
//    }
//}
