using Preagonal.GameServer.Network;
using Xunit;

namespace Network.Tests;

public sealed class ClientCertificationHarnessTests
{
    [Fact]
    public void MatchingCaptureStepsAreCertified()
    {
        var comparison = ClientCertificationHarness.Compare(
            new("login-reject", new byte[] { 0x30, 0x41, 0x0A }),
            new("login-reject", new byte[] { 0x30, 0x41, 0x0A }));

        Assert.True(comparison.Certified);
        Assert.Equal(ClientCaptureMismatchKind.None, comparison.MismatchKind);
        Assert.Null(comparison.FirstMismatchOffset);
    }

    [Fact]
    public void FirstByteMismatchFailsCertificationWithExactOffset()
    {
        var comparison = ClientCertificationHarness.Compare(
            new("movement", new byte[] { 0x08, 0x20, 0x21 }),
            new("movement", new byte[] { 0x08, 0x20, 0x22 }));

        Assert.False(comparison.Certified);
        Assert.Equal(ClientCaptureMismatchKind.ByteMismatch, comparison.MismatchKind);
        Assert.Equal(2, comparison.FirstMismatchOffset);
        Assert.Equal(0x21, comparison.ExpectedByte);
        Assert.Equal(0x22, comparison.ActualByte);
    }

    [Fact]
    public void LengthMismatchFailsCertificationAtSharedLengthBoundary()
    {
        var comparison = ClientCertificationHarness.Compare(
            new("file-transfer", new byte[] { 0x66, 0x41, 0x42 }),
            new("file-transfer", new byte[] { 0x66, 0x41 }));

        Assert.False(comparison.Certified);
        Assert.Equal(ClientCaptureMismatchKind.LengthMismatch, comparison.MismatchKind);
        Assert.Equal(2, comparison.FirstMismatchOffset);
        Assert.Equal(3, comparison.ExpectedLength);
        Assert.Equal(2, comparison.ActualLength);
    }

    [Fact]
    public void FlowComparisonPreservesStepOrderAndLabels()
    {
        var result = ClientCertificationHarness.CompareFlow(
            new(
                "cpp-login",
                new[]
                {
                    new ClientCaptureStep("signature", new byte[] { 0x39 }),
                    new ClientCaptureStep("unknown168", new byte[] { 0x68 })
                }),
            new(
                "csharp-login",
                new[]
                {
                    new ClientCaptureStep("signature", new byte[] { 0x39 }),
                    new ClientCaptureStep("unknown168", new byte[] { 0x69 })
                }));

        Assert.False(result.Certified);
        Assert.Equal(2, result.StepResults.Count);
        Assert.True(result.StepResults[0].Certified);
        Assert.False(result.StepResults[1].Certified);
        Assert.Equal("unknown168", result.StepResults[1].Expected.Label);
    }

    [Fact]
    public void MissingFlowStepFailsCertification()
    {
        var result = ClientCertificationHarness.CompareFlow(
            new(
                "cpp",
                new[] { new ClientCaptureStep("shutdown", new byte[] { 0x30 }) }),
            new("csharp", Array.Empty<ClientCaptureStep>()));

        Assert.False(result.Certified);
        Assert.Single(result.StepResults);
        Assert.Equal(ClientCaptureMismatchKind.MissingStep, result.StepResults[0].MismatchKind);
    }
}
