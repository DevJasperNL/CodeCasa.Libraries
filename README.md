# CodeCasa.Libraries
A collection of .NET libraries focused on smart home automation and general-purpose utilities.

## AutomationPipelines

Composable, reactive, and layered logic pipelines for automation.

In complex systems, multiple conditions often need to influence a single behavior. Instead of funneling all logic into a single block, **AutomationPipelines** encourages clean separation of concerns using a flexible, node-based pipeline model.

At its core is the `Pipeline<TState>` class, which coordinates a chain of self-contained logic units (`IPipelineNode<TState>`). Each node can independently process input, produce output, or opt out entirely—while maintaining full support for dependency injection.

### Why use AutomationPipelines?

* **Modular Logic**: Each pipeline node is a focused class with a single responsibility.
* **Dependency Injection Support**: Nodes can depend on external services, entities, or sensors—perfect for reactive systems like NetDaemon or Home Assistant.
* **Prioritized Overrides**: Later nodes in the pipeline can override the output of earlier ones, enabling intuitive layering and override mechanisms.
* **Reactive and Self-Contained**: Nodes are notified when input changes and can independently decide their output—or disable themselves to pass values through untouched.

This approach leads to cleaner, more maintainable automation logic—especially in event-driven or rules-based environments.

### Real-World Usage

This library was designed with automation platforms like [NetDaemon](https://netdaemon.xyz/) in mind but is suitable for any scenario where behavior is built from layered rules and inputs.

Check out [CodeCasa on GitHub](https://github.com/DevJasperNL/CodeCasa) for real-world examples and usage patterns.

### Example

```csharp
[NetDaemonApp]
internal class PipelineTest
{
    public PipelineTest(IPipeline<string> messagesPipeline)
    {
        messagesPipeline
            .RegisterNode<DefaultMessageNode>()
            .RegisterNode<OfficeLightsMessageNode>()
            .RegisterNode<CherryOnTopMessageNode>()
            .SetOutputHandler(Console.WriteLine);
    }
}

// Provides a default message when no other conditions are met
public class DefaultMessageNode : PipelineNode<string>
{
    public DefaultMessageNode()
    {
        Output = "Default message";
    }
}

// Overrides message if the office lights are turned on
public class OfficeLightsMessageNode : PipelineNode<string>
{
    public OfficeLightsMessageNode(LightEntities lightEntities)
    {
        lightEntities.OfficeLights.SubscribeOnOff(
            () => Output = "ON",
            DisableNode
        );
    }
}

// Modifies the incoming message by appending a "cherry on top"
public class CherryOnTopMessageNode : PipelineNode<string>
{
    protected override void InputReceived(string? input)
    {
        Output = string.IsNullOrEmpty(input)
            ? "Just a cherry"
            : $"{input} with a cherry on top";
    }
}
```

In this example:

* The `DefaultMessageNode` sets an initial message.
* The `OfficeLightsMessageNode` conditionally overrides it based on light state.
* The `CherryOnTopMessageNode` decorates the result with additional context.

The output handler is called whenever the final result changes, keeping logic reactive and centralized.
