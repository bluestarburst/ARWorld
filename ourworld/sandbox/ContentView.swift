//
//  ContentView.swift
//  sandbox
//
//  Created by David Peicho on 1/21/21.
//

import SwiftUI
import Firebase
import FirebaseAuth

struct ContentView: View {
    @Environment(\.scenePhase) private var phase
    @State var page = (Auth.auth().currentUser?.uid != nil) ? 1 : 0
    
    @State var isLoaded = true
    @State var offset = CGFloat(0)
    @State var normal = CGFloat(100)
    
    @State var pages = 0
    
    @State var disabled = false
    
    @State var findDisabled = false
    
    func changePage(num: Int) {
        if (num == 1) {
            withAnimation {
                pages = 1
                offset = 0
            }
        } else if (num == 0) {
            withAnimation {
                pages = 1
                offset = 0
                page = 0
            }
        }
    }
    
    @State var refresh = false
    
    var body: some View {
        ZStack {
            if (refresh == false) {
                if (page == 0) {
                    AuthView(page: $page)
                } else {
                    
                    HStack {
                        VStack {
                            Spacer()
                        }
                        .frame(width: UIScreen.screenWidth * 0.05)
                        .background(.black.opacity(0.02))
                        .gesture(
                            DragGesture()
                                .onChanged{ gesture in
                                    withAnimation {
                                        offset = normal + gesture.translation.width
                                    }
                                }
                                .onEnded { _ in
                                    if (offset > -230) {
                                        //                                    UnityBridge.getInstance().unload()
                                        withAnimation{
                                            offset = 0
                                            pages = 1
                                            
                                        }
                                    } else {
                                        withAnimation {
                                            offset = normal - 200
                                            
                                        }
                                    }
                                }
                        )
                        Spacer()
                        VStack {
                            Spacer()
                        }
                        .frame(width: 10)
                        .background(.black.opacity(0.02))
                        .gesture(
                            DragGesture()
                                .onChanged{ gesture in
                                    if (pages == 1) {
                                        withAnimation {
                                            offset = gesture.translation.width
                                        }
                                    }
                                }
                                .onEnded { _ in
                                    if (offset < -100) {
                                        //                                    UnityBridge.getInstance().show()
                                        withAnimation{
                                            offset = normal - 200
                                            pages = 0
                                            
                                        }
                                    } else {
                                        withAnimation {
                                            offset = 0
                                            
                                        }
                                        
                                    }
                                    
                                }
                        )
                        
                    }
                    
                    UnityView(isLoaded: $isLoaded, changePage: self.changePage)
                        .gesture(DragGesture())
                        .onAppear {
                            DataHandler.shared.getUID()
                        }
                        .id(1)
                    
                    GeometryReader { geometry in
                        VStack {
                            
                            HStack {
                                Spacer()
                            }.ignoresSafeArea()
                            
                            FindView(bool: $findDisabled)
                                .padding(.top,30)
                                .onChange(of: findDisabled) { _ in
                                    withAnimation {
                                        offset = normal - 200
                                        pages = 0
                                    }
                                    findDisabled = false
                                }
                                
                            
                            Spacer()
                        }
                        
                        
                        .background(.black)
                        .cornerRadius(24)
                        
                        .ignoresSafeArea()
                        
                        
                        .offset(x: offset)
                        .onAppear {
                            normal = -geometry.size.width
                            offset = -geometry.size.width - 200
                        } .onChange(of: geometry.size) {_ in
                            withAnimation {
                                if (pages == 1) {
                                    normal = -geometry.size.width
                                } else {
                                    offset = -geometry.size.width - 200
                                }
                            }
                            
                            UIScreen.screenHeight = geometry.size.height
                            UIScreen.screenWidth = geometry.size.width
                        }
                        
                    }
                    if (offset > normal) {
                        HStack {
                            
                            Spacer()
                            VStack {
                                Spacer()
                            }
                            .frame(width: UIScreen.screenWidth * 0.05)
                            .background(.black.opacity(0.02))
                            .gesture(
                                DragGesture()
                                    .onChanged{ gesture in
                                        if (pages == 1) {
                                            withAnimation {
                                                offset = gesture.translation.width
                                            }
                                        }
                                    }
                                    .onEnded { _ in
                                        if (offset < -100) {
                                            //                                    UnityBridge.getInstance().show()
                                            withAnimation{
                                                offset = normal - 200
                                                pages = 0
                                                
                                            }
                                        } else {
                                            withAnimation {
                                                offset = 0
                                                
                                            }
                                            
                                        }
                                        
                                    }
                            )
                            
                        }
                    }
                    
                    
                    
                    PreviewObj(disabled: $disabled).onChange(of: disabled) { dis in
                        withAnimation{
                            offset = normal
                            pages = 0
                        }
                    }
                    
                    
                    
                }
            }
        }.onAppear {
            setupColorScheme()
        }
    }
    
    private func setupColorScheme() {
        // We do this via the window so we can access UIKit components too.
        let window = UIApplication.shared.windows.first
        window?.overrideUserInterfaceStyle = .dark
        window?.tintColor = UIColor(Color.pink)
    }}



// Our custom view modifier to track rotation and
// call our action
struct DeviceRotationViewModifier: ViewModifier {
    let action: (UIDeviceOrientation) -> Void
    
    func body(content: Content) -> some View {
        content
            .onAppear()
            .onReceive(NotificationCenter.default.publisher(for: UIDevice.orientationDidChangeNotification)) { _ in
                action(UIDevice.current.orientation)
            }
    }
}

// A View wrapper to make the modifier easier to use
extension View {
    func onRotate(perform action: @escaping (UIDeviceOrientation) -> Void) -> some View {
        self.modifier(DeviceRotationViewModifier(action: action))
    }
}
