using Moq;

namespace AutomationPipelines.Tests;

[TestClass]
public sealed class PipelineNodeTests
{
    [TestMethod]
    public void Input_Ignored()
    {
        // Arrange
        string? emittedOutput = null;

        var pipelineNode = new Mock<PipelineNode<string>>().Object;
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        // Act
        pipelineNode.Input = "Test";

        // Assert
        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);
    }

    [TestMethod]
    public void PassThrough_ExistingInputPassed()
    {
        const string testValue = "Test";
        string? emittedOutput = null;

        var pipelineNode = new Mock<PipelineNode<string>>().Object;
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipelineNode.Input = testValue;

        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);

        pipelineNode.PassThrough = true;
            
        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);
    }

    [TestMethod]
    public void PassThrough_NewInputPassed()
    {
        const string testValue = "Test";
        string? emittedOutput = null;

        var pipelineNode = new Mock<PipelineNode<string>>().Object;
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipelineNode.PassThrough = true;
            
        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);

        pipelineNode.Input = testValue;

        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);
    }
}