using System.Text.Json.Serialization;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Logging.Console;
using Preagonal.Common.Scripting;
using Preagonal.GameServer.Configuration;
using Preagonal.GameServer.Services;

namespace Preagonal.GameServer;

/// <summary>
/// </summary>
public class Startup(IConfiguration configuration)
{
	private readonly IConfigurationRoot _configuration = (IConfigurationRoot)configuration;

	public void ConfigureServices(IServiceCollection services)
	{
		services.Configure<Settings>(_configuration);

		services.AddOptions();
		services.AddOptions<GameServerSettings>().Bind(_configuration.GetSection(nameof(GameServerSettings)));

		var hubSettings = _configuration
		                  .GetSection(nameof(GameServerSettings))
		                  .Get<GameServerSettings>()
		                  ?? throw new InvalidOperationException("Missing GameServerSettings configuration.");

		services.AddCors();

		services.AddResponseCompression(
			options =>
			{
				options.EnableForHttps = true;
				options.Providers.Add<GzipCompressionProvider>();
			}
		);

		services.AddLogging(
			configure =>
			{
				configure.AddSimpleConsole(options =>
				{
					options.IncludeScopes   = true;
					options.SingleLine      = true;
					options.TimestampFormat = "[hh:mm:ss] ";
					options.ColorBehavior   = LoggerColorBehavior.Enabled;
				});
			}
		);

		services.AddControllers(
			        options =>
			        {
				        options.RespectBrowserAcceptHeader = true;
				        options.EnableEndpointRouting      = false;
				        //AuthorizationPolicy policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
				        //options.Filters.Add( new AuthorizeFilter( policy ) );
			        }
		        )
		        .AddMvcOptions(
			        o => o.OutputFormatters.Add(
				        new XmlSerializerOutputFormatter(new XmlWriterSettings { Indent = true })
			        )
		        )
		        .AddJsonOptions(
			        options => options.JsonSerializerOptions.DefaultIgnoreCondition =
				        JsonIgnoreCondition.WhenWritingNull
		        );
		/*
		services.AddSwaggerGen(
			c =>
			{
				if (File.Exists(Path.Combine( AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml")))
					c.IncludeXmlComments( Path.Combine( AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml" ) );				c.SwaggerDoc("v1", new() { Title = "Preagonal Api", Version = "v1" });
				c.AddSecurityDefinition(
					JwtBearerDefaults.AuthenticationScheme,
					new OpenApiSecurityScheme
					{
						BearerFormat = "JWT",
						Name         = "Authorization",
						In           = ParameterLocation.Header,
						Type         = SecuritySchemeType.Http,
						Scheme       = JwtBearerDefaults.AuthenticationScheme,
						Description  = "Put **_ONLY_** your JWT Bearer token on textbox below!",
					}
				);

				c.OperationFilter<AuthorizeOnlyOperationFilter>();
			}
		);

		services.AddHttpContextAccessor();

		services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
		        .AddJwtBearer(
			        options =>
			        {
				        options.TokenValidationParameters = new()
				        {
					        ValidateIssuerSigningKey = true,
					        IssuerSigningKey =
						        new SymmetricSecurityKey(
							        Encoding.ASCII.GetBytes(
								        hubSettings.JwtSecretKey
							        )
						        ),
					        ValidateIssuer   = false,
					        ValidateAudience = false,
				        };
			        }
		        );
		services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
		services.AddScoped<IPasswordHasher<PlayerAccount>, PasswordHasher<PlayerAccount>>();
		services.AddScoped<IPasswordHasher<RegisteredServer>, PasswordHasher<RegisteredServer>>();
				#region MySQL Connection

		   switch (databaseSettings.Type)
		   {
		    case DatabaseType.MySql:
		    {
			    var connectionString = $"server={databaseSettings.Server};" +
			                           $"port={databaseSettings.Port};" +
			                           $"database={databaseSettings.Database};" +
			                           $"user={databaseSettings.Username};" +
			                           $"password={databaseSettings.Password}";
			    var mysqlServerVersion = ServerVersion.AutoDetect(connectionString);
			    services.AddDbContext<ListServerDbContext>(options => options.UseMySql(connectionString, mysqlServerVersion));
			    break;
		    }
		    case DatabaseType.SqlServer:
			    //services.AddDbContext<ListServerDbContext>(options => options.UseSql(connectionString, mysqlServerVersion));
			    break;
		    case DatabaseType.SQLite:
			    services.AddDbContext<ListServerDbContext>(options => options.UseSqlite(databaseSettings.Database));
			    break;
		    case DatabaseType.None:
		    default:
			    throw new ArgumentOutOfRangeException(nameof(databaseSettings.Type), databaseSettings.Type, "Missing database type.");
		   }

		   services.AddScoped<IUserRepository, UserRepository>();
		   services.AddScoped<IPlayerAccountRepository, PlayerAccountRepository>();
		   services.AddScoped<IServerRepository, ServerRepository>();
		   services.AddScoped<IPCIdRepository, PCIdRepository>();
		   services.AddScoped<IGuildRepository, GuildRepository>();

		   #endregion

		*/

		services.AddSingleton<IScriptManager, ScriptManager>(ScriptManager.CreateInstance);
		services.AddSingleton<IServiceProvider, ServiceProvider>();

		services.AddSingleton<IGameServerService, GameServerService>();
		services.AddHostedService<IGameServerService>(p => p.GetRequiredService<IGameServerService>());
	}

	/// <summary>
	///
	/// </summary>
	/// <param name="app"></param>
	/// <param name="env"></param>
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		//app.UpdateDatabase();

		if (env.IsDevelopment())
			app.UseDeveloperExceptionPage();
		else
			app.UseHsts();

		//app.UseMiddleware<PrependBearerSchemeMiddleware>();

		app.UseRouting();
		app.UseAuthentication();
		/*
		app.UseLoadCurrentUser();
		app.UseSwagger();
		app.UseSwaggerUI();
		*/

		app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
		app.UseHttpsRedirection();
		app.UseResponseCompression();

		//app.UseAuthorization();
		app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
	}
}