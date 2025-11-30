# Contributing to DatingApp

Thank you for your interest in contributing! This guide explains how to work with this repository.

## 🔒 Repository Access

**Important:** This repository is read-only for contributors. You cannot push directly to this repository.

### How to Work with This Repository

1. **Fork the Repository** (Recommended)
   - Click the "Fork" button on GitHub
   - This creates your own copy at `your-username/DattingApp-Angular-DotNet`
   - You have full control over your fork

2. **Clone Your Fork**
   ```bash
   git clone https://github.com/your-username/DattingApp-Angular-DotNet.git
   cd DattingApp-Angular-DotNet
   ```

3. **Make Changes**
   - Create a branch: `git checkout -b feature/your-feature`
   - Make your changes
   - Commit: `git commit -m "Add your feature"`

4. **Push to Your Fork**
   ```bash
   git push origin feature/your-feature
   ```
   - This pushes to **your fork**, not the original repository

5. **Create a Pull Request** (if you want to contribute back)
   - Go to the original repository on GitHub
   - Click "New Pull Request"
   - Select your fork and branch
   - The repository owner will review and merge if approved

## 🐳 Docker Hub Configuration (For Forks)

If you fork this repository and want to use the GitHub Actions workflow:

1. **Update the workflow** (`.github/workflows/docker-push.yml`):
   - Change `neokyuubi/datingapp:latest` to `your-dockerhub-username/your-image-name:latest`
   - Update the repository check: `github.repository == 'your-username/DattingApp-Angular-DotNet'`

2. **Add your secrets** in your fork's GitHub Settings:
   - `DOCKERHUB_USERNAME` - Your Docker Hub username
   - `DOCKERHUB_TOKEN` - Your Docker Hub access token
   - `RENDER_DEPLOY_HOOK` - Your Render deploy hook (optional)

## 🧪 Testing Locally

You can test the application locally without any special permissions:

1. Follow the [Local Development Guide](../README.md#-local-development-with-docker-recommended) in the README
2. Use your own environment variables (create `.env` from `.env.example`)
3. Run with Docker Compose: `docker-compose -f docker-compose.local.yml up`

## 📝 Code of Conduct

- Be respectful and constructive
- Follow the existing code style
- Test your changes before submitting
- Keep commits clean and descriptive

## ❓ Questions?

If you have questions, please open an issue in the repository.

