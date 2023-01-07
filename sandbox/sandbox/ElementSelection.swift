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
    
    var body: some View {
        Button(action: {if (url != nil) {UnityBridge.getInstance().api.addObject(type: "poster", user: user, id: id)}}, label: {
            ZStack {
                if (url != nil) {
                    AsyncImage(
                        url: url!,
                        placeholder: {
                            ProgressView()
                        }
                    )
                }
                Color(.gray).opacity(0.5)
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

struct ElementSelection: View {
    
    @State var minOffset = CGFloat(80)
    @State var offset = CGFloat(80)
    @State var prevOffset = CGFloat(80)
    
    @State var showImagePicker = false
    
    @Binding var disabled: Bool
    
    @State var selectedImage: UIImage = UIImage()
    
    
    
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
                    
                    ScrollView {
                        Button(action: {showImagePicker = true}, label: {
                            HStack {
                                Spacer()
                                Text("add new image")
                                    .foregroundColor(.white)
                                Spacer()
                            }
                            .padding()
                            .background(.gray.opacity(0.25))
                        })
                        .frame(width: .infinity)
                        .cornerRadius(16)
                        .padding(.vertical, 5)
                        .padding(.horizontal, 25)
                        ImageLoop()
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
            ImagePicker(sourceType: .photoLibrary, selectedImage: $selectedImage)
        }
        
    }
}

struct ImageLoop: View {
    
    @State var posters: [String:URL] = [:]
    @State var postersL: [URL] = []
    @State var postersI: [String] = []
    @State var postersU: [String] = []
    
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
    }
    
    var body: some View {
        VStack {
            ForEach(0..<5) { index10 in
                HStack {
                    Spacer()
                    ForEach(0..<4) {index in
                        let temp = (index10 * 4) + index
                        if (postersL.count > temp) {
                            Box(url: postersL[temp], user: postersU[temp], id: postersI[temp] )
                        } else {
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

struct ElementSelection_Previews: PreviewProvider {
    
    static var previews: some View {
        ElementSelection(disabled: .constant(true))
    }
}

struct ImagePicker: UIViewControllerRepresentable {
    @Environment(\.presentationMode) private var presentationMode
    var sourceType: UIImagePickerController.SourceType = .photoLibrary
    @Binding var selectedImage: UIImage

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
                DataHandler.shared.storeImg(img: image)
            }

            parent.presentationMode.wrappedValue.dismiss()
        }

    }
}
