namespace XmppApi.Network;

public enum SocketErrorStatus
{
    Unknown,
    ConnectionRefused,
    TimedOut,
    HostNotFound,
    NetworkDown,
    NetworkUnreachable,
    AddressAlreadyInUse,
    ConnectionReset,
    NotConnected,
    OperationAborted
    // Add more as needed
}