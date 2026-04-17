using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.DevUI.Example
{
    internal sealed partial class FormatterExecutor(): Executor("FormatterExecutor")
    {
        protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
        {
            return protocolBuilder;
        }

        [MessageHandler]
        private async ValueTask<string> HandleAsync(string feedback, IWorkflowContext context)
        {
            var output = $"""
                === Content Review Complete ===
                Reviewer feedback:

                {feedback}

                ==============================

                """;

            return output.Trim();
        }
    }

}
