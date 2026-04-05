using ActiproSoftware.UI.Avalonia.Themes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Database;
using ChatApplication.UI.Views;
using ChatApplication.Common;
using ChatApplication.Implementations.Storage;
using ChatApplication.Implementations.Config;
using ChatCore.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace ChatApplication;

/// <summary>
/// Main window for the application, managing navigation and layout between views.
/// </summary>
public partial class WindowMain : Window
{
    /// <summary>
    /// Chat view instance for machine-to-machine messaging.
    /// </summary>
    public ChatView ChatView = new ChatView();

    public SqliteChatStorage databaseHelper = new SqliteChatStorage();

    /// <summary>
    /// Initializes a new instance of the <see cref="WindowMain"/> class.
    /// </summary>
    ///
    public WindowMain()
    {
        InitializeComponent();
        setApplicationVersioninTitle(); // Set application title using assembly metadata
        Globals.InitConfigs(); // Load global config values

        titleTextBlock.Text = Globals.MainTitle;
        subtitleTextBlock.Text = Globals.SubTitle;

        transitionControl.Content = ChatView; // Set initial view
        
        // Load instances list
        LoadInstancesList();
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    // NON-PUBLIC PROCEDURES
    /////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Sets the window title and version text from the assembly product attribute.
    /// The version number automatically increments with each build.
    /// The app name is read from the assembly's product attribute stated in csproj file.
    /// </summary>
    private void setApplicationVersioninTitle()
    {
        AssemblyProductAttribute productAttr = typeof(WindowMain).Assembly
            .GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0] as AssemblyProductAttribute;

        // Title = $"{productAttr.Product}";
        versionTextBlock.Text = $"{productAttr.Product}";
    }

    /// <summary>
    /// Handles property changes in the ViewModel, especially for navigation direction.
    /// </summary>
    private void OnApplicationViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ApplicationViewModel.ViewTransitionForward))
        {
            if (transitionControl.PageTransition is DirectionalPageSlide transition)
                transition.UseForwardDirection = ViewModel?.ViewTransitionForward ?? true;
        }
    }

    /// <summary>
    /// Prepares the View menu based on the current theme. (Stubbed - logic not implemented)
    /// </summary>
    private void OnViewMenuOpening(object? sender, EventArgs e)
    {
        if (ModernTheme.TryGetCurrent(out var theme))
        {
            var definition = theme.Definition;
            if (definition is not null)
            {
                // Add theme-based logic if required
            }
        }
    }

    /////////////////////////////////////////////////////////////////////////////////////////////////////
    // PUBLIC PROCEDURES
    /////////////////////////////////////////////////////////////////////////////////////////////////////

    /// <summary>
    /// Called when the window is attached to the visual tree. Initializes notification manager.
    /// </summary>
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        var notificationManager = new WindowNotificationManager(TopLevel.GetTopLevel(this));
        if ((notificationManager is not null) && (ViewModel is not null))
            ViewModel.MessageService = new NotificationMessageService(notificationManager);

        base.OnAttachedToVisualTree(e);
    }

    /// <summary>
    /// Toggles the sidebar pane visibility when the application button is clicked.
    /// </summary>
    private void ApplicationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        splitView.IsPaneOpen = !splitView.IsPaneOpen;
    }

    /// <summary>
    /// Opens the Add New Instance dialog when the Add Instance button is clicked.
    /// </summary>
    private async void AddInstanceButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new AddInstanceWindow();
        var result = await dialog.ShowDialog<InstanceConfig?>(this);
        
        if (result != null)
        {
            // Load existing instances, add the new one, and save
            var configProvider = new IniConfigProvider();
            var instances = configProvider.GetInstances().ToList();
            instances.Add(result);
            configProvider.SaveInstances(instances);
            
            // Refresh the instances list in UI
            LoadInstancesList();
        }
    }

    /// <summary>
    /// Loads and displays all instances in the sidebar listbox.
    /// </summary>
    private void LoadInstancesList()
    {
        try
        {
            var configProvider = new IniConfigProvider();
            var instances = configProvider.GetInstances().ToList();
            InstancesListBox.Items.Clear();
            foreach (var instance in instances)
            {
                InstancesListBox.Items.Add(instance);
            }
        }
        catch (Exception ex)
        {
            // Log error if needed
            System.Diagnostics.Debug.WriteLine($"Error loading instances: {ex.Message}");
        }
    }

    /// <summary>
    /// Handles pointer (mouse) release to support back/forward navigation via mouse buttons.
    /// </summary>
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!e.Handled)
        {
            switch (e.InitialPressMouseButton)
            {
                case MouseButton.XButton1:
                    ViewModel?.NavigateViewBackward();
                    break;
                case MouseButton.XButton2:
                    ViewModel?.NavigateViewForward();
                    break;
            }
        }
    }

    /// <summary>
    /// Gets or sets the ViewModel associated with the window.
    /// </summary>
    public ApplicationViewModel? ViewModel
    {
        get => DataContext as ApplicationViewModel;
        set => DataContext = value;
    }
}
