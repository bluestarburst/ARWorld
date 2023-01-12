//
//  ElementSelection.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 1/3/23.
//

import SwiftUI

struct SettingsSelection: View {
    
    @State var minOffset = CGFloat(80)
    @State var offset = CGFloat(80)
    @State var prevOffset = CGFloat(80)
    
    @Binding var disabled: Bool
    @Binding var showMesh: Bool
    
    @State private var scrollWidth = CGFloat.zero
    
    @State private var type: String = "general"
    
    
    
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
                
                VStack {
                    GeometryReader { geometry in
                        ScrollView(.horizontal, showsIndicators: true) {
                            HStack {
                                Button(action: {withAnimation{type="general"}}, label: {
                                    Image(systemName: "gearshape.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "general" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "general")
                                
                                Button(action: {withAnimation{type="posters"}}, label: {
                                    Image(systemName: "doc.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "posters" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "posters")
                                
                                Button(action: {withAnimation{type="info"}}, label: {
                                    Image(systemName: "info.circle.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "info" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "info")
                            }
                            .padding(.horizontal,25)
                            .frame(minWidth: geometry.size.width)
                        }
                        .padding(.bottom, 10)
                    }.frame(height: 40)
                    
                    ScrollView {
                        Toggle("Show Mesh", isOn: $showMesh)
                            .onChange(of: showMesh) { change in
                                if (change) {
                                    UnityBridge.getInstance().api.changeSettings(change: "mesh-on")
                                } else {
                                    UnityBridge.getInstance().api.changeSettings(change: "mesh-off")
                                }
                            }
                            .padding (30)
                    }
                }.gesture(
                    DragGesture()
                        .onChanged {gesture in
                            offset = prevOffset
                        }
                )
            }
            .frame(height: 500)
            .background(.black)
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
                                disabled = false
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
        
    }
}

