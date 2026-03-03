using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ExchangeRate.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ExchangeRate.Desktop;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly string _connectionString;
    private System.Timers.Timer? _autoRefreshTimer;
    private bool _isRefreshing = false;

    public MainWindow()
    {
        InitializeComponent();

        // Load connection string from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Setup auto refresh timer (10 seconds)
        SetupAutoRefreshTimer();

        // Initial data load
        Loaded += async (s, e) => await RefreshDataAsync();
    }

    private void SetupAutoRefreshTimer()
    {
        _autoRefreshTimer = new System.Timers.Timer(10000); // 10 seconds
        _autoRefreshTimer.Elapsed += async (s, e) =>
        {
            await Dispatcher.InvokeAsync(async () => await RefreshDataAsync());
        };
        _autoRefreshTimer.AutoReset = true;
        _autoRefreshTimer.Start();
    }

    private AppDbContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(_connectionString);
        return new AppDbContext(optionsBuilder.Options);
    }

    private async Task RefreshDataAsync()
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
        {
            StatusLabel.Text = "Refreshing...";

            using var context = CreateDbContext();

            // Load exchange rates (latest 100)
            var rates = await context.ExchangeRates
                .OrderByDescending(r => r.Timestamp)
                .Take(100)
                .ToListAsync();

            ExchangeRateDataGrid.ItemsSource = rates;
            RecordCountLabel.Text = $"Records: {rates.Count}";

            // Check worker status
            var workerStatus = await context.WorkerStatuses.FirstOrDefaultAsync();
            UpdateWorkerStatusDisplay(workerStatus);

            LastRefreshLabel.Text = $"Last refresh: {DateTime.Now:HH:mm:ss}";
            StatusLabel.Text = "Ready";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = $"Error: {ex.Message}";
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    private void UpdateWorkerStatusDisplay(Data.Models.WorkerStatus? status)
    {
        if (status != null)
        {
            var timeSinceLastHeartbeat = (DateTime.Now - status.LastHeartbeat).TotalSeconds;
            var isOnline = status.IsActive && timeSinceLastHeartbeat <= 15;

            if (isOnline)
            {
                WorkerStatusLabel.Text = "🟢 Worker Online";
                WorkerStatusLabel.Foreground = new SolidColorBrush(Colors.White);
                WorkerStatusBorder.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                WorkerStatusLabel.Text = "🔴 Worker Offline";
                WorkerStatusLabel.Foreground = new SolidColorBrush(Colors.White);
                WorkerStatusBorder.Background = new SolidColorBrush(Colors.Red);
            }
        }
        else
        {
            WorkerStatusLabel.Text = "⚪ Worker Unknown";
            WorkerStatusLabel.Foreground = new SolidColorBrush(Colors.White);
            WorkerStatusBorder.Background = new SolidColorBrush(Colors.Gray);
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await RefreshDataAsync();
    }

    private void AutoRefreshCheckBox_Changed(object sender, RoutedEventArgs e)
    {
        if (_autoRefreshTimer != null)
        {
            _autoRefreshTimer.Enabled = AutoRefreshCheckBox.IsChecked == true;
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _autoRefreshTimer?.Stop();
        _autoRefreshTimer?.Dispose();
        base.OnClosed(e);
    }
}