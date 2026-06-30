using System.ComponentModel;
using System.Globalization;
using Preagonal.GameServer.Persistence;

namespace Preagonal.GameServer.Configuration;

public class ServerOptions : ISettings, IAccountLoadSettings
{
	private Gs2Settings? _settings;
	public  void         SetSettings(Gs2Settings settings) => _settings = settings;

	/// <summary>
	/// Sets the location where new players start on the server.
	/// By default, the server uses the values inside the defaultaccount.
	/// These values will override defaultaccount.
	/// </summary>
	[IniKey("startlevel")]
	[Description("Sets the location where new players start on the server. By default, the server uses the values inside the defaultaccount. These values will override defaultaccount.")]
	public string? StartLevel
	{
		get => _settings?.GetString("startlevel", null) ?? null;
		set => _settings?.SetValue("", "startlevel", value);
	}

	/// <summary>
	/// Sets the location where new players start on the server.
	/// By default, the server uses the values inside the defaultaccount.
	/// These values will override defaultaccount.
	/// </summary>
	[IniKey("startx")]
	[Description("Sets the location where new players start on the server. By default, the server uses the values inside the defaultaccount. These values will override defaultaccount.")]
	public float?  StartX
	{
		get => _settings?.GetFloat("startx", null) ?? null;
		set => _settings?.SetValue("", "startx", value?.ToString());
	}

	/// <summary>
	/// Sets the location where new players start on the server.
	/// By default, the server uses the values inside the defaultaccount.
	/// These values will override defaultaccount.
	/// </summary>
	[IniKey("starty")]
	[Description("Sets the location where new players start on the server. By default, the server uses the values inside the defaultaccount. These values will override defaultaccount.")]
	public float?  StartY
	{
		get => _settings?.GetFloat("starty", null) ?? null;
		set => _settings?.SetValue("", "starty", value?.ToString());
	}

	/// <summary>Specifies where players go when they say "unstick me".</summary>
	[IniKey("unstickmelevel")]
	[Description("Specifies where players go when they say \"unstick me\".")]
	public string UnstickMeLevel
	{
		get => GetString("unstickmelevel", "onlinestartlocal.nw")!;
		set => _settings?.SetValue("", "unstickmelevel", value);
	}

	/// <summary>Specifies where players go when they say "unstick me".</summary>
	[IniKey("unstickmex")]
	[Description("Specifies where players go when they say \"unstick me\".")]
	public float UnstickMeX
	{
		get => GetFloat("unstickmex", 30)!.Value;
		set => _settings?.SetValue("", "unstickmex", value.ToString("F"));
	}

	/// <summary>Specifies where players go when they say "unstick me".</summary>
	[IniKey("unstickmey")]
	[Description("Specifies where players go when they say \"unstick me\".")]
	public float UnstickMeY
	{
		get => GetFloat("unstickmey", 30.5f)!.Value;
		set => _settings?.SetValue("", "unstickmey", value.ToString("F"));
	}

	/// <summary>Specifies where players go when they say "unstick me".</summary>
	[IniKey("unstickmetime")]
	[Description("Specifies where players go when they say \"unstick me\".")]
	public float UnstickMeTime
	{
		get => GetFloat("unstickmetime", 30f)!.Value;
		set => _settings?.SetValue("", "unstickmetime", value.ToString("F"));
	}

	/// <summary>Players in these levels can't warp out nor can they PM other players.</summary>
	[IniKey("jaillevels")]
	[Description("Players in these levels can't warp out nor can they PM other players.")]
	public string[] JailLevels
	{
		get => GetString("jaillevels", "police2.graal,police4.graal")!.Split(',');
		set => _settings?.SetValue("", "jaillevels", string.Join(',', value));
	}

	/// <summary>Enable/disable explosions.</summary>
	[IniKey("noexplosions")]
	[Description("Enable/disable explosions.")]
	public bool NoExplosions
	{
		get => GetBool("noexplosions", false);
		set => _settings?.SetValue("", "noexplosions", value.ToString());
	}

	/// <summary>Enable/disable the ability of the player to change their look.</summary>
	[IniKey("setbodyallowed")]
	[Description("Enable/disable the ability of the player to change their look.")]
	public bool SetBodyAllowed
	{
		get => GetBool("setbodyallowed", true);
		set => _settings?.SetValue("", "setbodyallowed", value.ToString());
	}

	/// <summary>Enable/disable the ability of the player to change their look.</summary>
	[IniKey("setheadallowed")]
	[Description("Enable/disable the ability of the player to change their look.")]
	public bool SetHeadAllowed
	{
		get => GetBool("setheadallowed", true);
		set => _settings?.SetValue("", "setheadallowed", value.ToString());
	}

