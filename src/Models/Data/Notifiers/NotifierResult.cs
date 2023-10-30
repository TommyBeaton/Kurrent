using System.Net;

namespace Kurrent.Models.Data.Notifiers;

public class NotifierResult
{
    public HttpStatusCode StatusCode { get; set; }
    public string? Error { get; set; }
    public bool IsSuccess { get; set; }
}