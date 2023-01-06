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
    @State private var color = Color(
        .sRGB,
        red: 0.98, green: 0.9, blue: 0.2)
    
    @Binding var isLoaded: Bool
    @StateObject var manager = LocationManager()
    
    @State private var showElementOptions = false
    @State private var showElementSelection = false
    
    var body: some View {
        ZStack {
            VStack {
                Spacer()
                HStack {
                    Spacer()
                    VStack {
                        Spacer()
                        Button( action: {DataHandler.shared.tryStore()}, label: {
                            Image(systemName: "photo")
                                .imageScale(.medium)
                                .font(.title2)
                                .foregroundColor(.white)
                                .padding(10)
                                .background(Color(.white).opacity(0.1))
                                .clipShape(Circle())
                                .padding(.vertical,5)
                        })
                        Button( action: {DataHandler.shared.signOut()}, label: {
                            Image(systemName: "camera")
                                .imageScale(.medium)
                                .font(.title2)
                                .foregroundColor(.white)
                                .padding(10)
                                .background(Color(.white).opacity(0.1))
                                .clipShape(Circle())
                                .padding(.vertical,5)
                        })
                        Button( action: {withAnimation {showElementOptions = true}}, label: {
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
            .padding()
            
            if (showElementOptions) {
                VStack {
                    
                    Spacer()
                    Text("Choose an Element to Create")
                        .font(.title)
                        .multilineTextAlignment(.center)
                        .padding(.horizontal, 20)
                    HStack {
                        Spacer()
                        VStack {
                            Button( action: {withAnimation {showElementSelection = true;showElementOptions = false}}, label: {
                                Image(systemName: "photo")
                                    .imageScale(.medium)
                                    .font(.title)
                                    .foregroundColor(.white)
                                    .padding(10)
                                    .background(Color(.white).opacity(0.1))
                                    .clipShape(Circle())
                                    .padding(.horizontal,5)
                            })
                            Text("Photo")
                                .font(.title2)
                        }
                        VStack {
                            Button( action: {print("add")}, label: {
                                Image(systemName: "cube")
                                    .imageScale(.medium)
                                    .font(.title)
                                    .foregroundColor(.white)
                                    .padding(10)
                                    .background(Color(.white).opacity(0.1))
                                    .clipShape(Circle())
                                    .padding(.horizontal,5)
                            })
                            Text("3D Object")
                                .font(.title2)
                        }
                        VStack {
                            Button( action: {print("add")}, label: {
                                Image(systemName: "sparkle")
                                    .imageScale(.medium)
                                    .font(.title)
                                    .foregroundColor(.white)
                                    .padding(10)
                                    .background(Color(.white).opacity(0.1))
                                    .clipShape(Circle())
                                    .padding(.horizontal,5)
                            })
                            Text("Effect")
                                .font(.title2)
                        }
                        Spacer()
                    }
                    Spacer()
                    
                }
                .frame(width: .infinity, height: .infinity)
                .background(
                    Color(.black)
                        .opacity(0.5)
                        .onTapGesture {
                            withAnimation {
                                showElementOptions = false
                            }
                        }
                        .transition(.opacity)
                )
                
            }
            
            
            
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
                    ElementSelection(disabled: $showElementSelection)
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