	/// <summary>Enable/disable the ability of the player to change their look.</summary>
	[IniKey("setshieldallowed")]
	[Description("Enable/disable the ability of the player to change their look.")]
	public bool SetShieldAllowed
	{
		get => GetBool("setshieldallowed", true);
		set => _settings?.SetValue("", "setshieldallowed", value.ToString());
	}

	/// <summary>Enable/disable the ability of the player to change their look.</summary>
	[IniKey("setswordallowed")]
	[Description("Enable/disable the ability of the player to change their look.")]
	public bool SetSwordAllowed
	{
		get => GetBool("setswordallowed", true);
		set => _settings?.SetValue("", "setswordallowed", value.ToString());
	}

	/// <summary>Enable/disable the ability of the player to change their look.</summary>
	[IniKey("setcolorsallowed")]
	[Description("Enable/disable the ability of the player to change their look.")]
	public bool SetColorsAllowed
	{
		get => GetBool("setcolorsallowed", true);
		set => _settings?.SetValue("", "setcolorsallowed", value.ToString());
	}

	/// <summary>Defines the amount of Gralats a player drops with they die.</summary>
	[IniKey("mindeathgralats")]
	[Description("Defines the amount of Gralats a player drops with they die.")]
	public int MinDeathGralats
	{
		get => GetInt("mindeathgralats", 1);
		set => _settings?.SetValue("", "mindeathgralats", value.ToString());
	}

	/// <summary>Defines the amount of Gralats a player drops with they die.</summary>
	[IniKey("maxdeathgralats")]
	[Description("Defines the amount of Gralats a player drops with they die.")]
	public int MaxDeathGralats
	{
		get => GetInt("maxdeathgralats", 50);
		set => _settings?.SetValue("", "maxdeathgralats", value.ToString());
	}

	/// <summary>If set to false, only players with the Change Staff Accounts right can alter gralats.</summary>
	[IniKey("normaladminscanchangegralats")]
	[Description("If set to false, only players with the Change Staff Accounts right can alter gralats.")]
	public bool NormalAdminsCanChangeGralats
	{
		get => GetBool("normaladminscanchangegralats", true);
		set => _settings?.SetValue("", "normaladminscanchangegralats", value.ToString());
	}

	/// <summary>These guilds appear in the "Staff" section of the player list.</summary>
	[IniKey("staffguilds")]
	[Description("These guilds appear in the \"Staff\" section of the player list.")]
	public string[] StaffGuilds
	{
		get =>
			GetString(
				"staffguilds",
				"Server,Manager,Owner,Admin,FAQ,LAT,NAT,GAT,GP,GP Chief,Bugs Admin,NPC Admin,Gani Team,GFX Admin,Events Team,Events Admin,Guild Admin"
			)!.Split(',');
		set => _settings?.SetValue("", "staffguilds", string.Join(',', value));
	}

	/// <summary>
	/// Accounts which are recognized by the server as staff. To be allowed access to RC, your account must be here.
	/// (Manager) and the like are just placeholders to organize the list. They are not guilds.
	/// </summary>
	[IniKey("staff")]
	[Description("Accounts which are recognized by the server as staff. To be allowed access to RC, your account must be here. (Manager) and the like are just placeholders to organize the list. They are not guilds.")]
	public string[] Staff
	{
		get => GetString("staff", "(Manager),YOURACCOUNT")!.Split(',');
		set => _settings?.SetValue("", "staff", string.Join(',', value));
	}

	/// <summary>
	/// Enables/disables item dropping from various sources.
	/// bushitems also affects certain tiles other than bushes.
	/// tiledroprate affects bushitems only.
	/// If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.
	/// </summary>
	[IniKey("bushitems")]
	[Description("Enables/disables item dropping from various sources. bushitems also affects certain tiles other than bushes. tiledroprate affects bushitems only. If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.")]
	public bool BushItems
	{
		get => GetBool("bushitems", true);
		set => _settings?.SetValue("", "bushitems", value.ToString());
	}

	/// <summary>
	/// Enables/disables item dropping from various sources.
	/// bushitems also affects certain tiles other than bushes.
	/// tiledroprate affects bushitems only.
	/// If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.
	/// </summary>
	[IniKey("vasesdrop")]
	[Description("Enables/disables item dropping from various sources. bushitems also affects certain tiles other than bushes. tiledroprate affects bushitems only. If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.")]
	public bool VasesDrop
	{
		get => GetBool("vasesdrop", true);
		set => _settings?.SetValue("", "vasesdrop", value.ToString());
	}

