# VoiceMod Test
Author: Nicholas Bonello

The goal of the project was to provide a single executable file that would start up a server if the requested port is free, and allow connections for clients to simulate a chatroom. If the port was already in use, the executable would instead create a client to connect to the chatroom server. The executable currently running the server could also run its own client. 

This project simulates a chatroom, where a host can create a server and provide others with the url that can be used for forum/chatroom conversations.  

## Requirements
- Provide a single binary that receives a port number as a parameter
    - If the port is free, the program will open it and will be listening while awaiting requests.
    - If the port is busy, the program will try to log in to start a chat session.
- Messages received and sent will be displayed on the terminal screen, showing the name of the author of the message.
- The program will display each user’s login and end of session.


## Prerequisites
- NET Core 3.1
- Fleck 1.1.0

## Usage

#### How to run

- Run the executable and pass the port as a command line argument. Eg. ./voicemod.exe 8080
- If the port is available, the executable will startup a WebSocket Server listening to the provided port. This can be confirmed by log files and console text.
- Only server-side logs are stored to log text files.
- The same executable will also start a client connected to the same port. The client requires a username so that other clients can see who is sending messages. Not entering a valid username (or simply skipping this step) will provide a default username - "anon".
- Once the client is connected to the server, you can begin sending messages that will be broadcast to everyone on the server. At this point you will be alone as the host. Share the port to other clients for access.
- Run a new instance of the executable using the same port. The new client will find that the port is already in use and will only create a new client instance connecting to the server. Communication between the two clients can now be observed. 

## Architecture
- The Server was designed to be a Singleton object. Each port can only have a single server running which will solely be responsible for handling all incoming requests. The messages will be processed synchronously which help to ensure message ordering across all clients. Messages will be received one by one, and broadcast to all clients in the same order.
- The Mediator Pattern was used to define and encapsulate the communication between clients and the server. The advtange this provides is that clients and the server are loosely coupled meaning that communication between the two can happen independently of eachothers implementation. The MessageClient delegates the communication to the ClientWebSocket rather than communicating directly with the MessageServer.

## Assumptions
- A client can only connect to a single server at a time.
- When the host client terminates its session, the server is closed and all currently connected clients will be disconnected.

## Future Work and Improvements
- No client authentication. Anyone can join, and anyone can use any username.
- Messages are currently not stored. If clients disconnect and reconnect they would lose all the message history. This is not a flaw, but could be added. 
- If messages fail to be received by a client, that message will be lost. The server does not check that each message was received succesfully and is not resent.  
- Improve exception handling. 
-- Temporary client disconnection would cause the client to be disconnected.
-- Messages that fail to be sent will be lost forever. We could add message retries.
- Functionality: Allow clients to enter multiple lobbies at the same time or send private messages
- Add functionality to make a new client the host when the original host quits. At the moment, the server and the first client are bound together, meaning when the first client quits the session, the server shuts down too. This would allow the server to live on even if the original process is no longer running. My initial proposal is to keep track of the order of client connections and attempt to notify the first client that the server was disconnected and this client should reopen the server. All other clients would attempt to reconnect.
- Add component tests. Unit tests would not provide much use in this scenario since there is no logic on both the server and the client that can be tested independantly. However, the implementation provides a testable solution since we wrap everything in interfaces that can be used to mock requirements for the System under Test. Once logic would be added, this could easily be tested.

