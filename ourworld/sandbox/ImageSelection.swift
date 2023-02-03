//
//  ElementSelection.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 1/3/23.
//

import WrappingHStack
import SwiftUI

struct Box: View {
    
    @State var url: URL? = nil
    @State var user: String = ""
    @State var id: String = ""
    
    @State var type: String = "posters"
    
    @State var fav: Bool = false
    
    var body: some View {
        Button(action: {if (url != nil) {DataHandler.shared.setPreview(type, user, id, url!)}
        }, label: {
            ZStack {
                Color(.gray).opacity(0.5)
                if (url != nil) {
                    AsyncImage(
                        url: url!,
                        placeholder: {
                            ProgressView()
                        }
                    )
                }
            }
        })
        .frame(width: 70, height: 70)
        .clipShape(RoundedRectangle(cornerRadius: 10))
        .overlay(RoundedRectangle(cornerRadius: 10)
            .stroke(fav ? .yellow : .white, lineWidth: 1)
        )
        .opacity(url == nil ? 0.5 : 1)
        .padding(5)
    }
}

struct ImageSelection: View {
    
    @State var minOffset = CGFloat(80)
    @State var offset = CGFloat(80)
    @State var prevOffset = CGFloat(80)
    
    @State var showImagePicker = false
    
    @Binding var disabled: Bool
    
    @State var selectedImage: UIImage = UIImage()
    
    @State private var scrollWidth = CGFloat.zero
    
    @State private var type: String = "stickers"
    
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
                        .foregroundColor(.pink)
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
                        Button(action: {showImagePicker = true;withAnimation{favorite = true}}, label: {
                            HStack {
                                Spacer()
                                switch type {
                                case "posters":
                                    Text("add new poster")
                                        .foregroundColor(.white)
                                case "stickers":
                                    Text("add new sticker")
                                        .foregroundColor(.white)
                                case "images":
                                    Text("add new image")
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
                        ImageLoop(type: $type, disabled: $disabled, fav: $favorite)
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

struct ImageLoop: View {
    
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
            DataHandler.shared.addNextUserPoster = addItem
            DataHandler.shared.getUserPosters(type: type, fav: fav)
        }
        .onChange(of: $type.wrappedValue) { _ in
            arr = []
            withAnimation {
                refresh = false
            }
            DataHandler.shared.getUserPosters(type: type, fav: fav)
        }
        .onChange(of: $fav.wrappedValue) { _ in
            arr = []
            withAnimation {
                refresh = false
            }
            DataHandler.shared.getUserPosters(type: type, fav: fav)
        }
    }
}

struct ImageSelection_Previews: PreviewProvider {
    
    static var previews: some View {
        ImageSelection(disabled: .constant(true), changeSelection: {_ in})
    }
}

struct ImagePicker: UIViewControllerRepresentable {
    @Environment(\.presentationMode) private var presentationMode
    var sourceType: UIImagePickerController.SourceType = .photoLibrary
    @Binding var selectedImage: UIImage
    @Binding var type: String

    func makeUIViewController(context: UIViewControllerRepresentableContext<ImagePicker>) -> UIImagePickerController {

        let imagePicker = UIImagePickerController()
        imagePicker.allowsEditing = false
        imagePicker.sourceType = sourceType
        imagePicker.delegate = context.coordinator

        return imagePicker
    }

    func updateUIViewController(_ uiViewController: UIImagePickerController, context: UIViewControllerRepresentableContext<ImagePicker>) {

    }

    func makeCoordinator() -> Coordinator {
        Coordinator(self)
    }

    final class Coordinator: NSObject, UIImagePickerControllerDelegate, UINavigationControllerDelegate {

        var parent: ImagePicker

        init(_ parent: ImagePicker) {
            self.parent = parent
        }

        func imagePickerController(_ picker: UIImagePickerController, didFinishPickingMediaWithInfo info: [UIImagePickerController.InfoKey : Any]) {

            if let image = info[UIImagePickerController.InfoKey.originalImage] as? UIImage {
                parent.selectedImage = image
                DataHandler.shared.storeImg(img: image, type: parent.type)
            }

            parent.presentationMode.wrappedValue.dismiss()
        }

    }
}