	/// <summary>
	/// Enables/disables item dropping from various sources.
	/// bushitems also affect certain tiles other than bushes.
	/// tiledroprate affects bushitems only.
	/// If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.
	/// </summary>
	[IniKey("baddyitems")]
	[Description("Enables/disables item dropping from various sources. bushitems also affects certain tiles other than bushes. tiledroprate affects bushitems only. If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.")]
	public bool BaddyItems
	{
		get => GetBool("baddyitems", false);
		set => _settings?.SetValue("", "baddyitems", value.ToString());
	}

	/// <summary>
	/// Enables/disables item dropping from various sources.
	/// bushitems also affect certain tiles other than bushes.
	/// tiledroprate affects bushitems only.
	/// If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.
	/// </summary>
	[IniKey("dropitemsdead")]
	[Description("Enables/disables item dropping from various sources. bushitems also affects certain tiles other than bushes. tiledroprate affects bushitems only. If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.")]
	public bool DropItemsDead
	{
		get => GetBool("dropitemsdead", true);
		set => _settings?.SetValue("", "dropitemsdead", value.ToString());
	}

	/// <summary>
	/// Enables/disables item dropping from various sources.
	/// bushitems also affects certain tiles other than bushes.
	/// tiledroprate affects bushitems only.
	/// If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.
	/// </summary>
	[IniKey("tiledroprate")]
	[Description("Enables/disables item dropping from various sources. bushitems also affects certain tiles other than bushes. tiledroprate affects bushitems only. If making a 1.41 server, set bushitems, vasesdrop, and baddyitems to false as the 1.41 client generates items.")]
	public int TileDropRate
	{
		get => GetInt("tiledroprate", 50);
		set => _settings?.SetValue("", "tiledroprate", value.ToString());
	}

	/// <summary>If enabled, it will allow negative power swords which will heal players when used.</summary>
	[IniKey("healswords")]
	[Description("If enabled, it will allow negative power swords which will heal players when used.")]
	public bool HealSwords
	{
		get => GetBool("healswords", false);
		set => _settings?.SetValue("", "healswords", value.ToString());
	}

	/// <summary>
	/// Timeout in seconds for respawning objects.
	/// respawntime affects tile changes.
	/// </summary>
	[IniKey("respawntime")]
	[Description("Timeout in seconds for respawning objects. respawntime affects tile changes.")]
	public int RespawnTime
	{
		get => GetInt("respawntime", 15);
		set => _settings?.SetValue("", "respawntime", value.ToString());
	}

	/// <summary>Timeout in seconds for respawning objects.</summary>
	[IniKey("horselifetime")]
	[Description("Timeout in seconds for respawning objects.")]
	public int HorseLifeTime
	{
		get => GetInt("horselifetime", 30);
		set => _settings?.SetValue("", "horselifetime", value.ToString());
	}

	/// <summary>Timeout in seconds for respawning objects.</summary>
	[IniKey("baddyrespawntime")]
	[Description("Timeout in seconds for respawning objects.")]
	public int BaddyRespawnTime
	{
		get => GetInt("baddyrespawntime", 60);
		set => _settings?.SetValue("", "baddyrespawntime", value.ToString());
	}

	/// <summary>Allows any player to use the warpto command.</summary>
	[IniKey("warptoforall")]
	[Description("Allows any player to use the warpto command.")]
	public bool WarptoForAll
	{
		get => GetBool("warptoforall", false);
		set => _settings?.SetValue("", "warptoforall", value.ToString());
	}

	/// <summary>Alters the possible status options in the player list.</summary>
	[IniKey("playerlisticons")]
	[Description("Alters the possible status options in the player list.")]
	public string PlayerListIcons
	{
		get => GetString("playerlisticons", "Online,Away,DND,Eating,Hiding,No PMs,RPing,Sparring,PKing")!;
		set => _settings?.SetValue("", "playerlisticons", value);
	}

	/// <summary>
	/// Selects what is displayed in the player's profile.
	/// Name:=variable, where variable can also be a flag on the player's account.
	/// </summary>
	[IniKey("profilevars")]
	[Description("Selects what is displayed in the player's profile. Name:=variable, where variable can also be a flag on the player's account.")]
	public string ProfileVars
	{
		get => GetString("profilevars", "Kills:=playerkills,Deaths:=playerdeaths,Maxpower:=playerfullhearts,Rating:=playerrating,Alignment:=playerap,Gralat:=playerrupees,Swordpower:=playerswordpower,Spin Attack:=canspin")!;
		set => _settings?.SetValue("", "profilevars", value);
	}

