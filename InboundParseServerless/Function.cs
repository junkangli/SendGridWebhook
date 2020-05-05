using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.SimpleSystemsManagement;
using Amazon.SimpleSystemsManagement.Model;
using StrongGrid;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace InboundParseServerless
{
    public class Function
    {
        public Function()
        {
            _ssmClient = new AmazonSimpleSystemsManagementClient();
        }

        private readonly AmazonSimpleSystemsManagementClient _ssmClient;

        private const string AuthenticationKeyParameterName = "/inboundparse/authkey";
        private const string AuthenticationQueryParameterKey = "authkey";

        public async Task<APIGatewayHttpApiV2ProxyResponse> InboundParse(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            APIGatewayHttpApiV2ProxyResponse response;

            var getParameterResponse = await _ssmClient.GetParameterAsync(new GetParameterRequest 
            { 
                Name = AuthenticationKeyParameterName,
                WithDecryption = true
            });
            var authenticationKey = getParameterResponse?.Parameter?.Value;

            string authenticationKeyPresented = null;
            request.QueryStringParameters?.TryGetValue(AuthenticationQueryParameterKey, out authenticationKeyPresented);

            if (string.IsNullOrEmpty(authenticationKeyPresented) || string.IsNullOrEmpty(authenticationKey)
                || authenticationKeyPresented != authenticationKey)
            {
                context.Logger.LogLine($"Failed to authenticate.");

                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
            }

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
