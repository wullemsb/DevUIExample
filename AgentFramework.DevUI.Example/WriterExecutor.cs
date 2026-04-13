using Microsoft.Agents.AI;

using Microsoft.Agents.AI.Workflows;

using Microsoft.Extensions.AI;
namespace AgentFramework.DevUI.Example
{
    internal sealed partial class WriterExecutor(AIAgent agent) : Executor("WriterExecutor")
    {
        protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
        {
            return protocolBuilder;
        }

        [MessageHandler]

        private async ValueTask<string> HandleAsync(string topic, IWorkflowContext context)
        {
            var response = await agent.RunAsync(topic);
            return response.Text;
        }
    }
}
