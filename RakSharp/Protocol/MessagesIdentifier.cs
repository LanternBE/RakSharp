namespace RakSharp.Protocol;

public static class MessagesIdentifier {
    
    public static readonly byte[] Magic = [0x00, 0xFF, 0xFF, 0x00, 0xFE, 0xFE, 0xFE, 0xFE, 0xFD, 0xFD, 0xFD, 0xFD, 0x12, 0x34, 0x56, 0x78];
    public const byte RakNetVersion = 11;

    public enum OfflineMessages {
        
        UnconnectedPing = 1,
        UnconnectedPingOpenConnections = 2,
        OpenConnectionRequestFirst = 5,
        OpenConnectionReplyFirst = 6,
        OpenConnectionRequestSecond = 7,
        OpenConnectionReplySecond = 8,
        RemoteSystemRequiresPublicKey = 10,
        ConnectionAttemptFailed = 17,
        AlreadyConnected = 18,
        ConnectionLost = 22,
        IncompatibleProtocolVersion = 25,
        UnconnectedPong = 28
    }

    public enum OnlineMessages {
        
        ConnectedPing = 0,
        ConnectedPong = 3,
        ConnectionRequest = 9,
        ConnectionRequestAccepted = 16,
        NewIncomingConnection = 19,
        DisconnectionNotification = 21,
        Datagram = 132, // aka FrameSet
        NegativeAcknowledgement = 160,
        Acknowledgement = 192
    }
}