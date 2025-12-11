namespace AutomationPipelines.Tests;

[TestClass]
public sealed class PipelineNodeTests
{
    [TestMethod]
    public void Input_IgnoredByDefault()
    {
        // Arrange
        string? emittedOutput = null;

        var pipelineNode = new TestablePipelineNode<string>();
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

        var pipelineNode = new TestablePipelineNode<string>();
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

        var pipelineNode = new TestablePipelineNode<string>();
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipelineNode.PassThrough = true;
            
        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);

        pipelineNode.Input = testValue;

        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);
    }

    [TestMethod]
    public void PassThrough_ResetOnOutput()
    {
        const string inputValue = "Input";
        const string testValue = "Test";
        string? emittedOutput = null;

        var pipelineNode = new TestablePipelineNode<string>();
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipelineNode.Input = inputValue;

        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);

        pipelineNode.PassThrough = true;

        Assert.AreEqual(inputValue, emittedOutput);
        Assert.AreEqual(inputValue, pipelineNode.Output);

        pipelineNode.Output = testValue;

        Assert.IsFalse(pipelineNode.PassThrough);
        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);
    }

    [TestMethod]
    public void ChangeOutputAndTurnOnPassThroughOnNextInput_PassThroughTrueAfterNewInput()
    {
        const string inputValue1 = "Input1";
        const string inputValue2 = "Input2";
        const string inputValue3 = "Input3";
        const string testValue = "Test";
        string? emittedOutput = null;

        var pipelineNode = new TestablePipelineNode<string>();
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipelineNode.Input = inputValue1;

        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);

        pipelineNode.ChangeOutputAndTurnOnPassThroughOnNextInput(testValue);

        Assert.IsFalse(pipelineNode.PassThrough);
        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);
        
        pipelineNode.Input = inputValue2;

        Assert.IsTrue(pipelineNode.PassThrough);
        Assert.AreEqual(inputValue2, emittedOutput);
        Assert.AreEqual(inputValue2, pipelineNode.Output);

        pipelineNode.Input = inputValue3;

        Assert.IsTrue(pipelineNode.PassThrough);
        Assert.AreEqual(inputValue3, emittedOutput);
        Assert.AreEqual(inputValue3, pipelineNode.Output);
    }

    [TestMethod]
    public void ChangeOutputAndTurnOnPassThroughOnNextInput_Reset()
    {
        const string inputValue1 = "Input1";
        const string inputValue2 = "Input2";
        const string testValue = "Test";
        string? emittedOutput = null;

        var pipelineNode = new TestablePipelineNode<string>();
        pipelineNode.OnNewOutput.Subscribe(o => emittedOutput = o);

        pipelineNode.Input = inputValue1;

        Assert.IsNull(emittedOutput);
        Assert.IsNull(pipelineNode.Output);

        pipelineNode.ChangeOutputAndTurnOnPassThroughOnNextInput(testValue);

        Assert.IsFalse(pipelineNode.PassThrough);
        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);

        // This should reset the pass-through on new input behavior.
        pipelineNode.PassThrough = pipelineNode.PassThrough;

        pipelineNode.Input = inputValue2;

        Assert.IsFalse(pipelineNode.PassThrough);
        Assert.AreEqual(testValue, emittedOutput);
        Assert.AreEqual(testValue, pipelineNode.Output);
    }
}