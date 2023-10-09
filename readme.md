# 🌟 Lighthouse 🌟

Lighthouse auto-updates your Kubernetes manifests with the latest image tags from your container registry. Developed in .NET, it supports polling and webhooks for ACR and DockerHub.

## ⏰ Upcoming Features

- **Direct K8s Updates**: Directly update Kubernetes.
- **Notifications**: Receive alerts when your manifests are updated.

## 🚏 Getting Started with Lighthouse

### How Lighthouse Works
Lighthouse is a Kubernetes service that monitors your container registry for new image tags.
When a new tag is detected, Lighthouse will update your Kubernetes manifests to use the latest tag.
This ensures that your Kubernetes deployments are always pointing to the latest image tags.
It currently supports polling and webhooks for ACR and DockerHub. All of this can be setup in a few simple steps.

### Configuring Lighthouse
To get Lighthouse working you will need to pass it a configuration file. You can find an [example ConfigMap here](https://github.com/TommyBeaton/Lighthouse/blob/main/kustomize/example/config.yaml).
#### 📝 Config Overview

Here is the `appsettings.k8s.json` configuration explained:

- **Pollers**: Configurations for periodic checks.
    - `EventName`: Unique poller name.
    - `Type`: Registry type (`acr` for Azure Container Registry, `docker` for DockerHub).
    - `IntervalInSeconds`: Polling frequency.
    - `Url`: Container registry URL.
    - `Images`: Images to monitor.
    - `Username` & `Password`: Registry credentials.

- **Webhooks**: Set up to listen for registry push events.
    - `EventName`: Unique webhook name.
    - `Path`: Endpoint path.
    - `Type`: Registry sending the webhook.

- **Subscriptions**: Determines how updates are processed.
    - `Name`: Subscription name.
    - `EventName`: Link to a Poller or Webhook.
    - `RepositoryName` & `Branch`: Kubernetes manifest location.

- **Repositories**: Storage details for Kubernetes manifests.
    - `Name`: Repository name.
    - `Url`: Git URL.
    - `Username` & `Password`: Repo access credentials.
    - `FileExtensions`: Target file types, e.g., `.yaml`.


### Annotations for Auto-Updates

To auto-update images in your Kubernetes manifests:
1. Add the Lighthouse comment to images you want to auto-update in your Kubernetes manifest.
2. When a new image is received by Lighthouse, it will access the subscribed repos and update manifests with the newest tag.
```yaml
image: <IMAGE>:<TAG> # lighthouse update; regex: <REGEX>;
```

#### Example
The below example will auto-update the image `foo-api:1.0.1-development` when a new tag matching the regex is detected.

```yaml
image: foo.azurecr.io/foo-api:1.0.1-development # lighthouse update; regex: .*dev*.;
```

### 🚀 Deploying Lighthouse To Kubernetes

1. **Configuration**:
    - Create a `ConfigMap` with a `appsettings.k8s.json` key and your config as JSON. [Example ConfigMap](https://github.com/TommyBeaton/Lighthouse/blob/main/kustomize/example/config.yaml).

2. **Ingress Setup**:
    - Deploy an ingress if you're using webhooks. [Example Ingress](https://github.com/TommyBeaton/Lighthouse/blob/main/kustomize/example/ingress.yaml).

3. **Kustomization**:
    - Create a `kustomization.yaml` for Lighthouse. Be sure to reference the base and your ConfigMap  [Example Kustomization](https://github.com/TommyBeaton/Lighthouse/blob/main/kustomize/example/kustomization.yaml).
    ```yaml
    resources:
      - https://github.com/TommyBeaton/Lighthouse/kustomize/base
      - config.yaml
    ```

4. **Apply Kustomization**:
    ```bash
    kubectl apply -k <path/to/kustomization/file>
    ```

5. **Check Lighthouse**:
- Access via `http://lighthouse.yourdomain.com/health` or use port-forward:
    ```bash
    kubectl port-forward svc/lighthouse-service 8080:80 
    ```

## 💻 Local Setup

1. Create `appsettings.Local.json` in `src`.
2. Use `appsettings.example.json` as a reference. [Link.](https://github.com/TommyBeaton/Lighthouse/blob/main/src/appsettings.example.json)
3. Execute `dotnet run --environment Local` in `src`.

### 🐳 Docker Build

```bash
docker build -t <image-name> -f src/Dockerfile .
```

## 📜 License

[MIT License](https://choosealicense.com/licenses/mit/).

## 🤝 Connect

Join me on [LinkedIn](https://www.linkedin.com/in/tommy-beaton/).
