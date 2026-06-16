using GServ.Game;
using GServ.Protocol;

namespace GServ.Network;

public interface ILiveWorldSessionSink
{
    ushort PlayerId { get; }
    void QueuePacket(byte[] packet);
}

public sealed record LiveWorldForwardingDelivery(ushort PlayerId, byte[] Packet);

public enum LiveWorldPlayerPropsForwardingStatus
{
    Delivered,
    Blocked
}

public sealed record LiveWorldPlayerPropsForwardingResult(
    LiveWorldPlayerPropsForwardingStatus Status,
    string Message,
    IReadOnlyList<LiveWorldForwardingDelivery> Deliveries)
{
    public static LiveWorldPlayerPropsForwardingResult Delivered(IReadOnlyList<LiveWorldForwardingDelivery> deliveries) =>
        new(LiveWorldPlayerPropsForwardingStatus.Delivered, "Applied and forwarded confirmed player props.", deliveries);

    public static LiveWorldPlayerPropsForwardingResult Blocked(string message) =>
        new(LiveWorldPlayerPropsForwardingStatus.Blocked, message, []);
}

public static class LiveWorldSessionForwarder
{
    public static IReadOnlyList<LiveWorldForwardingDelivery> ForwardConfirmedOneLevelPacket(
        RuntimeServer server,
        RuntimeLevel level,
        byte[] packet,
        IReadOnlyDictionary<ushort, ILiveWorldSessionSink> sinks,
        IReadOnlySet<ushort>? exclude = null)
    {
        var recipients = LiveWorldForwardingSelector.SelectOneLevelRecipients(
            server,
            level,
            exclude);

        return Deliver(packet, recipients, sinks);
    }

    public static IReadOnlyList<LiveWorldForwardingDelivery> ForwardConfirmedLevelAreaPacket(
        RuntimeServer server,
        RuntimePlayer sender,
        byte[] packet,
        IReadOnlyDictionary<ushort, ILiveWorldSessionSink> sinks,
        IReadOnlySet<ushort>? exclude = null)
    {
        var recipients = LiveWorldForwardingSelector.SelectLevelAreaRecipients(
            server,
            sender,
            exclude ?? new HashSet<ushort> { sender.Id });

        return Deliver(packet, recipients, sinks);
    }

    public static IReadOnlyList<LiveWorldForwardingDelivery> ApplyAndForwardConfirmedPlayerProps(
        RuntimeServer server,
        RuntimePlayer sender,
        IEnumerable<IncomingPlayerPropertyUpdate> updates,
        bool senderSupportsPreciseMovement,
        IReadOnlyDictionary<ushort, ILiveWorldSessionSink> sinks)
    {
        var result = TryApplyAndForwardConfirmedPlayerProps(
            server,
            sender,
            updates,
            senderSupportsPreciseMovement,
            sinks);

        if (result.Status == LiveWorldPlayerPropsForwardingStatus.Blocked)
            throw new NotSupportedException(result.Message);

        return result.Deliveries;
    }

    public static LiveWorldPlayerPropsForwardingResult TryApplyAndForwardConfirmedPlayerProps(
        RuntimeServer server,
        RuntimePlayer sender,
        IEnumerable<IncomingPlayerPropertyUpdate> updates,
        bool senderSupportsPreciseMovement,
        IReadOnlyDictionary<ushort, ILiveWorldSessionSink> sinks)
    {
        var updateArray = updates.ToArray();
        foreach (var update in updateArray)
        {
            try
            {
                RuntimePlayerPropsApplier.ApplyConfirmed(sender, [update]);
            }
            catch (NotSupportedException ex)
            {
                return LiveWorldPlayerPropsForwardingResult.Blocked(
                    $"{CppNameOf(update.PropertyId)} was parsed with source-confirmed bytes, but its runtime side effects are not ported yet: {ex.Message}");
            }
        }

        var packet = IncomingPlayerPropsForwarding.BuildOtherPlayerPropsPacket(
            sender.Id,
            sender.PixelX,
            sender.PixelY,
            sender.PixelZ,
            updateArray,
            senderSupportsPreciseMovement,
            appendNewline: true,
            state: new IncomingPlayerPropsForwardingState(
                (byte)(sender.Hitpoints * 2.0f),
                CurrentLevelName: BuildCurrentLevelPropValue(sender),
                AccountName: sender.AccountName,
                AccountIp: sender.AccountIp,
                CommunityName: sender.CommunityName,
                EloRating: sender.EloRating,
                EloDeviation: sender.EloDeviation));

        var deliveries = ForwardConfirmedLevelAreaPacket(
            server,
            sender,
            packet,
            sinks,
            new HashSet<ushort> { sender.Id });

        return LiveWorldPlayerPropsForwardingResult.Delivered(deliveries);
    }

    private static IReadOnlyList<LiveWorldForwardingDelivery> Deliver(
        byte[] packet,
        IReadOnlyList<ushort> recipients,
        IReadOnlyDictionary<ushort, ILiveWorldSessionSink> sinks)
    {
        var deliveries = new List<LiveWorldForwardingDelivery>();
        foreach (var recipient in recipients)
        {
            if (!sinks.TryGetValue(recipient, out var sink))
                continue;

            sink.QueuePacket(packet);
            deliveries.Add(new LiveWorldForwardingDelivery(recipient, packet));
        }

        return deliveries;
    }

    private static string BuildCurrentLevelPropValue(RuntimePlayer sender)
    {
        if (sender.Level?.Map is { Type: RuntimeMapType.Gmap } map)
            return map.Name;

        if (sender.Level?.IsSingleplayer == true)
            return sender.CurrentLevelName + ".singleplayer";

        return sender.CurrentLevelName;
    }

    private static string CppNameOf(PlayerPropertyId propertyId) =>
        propertyId switch
        {
            PlayerPropertyId.Nickname => "PLPROP_NICKNAME",
            PlayerPropertyId.CarryNpc => "PLPROP_CARRYNPC",
            PlayerPropertyId.GmapLevelX => "PLPROP_GMAPLEVELX",
            PlayerPropertyId.GmapLevelY => "PLPROP_GMAPLEVELY",
            PlayerPropertyId.Status => "PLPROP_STATUS",
            _ => $"PLPROP_{(byte)propertyId}"
        };
}
