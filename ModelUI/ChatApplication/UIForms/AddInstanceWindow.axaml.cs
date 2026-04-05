using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ChatCore.Models;
using System;

namespace ChatApplication;

/// <summary>
/// Dialog window for creating a new chat instance.
/// </summary>
public partial class AddInstanceWindow : Window
{
    public AddInstanceWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Closes the dialog without saving.
    /// </summary>
    private void CancelButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Close(null);
    }

    /// <summary>
    /// Validates and creates a new instance from the form inputs.
    /// </summary>
    private void CreateButton_OnClick(object? sender, RoutedEventArgs e)
    {
        // Get form values
        string name = InstanceNameTextBox.Text?.Trim() ?? "";
        string localIp = LocalIpTextBox.Text?.Trim() ?? "127.0.0.1";
        string localPort = LocalPortTextBox.Text?.Trim() ?? "9000";
        string remoteIp = RemoteIpTextBox.Text?.Trim() ?? "127.0.0.1";
        string remotePort = RemotePortTextBox.Text?.Trim() ?? "9001";

        // Validate
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowError("Instance name is required.");
            return;
        }

        if (!IsValidIp(localIp))
        {
            ShowError($"Invalid local IP: {localIp}");
            return;
        }

        if (!IsValidPort(localPort))
        {
            ShowError($"Invalid local port: {localPort}");
            return;
        }

        if (!IsValidIp(remoteIp))
        {
            ShowError($"Invalid remote IP: {remoteIp}");
            return;
        }

        if (!IsValidPort(remotePort))
        {
            ShowError($"Invalid remote port: {remotePort}");
            return;
        }

        // Create instance config
        var instance = new InstanceConfig
        {
            Name = name,
            LocalIp = localIp,
            LocalPort = localPort,
            RemoteIp = remoteIp,
            RemotePort = remotePort
        };

        // Close dialog with result
        Close(instance);
    }

    /// <summary>
    /// Validates IP address format.
    /// </summary>
    private bool IsValidIp(string ip)
    {
        return System.Net.IPAddress.TryParse(ip, out _);
    }

    /// <summary>
    /// Validates port number range (1-65535).
    /// </summary>
    private bool IsValidPort(string port)
    {
        return int.TryParse(port, out int portNum) && portNum > 0 && portNum <= 65535;
    }

    /// <summary>
    /// Shows an error message to the user.
    /// </summary>
    private void ShowError(string message)
    {
        // For now, we'll just use a simple notification
        // In a more complete implementation, this could show a proper error dialog
        System.Diagnostics.Debug.WriteLine($"Validation Error: {message}");
        // You can also update a status label if needed
    }
}
