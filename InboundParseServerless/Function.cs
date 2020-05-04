using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using StrongGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InboundParseServerless
{
    public class Functions
    {
        public APIGatewayHttpApiV2ProxyResponse InboundParse(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            APIGatewayHttpApiV2ProxyResponse response;

            try
            {
                context.Logger.LogLine("Request Headers:");
                foreach (var header in request.Headers)
                {
                    context.Logger.LogLine($"{header.Key}: {header.Value}");
                }
                context.Logger.LogLine($"Request IsBase64Encoded:\n{request.IsBase64Encoded}");
                context.Logger.LogLine($"Request Body:\n{request.Body}");

                var requestBody = new MemoryStream(Convert.FromBase64String(request.Body));

                var parser = new WebhookParser();
                var inboundEmail = parser.ParseInboundEmailWebhook(requestBody);
                context.Logger.LogLine($"InboundEmail Subject:\n{inboundEmail.Subject}");

                response = new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = "Success",
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }
            catch (Exception ex)
            {
                var exception = ex.ToString();
                context.Logger.LogLine($"Exception occurred:\n{exception}");

                response = new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Body = ex.Message,
                    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
                };
            }

            return response;
        }
    }
}
