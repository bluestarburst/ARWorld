//
//  ContentView.swift
//  sandbox
//
//  Created by David Peicho on 1/21/21.
//

import SwiftUI
import Firebase
import FirebaseAuth

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
    @Published var verificationError = false
    @Published var phoneError = false
    
    @Published var isLoading = false
    @Published var isLoggedIn = false //should be false
    @Published var shouldSkipCreateAcc = "c" //should be c      b is createprofile
    @Published var verifyScreen = false
    
    @Published var initializing = true
    @Published var loading = false
    
    @Published var changePage: () -> Void = {}
    
    func authPhone() {
        withAnimation {
            self.loading = true
        }
        
        
                Auth.auth().settings?.isAppVerificationDisabledForTesting = false // false
        //
        var newCountryCode = countryCode.replacingOccurrences(of: "+", with: "")
        print(newCountryCode)
        
        var newPhNumber = phNumber.replacingOccurrences(of: "(", with: "")
        newPhNumber = newPhNumber.replacingOccurrences(of: ")", with: "")
        newPhNumber = newPhNumber.replacingOccurrences(of: "-", with: "")
        print(newPhNumber)
        
//        UnityBridge.getInstance().api.getPhoneResult = {result in
//            switch (result) {
//            case "verify":
//                withAnimation {
//                    self.loading = false
//                    self.phoneError = false
//                    self.verificationError = false
//                    self.isViewing = false
//                    self.verifyScreen = true
//                }
//                break
//            case "error":
//                withAnimation {
//                    self.loading = false
//                    self.phoneError = true
//                }
//                break
//            default:
//                print("strange")
//            }
//        }
//        UnityBridge.getInstance().api.phoneLogin(cc: newCountryCode, ph: newPhNumber)
        //
                PhoneAuthProvider.provider().verifyPhoneNumber("+\(newCountryCode + newPhNumber)",uiDelegate: nil) {
                    ID, err in
                    if let error = err{
                        withAnimation {
                            self.loading = false
                            self.phoneError = true
                        }
                        return
                    }
        
                    print(ID)
        
                    self.ID = ID!
                    withAnimation {
                        self.loading = false
                        self.phoneError = false
                        self.verificationError = false
                        self.isViewing = false
                        self.verifyScreen = true
                    }
                }
    }
    
    func LoginUser() {
        withAnimation {
            self.loading = true
        }
        self.initializing = true
                let credential = PhoneAuthProvider.provider().credential(withVerificationID: self.ID, verificationCode: self.verificationCode)
        
                Auth.auth().signIn(with: credential, completion: { result, err in
                    if err != nil {
                        withAnimation {
                            self.loading = false
                            self.verificationError = true
                        }
                        return
                    }
        
                    print("success")
        
                    //                DataHandler.shared.load()
                    DataHandler.shared.getUID()
        
                    self.isLoggedIn = true
        
                    withAnimation{
                        self.loading = false
                        self.verifyScreen = false
                        self.isViewing = true
                        self.changePage()
                    }
        
        
                })
    }
}

struct AuthView: View {
    
    enum Field: Hashable {
        case myField
        case verification
    }
    
    @Binding var page: Int
    
    
    @FocusState private var focusedField: Field?
    @StateObject var model = LoginViewModel()
    
    var body: some View {
        ZStack {
            if (model.isViewing) {
                VStack {
                    Spacer()
                    if (model.phoneError == true) {
                        HStack {
                            Image(systemName: "exclamationmark.triangle")
                                .imageScale(.large)
                                .foregroundColor(.red)
                            Text("The country code or phone number is invalid! Please check the values and try again")
                                .foregroundColor(.red)
                        }
                        .padding(.bottom,10)
                        .transition(.opacity)
                    }
                    Text("Enter your phone number to get started!")
                        .onAppear {
                            model.changePage = {page = 1}
                        }
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
                        
                        TextField("Phone Number", text:$model.phNumber)
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
                    
                    Button(action: {withAnimation {focusedField = nil;model.authPhone()}}, label: {
                        Text("login")
                            .fontWeight(.bold)
                            .foregroundColor(.white)
                            .padding(.vertical)
                            .frame(maxWidth: .infinity)
                            .background(Color.pink)
                            .cornerRadius(8)
                    })
                    .disabled(model.countryCode == "" || model.phNumber == "" || model.isLoading)
                    .opacity(model.countryCode == "" || model.phNumber == "" ? 0.6 : 1)
                    .padding(.top,10)
                    .padding(.bottom,15)
                    .padding(.horizontal)
                    Text("Standard data rates may apply blah blah blah")
                        .fontWeight(.light)
                        .foregroundColor(.gray)
                    Text("By using this app you agree to the [EULA](https://bluestarburst.github.io/storyboard-public/eula/) and [Privacy Policy](https://bluestarburst.github.io/storyboard-public/) found here")
                        .fontWeight(.light)
                        .foregroundColor(.gray)
                    Spacer()
                    
                }
                .transition(.move(edge: .bottom))
            }
            if (model.verifyScreen == true) {
                VStack {
                    
                    HStack {
                        Button(action: {withAnimation{model.isViewing = true;model.verifyScreen = false}}) {
                            Image(systemName: "chevron.left")
                                .imageScale(.large)
                                .foregroundColor(.white)
                        }
                        Spacer()
                    }
                    Spacer()
                    
                    if (model.verificationError == true) {
                        HStack {
                            Image(systemName: "exclamationmark.triangle")
                                .imageScale(.large)
                                .foregroundColor(.red)
                            Text("That code is invalid! Please check the code and try again")
                                .foregroundColor(.red)
                        }
                        .padding(.bottom,10)
                        .transition(.opacity)
                    }
                    
                    
                    Text("You 'should' be recieving a six-digit verification code via text")
                        .padding(.bottom, 12)
                        .multilineTextAlignment(.center)
                        .foregroundColor(Color.gray)
                    
                    Text("Enter the code below to get log in!")
                        .padding(.bottom, 12)
                    TextField("six-digit code", text:$model.verificationCode)
                        .keyboardType(.numberPad)
                        .padding(.vertical,12)
                    
                        .padding(.horizontal)
                        .background(
                            RoundedRectangle(cornerRadius: 8)
                                .stroke(model.verificationCode.count != 6 ? Color.gray :
                                            Color.pink,lineWidth: 1.5
                                       )
                        )
                        .focused($focusedField, equals: .verification)
                        .onAppear {
                            focusedField = .verification
                        }
                    Button(action: model.LoginUser, label: {
                        Text("verify")
                            .fontWeight(.bold)
                            .foregroundColor(.white)
                            .padding(.vertical)
                            .frame(maxWidth: .infinity)
                            .background(Color.pink)
                            .cornerRadius(8)
                    })
                    .disabled(model.verificationCode.count != 6 || model.isLoading)
                    .opacity(model.verificationCode.count != 6 ? 0.6 : 1)
                    .padding(.top,10)
                    .padding(.bottom, 40)
                    Spacer()
                }
                .padding()
                .transition(.move(edge: .bottom))
            }
        }
    }
}
