//
//  ElementSelection.swift
//  sandbox
//
//  Created by Bryant Hargreaves on 1/3/23.
//

import SwiftUI

struct Box: View {
    
    @State var url: URL? = nil
    @State var user: String = ""
    @State var id: String = ""
    
    @State var type: String = "posters"
    
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
            .stroke(.white, lineWidth: 1)
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
                    }.frame(height: 40)
                    
                    ScrollView {
                        Button(action: {showImagePicker = true}, label: {
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
                        ImageLoop(type: $type, disabled: $disabled)
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
    
    @State var posters: [String:URL] = [:]
    @State var postersL: [URL] = []
    @State var postersI: [String] = []
    @State var postersU: [String] = []
    
    @State var stickers: [String:URL] = [:]
    @State var stickersL: [URL] = []
    @State var stickersI: [String] = []
    @State var stickersU: [String] = []
    
    @State var images: [String:URL] = [:]
    @State var imagesL: [URL] = []
    @State var imagesI: [String] = []
    @State var imagesU: [String] = []
    
    @Binding var type: String
    
    @Binding var disabled: Bool
    
    func updatePosters() {
        var tempPost = DataHandler.shared.posters
        for id in tempPost.keys {
            if (posters[id] == nil) {
                posters[id] = tempPost[id]
                postersL.append(tempPost[id]!)
                postersI.append(id)
                postersU.append(DataHandler.shared.posterData[id]!)
            }
        }
        
        tempPost = DataHandler.shared.stickers
        for id in tempPost.keys {
            if (stickers[id] == nil) {
                stickers[id] = tempPost[id]
                stickersL.append(tempPost[id]!)
                stickersI.append(id)
                stickersU.append(DataHandler.shared.stickerData[id]!)
            }
        }
        
        tempPost = DataHandler.shared.images
        for id in tempPost.keys {
            if (images[id] == nil) {
                images[id] = tempPost[id]
                imagesL.append(tempPost[id]!)
                imagesI.append(id)
                imagesU.append(DataHandler.shared.imageData[id]!)
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
                        case "posters":
                            if (postersL.count > temp) {
                                Box(url: postersL[temp], user: postersU[temp], id: postersI[temp], type: type )
                                    .onTapGesture {
                                        withAnimation {
                                            disabled = false
                                        }
                                    }
                            } else {
                                Box()
                            }
                        case "stickers":
                            if (stickersL.count > temp) {
                                Box(url: stickersL[temp], user: stickersU[temp], id: stickersI[temp], type: type )
                                    .onTapGesture {
                                        withAnimation {
                                            disabled = false
                                        }
                                    }
                            } else {
                                Box()
                            }
                        case "images":
                            if (imagesL.count > temp) {
                                Box(url: imagesL[temp], user: imagesU[temp], id: imagesI[temp], type: type )
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
            DataHandler.shared.updatePosters = updatePosters
            DataHandler.shared.getUserPosters()
        }
    }
}

struct ImageSelection_Previews: PreviewProvider {
    
    static var previews: some View {
        ImageSelection(disabled: .constant(true))
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
