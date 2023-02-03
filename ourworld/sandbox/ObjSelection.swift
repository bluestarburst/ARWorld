//
//  ElementSelection.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 1/3/23.
//

import WrappingHStack
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
    
    @State private var favorite = true
    
    var changeSelection: (String) -> Void
    
    var body: some View {
        VStack {
            Spacer()
            
            HStack {
                Spacer()
                Button( action: {changeSelection("image")}, label: {
                    Image(systemName: "photo")
                        .imageScale(.large)
                        .font(.title2)
                        .foregroundColor(.white)
                        .padding(10)
                        .background(Color(.white).opacity(0.1))
                        .clipShape(Circle())
                        .padding(.vertical,5)
                })
                Button( action: {changeSelection("object")}, label: {
                    Image(systemName: "cube")
                        .imageScale(.large)
                        .font(.title2)
                        .foregroundColor(.pink)
                        .padding(10)
                        .background(Color(.white).opacity(0.1))
                        .clipShape(Circle())
                        .padding(.vertical,5)
                })
                Button( action: {changeSelection("effect")}, label: {
                    Image(systemName: "sparkle")
                        .imageScale(.large)
                        .font(.title2)
                        .foregroundColor(.white)
                        .padding(10)
                        .background(Color(.white).opacity(0.1))
                        .clipShape(Circle())
                        .padding(.vertical,5)
                })
                Spacer()
            }
            .offset(y: offset)
            
            
            VStack {
                
                
                HStack {
                    RoundedRectangle(cornerRadius: 10)
                        .frame(width: 30, height: 10)
                        .foregroundColor(.white)
                }
                .padding()
                
                
                VStack {
                    GeometryReader { geometry in
                        ZStack {
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
                            HStack {
                                Spacer()
                                Button(action: {withAnimation{favorite = !favorite}}, label: {
                                    Image(systemName: favorite ? "star.fill" : "star")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(favorite ? .yellow : .gray)
                                .padding(.horizontal,10)
                            }
                            .padding(.bottom, 10)
                            .padding(.trailing, 20)
                        }
                    }.frame(height: 40)
                    
                    ScrollView {
                        ObjLoop(type: $type, disabled: $disabled, fav: $favorite)
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
    
    @Binding var type: String
    
    @Binding var disabled: Bool
    
    @Binding var fav: Bool
    
    @State var arr: [[String: Any]] = []
    @State var refresh = false
    
    func addItem(_ user: String, _ id: String, _ url: URL) {
        withAnimation {
            arr.append([
                "user": user,
                "id": id,
                "url": url
            ])
        }
    }
    
    var body: some View {
        VStack {
            
            if (arr.count > 0 && refresh) {
                WrappingHStack(0..<arr.count, id:\.self, alignment: .center) {
                    let index = $0
                    let dat = arr[index]
                    Box(url: dat["url"] as? URL, user: dat["user"] as! String, id: dat["id"] as! String, type: type, fav: fav )
                        .onTapGesture {
                            withAnimation {
                                disabled = false
                            }
                        }.transition(.opacity)
                    if (arr.count - 1 == index) {
                        let rem = 4 - (arr.count % 4)
                        if (rem != 4) {
                            ForEach(1...rem, id: \.self) { _ in
                                Box()
                            }
                        }

                    }
                }
                .frame(width:UIScreen.screenWidth)
                
            } else {
                Text("")
                    .onAppear {
                        withAnimation {
                            refresh = true
                        }
                    }.onChange(of: arr.count) { _ in
                        withAnimation {
                            refresh = !refresh
                        }
                    }
            }
                
            
        }
        .onAppear {
            DataHandler.shared.addNextUserObj = addItem
            DataHandler.shared.getUserObjects(type: type, fav: fav)
        }
        .onChange(of: $type.wrappedValue) { _ in
            arr = []
            withAnimation {
                refresh = false
            }
            DataHandler.shared.getUserObjects(type: type, fav: fav)
        }
        .onChange(of: $fav.wrappedValue) { _ in
            arr = []
            withAnimation {
                refresh = false
            }
            DataHandler.shared.getUserObjects(type: type, fav: fav)
        }
    }
}

struct ObjSelection_Previews: PreviewProvider {
    static var previews: some View {
        ObjSelection(disabled: .constant(true), changeSelection: {_ in})
    }
}
