namespace SmartTaskManagement.Application.Common;

/// <summary>
/// Categorizes an expected Application failure so the API can map it onto the
/// correct HTTP status code without the Application layer knowing about HTTP.
/// Mapping contract (applied at the API boundary):
/// <list type="bullet">
///   <item><description><see cref="Validation"/> → HTTP 400</description></item>
///   <item><description><see cref="NotFound"/> → HTTP 404</description></item>
///   <item><description><see cref="Forbidden"/> → HTTP 403</description></item>
/// </list>
/// </summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Forbidden
}
