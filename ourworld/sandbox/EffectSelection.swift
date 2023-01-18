//
//  ElementSelection.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 1/3/23.
//

import WrappingHStack
import SwiftUI



struct EffectSelection: View {
    
    @State var minOffset = CGFloat(80)
    @State var offset = CGFloat(80)
    @State var prevOffset = CGFloat(80)
    
    @State var showImagePicker = false
    
    @Binding var disabled: Bool
    
    @State var selectedImage: UIImage = UIImage()
    
    @State private var scrollWidth = CGFloat.zero
    
    @State private var type: String = "spotlights"
    
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
                        .foregroundColor(.white)
                        .padding(10)
                        .background(Color(.white).opacity(0.1))
                        .clipShape(Circle())
                        .padding(.vertical,5)
                })
                Button( action: {changeSelection("effect")}, label: {
                    Image(systemName: "sparkle")
                        .imageScale(.large)
                        .font(.title2)
                        .foregroundColor(.pink)
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
                        ScrollView(.horizontal, showsIndicators: true) {
                            HStack {
                                Button(action: {withAnimation{type="spotlights"}}, label: {
                                    Image(systemName: "cone.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "spotlights" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "spotlights")
                                
                                Button(action: {withAnimation{type="arealights"}}, label: {
                                    Image(systemName: "lightbulb.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "arealights" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "arealights")
                                
                            }
                            .padding(.horizontal,25)
                            .frame(minWidth: geometry.size.width)
                        }
                        .padding(.bottom, 10)
                    }.frame(height: 40)
                    
                    ScrollView {
                        Button(action: {UnityBridge.getInstance().api.addObject(type: type, user: "", id: "")}, label: {
                            HStack {
                                Spacer()
                                switch type {
                                case "spotlights":
                                    Text("add new spotlight")
                                        .foregroundColor(.white)
                                case "arealights":
                                    Text("add new area light")
                                        .foregroundColor(.white)
                                default:
                                    Text("add new element")
                                        .foregroundColor(.white)
                                }
                                
                                Spacer()
                            }
                            .padding()
                            .background(.gray.opacity(0.25))
                        })
                        .cornerRadius(16)
                        .padding(.vertical, 5)
                        .padding(.horizontal, 25)
//                        ImageLoop(type: $type, disabled: $disabled)
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
            
        }
        
    }
}

//struct ImageLoop: View {
//
//    @Binding var type: String
//
//    @Binding var disabled: Bool
//
//    @State var arr: [[String: Any]] = []
//    @State var refresh = false
//
//    func addItem(_ user: String, _ id: String, _ url: URL) {
//        withAnimation {
//            arr.append([
//                "user": user,
//                "id": id,
//                "url": url
//            ])
//        }
//    }
//
//    var body: some View {
//        VStack {
//
//            if (arr.count > 0 && refresh) {
//                WrappingHStack(0..<arr.count, id:\.self, alignment: .center) {
//                    let index = $0
//                    let dat = arr[index]
//                    Box(url: dat["url"] as? URL, user: dat["user"] as! String, id: dat["id"] as! String, type: type )
//                        .onTapGesture {
//                            withAnimation {
//                                disabled = false
//                            }
//                        }.transition(.opacity)
//                    if (arr.count - 1 == index) {
//                        let rem = 4 - (arr.count % 4)
//                        if (rem != 4) {
//                            ForEach(1...rem, id: \.self) { _ in
//                                Box()
//                            }
//                        }
//
//                    }
//                }
//                .frame(width:UIScreen.screenWidth)
//
//            } else {
//                Text("")
//                    .onAppear {
//                        withAnimation {
//                            refresh = true
//                        }
//                    }.onChange(of: arr.count) { _ in
//                        withAnimation {
//                            refresh = !refresh
//                        }
//                    }
//            }
//
//
//        }
//        .onAppear {
//            DataHandler.shared.addNextUserPoster = addItem
//            DataHandler.shared.getUserPosters(type: type)
//        }
//        .onChange(of: $type.wrappedValue) { _ in
//            arr = []
//            withAnimation {
//                refresh = false
//            }
//            DataHandler.shared.getUserPosters(type: type)
//        }
//    }
//}

struct EffectSelection_Previews: PreviewProvider {
    
    static var previews: some View {
        EffectSelection(disabled: .constant(true), changeSelection: {_ in})
    }
}

