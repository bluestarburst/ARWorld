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
    
    func changePage(num: Int) {
        if (num == 1) {
            withAnimation {
                pages = 1
                offset = 0
            }
        }
    }

    var body: some View {
        ZStack {
            if (page == 0) {
                AuthView(page: $page)
            } else {
                
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
                        }
                        
                        FindView()
                            
                        Spacer()
                    }
                    .background(.black)
                    .offset(x: offset)
                    .onAppear {
                        normal = -geometry.size.width
                        offset = -geometry.size.width
                    }
                }
                
                HStack {
                    VStack {
                        Spacer()
                    }
                    .frame(width: 10)
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
                                        offset = normal
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
                                        offset = normal
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
                
                PreviewObj(disabled: $disabled).onChange(of: disabled) { dis in
                    withAnimation{
                        offset = normal
                        pages = 0
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
