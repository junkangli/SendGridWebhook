# SendGrid Inbound Email Parse Webhook AWS Serverless Application Project

This .NET Core project implements a SendGrid webhook for parsing emails using AWS Serverless Application Model.

## Deploy Serverless

To install the tools used to deploy from the command line, use the **dotnet tool install** command.
```
dotnet tool install -g Amazon.Lambda.Tools
```

To update to the latest version of the tools, use the **dotnet tool update** command.
```
dotnet tool update -g Amazon.Lambda.Tools
```

Update the values in **aws-lambda-tools-defaults.json** file. In particular, the **profile** and **s3-bucket** parameters.

To deploy the .NET Core Serverless application, run the below command:
```
cd "InboundParseServerless"
dotnet lambda deploy-serverless
```
