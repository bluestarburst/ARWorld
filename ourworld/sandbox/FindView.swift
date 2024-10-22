//
//  ElementSelection.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 1/3/23.
//

import SwiftUI
import WrappingHStack

struct FindView: View {
    
    enum Field: Hashable {
        case myField
    }
    
    @State private var scrollWidth = CGFloat.zero
    
    @State private var type: String = "stickers"
    
    @State private var search: String = ""
    
    @FocusState private var focusField: Field?
    
    @Binding var bool: Bool
    
    var body: some View {
        VStack {
            Spacer()
            
            VStack {
                
                VStack {
                    GeometryReader { geometry in
                        ScrollView(.horizontal, showsIndicators: false) {
                            HStack {
                                
                                Button(action: {withAnimation{type="stickers"}}, label: {
                                    Image(systemName: "moon.stars.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "stickers" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "stickers")
                                
                                Button(action: {withAnimation{type="posters"}}, label: {
                                    Image(systemName: "doc.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "posters" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "posters")
                                
                                Button(action: {withAnimation{type="images"}}, label: {
                                    Image(systemName: "photo.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "images" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "images")
                                Button(action: {withAnimation{type="objects"}}, label: {
                                    Image(systemName: "cube.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "objects" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "objects")
                                Button(action: {withAnimation{type="filter"}}, label: {
                                    Image(systemName: "sparkle")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "filter" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "filter")
                                Button(action: {withAnimation{type="light"}}, label: {
                                    Image(systemName: "lightbulb.fill")
                                })
                                .imageScale(.medium)
                                .font(.title)
                                .foregroundColor(type == "light" ? .pink : .white)
                                .padding(.horizontal,10)
                                .disabled(type == "light")
                            }
                            .padding(.horizontal,25)
                            .frame(minWidth: geometry.size.width)
                        }
                    }.frame(height: 40)
                    
                    HStack {
                        TextField("Search Tags", text: $search)
                            .padding(10)
                            .background(
                                Color(.gray)
                                    .opacity(0.2)
                                    .cornerRadius(16)
                            )
                            .padding(.leading, 20)
                            .padding(.vertical, 7)
                            .focused($focusField, equals: .myField)
                            .onTapGesture {
                                focusField = .myField
                            }
                        Button(action: {withAnimation{bool=true}}, label: {
                            Image(systemName: "xmark")
                        })
                        .imageScale(.medium)
                        .font(.title)
                        .foregroundColor(.white)
                        .padding(.horizontal,10)
                    }
                    
                    ScrollView {
                        FindLoop(type: $type)
                    }
                }
            }
            .background(.black)
            .transition(.move(edge: .bottom))
            .onTapGesture {
                focusField = nil
            }
            
        }
        
    }
}

struct FindLoop: View {
    
    @Binding var type: String
    
//    @Binding var disabled: Bool
    
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
                    Box(url: dat["url"] as? URL, user: dat["user"] as! String, id: dat["id"] as! String, type: type )
                        .onTapGesture {
                            withAnimation {
                                //                            disabled = false
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
                
                /*
                 WrappingHStack {
                     ForEach(0...<arr.count, id:\.self) {
                         let index = $0
                         let dat = arr[index]
                         Box(url: dat["url"] as? URL, user: dat["user"] as! String, id: dat["id"] as! String, type: type )
                             .onTapGesture {
                                 withAnimation {
                                     //                            disabled = false
                                 }
                             }.transition(.opacity)
                     }
                 }
                 */
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
            DataHandler.shared.addNextTop = addItem
            DataHandler.shared.getTopThumbs(type: type)
        }
        .onChange(of: $type.wrappedValue) { _ in
            arr = []
            withAnimation {
                refresh = false
            }
            DataHandler.shared.getTopThumbs(type: type)
        }
    }
}

struct FindView_Previews: PreviewProvider {
    
    static var previews: some View {
        FindView(bool: .constant(false))
    }
}

extension UIScreen {
    static var screenWidth = UIScreen.main.bounds.size.width
    static var screenHeight = UIScreen.main.bounds.size.height
    static var screenSize = UIScreen.main.bounds.size
}
