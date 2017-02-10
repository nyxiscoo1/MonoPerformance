using Owin;

namespace Service.Middleware.RequestLogger
{
    public static class RequestLoggerMiddlewareExtensions
    {
        public static IAppBuilder UseRequestLogger(this IAppBuilder appBuilder)
        {
            return appBuilder.Use(new RequestLoggerMiddleware().Invoke);
        }
    }
}