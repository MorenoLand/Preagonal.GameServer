using GServ.Scripting;
using Xunit;

namespace GServ.Scripting.Tests;

public sealed class ScriptingBoundaryTests
{
    [Fact]
    public void ScriptingRuntimeIsExplicitlyUnimplementedUntilV8BehaviorIsPorted()
    {
        Assert.False(ScriptingRuntimeStatus.IsRuntimeImplemented);
        Assert.Contains("V8NPCSERVER", ScriptingRuntimeStatus.Blocker);
    }
}
