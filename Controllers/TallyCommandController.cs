using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using TallyServer.Hubs;

namespace TallyServer.Controllers;

[ApiController]
[Route("api/tally")]
public class TallyCommandController : ControllerBase
{
    private readonly IHubContext<TallyHub> _hubContext;

    public TallyCommandController(
        IHubContext<TallyHub> hubContext)
    {
        _hubContext = hubContext;
    }

    // =====================================================
    // SEND COMMAND TO TALLY CONNECTOR
    // =====================================================

    [HttpPost("command")]
    public async Task<IActionResult> SendCommand(
     [FromBody] TallyCommandRequest request)
    {
        Console.WriteLine("=================================");
        Console.WriteLine("COMMAND RECEIVED FROM NODE");
        Console.WriteLine($"Device ID: {request.DeviceId}");
        Console.WriteLine($"Job ID: {request.JobId}");
        Console.WriteLine($"Command: {request.Command}");
        Console.WriteLine("=================================");

        if (string.IsNullOrWhiteSpace(
            request.DeviceId))
        {
            return BadRequest(new
            {
                success = false,
                message = "deviceId is required."
            });
        }

        if (string.IsNullOrWhiteSpace(
            request.Command))
        {
            return BadRequest(new
            {
                success = false,
                message = "command is required."
            });
        }

        var connectionId =
            TallyHub.GetConnectionId(
                request.DeviceId
            );

        if (connectionId == null)
        {
            Console.WriteLine(
                "TALLY CONNECTOR NOT FOUND / OFFLINE"
            );

            return NotFound(new
            {
                success = false,
                message =
                    "Tally connector is offline."
            });
        }

        Console.WriteLine(
            $"Sending command to Connection ID: {connectionId}"
        );

        await _hubContext.Clients
            .Client(connectionId)
            .SendAsync(
                "ReceiveCommand",
                new
                {
                    jobId = request.JobId,
                    command = request.Command
                }
            );

        Console.WriteLine(
            "COMMAND SENT TO TALLY CONNECTOR SUCCESSFULLY"
        );

        return Ok(new
        {
            success = true,
            message =
                "Command sent to Tally connector.",
            data = new
            {
                request.DeviceId,
                request.JobId,
                request.Command
            }
        });
    }
}

public class TallyCommandRequest
{
    public string DeviceId { get; set; } = "";

    public int JobId { get; set; }

    public string Command { get; set; } = "";
}