	/// <summary>
	/// Global guild settings.
	/// If globalguilds is true, global guilds are allowed. If false, allowedglobalguilds specifies which guilds are allowed.
	/// </summary>
	[IniKey("globalguilds")]
	[Description("Global guild settings. If globalguilds is true, global guilds are allowed. If false, allowedglobalguilds specifies which guilds are allowed.")]
	public bool GlobalGuilds
	{
		get => GetBool("globalguilds", true);
		set => _settings?.SetValue("", "globalguilds", value.ToString());
	}

	/// <summary>
	/// Global guild settings.
	/// If globalguilds is true, global guilds are allowed. If false, allowedglobalguilds specifies which guilds are allowed.
	/// </summary>
	[IniKey("allowedglobalguilds")]
	[Description("Global guild settings. If globalguilds is true, global guilds are allowed. If false, allowedglobalguilds specifies which guilds are allowed.")]
	public string[] AllowedGlobalGuilds
	{
		get => GetString("allowedglobalguilds", string.Empty)!.Split(',');
		set => _settings?.SetValue("", "allowedglobalguilds", string.Join(',', value));
	}

	/// <summary>
	/// AP system settings.
	/// If apsystem is set to true, it activates some restrictions regarding hearts for low AP players.
	/// For the aptime# options, the values are the time in seconds it takes to recharge one point of AP for the given range.
	/// aptime4 is used for AP values between 80 and 100. aptime3 for 60 through 80. And so on until 0 is between 0 and 20.
	/// </summary>
	[IniKey("apsystem")]
	[Description("AP system settings. If apsystem is set to true, it activates some restrictions regarding hearts for low AP players. For the aptime# options, the values are the time in seconds it takes to recharge one point of AP for the given range. aptime4 is used for AP values between 80 and 100. aptime3 for 60 through 80. And so on until 0 is between 0 and 20.")]
	public bool ApSystem
	{
		get => GetBool("apsystem", true);
		set => _settings?.SetValue("", "apsystem", value.ToString());
	}

	/// <summary>AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 0 and 20.</summary>
	[IniKey("aptime0")]
	[Description("AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 0 and 20.")]
	public int ApTime0
	{
		get => GetInt("aptime0", 30);
		set => _settings?.SetValue("", "aptime0", value.ToString());
	}

	/// <summary>AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 20 and 40.</summary>
	[IniKey("aptime1")]
	[Description("AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 20 and 40.")]
	public int ApTime1
	{
		get => GetInt("aptime1", 90);
		set => _settings?.SetValue("", "aptime1", value.ToString());
	}

	/// <summary>AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 40 and 60.</summary>
	[IniKey("aptime2")]
	[Description("AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 40 and 60.")]
	public int ApTime2
	{
		get => GetInt("aptime2", 300);
		set => _settings?.SetValue("", "aptime2", value.ToString());
	}

	/// <summary>AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 60 and 80.</summary>
	[IniKey("aptime3")]
	[Description("AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 60 and 80.")]
	public int ApTime3
	{
		get => GetInt("aptime3", 600);
		set => _settings?.SetValue("", "aptime3", value.ToString());
	}

	/// <summary>AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 80 and 100.</summary>
	[IniKey("aptime4")]
	[Description("AP system setting. Time in seconds it takes to recharge one point of AP for AP values between 80 and 100.")]
	public int ApTime4
	{
		get => GetInt("aptime4", 1200);
		set => _settings?.SetValue("", "aptime4", value.ToString());
	}

	/// <summary>Defines limits to hearts, swords, and shields.</summary>
	[IniKey("heartlimit")]
	[Description("Defines limits to hearts, swords, and shields.")]
	public int HeartLimit
	{
		get => GetInt("heartlimit", 3);
		set => _settings?.SetValue("", "heartlimit", value.ToString());
	}

	/// <summary>Defines limits to hearts, swords, and shields.</summary>
	[IniKey("swordlimit")]
	[Description("Defines limits to hearts, swords, and shields.")]
	public int SwordLimit
	{
		get => GetInt("swordlimit", 3);
		set => _settings?.SetValue("", "swordlimit", value.ToString());
	}

	/// <summary>Defines limits to hearts, swords, and shields.</summary>
	[IniKey("shieldlimit")]
	[Description("Defines limits to hearts, swords, and shields.")]
	public int ShieldLimit
	{
		get => GetInt("shieldlimit", 3);
		set => _settings?.SetValue("", "shieldlimit", value.ToString());
	}

	/// <summary>Enables or disables the putnpc script command.</summary>
	[IniKey("putnpcenabled")]
	[Description("Enables or disables the putnpc script command.")]
	public bool PutNpcEnabled
	{
		get => GetBool("putnpcenabled", true);
		set => _settings?.SetValue("", "putnpcenabled", value.ToString());
	}

