using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace AgentFramework.DevUI.Example
{
    internal sealed partial class ReviewerExecutor(AIAgent agent): Executor("ReviewerExecutor")
    {
        protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
        {
            return protocolBuilder;
        }

        [MessageHandler]
        private async ValueTask<string> HandleAsync(string draft, IWorkflowContext context)
        {
            var response = await agent.RunAsync($"Review this draft and give concise feedback: {draft}");
            return response.Text;
        }
    }
}
