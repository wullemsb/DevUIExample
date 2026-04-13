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
        private async ValueTask HandleAsync(string feedback, IWorkflowContext context)
        {
            var output = $"""
                === Content Review Complete ===
                Reviewer feedback:

                {feedback}

                ==============================

                """;

            await context.YieldOutputAsync(output.Trim());
        }
    }

}