	/// <summary>If true, disable the ability.</summary>
	[IniKey("dontchangekills")]
	[Description("If true, disable the ability.")]
	public bool DontChangeKills
	{
		get => GetBool("dontchangekills", false);
		set => _settings?.SetValue("", "dontchangekills", value.ToString());
	}

	/// <summary>
	/// Flag options.
	/// If dontaddserverflags is true, any server. flag changes sent by the client are rejected.
	/// </summary>
	[IniKey("dontaddserverflags")]
	[Description("Flag options. If dontaddserverflags is true, any server. flag changes sent by the client are rejected.")]
	public bool DontAddServerFlags
	{
		get => GetBool("dontaddserverflags", false);
		set => _settings?.SetValue("", "dontaddserverflags", value.ToString());
	}

	/// <summary>
	/// Flag options.
	/// If cropflags is true, any client and server flags will be cropped to 223 characters.
	/// The flag name and equal sign are INCLUSIVE!
	/// It is recommended to not turn this off unless you know the repercussions of doing so.
	/// </summary>
	[IniKey("cropflags")]
	[Description("Flag options. If cropflags is true, any client and server flags will be cropped to 223 characters. The flag name and equal sign are INCLUSIVE! It is recommended to not turn this off unless you know the repercussions of doing so.")]
	public bool CropFlags
	{
		get { return GetBool("cropflags", true); }
		set => _settings?.SetValue("", "cropflags", value.ToString());
	}

	/// <summary>If true, idle players are removed after maxnomovement seconds.</summary>
	[IniKey("disconnectifnotmoved")]
	[Description("If true, idle players are removed after maxnomovement seconds.")]
	public bool DisconnectIfNotMoved
	{
		get => GetBool("disconnectifnotmoved", true);
		set => _settings?.SetValue("", "disconnectifnotmoved", value.ToString());
	}

	/// <summary>If true, idle players are removed after maxnomovement seconds.</summary>
	[IniKey("maxnomovement")]
	[Description("If true, idle players are removed after maxnomovement seconds.")]
	public int MaxNoMovement
	{
		get => GetInt("maxnomovement", 1200);
		set => _settings?.SetValue("", "maxnomovement", value.ToString());
	}

	/// <summary>If true, moved push/pull blocks aren't sent to other players.</summary>
	[IniKey("clientsidepushpull")]
	[Description("If true, moved push/pull blocks aren't sent to other players.")]
	public bool ClientsidePushPull
	{
		get => GetBool("clientsidepushpull", true);
		set => _settings?.SetValue("", "clientsidepushpull", value.ToString());
	}

	/// <summary>If false, it will prevent the player from obtaining items like bomb, bow, superbomb, etc.</summary>
	[IniKey("defaultweapons")]
	[Description("If false, it will prevent the player from obtaining items like bomb, bow, superbomb, etc.")]
	public bool DefaultWeapons
	{
		get => GetBool("defaultweapons", true);
		set => _settings?.SetValue("", "defaultweapons", value.ToString());
	}

	/// <summary>List of weapon names (comma separated) that will be given to the player each time they connect.</summary>
	[IniKey("protectedweapons")]
	[Description("List of weapon names (comma separated) that will be given to the player each time they connect.")]
	public string ProtectedWeapons
	{
		get => GetString("protectedweapons", string.Empty)!;
		set => _settings?.SetValue("", "protectedweapons", value);
	}

	/// <summary>
	/// List of bigmap.txt type maps used by the server. It lets the server know the level layout
	/// so you can see players move and talk in adjacent levels.
	/// </summary>
	[IniKey("maps")]
	[Description("List of bigmap.txt type maps used by the server. It lets the server know the level layout so you can see players move and talk in adjacent levels.")]
	public string[] Maps
	{
		get => GetString("maps", string.Empty)!.Split(',');
		set => _settings?.SetValue("", "maps", string.Join(',', value));
	}

	/// <summary>List of gmaps to be used by the server.</summary>
	[IniKey("gmaps")]
	[Description("List of gmaps to be used by the server.")]
	public string[] GMaps
	{
		get => GetString("gmaps", string.Empty)!.Split(',');
		set => _settings?.SetValue("", "gmaps", string.Join(',', value));
	}

	/// <summary>
	/// List of group instanced maps used by the server.
	/// Use full filenames, even for gmaps.
	/// </summary>
	[IniKey("groupmaps")]
	[Description("List of group instanced maps used by the server. Use full filenames, even for gmaps.")]
	public string[] GroupMaps
	{
		get => GetString("groupmaps", string.Empty)!.Split(',');
		set => _settings?.SetValue("", "groupmaps", string.Join(',', value));
	}

