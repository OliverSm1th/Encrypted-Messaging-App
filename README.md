<h1 align="center">
  <img src="Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Resources/mipmap-xhdpi/messaging_icon.png">

  Encrypted Messaging App
</h1>

<p align="center">
<b>A-Level Computer Science NEA Project</b> submitted in <b>March 2022</b>
</p>


<details>
    <summary>Project Objectives</summary>

1. Create encryption algorithms which would allow secure communication between 2 parties 

    a. Create a Diffie-Hellman algorithm that allows secure private key communication between the 2 users

    - Use a random number generator to generate a and b and use pre-set values for p and g

    - Use these values to calculate the public keys for both users and, after sharing them to each other, use them to calculate the shared master key

    b. Create a secure symmetric algorithm that encrypts and decrypts the message at both ends using the same secure key, using the AES encryption standard

2. The users must be able to communicate with each other

    a. The servers should store the following information in an organised manner and allow the appropriate users to retrieve it from the server:
    - User Details (username, password, requests, chat’s)
    - Chat Info (users, messages, encryption info)

    b. The user must be able to send messages in a chat and the other user must be able to receive these messages

    c. It must allow the user to send and accept friend requests before sending messages (Diffie-hellman initialisation)

3. Display all information to the user in a simple and intuitive manner.

    a. It must have a login screen with entry fields and a submit button.
    - It should have 2 entry fields, one for the email address and one for the password. When the user presses submit, it should check the email and password to check if it’s valid 

    b. It must have a registration page with entry fields and a register button
    - It should have 4 entry fields for username, email address, password and confirm password. When the user presses register, it should check the values to see that they’re valid

    c. It must display current chats in a menu and allow the user to select one
    
    d. It must display chat history with a recipient with an option for the user to send a message

    e. It could display information about the encryption process to make it clear to the user that their message is secure

    ---
</details>

## Design 📱

### User Interface

⠀⠀Login Page ⠀|Register Page  |Forgot Password  |⠀⠀Chat List⠀⠀|Friend Request List  |Chat Message History
:-------------------------:|:-------------------------:|:-------------------------:|:-------------------------:|:-------------------------:|:-------------------------:
<img src="https://i.imgur.com/pJQuLi4.png" width="100%"> | <img src="https://i.imgur.com/NFdRdvV.png" width="100%"> | <img src="https://i.imgur.com/bm9VwlZ.png" width="100%">|<img src="https://i.imgur.com/We7ihqu.png" width="100%"> |  <img src="https://i.imgur.com/0zRu1Ht.png" width="100%"> | <img src="https://i.imgur.com/seQdTBp.png" width="100%">


<img  align="right" src="https://i.imgur.com/cXqZnyZ.png" height="250" >

### Friend Request Structure
It's organised so that both users get to initialise the Diffie Hellman, generating a shared public key for AES encryption.


<details>
    <summary><b>Diagram Explained</b></summary>

1. When <u>UserA</u> wants to send a friend request to UserB, User A initialises their Diffie-Hellman and saves the private key in the phone’s storage. It then creates a PendingRequest object with the sourceUserID (A’s ID), targetUserID (B’s ID) and the Diffie-Hellman public encryptionInfo (*global, prime, A-public-key*). 
2. When <u>UserB</u> detects that it has a pending request, it fetches the information and adds it to the *requestList* with all the relevant information to show to the user.
3. If the user chooses to accept the request, it uses the information from the pending request to generate a Diffie-Hellman session, saving its private key in the phone’s storage. UserB then creates a new chat on the server with all the encryption information and get’s its chatID. It then deletes the pending request and creates an AcceptedRequest object with the updated encryptionInfo (*global, prime, A-public-key, B-public-key*) and the newChatID. Finally, UserB adds the chatID to User B’s chat list on the server and adds the chat to chatList for the user to see.
4. When <u>UserA</u> detects that it has an acceptedRequest (using a listener), it uses the chatID to establish the chat on the device and generates the shared key using the encryptionInfo and the saved private key. UserA then deletes the acceptedRequest object and adds the newchatID to the user on the server. Finally, UserA adds the chat to the chatList for the user to see.

