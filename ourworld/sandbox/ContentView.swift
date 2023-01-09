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
    
    var body: some View {
        ZStack {
            if (page == 0) {
                AuthView(page: $page)
            } else {
                UnityView(isLoaded: $isLoaded)
                    .onAppear {
                        DataHandler.shared.getUID()
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
