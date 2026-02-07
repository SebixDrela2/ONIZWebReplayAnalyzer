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
        await replayService.MassAnalyzeReplays(async (int progress, TimeSpan averageTimeLeft) =>
        {
            await Clients.Caller.SendAsync(ResponseMassReplayAnalysisMethodName, progress, averageTimeLeft, CancellationToken.None);
        });
    }
}