---
</details>
<br><br><br><br>

### Server Structure
This project uses a Cloud Firestore from [Google Firebase](https://firebase.google.com/docs/firestore) to store all user data, as well as requests, messages and chats. The table below describes how the data is stored with each item row being a separate collection. Some collections have access restricted (e.g requests only readable by target user). This is enforced using the 'rules' feature in Firestore 
<details>
    <summary><b>Firestore Structure Table</b></summary>

|**Writing/Changing Values**|**Server**      |**Fetching Data**     |
|:---------|:-------------------------:|----------------------:|
|  |**Pending Requests**|*Only accessible by targetUser*|
|**Created** when a user (*User A*) sends a friend request to another user (*User B*)<br>**Deleted** when a user (*User B*) has accepted the friend request | <p align="left">TargetUserID (*User B ID*) : <u>Request</u></p> <u>Request:</u> sourceUser (*User A*, *User*), encryptionInfo (*incomplete, DHData*) | **Listener** created by *targetUser* (*userB*) to monitor any pending requests for the user<br> **On Request Received**: Request is added to RequestList and displayed to the user
|  |**Accepted Requests**|*Only accessible by sourceUser*|
|**Created** when a user (*User B*) accepts a pending friend request<br>**Deleted** when a user (*User A*) receives the acceptedRequest  | <p align="left">SourceUserID (*User A ID*) : <u>Accepted Request</u></p> <u>Accepted Request:</u> targetUser (*User B*, *User*), encryptionInfo (*complete, DHData*), newChatID (*string*) | **Listener** created by *sourceUser* (*userA*) to monitor and handle any accepted requests<br> **On Accepted Request Received**: Accepted Request is deleted and the new chat is established on the device. The chatID is also added to the user on the server
|  |**Chats**|*Only accessible by users in the chat*|
|**Created** when a user (*User B*) accepts a pending friend request | <p align="left">ChatID : <u>Chat</u></p> <u>Chat:</u> encryptionInfo (*DHData*), title (*string*), users (*User[]*), messages (*Message[]*) | **Listener** created by a user when chatID is added to the user in the server<br> **On Chat Updated/Created**: Update/Create the chat object
|  |**Messages[]**|     |
|**Created** when a user sends a message into a chat. Stores the encrypted contents of the message|  <u>Message:</u> encryptedContent (*string*), sendDate (*dateTime*), author (*User*), readUsers (*User[]*) | **On Message Updated/Created**: Update/Create the message object and therefore update the parent chat object so it's displayed to the user
|  |**User**|     |*Only accessible by the user*|
|**Created** when a user registers their account | <p align="left">UserID : <u>User</u></p> <u>User:</u> chatsID (*string[]*), username (*string*) | **Listener** created by the user to detect when the chatsID is changed<br> **On chatsID Updated**: Initialise the new chat to ChatList
|  |**PublicUser**|     |*Accessible by anyone*|
|**Created** when a user registers their account | <p align="left">UserID : username (*string*)</p> | **Accessed** when a user wants to get a username from a userID
|  |**PublicUserID**|     |*Accessible by anyone*|
|**Created** when a user registers their account | <p align="left">Username : userID (*string*)</p> | **Accessed** when a user wants to get a userID from a username (send request based on a username)
</details>

<details>
    <summary><b>Rules</b> from Cloud Firestore</summary>

```javascript
rules_version = '2';
service cloud.firestore {
match /databases/{database}/documents {
    match /users/{userID}{
        allow read, write: if request.auth != null && request.auth.uid== userID;
    }
    match /requests/accepted/{userID}/{document=**}{
    allow write 
        allow read: if request.auth != null && request.auth.uid== userID;
    }
    match /requests/pending/{userID}/{document=**}{
    allow write 
        allow read, write: if true;
    }
    match /chats/{chatID}{
        allow read, write: if request.auth != null && (resource == null || request.auth.uid in resource.data.userIDs);
    }
    match /usersPublic/{document=**} {
    allow read, write
    }
    match  /usersPublicID/{document=**}{
    allow read, write
    }
    match /other/{document=**}{
        allow read, write
    }
}
}
```
</details>

## Implementation 💻 
### Matrix Multiplication
*Location: [Encryption/AES/MixColumns](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/Encryption.cs#L665)  +  [InvMixColumns](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/Encryption.cs#L715)*

<details>
    <summary><b>MixColumns Recap</b></summary>

For the MixColumns step of the AES encryption algorithm, you have to multiply the 4x4 byte array by a predefined 4x4 byte array (see AES walkthrough). \
$\begin{bmatrix}b_{0j}\\b_{1j}\\b_{2j}\\b_{3j}\end{bmatrix}=\begin{bmatrix}2&3&1&1\\1&2&3&1\\1&1&2&3\\3&1&1&2\end{bmatrix}\begin{bmatrix}a_{0j}\\a_{1j}\\a_{2j}\\a_{3j}\end{bmatrix}\qquad 0\le j\le 3$\
This is done by looping through every column of the message and calculating each value (e.g $b_0$) through the following equation:  $b_0=2a_0+3a_1+1a_2+1a_1$, with you moving down the rows of the pre-defined matrix as you calculate subsequent values: \
$\qquad b_1=1a_0+2a_1+3a_2+1a_3\qquad b_2=1a_0+1a_1+2a_2+3a_3…$

---
</details>
<br>

As we’re working with bytes (*8 bit values*), each multiplication must produce a result which is contained within the byte. This is called **finite field multiplication** which is very complex and most AES implementations use pre-calculated lookup tables to get values (see *Galois Multiplication lookup tables*). However, I wanted to be able to calculate these values instead of just reading them off a table and so, after doing some research, I discovered that it would be easier to implement multiplication by the small, fixed constants (1,2,3) than a general finite-field multiplication method $(GF(2^8))$[[1]](#1). \
This led me to create two methods to help with matrix multiplication: [**MixMultiply**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/Encryption.cs#L811) and [**Multiply2**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/Encryption.cs#L838): \
**MixMultiply**  takes the large byte (from the message) and the small byte (from the pre-defined matrix) and performs different calculations on the large byte based on the value of the small byte (*1/2/3*):\
$\small\qquad 1\rightarrow$ Return byte      $\small 2\rightarrow$ Multiply2(byte)      $\small 3\rightarrow$ Multiply2(byte) OR byte\
**Multiply2** takes the byte, shifts it by 1 to the left and, if it goes outside of the byte, OR it with 0x11B (from field representation- see Key Scheduling)\
For decryption (**InvMixColumns**), the official method is to multiply it by a different pre-defined matrix, this time involving 9,11,13+14. This would be much more complex than 1,2,3 so I did some research for a better method. In the original documentation for the encryption standard [[2]](#2), it lists an alternative method involving applying the MixColumns method again and then multiplying it by a matrix which had 0,4+5.\
Therefore, I added more calculations to the MixMultiply method for 0, 4 and 5 and implemented the InvMixColumns method.

### Key Scheduling
*Location: [Encryption/AES/RConst](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/Encryption.cs#L593)*

<details>
    <summary><b>RConst Recap</b></summary>

As each round needs a different key, key scheduling is used to generate sub-keys for each round from the cipher key. The method of generating these keys is outlined in the Project Document and involves calculating the current round key from the previous round key using a key function ($g$). Part of this function requires the RoundConstant which is calculated through the equation:  $RC[1]=1,\quad RC[i]=RC[i-1]\times 2$.

---
</details>
<br>

When calculating the RConst, I got different values than in the examples ($01,02…,80,100,200$ vs $01,02,80,1B,36$). \
This is because, in a similar way to the matrix multiplication, the values must be kept inside the finite field:  $GF(2)[x]/[x^8+x^4+x^3+x+1]$.\
Finite fields use polynomials to represent the binary values so the reducing polynomial in the finite field definition is $100011011_2$ in binary or $11B_{16}$  in hex. The reducing polynomial is designed to reduce the results that are greater than a byte into a byte value. This is done by subtracting the reducing polynomial from the polynomial representation of the result, or XOR’ing the binary result with $11B_{16}$ [[3]](#3). This explains where the arbitrary value of $11B_{16}$ that is used in both Key Scheduling and Matrix Multiplication comes from.

### Firestore API
One of the most complex aspects of my project is its client-server model. As my project is a messaging app, it needs constant communication with the server. For this project, I have chosen Firestore from Firebase for my server storage as it offers free storage and has support for c#. However, as the Firestore extension works with Java HashMaps (dictionary-like objects with string keys and values), extra work is required in order to convert between HashMaps and objects for use in the server and storage in the server. \
My project contains the following files which are responsible for Firestore storage:
1. **IManageFirestoreService**- The interface with public methods used throughout the project to Fetch, Listen and Write data to the server
2. **FirestoreService**- The android implementation of this interface with a wide range of methods that can be used to change/read data on the server
3. **OnCompleteListener + OnEventListener**- Receiving the result from any Get/Listen requests and converting it to the required type before passing it to the relevant method.
4. **ListenerHelper**- Takes a HashMap input returned from the server and converts it to objects of classes (e.g *Chat, Message..*)

These files work together to allow the other parts of the project to easily work with the server. Below, I have created a walkthrough of how a method would get and write data from/to the server:

<details>
    <summary><b>Fetch/Get</b></summary>

*Location: [FirestoreService.FetchData<returnType>(pathInfo,arguments)](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Services/FirestoreService.cs#L134)*

For example, when the FriendRequest.cs/SendRequest wants to check if the user exists it calls:   [*FetchData<User>(“UserFromUsername”, (“USERNAME”, username))*](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/FriendRequestPage.xaml.cs#L150). 

The parameters: ‘pathInfo’ and ‘arguments’ are used by the ***GetPath*** method to expand the path, using the firestorePaths dictionary for commonly used paths. *‘UserFromUsername’* is a key in the dictionary so gets converted to *‘usersPublic/<USERNAME>'* and as *‘USERNAME’* is an argument, it gets replaced by the contents of the variable username  🡲 *‘usersPublic/Oliver’*

***GetReferenceFromPath*** then converts the expanded path into a point in the server. \
As firestore servers are arranged in the order of Collection🡲Document🡲 Collection…, our example would have a reference of: *Collection(“users”).Document(“Oliver”)*\
The FetchData method then takes the reference and calls **Get()** on it, adding an **OnCompleteListener** to it.\
When the server returns the required data, it activates the OnComplete method of the OnCompleteListener. This method takes the returned object and tries to convert it to the required type using the ***ListenerHelper*** class. This class has different Parse methods for all the complex models, including a general ***ToObject<type>()*** which can be used for simple models made up of base types. This value is then returned back to the caller of the initial method.

---
</details>

<details>
    <summary><b>Write/Set</b></summary>

*Location  [FirestoreService.WriteObject(obj, pathInfo, arguments)](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Services/FirestoreService.cs#L189)*

When a user wants to send a friend request, the [Request.cs/Send](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/Request.cs#L122) method would call:\
*WriteObject(requestObj, $“Requests/{SourceUser.Id}”, (“CUserID”, targetUserID))*. 

In a similar way to the Fetch method, the last 2 parameters are used by **GetPath** and **GetReferenceFromPath** to get the reference to write to. The method takes the object and converts it into a HashMap using **GetMap(obj)**.

This complex method loops through all the properties of the object and populates it with the property names and their associated values. For objects inside objects (*e.g KeyData in Requests*), it calls itself recursively until it reaches a base object. For object arrays, Firestore requires them to be in JavaLists, so a new JavaList is created and all the values are converted and added.\
Once the object has been converted to a HashMap, WriteData takes the reference and calls ***Set(HashMap)***.

---
</details>

The general hierachy of my project (in terms of server interaction) goes:\
(**Front End**) View Xaml 🡲 View C# 🡲 Model Code 🡲 FirestoreService 🡲 Firestore Server (**Back End**)

### Other API Interactions

[**FirebaseAuth**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Services/FirebaseAuth.cs)\
As well as the Firestore, my project also interacts with the Firebase Auth API to manage the register and log in systems as well as sending forgot password requests.

[**SQLite**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Services/SQLiteService.cs)\
In order to save and retrieve data between sessions, like the keyData for Diffie-Hellman or sharedKey for encryption/decryption, I interact with the SQLiteService which stores data in an SQL database on the local device

## Code Listing 📃

### Views
Views are pages which are used inside the app that are navigated between during its runtime. Each view is made up of the following components:
1. **An .xaml file** which contains the physical appearance of the page with all it’s predefined elements and templates for dynamic elements (e.g ChatList)
2. **A c# file (.cs)** which contains code that is fired when the elements in the page (or page itself) is interacted with (*e.g pressing a button or entering the page*)
My project is made up of 9 views which the user travels between constantly. Whilst 7 of these are normal pages, 2 are popups which use an extension to overlay themselves over other pages to allow information to popout to the user.

**LoginPage** ([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/LoginPage.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/LoginPage.xaml.cs)), 
**RegisterPage**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/RegisterPage.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/RegisterPage.xaml.cs)), 
**ForgotPasswordPage**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/ForgotPasswordPage.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/ForgotPasswordPage.xaml.cs)), 
**LoadingPage**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/LoadingPage.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/LoadingPage.xaml.cs)), 
**MainMessagePage**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/MainMessagePage.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/MainMessagePage.xaml.cs)), 
**ChatPage**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/ChatPage.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/ChatPage.xaml.cs)), 
**FriendRequestPage**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/FriendRequestPage.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/FriendRequestPage.xaml.cs)), 
**ChatPopup**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/ChatPopup.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/ChatPopup.xaml.cs)), 
**SettingsPopup**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/SettingsPopup.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Views/SettingsPopup.xaml.cs))

