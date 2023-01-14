//
//  ElementSelection.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 1/3/23.
//

import SwiftUI

struct ObjSelection: View {
    
    @State var minOffset = CGFloat(80)
    @State var offset = CGFloat(80)
    @State var prevOffset = CGFloat(80)
    
    @State var showImagePicker = false
    
    @Binding var disabled: Bool
    
    @State var selectedImage: UIImage = UIImage()
    
    @State private var scrollWidth = CGFloat.zero
    
    @State private var type: String = "objects"
    
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
                                Button(action: {withAnimation{type="objects"}}, label: {
                                    Image(systemName: "cube.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "objects" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "objects")
                                
                            }
                            .padding(.horizontal,25)
                            .frame(minWidth: geometry.size.width)
                        }
                        .padding(.bottom, 10)
                    }.frame(height: 40)
                    
                    ScrollView {
                        ObjLoop(type: $type, disabled: $disabled)
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
                        if (offset > 400 || gesture.predictedEndTranslation.height > 400) {
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
            
        }.sheet(isPresented: $showImagePicker) {
            ImagePicker(sourceType: .photoLibrary, selectedImage: $selectedImage, type: $type)
        }
        
    }
}

struct ObjLoop: View {
    
    @State var objects: [String:URL] = [:]
    @State var objectsL: [URL] = []
    @State var objectsI: [String] = []
    @State var objectsU: [String] = []
    
    @Binding var type: String
    
    @Binding var disabled: Bool
    
    func updateObjects() {
        var tempPost = DataHandler.shared.objects
        for id in tempPost.keys {
            if (objects[id] == nil) {
                objects[id] = tempPost[id]
                objectsL.append(tempPost[id]!)
                objectsI.append(id)
                objectsU.append(DataHandler.shared.objectData[id]!)
            }
        }
    }
    
    var body: some View {
        VStack {
            ForEach(0..<5) { index10 in
                HStack {
                    Spacer()
                    ForEach(0..<4) {index in
                        let temp = (index10 * 4) + index
                        switch type {
                        case "objects":
                            if (objectsL.count > temp) {
                                Box(url: objectsL[temp], user: objectsU[temp], id: objectsI[temp], type: type )
                                    .onTapGesture {
                                        withAnimation {
                                            disabled = false
                                        }
                                    }
                            } else {
                                Box()
                            }
                        default:
                            Box()
                        }
                    }
                    Spacer()
                }
            }
        }.onAppear {
            DataHandler.shared.updateObjects = updateObjects
            DataHandler.shared.getUserObjects()
        }
    }
}

struct ObjSelection_Previews: PreviewProvider {
    
    static var previews: some View {
        ObjSelection(disabled: .constant(true))
    }
}
