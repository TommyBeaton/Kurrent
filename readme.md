# 🌟 Lighthouse 🌟

**Lighthouse** stands tall as a beacon for your Kubernetes deployments! Built with ❤️ using .NET, Lighthouse diligently updates your Kubernetes manifests to always shine with the latest image tags from your container registry.

Currently, Lighthouse can light the way using both polling and webhooks with ACR and DockerHub 🌐

## ⏰ Future Enhancements
- 💡 **Direct K8s Updates:** A streamlined way to directly update Kubernetes without any middle agents.

## 🚀 Deploying Lighthouse on Kubernetes

### 📝 App Settings Explained

Before deploying, it's crucial to understand the `appsettings.json` configuration:

- **Pollers**: This array contains configurations for periodic checks on specified container registries.
   - `EventName`: A unique name for the poller.
   - `Type`: The type of the registry, e.g., `acr` for Azure Container Registry, `docker` for DockerHub.
   - `IntervalInSeconds`: Frequency of polling in seconds.
   - `Url`: URL of the container registry (needed for `acr` type).
   - `Images`: List of image names you want to track.
   - `Username` & `Password`: Credentials for accessing the registry (make sure to store these securely).

- **Webhooks**: An array that contains configurations for webhooks that listen for push events from registries.
   - `EventName`: A unique name for the webhook.
   - `Path`: The endpoint path.
   - `Type`: Type of the registry sending the webhook.

- **Subscriptions**: Configurations for how updates (from either Pollers or Webhooks) should be handled.
   - `Name`: Name of the subscription.
   - `EventName`: Links the subscription to a Poller or Webhook.
   - `RepositoryName` & `Branch`: Specifies where the Kubernetes manifests are located.

- **Repositories**: Where your Kubernetes manifests are stored.
   - `Name`: Name of the repository.
   - `Url`: Git URL of the repository.
   - `Username` & `Password`: Credentials for accessing the repository.
   - `FileExtensions`: File types to look for in the repository, e.g., `.yaml`.

### 🛠️ Deployment Steps

1. **Prepare Configuration**:
   - Create a `ConfigMap` from your `appsettings.json`. Please remember, sensitive information should be stored using Kubernetes Secrets. [Here](https://github.com/TommyBeaton/Lighthouse/blob/main/kustomize/example/config.yaml) is an example `ConfigMap`.

2. **Set up Ingress**:
   - If you have an ingress controller set up, deploy the [sample ingress](https://github.com/TommyBeaton/Lighthouse/blob/main/kustomize/example/ingress.yaml) to route traffic to your Lighthouse service.

3. **Deploy Lighthouse**:
   - Start by fetching the example `kustomization.yaml` from the [Lighthouse repo](https://github.com/TommyBeaton/Lighthouse/blob/main/kustomize/example/kustomization.yaml)
   
4. **Verify Deployment**:
   - Once deployed, check that the Lighthouse pods are running:
   ```bash 
   kubectl get pods -l app=lighthouse
   ```
   Ensure you see the status as `Running` for your Lighthouse pods.
5. **Apply the Kustomization**:
   - Deploy Lighthouse using your modified kustomization:
   ```bash
    kubectl apply -k <path/to/kustomization/file>
   ```
6. **Check Lighthouse Status**:
   - If you set up an ingress, call to `http://lighthouse.yourdomain.com/health` If you didn't set up an ingress, you can port-forward to your service:
   ```bash
   kubectl port-forward svc/lighthouse-service 8080:80 
   ```
   Then access Lighthouse via `http://localhost:8080`.

## 💻 Setting Up and Running Locally

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