### Models
Models are classes which are used to group together and organise data and perform operations on them. My program consists of the 5 main classes with a few sub classes:
1. [**Chat**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/Chat.cs)
2. [**CUser**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/CUser.cs)   (CurrentUser -*extends from User*)
3. [**Message**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/Message.cs)\
    a. [**MessageView**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/Message.cs#L93)    (*Only for use in Views*)\
4. [**Request**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/Request.cs)\
    a. [**KeyData**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/Request.cs#L12)\
    b. [**AcceptedRequest**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/Request.cs#L29)\
    c. [**Request**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/Request.cs#L81)   (*Pending*)
5. [**User**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Models/User.cs)

### Resources
Resources are files which are designed to carry out a specific task that is required throughout the app that is imported when needed. For my project I created the following resources:
1. [**Encryption**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/Encryption.cs)- Contains the code for my encryption, AES and Diffie-Hellman Implementations
2. [**Global Variables**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/GlobalVariables.cs)- Contain variables that are used in all models and views
3. [**LoggerService**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/LoggerService.cs)- Helps with debugging and error logging by outputting the line number and method name
4. [**Functions**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Resources/Functions.cs)- Used for general GUI tasks like setting an icon as invalid and changing the theme

### Services
Since Xamarin Forms is a cross-platform system, there are some aspects of the code that are specific to the operating system that the app is running on. These are services which use interfaces to ensure that the methods are implemented correctly in the platform-specific version (Android in my case)\
**AuthenticationService**([Interface](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Services/IAuthenticationService.cs))([Implementation](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Services/FirebaseAuth.cs)), 
**FirestoreService**([Interface](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Services/IManageFirestoreService.cs))([Implementation](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Services/FirestoreService.cs)), 
**ToastService**([Interface](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Services/IToastMessage.cs)),
[**SQLiteService**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/Services/SQLiteService.cs)

### Java/Android Resources
The services use some Java-specific methods inside the Android portion of the project which take in and use Java objects like the hashMap\
[**ListenerHelper**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Resources/ListenerHelper.cs), [**OnCompleteListener**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Resources/OnCompleteListener.cs), [**OnEventListener**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/Resources/OnEventListener.cs)

### Other Misc Files
**App**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/App.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/App.xaml.cs)), **AppShell**([.xaml](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/AppShell.xaml))([.cs](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/AppShell.xaml.cs)), 
[**AssemblyInfo**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App/AssemblyInfo.cs), [**MainActivity.cs**](https://github.com/OliverS204/Encrypted-Messaging-App/blob/main/Encrypted%20Messaging%20App/Encrypted%20Messaging%20App.Android/MainActivity.cs)

## Testing 🧪
**Video Links:**
1.  https://youtu.be/P7gcQD4bnyA  (Most of the testing)
2.  https://youtu.be/cGjcI1WZ-cs   (Password testing + log out)


<details>
    <summary><b>Testing Table</b></summary>

| **Test #** | **Description** | **Input** | **Expected Results** | **Evidence** | **Pass/Fail** |
:----------: | --------------- | --------- | ---------------------| ------------ | ------------- |
| | | <p style="text-align: center;">**Register Page**</p> | | | |
|       **1.1**      | Valid Data for RegisterPage fields       | **Normal Data**- Valid username, email address and password:      Username: _TestUser_      Email:  test\@test.user      Password: passwordConfirm Password: password | The values are detected as being valid and the register button is active      | **Video1**   0:42       | **Pass**      |
|       **1.2**      | Invalid Data for RegisterPage fields     | **Invalid Data**-     Username:  “ “    Email: invalid\@email    Password: “ “Confirm Password: password        | The fields are shown to be invalid to the user and the register button is disabled     | **Video1**  0:05-0:20   | **Pass**      |
|       **1.3**      | Changing the password and confirm password visibility      | **Press Change Visibility Button** when the password + confirm password fields are filled     | The password and confirm password contents should switch between being visible and hidden       | **Video1**   0:36  | **Pass**      |
|       **1.4**      | Register **Success**   | **Press register button** when the fields are valid and unique (_no account already)_         | User information is added to server and the user goes to MainMessagePage      | **Video1**   0:47       | **Pass**      |
|       **1.5**      | Register **Failure**   | **Press register button** when some fields are already taken: Username: TestUser2(_taken_) Email: test2\@test.user (_taken_)      | The app indicates to the user which fields are incorrect and the register button turns red      | **Video1**   0:35       | **Pass**      |
|       **1.6**      | Open LoginPage         | **Press SignIn button** at bottom of screen      | The LoginPage opens     | **Video2**   0:03  | **Pass**      |
| | | <p style="text-align: center;">**Login Page**</p> | | | |
|       **2.1**      | Valid Data for LoginPage fields | **Normal Data**- Valid email address and password:    Email: _testuser2\@test.user_    Password: _password2_    | The values are detected as being valid and the login button is active         | **Video1**   1:56  | **Pass**      |
|       **2.2**      | Invalid Data for LoginPage fields        | **Invalid Data-**   Email- invalid\@email   Password- short_(password has a minimum length_)  | The invalid fields are marked as invalid to the user and the login button is disabled  | **Video1**   1:22  | **Pass**      |
|       **2.3**      | Changing the password visibility         | **Press Change Visibility Button** when the password field is filled        | The password contents should toggle between being visible and hidden | **Video1**    2:00 | **Pass**      |
|       **2.4**      | Login **Success**      | **Press login button** when the email and password are correct     | The user information is loaded and the user goes to MainMessagePage  | **Video1**   2:10  | **Pass**      |
|       **2.5**      | Login **Failure**      | **Press login button** when the email/password are incorrect       | The app indicates to the user which field are incorrect and the login button turns red | **Video1**  2:00-2:10       | **Pass**      |
|       **2.6**      | Opening RegisterPage   | **Press SignUp text** at bottom of screen        | The RegisterPage opens  | **Video1**  0:00   | **Pass**      |
|       **2.7**      | Opening ForgotPasswordPage      | **Press Forgot Password? text**         | The ForgotPasswordPage opens     | **Video1**   1:39       | **Pass**      |
| | | <p style="text-align: center;">**Forgot Password Page**</p> | | | |
|       **3.1**      | Valid Data for email address    | **Normal Data**- Valid email address    | The values are detected as being valid and the submit button is activated     | **Video2**   0:14  | **Pass**      |
|       **3.2**      | Invalid Data for email address  | **Invalid Data**- invalid\@email        | The email is detected as being invalid and the submit button is deactivated   | **Video2**   0:08       | **Pass**      |
|       **3.3**      | Forgot Password **Success**     | **Press submit button** when the email address is an address associated with a saved user     | The user is informed that an email has been sent to their account    | **Video2**  0:28        | **Pass**      |
|       **3.4**      | Forgot Password **Failure**     | **Press submit button** when the email address doesn’t belong to a user     | The submit button turns red to indicate a failed forgot password request      | **Video2**  0:17   | **Pass**      |
| | | <p style="text-align: center;">**Main Message Page**</p> | | | |
|       **4.1**      | Opening Settings Popup | **Press Settings icon**        | The SettingsPopup opens over the MainMessagePage   | **Video1**   2:23  | **Pass**      |
|       **4.2**      | Logging out from app   | **Press Leave icon**  | The user is sent back to the LoginPage    | **Video2**         |      |
|       **4.3**      | Is a chat displayed correctly in the list?        | **1. Send a friend request to a different user<br>2. Accept the friend request on their account**      | The chat is shown in the list with its title and id         | **Video1**   4:13  | **Pass**      |
|       **4.4**      | Opening a chat in the chat list | **Press on a chat in the list**         | The ChatPage opens, setup with the correct information      | **Video1**   4:32  | **Pass**      |
| | | <p style="text-align: center;">**Settings Popup**</p> | | | |
|       **5.1**      | Is the correct information displayed for the current user? |     | The correct username, chats number and request number is displayed as well as the current theme | **Video1**   2:23  | **Pass**      |
|       **5.2**      | Changing the current app theme  | **Select ‘Red’ theme in theme picker**  | The theme across the app is changed with accent colours turning red  | **Video1**   2:25  | **Pass**      |
|       **5.3**      | Leaving the Settings Popup      | **Press on the outside the popup**      | The SettingsPopup should close, showing the MainMessagePage | **Video1**   2:27  | **Pass**      |
| | | <p style="text-align: center;">**Chat Page**</p> | | | |
|       **6.1**      | Sending a message 🡲 Add to server       | **Write a message and press the send button**    | A message object should be added to the chat in the server. The message content should be encrypted      | **Video1**  4:41        | **Pass**      |
|       **6.2**      | Show messages in list on device | _Same_       | The message should be successfully decrypted and displayed in the messages list, with an indication of who sent the message (you or the other user)   | **Video1**  4:43   | **Pass**      |
|       **6.3**      | Editing the title 🡲 Update on server    | **Press the edit button and change the title**   | The title of the chat in the server should change to the new title   | **Video1**  4:59   | **Pass**      |
|       **6.4**      | Title updated on server 🡲 Title changed on device         | _Same_       | The title of the chat on the device should change in response to the changing title in the server\`      | **Video**  5:03    | **Pass**      |
|       **6.5**      | Opening ChatPopup      | **Press Chat Info in the secondary menu**        | The ChatPopup should open over the ChatPage        | **Video1**   4:50  | **Pass**      |
|       **6.7**      | Leave Chat    | **Press Leave in the secondary menu**   | The chat should be deleted in the server + the chatID should be deleted in the user object. The user should go back to the MainMessagePage   | **Video1**   5:18  | **Pass**      |
| | | <p style="text-align: center;">**Chat Popup**</p> | | | |
|       **7.1**      | Is the correct information displayed for the chat?         |     | The correct title, number of messages, users and encryption key is displayed as well as the correct encrypt status         | **Video1**  4:51   | **Pass**      |
|       **7.2**      | Is the decryption correctly disabled     | **Press ‘Decryption: Enabled’**         | The text should change to ‘Decryption: Disabled’ and the messages should show their encrypted content    | **Video1**  4:53   | **Pass**      |
|       **7.3**      | Is the decryption correctly enabled      | **Press ‘Decryption: Disabled’**        | The text should change to ‘Decryption: Encryption’ and the messages should show their original content   | **Video1**  4:57   | **Pass**      |
|       **7.4**      | Leaving the ChatPopup  | **Press outside the popup**    | The ChatPopup should close and the ChatPage should appear   | **Video1**  4:58   | **Pass**      |
| | | <p style="text-align: center;">**Friend Request Page**</p> | | | |
|       **8.1**      | Valid data for user (_Send Friend Request_)       | **Normal Data**- TestUser, TestUser2    | The user is detected as being valid and the submit button becomes active      | **Video1**  2:46   | **Pass**      |
|       **8.2**      | Invalid data for user   (_Send Friend Request_)   | **Invalid Data-** “ “ | The submit button is inactive and the user is shown as being invalid | **Video1**   2:32  | **Pass**      |
|       **8.3**      | Send Friend Request **Success** | **Press submit button** when the user field has a username that is associated to a different account   | The submit button should turn green and the pending friend request object should be added to the server  | **Video1**  2:51-3:06+ 3:20 | **Pass**      |
|       **8.4**      | Send Friend Request **Failure** _(Invalid user)_  | **Press submit button** when the user field has a username which doesn’t have an account      | The submit button should turn red and the user should be informed that no user exists  | **Video1**  2:38   | **Pass**      |
|       **8.5**      | Send Friend Request **Failure**(_Same user_)      | **Press submit button** when the user field has the username of the currently signed in user  | The submit button should turn red and the user should be informed that they can’t send requests to themself       | **Video1**  2:45   | **Pass**      |
|       **8.6**      | Send Friend Request **Failure**(_Already sent_)   | **Press submit button** when the user already has a pending friend request  | The submit button should turn red and the user should be informed that the request has already been sent | **Video1**   3:25  | **Pass**      |
|       **8.7**      | Is a friend request shown correctly in the list   | **1. Send a friend request to a different user<br>2. Switch to their account**       | The friend request should have the name of the other user and their id as well as a accept and deny button        | **Video1**   2:54+ 3:25     | **Pass**      |
|       **8.8**      | Denying a friend request        | **Press the ‘x’ button next to the request**     | The pending friend request object should be deleted from the server and the request should disappear from the list         | **Video1**   3:15  | **Pass**      |
|       **8.9**      | Accepting a friend request      | **Press the ‘✓’ button next to the request**     | The pending request object should be deleted from the server and the accepted friend request should be added for the other user. A new chat object should be added to the server and the associated chatID should be added to the user’s entry in the server under chatIDs | **Video1**  3:50   | **Pass**      |

</details>


## References 🔎
<a id="1">[1]</a>
"How to solve MixColumns - aes - Cryptography Stack Exchange." 19 Apr. 2012, https://crypto.stackexchange.com/questions/2402/how-to-solve-mixcolumns. Accessed 5 Mar. 2022.\
<a id="2">[2]</a>
"The Design of Rijndael - AES - Institute for Computing and ...." 26 Nov. 2001, https://cs.ru.nl/~joan/papers/JDA_VRI_Rijndael_2002.pdf. Accessed 5 Mar. 2022.\
<a id="3">[3]</a>
"How to solve MixColumns - Cryptography Stack Exchange." https://crypto.stackexchange.com/a/95775. Accessed 6 Mar. 2022.