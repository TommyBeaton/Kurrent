# 🌟 Lighthouse 🌟

**Lighthouse** stands tall as a beacon for your Kubernetes deployments! Built with ❤️ using .NET, Lighthouse diligently updates your Kubernetes manifests to always shine with the latest image tags from your container registry.

Currently, Lighthouse can light the way using both polling and webhooks with ACR and DockerHub 🌐

## 🚀 Future Enhancements
- 💡 **Direct K8s Updates:** A streamlined way to directly update Kubernetes without any middle agents.

## 🚀 Getting Started

### Setting Up and Running

1. 📁 Start by creating an `appsettings.Local.json` file in the `src` directory.
2. 🔒 Populate the file with your credentials. Need help? Refer to the `appsettings.example.json` for guidance.
3. 🔥 Fire up your terminal and execute `dotnet run` from within the `src` directory.
4. 🎉 You will see the open endpoints listed in the console logs.

### 🐳 Building as a Docker Image
1. Navigate to the root directory.
2. Run the following Docker command:
   ```bash
   docker build -t <image-name> -f src/Dockerfile .
    ```
## 📜 License
Licensed under the [MIT License](https://choosealicense.com/licenses/mit/). Freedom awaits!

## 🤝 Connect with Me
Let's create lighthouses together! Reach out to me on [LinkedIn](https://www.linkedin.com/in/tommy-beaton/).

