﻿// Ignore Spelling: App

using Helldivers2ModManager.Services;
using Helldivers2ModManager.Stores;
using Helldivers2ModManager.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace Helldivers2ModManager;

internal partial class App : Application
{
	public static new App Current => (App)Application.Current;
	
	public static Version Version { get; } = new Version(1, 1, 0, 0);

	public IHost Host { get; }

	private readonly ILogger? _logger;

	public App()
	{
		AppDomain.CurrentDomain.UnhandledException += (_, e) => LogUnhandledException(e.ExceptionObject as Exception);
		Current.DispatcherUnhandledException += (_, e) => LogUnhandledException(e.Exception);
		TaskScheduler.UnobservedTaskException += (_, e) => LogUnhandledException(e.Exception);

		HostApplicationBuilder builder = new();

		AddServices(builder.Services);
		AddStores(builder.Services);
		AddViewModels(builder.Services);
		builder.Services.AddLogging(log =>
		{
#if DEBUG
			log.AddDebug();
#endif
			log.AddFile("ModManager");
		});
		builder.Services.AddTransient<MainWindow>();
		
		Host = builder.Build();

		_logger = Host.Services.GetRequiredService<ILogger<App>>();
	}

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		MainWindow = Host.Services.GetRequiredService<MainWindow>();
		MainWindow.Show();
	}

	private static void AddServices(IServiceCollection services)
	{
		services.AddTransient<NexusService>();
	}

	private static void AddStores(IServiceCollection services)
	{
		services.AddSingleton(static provider => new NavigationStore(provider, provider.GetRequiredService<DashboardPageViewModel>()));
		services.AddSingleton<ModStore>();
		services.AddSingleton<SettingsStore>();
	}

	private static void AddViewModels(IServiceCollection services)
	{
		services.AddTransient<MainViewModel>();
		services.AddTransient<DashboardPageViewModel>();
		services.AddTransient<SettingsPageViewModel>();
		services.AddTransient<CreatePageViewModel>();
		services.AddTransient<HelpPageViewModel>();
	}

	private void LogUnhandledException(Exception? ex)
	{
		if (_logger is null)
			MessageBox.Show($"An unhandled exception occurred before logging could be initialized!\n\n{ex?.ToString()}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
		else
			_logger?.LogError(ex, "An unhandled exception occured!");
	}
}

