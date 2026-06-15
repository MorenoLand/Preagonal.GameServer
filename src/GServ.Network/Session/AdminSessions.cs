using GServ.Core.Ids;

namespace GServ.Network.Session;

public sealed class RemoteControlSession(PlayerId id) : ClientSessionBase(id, SessionKind.RemoteControl);

public sealed class NpcControlSession(PlayerId id) : ClientSessionBase(id, SessionKind.NpcControl);

public sealed class NpcServerSession(PlayerId id) : ClientSessionBase(id, SessionKind.NpcServer);
