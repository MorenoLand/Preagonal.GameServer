using Preagonal.GameServer.Network;
using Xunit;

namespace Network.Tests;

public sealed class ClientCertificationHarnessTests
{
    [Fact]
    public void MatchingCaptureStepsAreCertified()
    {
        var comparison = ClientCertificationHarness.Compare(
            new("login-reject", [0x30, 0x41, 0x0A]),
            new("login-reject", [0x30, 0x41, 0x0A]));

        Assert.True(comparison.Certified);
        Assert.Equal(ClientCaptureMismatchKind.None, comparison.MismatchKind);
        Assert.Null(comparison.FirstMismatchOffset);
    }

    [Fact]
    public void FirstByteMismatchFailsCertificationWithExactOffset()
    {
        var comparison = ClientCertificationHarness.Compare(
            new("movement", [0x08, 0x20, 0x21]),
            new("movement", [0x08, 0x20, 0x22]));

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
            new("file-transfer", [0x66, 0x41, 0x42]),
            new("file-transfer", [0x66, 0x41]));

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
                [
	                new ClientCaptureStep("signature", [0x39]),
                    new ClientCaptureStep("unknown168", [0x68]),
                ]
            ),
            new(
                "csharp-login",
                [
	                new ClientCaptureStep("signature", [0x39]),
                    new ClientCaptureStep("unknown168", [0x69]),
                ]
            ));

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
                [new ClientCaptureStep("shutdown", [0x30])]
            ),
            new("csharp", []));

        Assert.False(result.Certified);
        Assert.Single(result.StepResults);
        Assert.Equal(ClientCaptureMismatchKind.MissingStep, result.StepResults[0].MismatchKind);
    }
}