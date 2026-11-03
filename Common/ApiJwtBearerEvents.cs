using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace LibraryApi.Common;

// 401 and 403 are produced by the middleware before any controller runs, so they get the envelope here.
public class ApiJwtBearerEvents : JwtBearerEvents
{
    private const string UnauthorizedMessage = "Autentikasi diperlukan atau token tidak valid.";
    private const string ForbiddenMessage = "Anda tidak memiliki akses ke sumber daya ini.";

    public override Task Challenge(JwtBearerChallengeContext context)
    {
        // Suppress the default empty 401, but keep the WWW-Authenticate header that clients rely on.
        context.HandleResponse();
        context.Response.Headers.WWWAuthenticate = context.AuthenticateFailure is null
            ? JwtBearerDefaults.AuthenticationScheme
            : $"{JwtBearerDefaults.AuthenticationScheme} error=\"invalid_token\"";

        return WriteEnvelopeAsync(
            context.HttpContext,
            StatusCodes.Status401Unauthorized,
            ServiceResult.Fail(ErrorType.Unauthorized, UnauthorizedMessage));
    }

    // ErrorType is not serialized and has no Forbidden member, so the 403 body only differs by its message.
    public override Task Forbidden(ForbiddenContext context)
    {
        return WriteEnvelopeAsync(
            context.HttpContext,
            StatusCodes.Status403Forbidden,
            ServiceResult.Fail(ErrorType.Unauthorized, ForbiddenMessage));
    }

    private static Task WriteEnvelopeAsync(HttpContext httpContext, int statusCode, ServiceResult result)
    {
        httpContext.Response.StatusCode = statusCode;
        return httpContext.Response.WriteAsJsonAsync(result);
    }
}
