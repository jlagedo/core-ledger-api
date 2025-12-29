using CoreLedger.Application.Interfaces;
using CoreLedger.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreLedger.API.Controllers;

/// <summary>
/// Controller for Server-Sent Events (SSE) notifications.
/// Provides real-time job status updates via SSE streaming.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly INotificationChannelStore _channelStore;
    private readonly ILogger<NotificationsController> _logger;

    /// <summary>
    /// Initializes a new instance of the NotificationsController class.
    /// </summary>
    /// <param name="channelStore">The notification channel store for managing SSE connections.</param>
    /// <param name="logger">The logger for structured logging.</param>
    public NotificationsController(
        INotificationChannelStore channelStore,
        ILogger<NotificationsController> logger)
    {
        _channelStore = channelStore;
        _logger = logger;
    }

    /// <summary>
    /// Opens a Server-Sent Events (SSE) connection for real-time job status notifications.
    /// The connection remains open and sends events as they occur. Heartbeats are sent every 15 seconds.
    /// </summary>
    /// <param name="userId">The user ID to receive notifications for.</param>
    /// <param name="cancellationToken">Cancellation token to detect client disconnection.</param>
    /// <returns>An SSE stream of job status change events.</returns>
    /// <remarks>
    /// This endpoint returns a text/event-stream response that stays open.
    /// Clients should use the EventSource API or similar SSE client to consume events.
    ///
    /// Event format:
    /// - Heartbeats: ": heartbeat\n\n" (every 15 seconds)
    /// - Job status changes: "event: jobStatusChange\nid: {jobId}\ndata: {json}\n\n"
    ///
    /// Example client (JavaScript):
    /// ```javascript
    /// const eventSource = new EventSource('/api/notifications?userId=1000');
    /// eventSource.addEventListener('jobStatusChange', (event) => {
    ///   const data = JSON.parse(event.data);
    ///   console.log('Job status:', data);
    /// });
    /// ```
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int userId,
        CancellationToken cancellationToken = default)
    {
        // Check if the server is shutting down
        if (_channelStore.IsShuttingDown)
        {
            _logger.LogWarning("Rejecting SSE connection for userId {UserId} - server is shutting down", userId);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, "Server is shutting down");
        }

        // Set SSE headers
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        // Get or create a channel for this user
        var userChannel = _channelStore.GetOrCreate(userId);
        var startTime = DateTime.UtcNow;
        var messageCount = 0;

        _logger.LogInformation("SSE connection opened for userId {UserId}", userId);

        try
        {
            // Read from the channel asynchronously until it's completed or client disconnects
            await foreach (var message in userChannel.Channel.Reader.ReadAllAsync(cancellationToken))
            {
                // Write the SSE message to the response
                await Response.WriteAsync(message, cancellationToken);
                await Response.Body.FlushAsync(cancellationToken);

                // Only update LastActivity for non-heartbeat messages
                // Heartbeats start with ":" and should not reset the idle timer
                if (!message.StartsWith(":"))
                {
                    userChannel.UpdateLastActivity();
                    messageCount++;
                }
            }

            _logger.LogDebug("SSE channel completed for userId {UserId}", userId);
        }
        catch (OperationCanceledException)
        {
            // Normal disconnection - client closed the connection or request was cancelled
            _logger.LogDebug("SSE connection cancelled for userId {UserId}", userId);
        }
        catch (Exception ex)
        {
            // Unexpected error during SSE streaming
            _logger.LogError(ex, "Error in SSE connection for userId {UserId}", userId);
        }
        finally
        {
            // Always clean up the channel when the connection closes
            _channelStore.Remove(userId);

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "SSE connection closed for userId {UserId}, Duration={DurationSeconds}s, Messages={MessageCount}",
                userId, (int)duration.TotalSeconds, messageCount);
        }

        return new EmptyResult();
    }
}
