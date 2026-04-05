namespace ChatCore.Models;

/// <summary>
/// Helper class for converting between MessageType enum values and their tactical indicators.
/// Provides utilities for handling MR/PR (Machine Request/Response, Pilot Request/Response).
/// </summary>
public static class MessageTypeHelper
{
    /// <summary>
    /// Converts a message type to its tactical indicator string (e.g., "MR_Req", "PR_Rec", "ACK", "NAK").
    /// </summary>
    public static string GetIndicator(MessageType type)
    {
        return type switch
        {
            MessageType.MachineRequest => "MR_Req",
            MessageType.MachineResponse => "MR_Res",
            MessageType.PilotRequest => "PR_Req",
            MessageType.PilotResponse => "PR_Res",
            MessageType.Message => "",
            MessageType.Ack => "ACK",
            MessageType.Nak => "NAK",
            _ => ""
        };
    }

    /// <summary>
    /// Tries to parse a tactical indicator string back to its MessageType enum value.
    /// </summary>
    public static bool TryParseIndicator(string indicator, out MessageType type)
    {
        type = MessageType.Message;
        
        return indicator switch
        {
            "MR_Req" => (type = MessageType.MachineRequest) == MessageType.MachineRequest,
            "MR_Res" => (type = MessageType.MachineResponse) == MessageType.MachineResponse,
            "PR_Req" => (type = MessageType.PilotRequest) == MessageType.PilotRequest,
            "PR_Res" => (type = MessageType.PilotResponse) == MessageType.PilotResponse,
            "ACK" => (type = MessageType.Ack) == MessageType.Ack,
            "NAK" => (type = MessageType.Nak) == MessageType.Nak,
            "NACK" => (type = MessageType.Nak) == MessageType.Nak,  // Accept both NAK and NACK
            _ => false
        };
    }

    /// <summary>
    /// Creates a formatted message header with ID, STTN, dTTN, and message type indicator.
    /// Format: "[ID: {msgId}] {indicator} | STTN: {sttn} → dTTN: {dttn}"
    /// </summary>
    public static string FormatMessageHeader(string msgId, string sttn, string dttn, MessageType type)
    {
        var indicator = GetIndicator(type);
        
        if (string.IsNullOrEmpty(sttn) && string.IsNullOrEmpty(dttn))
            return $"[ID: {msgId}] {indicator}".Trim();
            
        if (string.IsNullOrEmpty(indicator))
            return $"[ID: {msgId}] | STTN: {sttn} → dTTN: {dttn}".Trim();

        return $"[ID: {msgId}] {indicator} | STTN: {sttn} → dTTN: {dttn}".Trim();
    }

    /// <summary>
    /// Checks if the message type is a request (MachineRequest or PilotRequest).
    /// </summary>
    public static bool IsRequest(MessageType type)
    {
        return type is MessageType.MachineRequest or MessageType.PilotRequest;
    }

    /// <summary>
    /// Checks if the message type is a response (MachineResponse or PilotResponse).
    /// </summary>
    public static bool IsResponse(MessageType type)
    {
        return type is MessageType.MachineResponse or MessageType.PilotResponse;
    }

    /// <summary>
    /// Gets the corresponding response type for a request type.
    /// Machine Request → Machine Response, Pilot Request → Pilot Response.
    /// </summary>
    public static MessageType GetResponseType(MessageType requestType)
    {
        return requestType switch
        {
            MessageType.MachineRequest => MessageType.MachineResponse,
            MessageType.PilotRequest => MessageType.PilotResponse,
            _ => MessageType.Message
        };
    }

    /// <summary>
    /// Checks if the message type is an acknowledgment (ACK or NAK).
    /// These message types are protocol messages and not typically shown in UI.
    /// </summary>
    public static bool IsAcknowledgment(MessageType type)
    {
        return type is MessageType.Ack or MessageType.Nak;
    }

    /// <summary>
    /// Checks if the message type is a positive acknowledgment (ACK).
    /// </summary>
    public static bool IsAck(MessageType type)
    {
        return type == MessageType.Ack;
    }

    /// <summary>
    /// Checks if the message type is a negative acknowledgment (NAK/NACK).
    /// </summary>
    public static bool IsNak(MessageType type)
    {
        return type == MessageType.Nak;
    }
}
