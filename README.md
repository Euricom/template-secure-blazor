# Blazor Starter App

A modern ASP.NET Core Blazor Server application with OpenID Connect (OIDC) authentication integration.

## Overview

This Blazor Starter App provides a foundation for building web applications using Blazor Server with the following features:

- OpenID Connect authentication
- Token management (access tokens, refresh tokens)
- Authorization and user management
- Modern UI components

## Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 or JetBrains Rider (recommended)
- An OIDC-compatible Identity Provider (e.g., Auth0, Duende IdentityServer, Microsoft Entra ID)

## Getting Started

### Environment Variables

The following environment variables need to be set up for proper configuration:

#### Required Authentication Variables
- `Authentication__OpenIdConnect__Authority`: URL of your OIDC identity provider (e.g., `https://your-auth-server.com`)
- `Authentication__OpenIdConnect__ClientId`: Client ID registered with your identity provider
- `Authentication__OpenIdConnect__ClientSecret`: Client secret for secure communication with your identity provider

#### Optional Environment Variables
- `ASPNETCORE_ENVIRONMENT`: Set to `Development` for development mode with detailed error information, or `Production` for deployed applications
- `ASPNETCORE_URLS`: Configure which URLs and ports the application listens on (e.g., `https://localhost:5001;http://localhost:5000`)

You can set these environment variables in several ways:
- Using the operating system's environment variables
- Through your IDE's launch configuration
- Using a `.env` file (requires additional setup)
- In Docker using environment variables in your docker-compose file or Dockerfile

### Configuration

1. Clone the repository
2. Configure the authentication settings in `appsettings.json`:
