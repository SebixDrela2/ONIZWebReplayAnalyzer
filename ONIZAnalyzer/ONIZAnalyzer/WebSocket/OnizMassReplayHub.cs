using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OhNoItsZombiesAnalyzer.Services;
using ONIZAnalyzer.Common.WebSocket;

namespace ONIZAnalyzer.WebSocket;

using static OnizMassAnalysisConstants;

public class OnizMassReplayHub : Hub
{
    [HubMethodName(RequestMassReplayAnalysisMethodName)]
    public async Task StartMassReplayAnalysis([FromServices]ReplayService replayService)
    {
        var connectionId = Context.ConnectionId;
        await replayService.MassAnalyzeReplaysAsync(SendAnalysisResponse);

        async Task SendAnalysisResponse(int progress, TimeSpan averageTimeLeft) 
            => await Clients.Client(connectionId).SendAsync(ResponseMassReplayAnalysisMethodName, progress, averageTimeLeft, CancellationToken.None);
    }
}
