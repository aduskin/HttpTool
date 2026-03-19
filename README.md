 
# HttpTool

A modern, feature-rich HTTP API testing tool built with WPF and .NET 10.

![image](https://github.com/Hero3821/HttpTool/blob/master/ScreenShot/cover.png)

## Features

### Core HTTP
- Support for GET, POST, PUT, DELETE, PATCH, HEAD, OPTIONS methods
- Request headers, query parameters, and body editing
- Multiple body types: None, JSON, Form-URL-Encoded, Multipart Form, Raw Text, Binary (file upload)
- Request timeout configuration
- Response viewer with status code, headers, and body

### Authentication
- None
- Basic Auth (username/password)
- Bearer Token
- API Key (header or query parameter)
- OAuth 2.0 (Client Credentials / Authorization Code)

### Project Management
- Create and manage multiple projects
- Organize API requests within projects
- Save and load projects from disk (.httptool format)
- Import/Export support:
  - Postman Collection v2.1
  - HAR (HTTP Archive)
  - Insomnia Export
  - Swagger/OpenAPI 3.0

### Environment Variables
- Define reusable variables with `{{variable}}` syntax
- Variable substitution in URLs, headers, and body

### Tools
- Cookie Manager
- Proxy Settings
- SSL/TLS Settings
- WebSocket / SSE client
- Request History

### Code Generation
Generate ready-to-use code snippets in 8 languages:
- C# (HttpClient)
- Python (requests)
- JavaScript (fetch)
- Java (HttpURLConnection)
- Go (net/http)
- PHP (cURL)
- Ruby (Net::HTTP)
- cURL

## Requirements

- Windows 10 / 11
- .NET 10.0 Runtime

## Build

```bash
# Clone the repository
git clone https://github.com/aduskin/HttpTool.git
cd HttpTool

# Build the solution
dotnet build HttpTool.slnx

# Run the desktop application
dotnet run --project HttpTool.Desktop
```

## Usage

1. **Create a project** — use `Ctrl+N` or File > New Project
2. **Add a request** — use Edit > New Request
3. **Configure the request** — set URL, method, headers, body, auth
4. **Send** — click the Send button or press `Enter` in the URL bar
5. **View the response** — status code, headers, and body are shown in the right panel

## Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| Ctrl+N | New Project |
| Ctrl+O | Open Project |
| Ctrl+S | Save Project |
| Alt+F4 | Exit |

## Project Structure

```
HttpTool/
├── HttpTool.Core/               # Domain models, interfaces, enums
│   ├── Enums/
│   ├── Interfaces/
│   └── Models/
├── HttpTool.Infrastructure/     # Service implementations
│   └── Services/
├── HttpTool.Desktop/            # WPF UI application
│   ├── Converters/
│   ├── Themes/
│   ├── ViewModels/
│   └── Views/
└── ThirdParty/                  # AduSkin UI library
```

## Tech Stack

- **UI**: WPF (.NET 10), [AduSkin](https://github.com/aduskin/AduSkin)
- **MVVM**: CommunityToolkit.Mvvm
- **DI**: Microsoft.Extensions.Hosting
- **HTTP**: System.Net.Http.HttpClient
- **WebSocket**: System.Net.WebSockets
- **Serialization**: System.Text.Json

## Contributors

<a href="https://github.com/aduskin" target="_blank"><img width="64px" src="https://avatars2.githubusercontent.com/u/33409777?s=460&u=536aecd59ce72fa64b09d2279821227bc6a721da&v=4"></a>
<a href="https://github.com/Haku-Men" target="_blank"><img width="64px" src="https://avatars2.githubusercontent.com/u/13210002?s=460&u=ae17e9b33173d1e2af00bccfc76c6ce540b0cdbf&v=4"></a>

欢迎热心网友们共同来维护这款软件
