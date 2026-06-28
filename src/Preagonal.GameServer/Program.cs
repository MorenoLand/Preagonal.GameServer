using System.Reflection;
using Preagonal.GameServer.Configuration;

namespace Preagonal.GameServer;

public static class Program
{

	/// <summary>
	///     Code borrowed from https://stackoverflow.com/questions/1600962/displaying-the-build-date
	/// </summary>
	public static DateTime BuildDateTime
	{
		get
		{
			var attribute = Assembly.GetExecutingAssembly()
			                        .GetCustomAttributes<AssemblyMetadataAttribute>()
			                        .FirstOrDefault(a => a.Key == "BuildTime");

			return attribute != null && DateTime.TryParse(attribute.Value, out var date) ? date : default;
		}
	}

	public static string? BuildVersion
	{
		get
		{
			var attribute = Assembly.GetExecutingAssembly()
			                        .GetCustomAttributes<AssemblyMetadataAttribute>()
			                        .FirstOrDefault(a => a.Key == "BuildVersion");

			return attribute?.Value;
		}
	}

	public static Task Main(string[] args)
		=> CreateWebHostBuilder(args).Build().RunAsync();

	private static IHostBuilder CreateWebHostBuilder(string[] args) =>
		Host.CreateDefaultBuilder(args)
		    .ConfigureAppConfiguration(
			    (_, config) =>
			    {
				    config.AddEnvironmentVariables();
				    config.AddCommandLine(args);
			    }
		    )
		    .ConfigureWebHostDefaults(webHost =>
		    {
			    webHost.ConfigureServices(services =>
			    {
				    services.AddSingleton<ICommandLineArguments>(new CommandLineArguments(args));
			    });
			    webHost.UseStartup<Startup>();
		    });
}