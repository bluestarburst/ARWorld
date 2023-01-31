//
//  UnityView.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 11/29/22.
//

import SwiftUI

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
    @State private var showMesh = false
    @State private var showChunks = false
    @State private var showLogs = false
    
    @State private var isAdding = false
    
    @State private var topRadius = 0.5
    @State private var botRadius = 1.5
    
    func changeSelection(changeType: String) {
        withAnimation {
            elementType = changeType
        }
    }
    
    var body: some View {
        ZStack {
            
            VStack {
                Spacer()
                HStack {
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
                            Button( action: {}, label: {
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
                    if (type == "spotlights") {
                        VStack {
                            Slider(value: $topRadius, in: 0...1.5) {
                                Text("top radius")
                            } minimumValueLabel: {
                                Text("0")
                            } maximumValueLabel: {
                                Text("1.5")
                            }.onChange(of: topRadius) { _ in
                                UnityBridge.getInstance().api.changeRadius(top: topRadius, bottom: botRadius)
                            }
                            
                            Slider(value: $botRadius, in: 0.1...2) {
                                Text("bottom radius")
                            } minimumValueLabel: {
                                Text("0.1")
                            } maximumValueLabel: {
                                Text("2")
                            }.onChange(of: botRadius) { _ in
                                UnityBridge.getInstance().api.changeRadius(top: topRadius, bottom: botRadius)
                            }
                        }.padding()
                            .onAppear {
                                UnityBridge.getInstance().api.changeRadius(top: 0.50, bottom: 1.50)
                            }
                    }
                    
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
                    .padding(.bottom, 30)
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
                    .padding(.bottom, 30)
                }.transition(.bottomAndFade)
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
                    } else {
                        isAdding = false
                        showButtons = true
                    }
                }
            }
            DataHandler.shared.setMapStatus = {
                mapStatus = DataHandler.shared.mapStatus
                withAnimation {
                    if (mapStatus == "mapped") {
                        mapOffset = -100
                        if (!isAdding) {
                            showButtons = true
                        }
                    } else if(mapStatus == "saving") {
                        mapOffset = 0
                    } else {
                        showButtons = false
                    }
                }
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