	/// <summary>The head used by RCs on the server.</summary>
	[IniKey("staffhead")]
	[Description("The head used by RCs on the server.")]
	public string StaffHead
	{
		get => GetString("staffhead", "head25.png")!;
		set => _settings?.SetValue("", "staffhead", value);
	}

	/// <summary>
	/// Sets the bigmap and minimap to use.
	/// Setting bigmap will break gmaps.
	/// bigmap = maptext,mapimage,defaultx,defaulty
	/// minimap = maptext,mapimage,defaultx,defaulty
	/// maptext is the bigmap.txt styled file with the levels.
	/// mapimage is the image to use.
	/// defaultx and defaulty is the position where the heads of players not on the map will be drawn.
	/// </summary>
	[IniKey("bigmap")]
	[Description("Sets the bigmap and minimap to use. Setting bigmap will break gmaps. bigmap = maptext,mapimage,defaultx,defaulty minimap = maptext,mapimage,defaultx,defaulty maptext is the bigmap.txt styled file with the levels. mapimage is the image to use. defaultx and defaulty is the position where the heads of players not on the map will be drawn.")]
	public string BigMap
	{
		get => GetString("bigmap", string.Empty)!;
		set => _settings?.SetValue("", "bigmap", value);
	}

	/// <summary>
	/// Sets the bigmap and minimap to use.
	/// Setting bigmap will break gmaps.
	/// bigmap = maptext,mapimage,defaultx,defaulty
	/// minimap = maptext,mapimage,defaultx,defaulty
	/// maptext is the bigmap.txt styled file with the levels.
	/// mapimage is the image to use.
	/// defaultx and defaulty is the position where the heads of players not on the map will be drawn.
	/// </summary>
	[IniKey("minimap")]
	[Description("Sets the bigmap and minimap to use. Setting bigmap will break gmaps. bigmap = maptext,mapimage,defaultx,defaulty minimap = maptext,mapimage,defaultx,defaulty maptext is the bigmap.txt styled file with the levels. mapimage is the image to use. defaultx and defaulty is the position where the heads of players not on the map will be drawn.")]
	public string MiniMap
	{
		get => GetString("minimap", string.Empty)!;
		set => _settings?.SetValue("", "minimap", value);
	}

	/// <summary>The server details seen from the server list.</summary>
	[IniKey("name")]
	[Description("The server details seen from the server list.")]
	public string Name
	{
		get => GetString("name", "SharpServer")!;
		set => _settings?.SetValue("", "name", value);
	}

	/// <summary>The server details seen from the server list.</summary>
	[IniKey("description")]
	[Description("The server details seen from the server list.")]
	public string Description
	{
		get => GetString("description", "CSharp GServer")!;
		set => _settings?.SetValue("", "description", value);
	}

	/// <summary>The server details seen from the server list.</summary>
	[IniKey("url")]
	[Description("The server details seen from the server list.")]
	public string Url
	{
		get => GetString("url", "https://github.com/MorenoLand/Preagonal.GameServer")!;
		set => _settings?.SetValue("", "url", value);
	}

	/// <summary>
	/// The information of the computer hosting the gserver. This gets sent to people wanting to connect.
	/// If myip is set to AUTO, it uses the IP address exposed to the list server.
	/// </summary>
	[IniKey("serverip")]
	[Description("The information of the computer hosting the gserver. This gets sent to people wanting to connect. If myip is set to AUTO, it uses the IP address exposed to the list server.")]
	public string ServerIp
	{
		get => GetString("serverip", "AUTO")!;
		set => _settings?.SetValue("", "serverip", value);
	}

	/// <summary>The information of the computer hosting the gserver. This gets sent to people wanting to connect.</summary>
	[IniKey("serverport")]
	[Description("The information of the computer hosting the gserver. This gets sent to people wanting to connect.")]
	public int ServerPort
	{
		get => GetInt("serverport", 15002);
		set => _settings?.SetValue("", "serverport", value.ToString());
	}

	/// <summary>The information of the computer hosting the gserver. This gets sent to people wanting to connect.</summary>
	[IniKey("serverinterface")]
	[Description("The information of the computer hosting the gserver. This gets sent to people wanting to connect.")]
	public string ServerInterface
	{
		get => GetString("serverinterface", "AUTO")!;
		set => _settings?.SetValue("", "serverinterface", value);
	}

