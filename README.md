# DatingApp - Angular and .NET7 Web Application

Welcome to DatingApp, a modern web application built with Angular14 for the frontend and .NET7 for the API backend. This application aims to connect people looking for meaningful relationships in a user-friendly and secure environment.

## 🚀 Quick Start

### Prerequisites

- [Docker](https://www.docker.com/) and Docker Desktop
- [.NET7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0) (for local development without Docker)
- [Node.js](https://nodejs.org/) v18+ (for local development without Docker)
- [Angular CLI](https://angular.io/cli) v14+ (for local development without Docker)

## 📦 Local Development with Docker (Recommended)

### Step 1: Set Up Environment Variables

1. Copy the example environment file:
   ```bash
   cp .env.example .env
   ```

2. Edit `.env` and fill in your values:
   ```env
   DATABASE_URL=postgresql://datingapp_user:datingapp_password@postgres:5432/datingapp_db?sslmode=disable
   TokenKey=your-secret-token-key-here
   CLOUDINARY_CLOUD_NAME=your-cloudinary-cloud-name
   CLOUDINARY_API_KEY=your-cloudinary-api-key
   CLOUDINARY_API_SECRET=your-cloudinary-api-secret
   ASPNETCORE_ENVIRONMENT=Production
   ASPNETCORE_URLS=http://+:8080
   ```

### Step 2: Start the Application

```bash
docker-compose -f docker-compose.local.yml up --build
```

The application will be available at: **http://localhost:8080**

This will:
- Start a local PostgreSQL database
- Build and run the Angular frontend + .NET API
- Automatically run database migrations
- Seed initial data

### Step 3: Stop the Application

```bash
docker-compose -f docker-compose.local.yml down
```

## 🖥️ Local Development (Without Docker)

### Step 1: Set Up Environment Variables

Set environment variables in your terminal or use a `.env` loader:

**Windows (PowerShell):**
```powershell
$env:DATABASE_URL="postgresql://user:password@localhost:5432/database"
$env:TokenKey="your-token-key"
$env:CLOUDINARY_CLOUD_NAME="your-cloud-name"
$env:CLOUDINARY_API_KEY="your-api-key"
$env:CLOUDINARY_API_SECRET="your-api-secret"
```

**Linux/Mac:**
```bash
export DATABASE_URL="postgresql://user:password@localhost:5432/database"
export TokenKey="your-token-key"
export CLOUDINARY_CLOUD_NAME="your-cloud-name"
export CLOUDINARY_API_KEY="your-api-key"
export CLOUDINARY_API_SECRET="your-api-secret"
```

### Step 2: Start the Backend

```bash
cd API
dotnet run
```

API will run on: `https://localhost:7192` or `http://localhost:5252`

### Step 3: Start the Frontend

```bash
cd client
npm install
npm start
```

Frontend will run on: `https://localhost:4200`

## ☁️ Deployment to Render

### Step 1: Create Services in Render

1. **PostgreSQL Database:**
   - Create a new PostgreSQL database
   - Copy the **Internal Database URL** (you'll need this)

2. **Web Service:**
   - Create a new Web Service
   - Connect your GitHub repository
   - Set the following:
     - **Build Command:** (leave empty, uses Dockerfile)
     - **Start Command:** (leave empty, uses Dockerfile)
     - **Dockerfile Path:** `Dockerfile` (or `API/Dockerfile` if building from API directory)

### Step 2: Configure Environment Variables in Render

In your Web Service settings, add these environment variables:

| Variable | Value | Description |
|----------|-------|-------------|
| `DATABASE_URL` | `postgresql://...` | Your Render database Internal URL |
| `TokenKey` | `your-secret-key` | JWT signing key (generate a secure random string) |
| `CLOUDINARY_CLOUD_NAME` | `your-cloud-name` | From Cloudinary dashboard |
| `CLOUDINARY_API_KEY` | `your-api-key` | From Cloudinary dashboard |
| `CLOUDINARY_API_SECRET` | `your-api-secret` | From Cloudinary dashboard |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Environment mode |

### Step 3: Deploy

1. Push to your `main` branch
2. Render will automatically build and deploy
3. Or manually trigger a deploy from the Render dashboard

### Step 4: Configure CORS (if needed)

The code automatically allows your Render URL. If you need to add more origins, update `API/Program.cs`:

```csharp
var allowedOrigins = new[] { 
    "https://your-app.onrender.com",
    "http://localhost:4200" 
};
```

## 🔄 Automatic Deployment

This project includes GitHub Actions that:
- Builds Docker image on push to `main`/`master`
- Pushes to Docker Hub (uses your own Docker Hub account)
- Triggers Render deployment (if `RENDER_DEPLOY_HOOK` secret is configured)

### Setup GitHub Secrets

1. Go to your GitHub repository → Settings → Secrets and variables → Actions
2. Add these **required** secrets:
   - `DOCKERHUB_USERNAME` - Your Docker Hub username
   - `DOCKERHUB_TOKEN` - Your Docker Hub access token
3. Add these **optional** secrets:
   - `DOCKERHUB_IMAGE_NAME` - Custom Docker image name (e.g., `yourusername/your-image`). If not set, defaults to `yourusername/datingapp`
   - `RENDER_DEPLOY_HOOK` - Your Render deploy hook URL (only if you want auto-deploy to Render)

### For Forks

If you fork this repository:
- The workflow will automatically use **your** Docker Hub account (from your secrets)
- The Docker image will be pushed to **your** Docker Hub: `yourusername/datingapp:latest` (or custom name if set)
- You need to add your own `DOCKERHUB_USERNAME` and `DOCKERHUB_TOKEN` secrets in your fork's settings
- Each fork uses its own Docker Hub account - no conflicts!

## 🔒 Security Notes

- ⚠️ **Never commit `.env` file to git!** (It's already in `.gitignore`)
- ✅ The `.env.example` file is safe to commit (it's a template)
- 🔑 Rotate your secrets if they were ever exposed in git history
- 🔐 **Repository Protection:** Only the owner can push to this repository. Forks cannot push to the original repo.

## 📝 Environment Variables Reference

All configuration is done through environment variables:

- `DATABASE_URL` - PostgreSQL connection string (format: `postgresql://user:pass@host:port/db`)
- `TokenKey` - JWT token signing key
- `CLOUDINARY_CLOUD_NAME` - Cloudinary cloud name
- `CLOUDINARY_API_KEY` - Cloudinary API key  
- `CLOUDINARY_API_SECRET` - Cloudinary API secret
- `ASPNETCORE_ENVIRONMENT` - `Development` or `Production`
- `ASPNETCORE_URLS` - URLs to listen on (default: `http://+:8080`)

## 🛠️ Troubleshooting

### Database Connection Issues
- Check that `DATABASE_URL` is correctly formatted
- For local Docker: ensure database container is running
- For Render: use the **Internal Database URL** (not external)

### SSL Connection Errors
- Local databases automatically disable SSL
- For Render databases, SSL is required (handled automatically)

### CORS Errors
- Check that your frontend URL is in the allowed origins list
- For local: `http://localhost:4200` is allowed
- For production: Your Render URL is automatically allowed

### Demonstration

![Demo 1](./Demos/Desktop%208-2-2023%205-12-35%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-20-32%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-14-02%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-26-06%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-09-49%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-23-10%20PM.gif)
![Demo 1](./Demos/Desktop%208-2-2023%205-19-06%20PM.gif)

