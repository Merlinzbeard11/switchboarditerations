using System.Threading;
using System.Threading.Tasks;

namespace EquifaxEnrichmentAPI.Api.Services;

/// <summary>
/// Interface for FCRA-compliant audit logging service.
/// Enables testability through dependency inversion (SOLID principle).
/// </summary>
public interface IAuditLoggingService
{
    /// <summary>
    /// Enqueue audit log entry for async processing (fire-and-forget, &lt; 1ms overhead).
    /// </summary>
    /// <param name="entry">Audit log entry to persist</param>
    /// <param name="cancellationToken">Cancellation token for graceful shutdown</param>
    /// <returns>ValueTask for fire-and-forget pattern</returns>
    ValueTask LogRequestAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
