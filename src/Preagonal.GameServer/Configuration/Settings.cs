namespace Preagonal.GameServer.Configuration;

public class Settings
{
	public required GameServerSettings GameServerSettings { get; set; }
	public required ServerOptions      ServerOptions      { get; set; }
	public required AdminConfig         AdminConfig         { get; set; }
}