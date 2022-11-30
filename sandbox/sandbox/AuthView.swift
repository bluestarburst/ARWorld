//
//  ContentView.swift
//  sandbox
//
//  Created by David Peicho on 1/21/21.
//

import SwiftUI

class LoginViewModel: ObservableObject {
    @Published var countryCode = ""
    @Published var phNumber = ""
    @Published var oldCountryCode = ""
    @Published var oldNumber = ""
    
    @Published var isViewing = true
    
    
    
    @Published var showAlert = false
    @Published var errorMsg = ""
    
    @Published var ID = ""
    @Published var verificationCode = ""
    
    @Published var isLoading = false
    @Published var isLoggedIn = false //should be false
    @Published var shouldSkipCreateAcc = "c" //should be c      b is createprofile
    @Published var verifyScreen = false
    
    @Published var initializing = true
    
//    func authPhone() {
//        Auth.auth().settings?.isAppVerificationDisabledForTesting = true // false
//        
//        var newCountryCode = countryCode.replacingOccurrences(of: "+", with: "")
//        print(newCountryCode)
//        
//        PhoneAuthProvider.provider().verifyPhoneNumber("+\(countryCode + phNumber)",uiDelegate: nil) {
//            ID, err in
//            if let error = err{
//                self.errorMsg = error.localizedDescription
//                self.showAlert.toggle()
//                return
//            }
//            
//            self.ID = ID!
//            self.verifyScreen = true
//            
//        }
//    }
}

struct AuthView: View {
    
    enum Field: Hashable {
        case myField
    }
    
    @FocusState private var focusedField: Field?
    @StateObject var model = LoginViewModel()
    var body: some View {
        if (model.isViewing) {
            VStack {
                Spacer()
                Text("Enter your phone number to get started!")
                HStack(spacing: 15) {
                    TextField("+1", text:$model.countryCode)
                        .focused($focusedField, equals: .myField)
                        .keyboardType(.numberPad)
                        .padding(.vertical,12)
                        .padding(.horizontal)
                        .frame(width: 50)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .stroke(model.countryCode == "" ? Color.gray :
                                            Color.pink,lineWidth: 1.5
                                       )
                        )
                        .onChange(of: model.countryCode) {
                            if (model.oldCountryCode.count == 0 && $0.count == 1) {
                                model.countryCode = "+" + $0
                            } else if (model.oldNumber.count == 2 && $0.count == 1) {
                                model.countryCode = ""
                            }
                            model.oldCountryCode = model.countryCode
                        }
                    
                    TextField("(650)-555-1234", text:$model.phNumber)
                        .focused($focusedField, equals: .myField)
                        .keyboardType(.numberPad)
                        .padding(.vertical,12)
                        .padding(.horizontal)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .stroke(model.phNumber == "" ? Color.gray :
                                            Color.pink,lineWidth: 1.5
                                       )
                        )
                        .onChange(of: model.phNumber) {
                            if (model.oldNumber.count == 0 && $0.count == 1) {
                                model.phNumber = "(" + $0
                            } else if (model.oldNumber.count == 4 && $0.count == 5) {
                                var temp = $0
                                temp.insert(")", at: temp.index(temp.endIndex, offsetBy: -1))
                                temp.insert("-", at: temp.index(temp.endIndex, offsetBy: -1))
                                model.phNumber = temp
                            } else if (model.oldNumber.count == 9 && $0.count == 10) {
                                var temp = $0
                                temp.insert("-", at: temp.index(temp.endIndex, offsetBy: -1))
                                model.phNumber = temp
                            } else if (model.oldNumber.count == 11 && $0.count == 10) {
                                var temp = $0
                                temp = String(temp[..<temp.index(temp.endIndex, offsetBy: -1)])
                                model.phNumber = temp
                            } else if (model.oldNumber.count == 7 && $0.count == 6) {
                                var temp = $0
                                temp = String(temp[..<temp.index(temp.endIndex, offsetBy: -1)])
                                temp = String(temp[..<temp.index(temp.endIndex, offsetBy: -1)])
                                model.phNumber = temp
                            } else if (model.oldNumber.count == 2 && $0.count == 1) {
                                model.phNumber = ""
                            } else if (model.oldNumber.count == 13 && $0.count == 14) {
                                focusedField = nil
                            } else if ($0.count > 14) {
                                model.phNumber = model.oldNumber
                            }
                            model.oldNumber = model.phNumber
                        }
                }
                .padding()
                
                Button(action: {withAnimation {focusedField = nil;}}, label: {
                    Text("login")
                        .fontWeight(.bold)
                        .foregroundColor(.white)
                        .padding(.vertical)
                        .frame(maxWidth: .infinity)
                        .background(Color.pink)
                        .cornerRadius(8)
                })
                .disabled(model.countryCode == "" || model.phNumber == "")
                .opacity(model.countryCode == "" || model.phNumber == "" ? 0.6 : 1)
                .padding(.top,10)
                .padding(.bottom,15)
                .padding(.horizontal)
                Text("Standard data rates may apply blah blah blah")
                    .fontWeight(.light)
                    .foregroundColor(.gray)
                Spacer()
                
            }
            .transition(.move(edge: .bottom))
        }
    }
}
