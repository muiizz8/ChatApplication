# ChatApplication (RxGUI)

A high-performance, cross-platform messaging and telemetry dashboard built with **Avalonia UI** and **.NET 9**. This application enables real-time communication over UDP and TCP, featuring a modern user interface powered by **Actipro Software** controls.

[![.NET 9.0](https://img.shields.io/badge/.NET-9.0-purple.svg)](https://dotnet.microsoft.com/download/dotnet/9.0)
[![Avalonia UI](https://img.shields.io/badge/Avalonia-11.3.12-red.svg)](https://avaloniaui.net/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

## 🚀 Features

- **Multi-Protocol Support:** Seamlessly send and receive data via UDP and TCP.
- **Instance Management:** Configure and switch between multiple network profiles (Local/Remote IP and Ports) effortlessly.
- **Modern UI/UX:** Leveraging **Actipro Software** controls for a professional, high-performance themed experience.
- **Real-time Telemetry:** Built-in JSON editor for flexible telemetry payload management.
- **Data Persistence:** Integrated **SQLite** support for long-term data logging and management.
- **Comprehensive Logging:** Real-time monitoring of incoming (RX) and outgoing (TX) traffic with timestamps.

## 🛠️ Technology Stack

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

## 🏁 Getting Started

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

## ⚙️ Configuration

The application uses a `Config.inf` file for persistent settings. Example structure:

```ini
[Network]
LocalIp=127.0.0.1
LocalPort=9000
RemoteIp=127.0.0.1
RemotePort=9001

[App]
InstanceId=InstanceA

[Instances]
Names=InstanceA, InstanceB
```

## 🖼️ Preview

![Application Screenshot](ModelUI/Docs/Screenshot%20(52).png)

## 🤝 Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.

## 📄 License

This project is licensed under the MIT License.
