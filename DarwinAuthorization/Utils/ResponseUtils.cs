using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarwinAuthorization.Utils
{
    public static class ResponseUtils
    {
        public static async void ConfigureResponse(HttpContext context, int statusCode, string message)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var responseBody = new
            {
                message
            };

            var responseBodyString = JsonConvert.SerializeObject(responseBody);
            await context.Response.WriteAsync(responseBodyString);
            return;
        }
    }
}