	/// <summary>
	/// The local IP address of the computer. Helps you connect to your server if your router can't route on
	/// its WAN-side IP address. Leave it as AUTO unless you know what you are doing.
	/// If you have a Linux server, you will want to change this, though.
	/// </summary>
	[IniKey("localip")]
	[Description("The local IP address of the computer. Helps you connect to your server if your router can't route on its WAN-side IP address. Leave it as AUTO unless you know what you are doing. If you have a Linux server, you will want to change this, though.")]
	public string LocalIp
	{
		get => GetString("localip", "AUTO")!;
		set => _settings?.SetValue("", "localip", value);
	}

	/// <summary>
	/// Specifies the location of the list server.
	/// DON'T CHANGE IF YOU DON'T KNOW WHAT YOU ARE DOING.
	/// </summary>
	[IniKey("listip")]
	[Description("Specifies the location of the list server. DON'T CHANGE IF YOU DON'T KNOW WHAT YOU ARE DOING.")]
	public string ListIp
	{
		get => GetString("listip", "listserver.graal.in")!;
		set => _settings?.SetValue("", "listip", value);
	}

	/// <summary>
	/// Specifies the location of the list server.
	/// DON'T CHANGE IF YOU DON'T KNOW WHAT YOU ARE DOING.
	/// </summary>
	[IniKey("listport")]
	[Description("Specifies the location of the list server. DON'T CHANGE IF YOU DON'T KNOW WHAT YOU ARE DOING.")]
	public int ListPort
	{
		get => GetInt("listport", 14900);
		set => _settings?.SetValue("", "listport", value.ToString());
	}

	/// <summary>Maximum number of players allowed on the server.</summary>
	[IniKey("maxplayers")]
	[Description("Maximum number of players allowed on the server.")]
	public int MaxPlayers
	{
		get => GetInt("maxplayers", 128);
		set => _settings?.SetValue("", "maxplayers", value.ToString());
	}

	/// <summary>Enables/disables staff only. If true, only accounts in the staff option are allowed on.</summary>
	[IniKey("onlystaff")]
	[Description("Enables/disables staff only. If true, only accounts in the staff option are allowed on.")]
	public bool OnlyStaff
	{
		get => GetBool("onlystaff", false);
		set => _settings?.SetValue("", "onlystaff", value.ToString());
	}

	/// <summary>Set to true to disable the folder configuration.</summary>
	[IniKey("nofoldersconfig")]
	[Description("Set to true to disable the folder configuration.")]
	public bool NoFoldersConfig
	{
		get => GetBool("nofoldersconfig", false);
		set => _settings?.SetValue("", "nofoldersconfig", value.ToString());
	}

	/// <summary>
	/// Determines whether or not to use the old "if (created)" style.
	/// In the old style, "if (created)" is called for each player that enters the level for their first time.
	/// </summary>
	[IniKey("oldcreated")]
	[Description("Determines whether or not to use the old \"if (created)\" style. In the old style, \"if (created)\" is called for each player that enters the level for their first time.")]
	public bool OldCreated
	{
		get => GetBool("oldcreated", true);
		set => _settings?.SetValue("", "oldcreated", value.ToString());
	}

	/// <summary>
	/// Determines whether the server handles certain things like signs and links.
	/// Don't set to true.
	/// </summary>
	[IniKey("serverside")]
	[Description("Determines whether the server handles certain things like signs and links. Don't set to true.")]
	public bool ServerSide
	{
		get => GetBool("serverside", true);
		set => _settings?.SetValue("", "serverside", value.ToString());
	}

	/// <summary>Enables triggeraction hacks: gr.addweapon, gr.deleteweapon.</summary>
	[IniKey("triggerhack_weapons")]
	[Description("Enables triggeraction hacks: gr.addweapon, gr.deleteweapon.")]
	public bool TriggerHackWeapons
	{
		get => GetBool("triggerhack_weapons", false);
		set => _settings?.SetValue("", "triggerhack_weapons", value.ToString());
	}

	/// <summary>Enables triggeraction hacks: gr.addguildmember, gr.removeguildmember, gr.removeguild, gr.setguild.</summary>
	[IniKey("triggerhack_guilds")]
	[Description("Enables triggeraction hacks: gr.addguildmember, gr.removeguildmember, gr.removeguild, gr.setguild.")]
	public bool TriggerHackGuilds
	{
		get => GetBool("triggerhack_guilds", false);
		set => _settings?.SetValue("", "triggerhack_guilds", value.ToString());
	}

	/// <summary>Enables triggeraction hacks: gr.setgroup, gr.setlevelgroup.</summary>
	[IniKey("triggerhack_groups")]
	[Description("Enables triggeraction hacks: gr.setgroup, gr.setlevelgroup.")]
	public bool TriggerHackGroups
	{
		get => GetBool("triggerhack_groups", true);
		set => _settings?.SetValue("", "triggerhack_groups", value.ToString());
	}

