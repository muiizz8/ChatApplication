# ChatApplication

A professional, cross-platform tactical messaging application built with **Avalonia UI** and **.NET 9**. Designed for structured, reliable communication between instances using a typed message protocol over UDP, TCP, or Proto.Actor transports.

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Avalonia UI](https://img.shields.io/badge/Avalonia-11.3.12-red.svg)](https://avaloniaui.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## Demo

https://github.com/user-attachments/assets/df0cef9c-ba75-4b53-8a8b-bd483e303150

---

## Key Features

- **Multi-Transport Messaging:** Switch between UDP, TCP, and Proto.Actor transports at runtime without restarting the server.
- **Typed Message Protocol:** Structured message types — Machine Request/Response and Pilot Request/Response — with automatic type detection based on reply context.
- **Request/Response Tracking:** Every message carries a unique ID (`msgId`). Replies link back to the original via `replyToId` and show an inline preview of the quoted message.
- **Tactical Track Numbers (STTN/dTTN):** Source and Destination Tactical Track Numbers are embedded in every wire message for sender/receiver identification.
- **Delivery Acknowledgment:** Automatic ACK on message receipt; NAK support for negative acknowledgment with `IsDeliveryFailed` state surfaced in the UI.
- **Reply Context UI:** Composing a reply automatically sets the correct response type and shows a reply banner — no manual type selection required.
- **Instance Management:** Add, configure, and switch between named network instances (Local/Remote IP + Port) from the sidebar. New instances are persisted to `Config.inf` immediately.
- **Persistent Configuration:** INI-based `Config.inf` managed via `Salaros.ConfigParser` for easy portability.
- **Data Persistence:** Integrated SQLite storage for chat history.
- **Modern UI/UX:** Built on **Actipro Software** controls and **Avalonia UI** with a dark-themed, professional layout.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | .NET 9.0 |
| UI | [Avalonia UI](https://avaloniaui.net/) v11.3.12 |
| UI Components | [Actipro Software Avalonia Controls](https://www.actiprosoftware.com/products/controls/avalonia) |
| Networking | [NetCoreServer](https://github.com/chronoxor/NetCoreServer) (UDP/TCP) |
| Actor Transport | [Proto.Actor](https://proto.actor/) v1.5.2 |
| JSON | [Newtonsoft.Json](https://www.newtonsoft.com/json) |
| Database | [sqlite-net-pcl](https://github.com/praeclarum/sqlite-net) |
| Configuration | [Salaros.ConfigParser](https://github.com/salaros/config-parser) |

---

- **Framework:** .NET 9.0
- **UI Framework:** [Avalonia UI](https://avaloniaui.net/)
- **Networking:** [NetCoreServer](https://github.com/chronoxor/NetCoreServer) for high-performance communication.
- **JSON Handling:** [Newtonsoft.Json](https://www.newtonsoft.com/json)
- **UI Components:** [Actipro Software Avalonia Controls](https://www.actiprosoftware.com/products/controls/avalonia)
- **Database:** [sqlite-net-pcl](https://github.com/praeclarum/sqlite-net)
- **Configuration:** Salaros.ConfigParser for INI-based settings.

## 📁 Project Structure

- **ModelUI/ChatApplication**: The main Avalonia UI application project.
- **ModelUI/ChatCore**: Core logic, shared interfaces, and data models.
- **ModelUI/Docs**: Documentation assets including screenshots and demos.
## Project Structure

```
ChatApplication/
├── ModelUI/
│   ├── ChatCore/                   # Transport-agnostic core library
│   │   ├── Engine/ChatEngine.cs    # Central message bus
│   │   └── Models/                 # ChatMessage, WireMessage, MessageType, MessageTypeHelper
│   └── ChatApplication/            # Avalonia UI host
│       ├── UI/Views/ChatView       # Main chat panel
│       ├── UIForms/
│       │   ├── WindowMain          # Shell window + sidebar
│       │   └── AddInstanceWindow   # Add instance dialog
│       └── Implementations/
│           ├── Transports/         # UdpTransport, TcpTransport, ProtoActorTransport
│           ├── Config/             # IniConfigProvider
│           └── Storage/            # SQLite storage
└── Config.inf                      # Runtime configuration
```

---

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### Running the Application

1. **Clone the repository:**
   ```bash
   git clone <repository-url>
   cd ChatApplication
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore ModelUI/ChatApplication.sln
   ```

3. **Run the project:**
   ```bash
   dotnet run --project ModelUI/ChatApplication/ChatApplication.csproj
   ```

---

## Configuration

The application reads `Config.inf` at startup. You can edit it directly or manage instances through the sidebar UI.

```ini
[Config]
MainTitle=ChatApplication
SubTitle=Chat

[Network]
LocalIp=127.0.0.1
LocalPort=9000
RemoteIp=127.0.0.1
RemotePort=9001

[App]
InstanceId=InstanceA

[Instances]
Names=InstanceA, InstanceB, InstanceC

[InstanceA]
LocalIp=127.0.0.1
LocalPort=9000
RemoteIp=127.0.0.1
RemotePort=9001

[InstanceB]
LocalIp=127.0.0.1
LocalPort=9001
RemoteIp=127.0.0.1
RemotePort=9000

[InstanceC]
LocalIp=127.0.0.1
LocalPort=5000
RemoteIp=127.0.0.1
RemotePort=5001
```

---

## Wire Message Format

Every message is sent as JSON over the selected transport:

Contributions are welcome! Feel free to open an issue or submit a pull request.
```json
{
  "msgType": "MachineRequest",
  "text": "System status check required",
  "msgId": "550e8400-e29b-41d4-a716-446655440000",
  "replyToId": "",
  "replyToText": "",
  "sttn": "A001",
  "dttn": "B002",
  "requiresYesNo": false
}
```

### Message Types

| Indicator | Enum | Direction |
|---|---|---|
| `MR_Req` | `MachineRequest` | Machine → Pilot |
| `MR_Res` | `MachineResponse` | Pilot → Machine |
| `PR_Req` | `PilotRequest` | Pilot → Machine |
| `PR_Res` | `PilotResponse` | Machine → Pilot |
| `ACK` | `Ack` | Either (delivery confirmation, not shown in UI) |
| `NAK` | `Nak` | Either (negative acknowledgment, not shown in UI) |

---

This project is licensed under the MIT License.
## Usage

1. **Select an Instance:** Pick a network profile from the sidebar or add a new one with **+ Add Instance**.
2. **Choose Transport:** Select UDP, TCP, or ProtoActor from the protocol dropdown.
3. **Start Server:** Click **Start Server** to begin listening on the configured local port.
4. **Send a Message:** Type in the message box and press **Send** or hit Enter. The message type is automatically set to `PilotRequest` for new messages.
5. **Reply to a Message:** Click a received message to set reply context — the type switches automatically to the correct response type and a reply banner appears above the input.
6. **Yes/No Responses:** Messages sent with **Requires Yes/No** enabled show inline **Yes** / **No** buttons for the recipient.

---

## Contributing

Contributions are welcome. Please open an issue or submit a pull request for bugs or feature requests.

## Credits

Special thanks to **[@uxmanz](https://github.com/uxmanz)** for the base Model UI ViewPanels template.

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
