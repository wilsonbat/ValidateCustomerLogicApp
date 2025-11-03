# Customer Validation Logic App Solution

This solution demonstrates an Azure Logic App integrated with Azure Functions to validate customer email and phone number inputs. The solution consists of three main components:

1. **ValidationLibrary**: A .NET library containing email and phone validation logic
2. **ValidateCustomerFunction**: Azure Functions that expose validation endpoints
3. **CustLogicApp**: Azure Logic App that orchestrates the validation workflow

![Logic App Workflow](./docs/images/workflow-overview.png)

## Prerequisites

### Windows Setup

1. **Visual Studio Code**
   - Download and install from [VS Code](https://code.visualstudio.com/)
   - Required Extensions:
     - Azure Tools (`ms-vscode.vscode-node-azure-pack`)
     - Azure Functions (`ms-azuretools.vscode-azurefunctions`)
     - Azure Logic Apps (Standard) (`ms-azuretools.vscode-azurelogicapps`)
     - C# (`ms-dotnettools.csharp`)

2. **.NET SDK**
   - Download and install [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

3. **Azure Functions Core Tools**
   ```powershell
   winget install Microsoft.AzureFunctionsCoreTools
   # Or using npm
   npm install -g azure-functions-core-tools@4
   ```

4. **Azurite (Azure Storage Emulator)**
   ```powershell
   npm install -g azurite
   ```

### macOS Setup

1. **Visual Studio Code**
   - Download and install from [VS Code](https://code.visualstudio.com/)
   - Install same extensions as Windows setup

2. **.NET SDK**
   ```bash
   brew install dotnet@8
   ```

3. **Azure Functions Core Tools**
   ```bash
   brew tap azure/functions
   brew install azure-functions-core-tools@4
   ```

4. **Azurite**
   ```bash
   npm install -g azurite
   ```

## Building and Running the Solution

### 1. Start Azurite (Local Storage Emulator)
Open a terminal and run:
```bash
azurite --silent --location . --debug debug.log &
```

You should see output similar to:
```
Azurite Blob service is starting at http://127.0.0.1:10000
Azurite Queue service is starting at http://127.0.0.1:10001
Azurite Table service is starting at http://127.0.0.1:10002
```
![Azurite setup & start](./docs/images/azurite-setup-and-start.png)

### 2. Build ValidationLibrary
```powershell
cd ValidationLibrary
dotnet build
```

### 3. Build and Start ValidateCustomerFunction
```powershell
cd ../ValidateCustomerFunction
dotnet build
func start --port 7072
```

You should see output indicating the functions are running:
```
ValidateEmail: [POST] http://localhost:7072/api/ValidateEmail
ValidatePhone: [POST] http://localhost:7072/api/ValidatePhone
```
![Azure function start](./docs/images/azure-functions-start.png)

### 4. Deploy Logic App Workflow
1. Open the `CustLogicApp` folder in VS Code
2. Navigate to the `workflow.json` file
3. The Logic App workflow will automatically start in the local development environment
4. Once in the Logic App workspace Run the app as .Net function to start the app as shown in following figure

![Logic App Local Run](./docs/images/logicapp-local-run.png)

## Testing the Solution

### Retrieving URL for LogicApp w/ signature

1. Go to your Logic App workspace and access the workflow.json file
2. Run debug mode for the logic app
3. Right click and go to overview to retrieve the url as shown in following figure

![Retieve Logic App URL](./docs/images/retrieve-logicapp-url-test.png)

### Using Postman

1. Create a new POST request to the Logic App endpoint (shown in VS Code when you run the workflow)
2. Set the request body to:
```json
{
    "customerId": "456789",
    "email": "baby.doe@example.com",
    "phoneNumber": "913-456-7890"
}
```
3. Send the request and observe the validation results
4. Sample postman collections are provided in repo folder correspondingly

### Using curl

- change the URL per local configuration
```bash
curl -X POST http://localhost:7071/api/CustLogicAppWorkflow/triggers/When_an_HTTP_request_is_received/invoke?api-version=2022-05-01&sp=%2Ftriggers%2FWhen_an_HTTP_request_is_received%2Frun&sv=1.0&sig=f8yYuX5W8VGOFtv2yTcRJFPrTerDMuz482tiukecc8E \
     -H "Content-Type: application/json" \
     -d "{\"customerId\": \"12345\",\"email\": \"john.doe@example.com\",\"phoneNumber\": \"123-456-7890\"}
```

## Running unit tests

This repository contains two test projects. Run them from PowerShell or your preferred terminal.

- ValidationLibrary unit tests

   ```powershell
   cd ValidationLibrary.Tests
   dotnet test --verbosity normal
   ```

- ValidateCustomerFunction unit tests (function logic)

   ```powershell
   cd ValidateCustomerFunction.Tests
   dotnet test --verbosity normal
   ```

You can also run both test projects from the solution root:

```powershell
dotnet test ./ValidationLibrary.Tests --verbosity normal
dotnet test ./ValidateCustomerFunction.Tests --verbosity normal
# or run all tests in the solution
dotnet test --verbosity normal
```

What to expect
- Tests build the projects and execute the test suite. Typical output ends with:

   Test run for ...
   Test Run Successful.
   Total tests: N. Passed: N. Failed: 0. Skipped: 0.

Quick troubleshooting
- If tests fail to build, run `dotnet restore` in the solution root first.
- If you hit package or version conflicts, ensure your SDK is .NET 8 and packages are up-to-date (see project files).
- Some tests mock function worker types — ensure no Functions host is actively locking files in the project output while running tests.


## Validation Rules

### Email Validation
- Must be a valid email format (e.g., user@domain.com)
- Cannot be empty or null
- Uses standard email format validation

### Phone Validation
- Supports US phone number format
- Optional +1 country code
- Supports formats: XXX-XXX-XXXX, (XXX) XXX-XXXX, etc.
- Cannot be empty or null

## Architecture Overview

The solution follows this flow:
1. HTTP request triggers Logic App workflow
2. Logic App parses input JSON
3. Calls ValidateEmail and ValidatePhone functions in parallel
4. Aggregates results and returns response

## Assumptions

The following assumptions describe input formats, design and coding choices, runtime expectations, and testing/deployment constraints for this project.

- Inputs & API contract
   - Functions accept JSON POST bodies matching `ValidationRequest` (examples below). When present, body values take precedence over route/query parameters for backward compatibility.
   - Requests MUST use `Content-Type: application/json` and UTF-8 encoding.
   - Expected request shapes:
      - Email: `{ "Email": "user@example.com" }`
      - Phone: `{ "PhoneNumber": "123-456-7890" }`

- Response contract
   - Successful responses return HTTP 200 with a JSON payload matching `ValidationResults` ({ "IsValid": bool, "Message": string }).
   - The project sets `Content-Type: application/json; charset=utf-8` for responses.

- Design & coding choices
   - Targets .NET 8 using the Azure Functions isolated worker model (`dotnet-isolated`).
   - Uses System.Text.Json for serialization and writes responses with async APIs to avoid synchronous IO issues.
   - Unit tests avoid mocking extension methods like `WriteAsJsonAsync`; they capture response bodies via `MemoryStream` instead.
   - On deserialization failure, the function falls back to route/query values instead of returning 4xx, to preserve compatibility with callers that don't send JSON bodies.

- Testing assumptions
   - Unit tests use xUnit and Moq. No Azure services are required for unit tests (Azurite is not required).
   - Ensure any running Functions host is stopped before running tests to avoid locked files in `bin/`.

- Local development & deployment
   - Local host is typically started with `func start --port <port>` and expects `host.json` and the `.csproj` at the project root (`ValidateCustomerFunction`).
   - Build changes require rebuilding (`dotnet build`/`dotnet publish`) and restarting the Functions host to pick up new DLLs.
   - Publish output is placed under `ValidateCustomerFunction\bin\Release\net8.0\publish` and can be deployed via VS Code or `az functionapp deployment source config-zip`.

- Security & observability
   - Functions use Function-level authorization by default; keys or managed identities are required for secure invocation.
   - Application Insights is enabled (configured in `host.json`) for monitoring and telemetry.

These assumptions are intended to make calling, testing and deploying the functions predictable. If you need stricter validation behavior (HTTP 4xx for malformed input) or different serialization settings, update the function code and tests accordingly.

## Troubleshooting

### Common Issues

1. **Azurite Connection Issues**
   - Ensure no other process is using the default ports (10000, 10001, 10002)
   - Check if Azurite is running with `ps aux | grep azurite`

2. **Function App Won't Start**
   - Verify port 7072 is not in use
   - Check if .NET SDK is properly installed
   - Ensure all dependencies are restored

3. **Logic App Issues**
   - Verify workflow.json syntax
   - Check VS Code Azure Logic Apps extension is properly installed
   - Ensure function app endpoints are accessible

### Logs Location
- Function App logs: Check the terminal running the function app
- Azurite logs: Check debug.log in the Azurite directory
- Logic App logs: Check VS Code's OUTPUT panel under "Logic App"

## Additional Resources

- [Azure Functions documentation](https://docs.microsoft.com/azure/azure-functions/)
- [Azure Logic Apps documentation](https://docs.microsoft.com/azure/logic-apps/)
- [Azurite documentation](https://docs.microsoft.com/azure/storage/common/storage-use-azurite)