	/// <summary>Enables triggeraction hacks: gr.appendfile, gr.writefile.</summary>
	[IniKey("triggerhack_files")]
	[Description("Enables triggeraction hacks: gr.appendfile, gr.writefile.")]
	public bool TriggerHackFiles
	{
		get => GetBool("triggerhack_files", false);
		set => _settings?.SetValue("", "triggerhack_files", value.ToString());
	}

	/// <summary>Enables triggeraction hacks: gr.rcchat.</summary>
	[IniKey("triggerhack_rc")]
	[Description("Enables triggeraction hacks: gr.rcchat.")]
	public bool TriggerHackRc
	{
		get => GetBool("triggerhack_rc", false);
		set => _settings?.SetValue("", "triggerhack_rc", value.ToString());
	}

	/// <summary>Enables triggeraction hacks: gr.es_clear, gr.es_set, gr.es_append, gr.es.</summary>
	[IniKey("triggerhack_execscript")]
	[Description("Enables triggeraction hacks: gr.es_clear, gr.es_set, gr.es_append, gr.es.")]
	public bool TriggerHackExecScript
	{
		get => GetBool("triggerhack_execscript", false);
		set => _settings?.SetValue("", "triggerhack_execscript", value.ToString());
	}

	/// <summary>Enables triggeraction hacks: gr.attr1-gr.attr30.</summary>
	[IniKey("triggerhack_props")]
	[Description("Enables triggeraction hacks: gr.attr1-gr.attr30.")]
	public bool TriggerHackProps
	{
		get => GetBool("triggerhack_props", false);
		set => _settings?.SetValue("", "triggerhack_props", value.ToString());
	}

	/// <summary>Enables triggeraction hacks: gr.updatelevel.</summary>
	[IniKey("triggerhack_levels")]
	[Description("Enables triggeraction hacks: gr.updatelevel.")]
	public bool TriggerHackLevels
	{
		get => GetBool("triggerhack_levels", false);
		set => _settings?.SetValue("", "triggerhack_levels", value.ToString());
	}

	/// <summary>Enables flag hacks: -gr_movement weapon.</summary>
	[IniKey("flaghack_movement")]
	[Description("Enables flag hacks: -gr_movement weapon.")]
	public bool FlagHackMovement
	{
		get => GetBool("flaghack_movement", true);
		set => _settings?.SetValue("", "flaghack_movement", value.ToString());
	}

	/// <summary>Enables flag hacks: gr.ip.</summary>
	[IniKey("flaghack_ip")]
	[Description("Enables flag hacks: gr.ip.")]
	public bool FlagHackIp
	{
		get => GetBool("flaghack_ip", false);
		set => _settings?.SetValue("", "flaghack_ip", value.ToString());
	}

	/// <summary>
	/// If folders config is disabled, put additional search directories besides "world" here.
	/// Comma delimited array.
	/// </summary>
	[IniKey("sharefolder")]
	[Description("If folders config is disabled, put additional search directories besides \"world\" here. Comma delimited array.")]
	public string ShareFolder
	{
		get => GetString("sharefolder", string.Empty)!;
		set => _settings?.SetValue("", "sharefolder", value);
	}

	/// <summary>Sets the language. Currently not implemented.</summary>
	[IniKey("language")]
	[Description("Sets the language. Currently not implemented.")]
	public string Language
	{
		get => GetString("language", "English")!;
		set => _settings?.SetValue("", "language", value);
	}

	/// <summary>Scripting.</summary>
	[IniKey("gs2default")]
	[Description("Scripting.")]
	public bool Gs2Default
	{
		get { return GetBool("gs2default", false); }
		set => _settings?.SetValue("", "gs2default", value.ToString());
	}

	/// <summary>Scripting.</summary>
	[IniKey("nickname")]
	[Description("Scripting.")]
	public string Nickname
	{
		get => GetString("nickname", string.Empty)!;
		set => _settings?.SetValue("", "nickname", value);
	}

	public bool IsLoaded => _settings?.IsLoaded ?? false;

	public bool Exists(string key) => _settings?.ContainsKey(key) ?? false;
	public string? GetString(string key, string? defaultValue) => _settings?.GetString(key, defaultValue) ?? defaultValue;
	public float? GetFloat(string key, float? defaultValue) => _settings?.GetFloat(key, defaultValue) ?? defaultValue;
	public bool GetBool(string key, bool defaultValue = true) => _settings?.GetBool(key, defaultValue) ?? defaultValue;
	public int GetInt(string key, int defaultValue = 1) => _settings?.GetInt(key, defaultValue) ?? defaultValue;
}