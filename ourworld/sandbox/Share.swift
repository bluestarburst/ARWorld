//
//  Share.swift
//  ourworld
//
//  Created by Bryant Hargreaves on 2/9/23.
//

import SwiftUI

struct Photo: Transferable {
    static var transferRepresentation: some TransferRepresentation {
        ProxyRepresentation(exporting: \.image)
    }
    public var image: Image
    public var caption: String
}

struct Share: View {
    @Binding var img: Photo
    var body: some View {
        ShareLink(
            item: img,
            preview: SharePreview(
                "ourworlds!",
                image: img.image))
        {
            Image(systemName: "square.and.arrow.up")
                .imageScale(.medium)
                .font(.title2)
                .foregroundColor(.white)
                .padding(10)
                .background(Color(.white).opacity(0.1))
                .clipShape(Circle())
                .padding(.vertical,5)
        }
    }
}

//struct Share_Previews: PreviewProvider {
//    static var previews: some View {
//        Share()
//    }
//}



//struct UIKitActivityView: UIViewControllerRepresentable {
//  @Binding var isPresented: Bool
//
//  let data: [Any]
//  let subject: String?
//  let message: String?
//
//  func makeUIViewController(context: Context) -> UIViewController {
//    HolderViewController(control: self)
//  }
//
//  func updateUIViewController(_ uiViewController: UIViewController, context: Context) {
//    let activityViewController = UIActivityViewController(
//      activityItems: data,
//      applicationActivities: nil
//    )
//
//    if isPresented && uiViewController.presentedViewController == nil {
//      uiViewController.present(activityViewController, animated: true)
//    }
//
//    activityViewController.completionWithItemsHandler = { (_, _, _, _) in
//      isPresented = false
//    }
//  }
//
//  class HolderViewController: UIViewController, UIActivityItemSource {
//    private let control: UIKitActivityView
//
//    init(control: UIKitActivityView) {
//      self.control = control
//      super.init(nibName: nil, bundle: nil)
//    }
//
//    required init?(coder: NSCoder) {
//      fatalError("init(coder:) has not been implemented")
//    }
//
//    func activityViewControllerPlaceholderItem(_ activityViewController: UIActivityViewController) -> Any {
//      control.message ?? ""
//    }
//
//    func activityViewController(_ activityViewController: UIActivityViewController,
//                                itemForActivityType activityType: UIActivity.ActivityType?) -> Any? {
//      control.message
//    }
//
//    func activityViewController(_ activityViewController: UIActivityViewController,
//                                subjectForActivityType activityType: UIActivity.ActivityType?) -> String {
//      control.subject ?? ""
//    }
//  }
//}
//
//
//@available(iOS 13.0, macOS 10.15, *)
//struct ShareLinkCompat<Item, Label>: View where Item : Transferable, Label : View {
//  let item: Item
//  let subject: String?
//  let message: String?
//  @ViewBuilder let label: () -> Label
//
//  @State private var isPresented = false
//
//  init(item: Item,
//       subject: String? = nil,
//       message: String? = nil,
//       @ViewBuilder label: @escaping () -> Label) {
//    self.item = item
//    self.subject = subject
//    self.message = message
//    self.label = label
//  }
//
//  var body: some View {
//    Button(action: {
//      isPresented = true
//    }, label: label)
//    .background(UIKitActivityView(isPresented: $isPresented,
//                                  data: [item, subject ?? ""],
//                                  subject: subject,
//                                  message: message))
//  }
//}
//
//struct DefaultShareLinkLabelCompat: View {
//  let title: String?
//
//  var body: some View {
//    HStack {
//      Image(systemName: "square.and.arrow.up")
//      Text(title ?? "Share")
//    }
//  }
//}
//
//extension ShareLinkCompat where Label == DefaultShareLinkLabelCompat {
//  init(_ title: String? = nil,
//       item: Item,
//       subject: String? = nil,
//       message: String? = nil) {
//    self.init(item: item,
//              subject: subject,
//              message: message,
//              label: { DefaultShareLinkLabelCompat(title: title) })
//  }
//}
