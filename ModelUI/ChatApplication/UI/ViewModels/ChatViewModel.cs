using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Avalonia.Threading;
using ChatApplication.Implementations.Config;
using ChatApplication.Implementations.Storage;
using ChatApplication.Implementations.Transports;
using ChatCore;
using ChatCore.Engine;
using ChatCore.Interfaces;
using ChatCore.Models;

namespace ChatApplication.UI.ViewModels;

public sealed class ChatViewModel : INotifyPropertyChanged
{
    private readonly ChatEngine _engine;

    public ObservableCollection<ChatMessage> Messages { get; } = new();
    public ObservableCollection<string> DebugLines { get; } = new();
    public ObservableCollection<InstanceConfig> Instances { get; } = new();
    public ObservableCollection<string> Contacts { get; } = new();

    public List<string> Protocols { get; } = ["UDP", "TCP", "ProtoActor"];

    private string _selectedProtocol = "UDP";
    public string SelectedProtocol
    {
        get => _selectedProtocol;
        set
        {
            if (_selectedProtocol == value) return;
            _selectedProtocol = value;
            OnPropertyChanged();
            SwitchTransport(value);
        }
    }

    /// <summary>
    /// The message being replied to. When set, outgoing message will be a Response type.
    /// </summary>
    private ChatMessage? _replyingToMessage;
    public ChatMessage? ReplyingToMessage
    {
        get => _replyingToMessage;
        set
        {
            _replyingToMessage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasReplyContext));
            OnPropertyChanged(nameof(ReplyContextLabel));
            OnPropertyChanged(nameof(CurrentMessageType));
        }
    }

    /// <summary>
    /// Gets the automatically determined message type based on context.
    /// New messages from Pilot are PR_Req, replies are PR_Res.
    /// </summary>
    public MessageType CurrentMessageType
    {
        get
        {
            // If replying to a message, automatically determine response type
            if (ReplyingToMessage != null)
            {
                return MessageTypeHelper.GetResponseType(ReplyingToMessage.MessageType);
            }

            // New message from Pilot is always a Pilot Request
            return MessageType.PilotRequest;
        }
    }

    /// <summary>
    /// Gets the display string for the current message type.
    /// </summary>
    public string CurrentMessageTypeLabel =>
        MessageTypeHelper.GetIndicator(CurrentMessageType);

    /// <summary>
    /// Whether we are currently replying to a message.
    /// </summary>
    public bool HasReplyContext => ReplyingToMessage != null;

    /// <summary>
    /// Display label for reply context (e.g., "Replying to Machine Request").
    /// </summary>
    public string ReplyContextLabel
    {
        get
        {
            if (ReplyingToMessage == null)
                return "";

            var originalType = MessageTypeHelper.GetIndicator(ReplyingToMessage.MessageType);
            return $"(Replying to: {originalType})";
        }
    }

    private bool _requiresYesNo;
    public bool RequiresYesNo
    {
        get => _requiresYesNo;
        set { _requiresYesNo = value; OnPropertyChanged(); }
    }

    private InstanceConfig? _selectedInstance;
    public InstanceConfig? SelectedInstance
    {
        get => _selectedInstance;
        set
        {
            _selectedInstance = value;
            OnPropertyChanged();
            if (value != null) _engine.SetCurrentInstance(value);
        }
    }

    private string _messageText = string.Empty;
    public string MessageText
    {
        get => _messageText;
        set { _messageText = value; OnPropertyChanged(); }
    }

    private string _connectionStatus = "Idle";
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set { _connectionStatus = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsTcp)); }
    }

    public bool IsTcp => SelectedProtocol == "TCP";

    public ChatViewModel()
    {
        var storage = new SqliteChatStorage();
        var config = new IniConfigProvider();

        _engine = new ChatModuleBuilder()
            .WithStorage(storage)
            .WithConfig(config)
            .WithTransport(new UdpTransport())
            .Build();

        _engine.MessageAdded += msg =>
            Dispatcher.UIThread.Post(() =>
            {
                Messages.Add(msg);
                if (!Contacts.Contains(msg.Remote)) Contacts.Add(msg.Remote);
            });

        _engine.ContactAdded += remote =>
            Dispatcher.UIThread.Post(() =>
            {
                if (!Contacts.Contains(remote)) Contacts.Add(remote);
            });

        _engine.DebugMessageAdded += text =>
            Dispatcher.UIThread.Post(() => DebugLines.Add(text));

        _engine.ConnectionStatusChanged += status =>
            Dispatcher.UIThread.Post(() => ConnectionStatus = status);

        _engine.MessageDelivered += msgId =>
            Dispatcher.UIThread.Post(() =>
            {
                var msg = Messages.FirstOrDefault(m => m.MessageId == msgId);
                if (msg != null) msg.IsDelivered = true;
            });

        LoadInitialData();
    }

    private void LoadInitialData()
    {
        foreach (var instance in _engine.GetInstances())
            Instances.Add(instance);

        if (Instances.Count > 0)
        {
            SelectedInstance = Instances[0];
            _engine.SetCurrentInstance(Instances[0]);
        }

        foreach (var msg in _engine.LoadHistory())
            Messages.Add(msg);

        foreach (var contact in _engine.LoadContacts())
            if (!Contacts.Contains(contact)) Contacts.Add(contact);
    }

    private void SwitchTransport(string protocol)
    {
        var wasRunning = _engine.IsServerRunning;
        IMessagingTransport transport = protocol switch
        {
            "TCP" => new TcpTransport(),
            "ProtoActor" => new ProtoActorTransport(),
            _ => new UdpTransport() // Default to UDP
        };
        _engine.SetTransport(transport);
        if (wasRunning) _engine.StartServer();
        OnPropertyChanged(nameof(IsTcp));
    }

    public void StartServer() => _engine.StartServer();
    public void StopServer() => _engine.StopServer();
    public void Connect() => _engine.Connect();
    public void Disconnect() => _engine.Disconnect();

    public void SendMessage()
    {
        if (string.IsNullOrWhiteSpace(MessageText)) return;

        // Use the automatically determined message type
        _engine.SendMessage(MessageText, CurrentMessageType, requiresYesNo: RequiresYesNo);
        MessageText = string.Empty;
        ReplyingToMessage = null;  // Clear reply context after sending
    }

    /// <summary>
    /// Sends a "Yes" quick response if RequiresYesNo is enabled.
    /// </summary>
    public void SendQuickYes()
    {
        SendMessage("Yes");
    }

    /// <summary>
    /// Sends a "No" quick response if RequiresYesNo is enabled.
    /// </summary>
    public void SendQuickNo()
    {
        SendMessage("No");
    }

    /// <summary>
    /// Sends a message with the automatically determined type and clears reply context.
    /// </summary>
    private void SendMessage(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;

        _engine.SendMessage(text, CurrentMessageType, requiresYesNo: false);
        MessageText = string.Empty;
        ReplyingToMessage = null;
    }

    public void SetReplyingTo(ChatMessage message)
    {
        ReplyingToMessage = message;
        // Focus on message input (you may need to add method to ChatView code-behind)
    }

    public void CancelReply()
    {
        ReplyingToMessage = null;
    }

    public void ClearChatHistory()
    {
        _engine.ClearHistory();
        Messages.Clear();
    }

    /// <summary>
    /// Sends a quick Yes/No response to a received message with proper reply context.
    /// </summary>
    public void SendQuickResponse(ChatMessage originalMsg, string response)
    {
        _engine.SendResponse(originalMsg, response);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
