# AKS Training Lab Instructions

All labs can be done within the Azure CLI (docker samples can be done on Katacoda). This is by design - to avoid issues with Docker Desktop during the workshop. If you have Docker Desktop, you can run the docker containers and K8S workloads locally, but it is not a dependency per se.  

## Lab 1: Getting started with Containers

1) [Open the Docker playground on KataKoda](https://www.katacoda.com/courses/docker/playground)
2) Run basic docker CLI commands\
   `docker run hello-world`\
   `docker images`\
   `docker run -it  ubuntu`
3) Switch to the NGINX playground. We'll quickly spin up an NGINX Webserver and access a static site from a browser. [Navigate to the docker playgroud](https://www.katacoda.com/courses/docker/create-nginx-static-web-server). Try a few of docker commands:\
`docker exec -it containerId /bin/sh`\
`docker pull debian`\
`docker rmi imageName`
   
## Lab 2: Running containers on Azure Container Instances

1) Start the Azure Cloud Shell.
2) Create a dedicated resource group for the labs in the portal or the CLI. All resources will be housed here.\
   `az group create --name aks-training --location eastus`
3) Create a Container Registry in the Azure Portal using a unique name. Alternatively, use CLI. Make a note of the ACR name. This will be used through the course\
`az acr create --resource-group aks-training --name yourACRName --sku Basic  --admin-enabled`
4) Clone the lab Git Repo the Azure Cloud Shell and navigate to the root folder of the repo.\
`git clone https://github.com/ManojG1978/aks-training.git`\
`cd aks-training/HelloWorldMVC`
5) Build the Docker Image for the Hello World ASP.NET Core MVC site and push it to the ACR\
`az acr build --registry yourACRName --image helloworld-mvc:1.0 .`
6) Create an Azure Container Instance in the portal using the image built. Select networking type as Public and give it a unique DNS label. A new Public IP is created for the container instance. Once created navigate to the URL and review the printed message (host name of the container)

## Lab 3: Getting Started with AKS

1) Create an AKS cluster in the Portal within the resource group created earlier. Choose Node Count as 1 and Node Size as *Standard_B2s*. Ensure you enable Virtual Nodes under the Node Pools tab. Also enable Enable Container Insights in the Tntegrations tab. Associate the Container Registry we created in step 2.
2) Navigate to the HelloWorld folder. This contains a simple console app.\
`cd HelloWorld`
3) Setup variables to be used in CLI commands later

```bash
AKS_NAME=aks-training-cluster
ACR_NAME=yourACRName
RG_NAME=aks-training
REGION=eastus
```

4) Push Image to your ACR Repo\
`az acr build --registry $ACR_NAME --image helloworld:1.0 .`
5) Attach the ACR to the AKS Cluster\
`az aks update -n $AKS_NAME -g $RG_NAME --attach-acr $(az acr show -n $ACR_NAME --query "id" -o tsv)`
6) Connect to AKS Cluster\
`az aks get-credentials --resource-group $RG_NAME --name $AKS_NAME`
7) Open k8s-pod.yaml using the built-in code editor of the shell. Update the image name with the ACR name in your resource group.
8) Deploy HelloWorld Pod from the manifest file\
`kubectl create -f ./k8s-pod.yaml`
9) Try some imperative commands with Kubectl like:\
`kubectl run -i --tty busybox --image=busybox -- sh`

## Lab 4: Deploying K8S Service and Load balancer

1) Navigate to the HelloWorldMVC folder of the repo.
2) Open k8s-deploy.yaml using the built-in code editor of the shell. Update the image name with the ACR name in your resource group.
3) Deploy the service (an ASP.NET Core Application)\
`kubectl create -f k8s-deploy.yaml`
4) Investigate the service and the deployment resources. Copy the Public IP of the service and open it on the browser. Note that it make take a couple of minutes to provision the public IP\
`kubectl get all`\
`kubectl get svc`

## Lab 5: Deploying a simple two tier app to AKS

1) Navigate to the voting-app-aspdotnet-core folder of the repo. This is the canonical two-tier sample from Microsoft and Docker, but built using ASP.NET Core.
2) Build the Docker Image for the Voting and push it to the ACR\
`az acr build --registry yourACRName --image vote-app:1.0 .`
3) Open k8s-deploy-aks.yaml using the built-in code editor of the shell. Update the image name with the ACR name in your resource group.
4) Deploy the application (front end ASP.NET Core, backend Redis)\
`kubectl create -f k8s-deploy-aks.yaml`
5) Investigate the service and the deployment resources. Copy the Public IP of the service and open it on the browser with port 5000. Note that it make take a couple of minutes to provision the public IP\
`kubectl get all`\
`kubectl get svc`

## Lab 6: Implementing Health and liveness checks

1) Navigate to the voting-app-aspdotnet-core folder of the repo.
2) Open k8s-deploy-healthchecks.yaml using the built-in code editor of the shell. Update the image name with the ACR name in your resource group.
3) If you already have another deployment of the voting app from the previous exercise, delete it\
`kubectl delete -f k8s-deploy-aks.yaml`
4) Deploy the version of voting application, which includes health checks included\
`kubectl create -f k8s-deploy-healthchecks.yaml`
5) Investigate the service and the deployment resources. Copy the Public IP of the service and open it on the browser with port 5000. Append /hc to the URL to see the health status. Append /liveness to the URL to check the liveness of the application\
`kubectl get all`\
`kubectl get svc`

## Lab 7: K8S Resource Requests and Limits

1) Navigate to the HelloWorld folder of the repo.
2) Deploy a pod which stresses memory.\
`kubectl create -f k8s-pod-limits.yaml`
3) Investigate the Pod lifecycle and check that the POD is terminated due to OOM limits.\
`kubectl get pod memory-stress`\
`kubectl describe pod memory-stress`

## Lab 8: K8S Jobs and Cronjob

1) Navigate to the *HelloWorld* folder of the repo.
2) Create a Job from *k8s-job.yaml*. This is a simple job that sleeps for 25 seconds. 3 such executions complete the overall job\
`kubectl create -f ./k8s-job.yaml`
3) Investigate the jobs, notice parallelism. Play around with commands and notice how the *backoffLimit* takes effect.\
`kubectl get jobs --watch`
4) Open *k8s-cronjob.yaml* using the built-in code editor of the shell. Update the image name with the ACR name in your resource group.
5) Create the Cronjob from the manifest\
`kubectl create -f ./k8s-cronjob.yaml`
6) Investigate the jobs, they run approximately every minute\
`kubectl get cronjob`\
`kubectl get jobs --watch`

## Lab 9: Implementing Persistent Volumes

1) Setup variables to hold values of your resource names

```bash
AKS_SHARE_NAME=redis
AKS_NAME=aks-training-cluster
ACR_NAME=yourACRName
RG_NAME=aks-training
REGION=eastus
AKS_STORAGE_ACCOUNT_NAME=yourStorageAccount
```

2) Create a storage account, which will house the volumes used by the pods\
`az storage account create -n $AKS_STORAGE_ACCOUNT_NAME -g $RG_NAME -l $REGION --sku Standard_LRS`\
`export AZURE_STORAGE_CONNECTION_STRING=$(az storage account show-connection-string -n $AKS_STORAGE_ACCOUNT_NAME -g $RG_NAME -o tsv)`
3) Create a file share to house the redis data files used by the voting app\
`az storage share create -n $AKS_SHARE_NAME --connection-string $AZURE_STORAGE_CONNECTION_STRING`
4) Extract the Storage account key and create a secret. This secret will be referenced by the pod volume\
`STORAGE_KEY=$(az storage account keys list --resource-group $RG_NAME --account-name $AKS_STORAGE_ACCOUNT_NAME --query "[0].value" -o tsv)`\
`kubectl create secret generic storage-key --from-literal=azurestorageaccountname=$AKS_STORAGE_ACCOUNT_NAME --from-literal=azurestorageaccountkey=$STORAGE_KEY`
5) Deploy the version of the voting app which has volumes attached to the redis pods. Make sure you update the image corresponding to your ACR. This manifest file creates the underlying Persistent Volume and Persistent Volume Claims (Delete any previous deployment of the voting app if they are already running)\
`kubectl create -f ./k8s-deploy-aks-pv.yaml`\
`kubectl delete -f ./k8s-deploy-aks-pv.yaml` # Delete old deployments
6) Investigate the K8s objects. Check the redis pod and ensure the volume is mounted.\
`kubectl get pvc`\
`kubectl describe pod yourredispodName`
7) Try adding some votes in the app. Delete the redis pod now. K8s will recreate the pod and mount the volume again. However, when you access the application, the previous data would show up. Without Persistent volumes, this data would have been lost permanently.\
`kubectl delete pod yourredispodName`

[For more information, check the AKS Doc sample](https://docs.microsoft.com/en-us/azure/aks/azure-files-volume)

## Lab 10: Leveraging Secrets

1) In this lab, we would set up a secret natively in K8s and access that within a pod. No Key Vault involved here. First create a generic secret containing two values - *user* and *password*\
`kubectl create secret generic k8ssecret --from-literal=user=u123 --from-literal=password=p123`
2) Navigate to Secrets folder in the repo and create the Pod from the manifest file. In this scenario, secrets are injected into the pod as environment variables and the pod basically echoes the values on the console.\
`kubectl create -f ./k8s-secrets-env.yaml`
3) Check the pod logs to see the un-encrypted values printed to the console
`kubectl logs secret-test-pod`

## Lab 11: Integrating with Azure Key Vault

### Part a) Using Service Principal

1) In this lab, we'll use secrets stored in the Azure KeyVault (as is usually the case with production apps). The first step is to install the Secrets Store CSI Driver to the AKS Cluster\
`helm repo add csi-secrets-store-provider-azure https://raw.githubusercontent.com/Azure/secrets-store-csi-driver-provider-azure/master/charts`\
`helm install csi-secrets-store-provider-azure/csi-secrets-store-provider-azure --generate-name`
2) Create a new Service Principal and note the client id and secret values\
`az ad sp create-for-rbac --name aksServicePrincipal --skip-assignment`
3) Setup a Key Vault in the portal, ensure you assign Get and List permissions to the newly created Service Principal.
4) Create a new Secret with name *connectionString*. Add a secret value.
5) Create a K8S secret to house the credentials of Service Principal\
`kubectl create secret generic secrets-store-creds --from-literal clientid=yourClientID --from-literal clientsecret=yourClientSecret`
6) Create the *ServiceProviderClass* object. Open the *k8s-secretprovider.yaml* in the Code editor (Secrets folder) and update  *userAssignedIdentityID* with your Service Principal ID, *keyvaultName* with the name of the vault you created, *subscriptionId* with your subscription ID, and *tenantId* with your tenant ID (run *az account list* for getting tenant ID and subscription ID)\
`kubectl create -f k8s-secretprovider.yaml`
7) Create a pod that mounts secrets from the key vault.\
`kubectl create -f k8s-nginx-secrets.yaml`
8) To display all the secrets that are contained in the pod, run the following command\
`kubectl exec -it nginx-secrets-store-inline -- ls /mnt/secrets-store/`
9) To display the contents of the *connectionString* secret, run the following command:\
`kubectl exec -it nginx-secrets-store-inline -- cat /mnt/secrets-store/connectionString`

### Part b) Using Managed Identities

1) In this lab, we leverage managed identities in lieu of Service Principal used in part a). This is a better approach as you don't need to store any client id and secret as a K8s secret. It is assumed you have created the key vault and Secret Store CSI driver earlier.
2) Create a user defined managed identity called *aks-training-identity*. Copy the clientId and principalId for later use.\
`az identity create -g aks-training -n aks-training-identity`
3) Assign the Reader role to the Azure AD identity that you created in the preceding step for your key vault, and then grant the identity permissions to get secrets from your key vault. Use the clientId and principalId from the previous step. Verify the assignments in the Azure portal (Access Control (IAM) Blade) in your Key Vault

```
az role assignment create --role "Reader" --assignee $principalId --scope /subscriptions/<<Your Subscription ID>>/resourceGroups/<<Your Resource Group>>/providers/Microsoft.KeyVault/vaults/<<Your Key vault name>

az keyvault set-policy -n <<your key vault name>> --secret-permissions get --spn $clientId

```

4) Assign the "*Managed Identity Operator*" and "*Virtual Machine Contributor*" roles to the managed identity of the Kubernetes cluster under Subscriptions-> Access Control (IAM). The managed identity will have a name like *aks-training-cluster-agentpool*, where *aks-training-cluster* is the name of the AKS cluster
5) Install the Azure Active Directory (Azure AD) identity into AKS. This creates the *aad-pod-identity* object in your cluster.\
`helm repo add aad-pod-identity https://raw.githubusercontent.com/Azure/aad-pod-identity/master/charts`\
`helm install pod-identity aad-pod-identity/aad-pod-identity`
6) Create the *ServiceProviderClass* object mapped to the managed identity. Open the *k8s-secretprovider-mi.yaml* in the Code editor (Secrets folder) and update  *userAssignedIdentityID* with your the client ID of the identity, *keyvaultName* with the name of the vault you created, *subscriptionId* with your subscription ID, and *tenantId* with your tenant ID (run *az account list* for getting tenant ID and subscription ID). Delete the ServiceProvider object if it already exists and recreate\
`kubectl create -f k8s-secretprovider-mi.yaml`
7) Create the Azure Identity and Binding objects. Open the *k8s-podIdentity.yaml* and update the *clientId* and *resourceId* fields with values in your subscription respectively.\
`kubectl create -f k8s-podIdentity.yaml`
8) Create a pod that mounts secrets from the key vault. Delete the pod if it already exists and then recreate\
`kubectl create -f k8s-nginx-secrets-mi.yaml`
9) To display all the secrets that are contained in the pod, run the following command\
`kubectl exec -it nginx-secrets-store-inline -- ls /mnt/secrets-store/`
10) To display the contents of the *connectionString* secret, run the following command:\
`kubectl exec -it nginx-secrets-store-inline -- cat /mnt/secrets-store/connectionString`

[For More information, refer to the Docs sample](https://docs.microsoft.com/en-us/azure/key-vault/general/key-vault-integrate-kubernetes)

## Lab 12: Implementing RBAC in AKS

1) Navigate to the RBAC folder. Create a certificate for user called *bob* using (OpenSSL)

```bash
openssl genrsa -out bob.key 2048
openssl req -new -key bob.key -out bob.csr -subj "/CN=bob/O=dev"\n
cat bob.csr | base64 | tr -d '\n'
```

2) Open *k8s-csr.yaml* and update the request field with the output from the previous command (base64 encoded key). Submit the  Certificate Signing Request to K8s\
`kubectl create -f k8s-csr.yaml`
3) Verify the request is still pending\
`kubectl get csr`
4) As cluster admin, approve the request\
`kubectl certificate approve bob`
5) Get the signed certificate from the cluster as a *.crt* file\
`kubectl get csr bob -o jsonpath='{.status.certificate}' | base64 --decode > bob.crt`
6) The file, bob.crt is the client certificate that’s used to authenticate Bob. With the combination of the private key (bob.key) and the approved certificate (bob.crt) from Kubernetes, you can get authenticated with the cluster. Add Bob to K8s\
`kubectl config set-credentials bob --client-certificate=bob.crt --client-key=bob.key --embed-certs=true`
7) Set the context for Bob\
`kubectl config set-context bob --cluster=aks-training-cluster --user=bob`
8) Create a new K8s namespace called *dev*\
`kubectl create namespace dev`
9) Check if Bob can list pods on the dev namespace\
`kubectl auth can-i list pods --namespace dev --as bob`
9) Create a K8S role called dev, which has full access to the dev namespace\
`kubectl create -f .\k8s-role-dev.yaml`
10) Create the role binding for the dev group\
`kubectl create -f .\k8s-rolebinding-dev.yaml`
11) Switch to Bob's context\
`kubectl config use-context bob`
12) Check if Bob can list pods on the dev namespace now\
`kubectl auth can-i list pods --namespace dev`
13) Try this exercise with another user (say Dave) assigned to the *sre* namespace.

## Lab 13: Autoscaling Pods and AKS Cluster

1) Auto-scale deployment using the Horizontal Pod Autoscaler (HPA) using a CPU metric threshold  
`kubectl autoscale deployment vote-app-deployment --cpu-percent=30 --min=3 --max=6`
2) Manually scale nodes in the cluster using this command:\
`az aks scale --resource-group aks-training --name aks-training-cluster --node-count 2`
3) Setup an autoscaler using the command below (Min 1, Max 2). Alternatively, you could do this in the portal for each of your node pools:

```
az aks update \
  --resource-group aks-training \
  --name aks-training-cluster  \
  --enable-cluster-autoscaler \
  --min-count 1 \
  --max-count 2	
```

[For more information, refer docs](https://docs.microsoft.com/en-us/azure/aks/cluster-autoscaler)

## Lab 14: Monitoring and Alerts with Container Insights/Azure Monitor

1) When we setup the cluster earlier in the lab, we had enabled Container Insights. If not, you can go to the Insights blade on the portal for your AKS cluster and enable it. It might take 5-10 minutes for the data to show up.
2) Review the Cluster, Nodes, Controllers, Containers and Deployment tabs respectively.  
3) For select pods (like Stress), select View Container logs from the right pane
4) Play with filters and try reviews log records. You can create your own queries like below:

```
ContainerInventory
| where  Image contains "stress"
```

5) To get familiar with alerts, you can select the *Recommended Alerts (Preview)* link from the menu. For this exercise, enable the alert for *OOM Killed Containers* and create an action group to email yourself. Try recreating the memory-stress pod from Lab 7 and verify you get an email alert.

## Lab 15: Deploying Ingress

1) Create a new namespace for ingress resources\
`kubectl create namespace ingress-basic`
2) Add the official stable repository for Helm\
`helm repo add stable https://kubernetes-charts.storage.googleapis.com/`
3) Use Helm to deploy an NGINX ingress controller

```
helm install nginx-ingress stable/nginx-ingress \
    --namespace ingress-basic \
    --set controller.replicaCount=2 \
    --set controller.nodeSelector."beta\.kubernetes\.io/os"=linux \
    --set defaultBackend.nodeSelector."beta\.kubernetes\.io/os"=linux
```

4) Review the services created for Ingress\
`kubectl get service -l app=nginx-ingress --namespace ingress-basic`
5) Navigate to the Ingress folder. Create two applications which will serve requests coming through Ingress\
`kubectl apply -f k8s-ingress-sample.yaml --namespace ingress-basic`
6) Create the Ingress Routes\
`kubectl apply -f k8s-ingress-route.yaml --namespace ingress-basic`
7) Test the ingress controller by access the public IP created from step 3. Test by adding */hello-world-one* and */hello-world-two* to the URL and see the requested routed to the respective backend deployments

[For more information, refer to the Docs sample](https://docs.microsoft.com/en-us/azure/aks/ingress-basic)

## Lab 16: Deploying Virtual Node ACI - Serverless Kubernetes

1) In Lab 2, if you created a cluster without enabling virtual nodes, create a new AKS cluster with Virtual Nodes enabled using the portal.
2) Switch to cluster-admin credentials for this cluster\
`az aks get-credentials -a --resource-group aks-training --name aks-aci`
3) Examine the node pools and check a node by name *virtual-node-aci-linux*\
`kubectl get nodes`
4) Review the node and observe the taint\
`kubectl describe node virtual-node-aci-linux`
5) Navigate to the ACI folder and create a sample deployment by running *k8s-virtual-node.yaml*, which gets scheduled on the virtual node (observe the toleration setting on the pod which matches the taint)\
`kubectl create -f k8s-virtual-node.yaml`
6) Inspect the pod and make a note of the Private IP address:\
`kubectl get pods -o wide`
7) Execute a test pod and access the site execute on the ACI Pod\
`kubectl run -it --rm virtual-node-test --image=debian`
8) On this pod, install *curl* utility\
`apt-get update && apt-get install -y curl`
9) Access curl -L http://10.241.0.4 (replace with the private IP from 5) from the pod and verify the HTML returned\
`curl -L http://10.241.0.4`

[For more information, refer to Doc sample](https://docs.microsoft.com/en-us/azure/aks/virtual-nodes-portal)

## Lab 17: Managing Deployment Rollout Strategy

1) Navigate to the HelloWorldMVC folder. Notice the *k8s-deploy-rollout.yaml* has a section called *strategy* in the deployment manifest. This is set to *RollingUpdate*
2) Deploy version 1.0 of the application to start with. Ensure you have updated the image name\
`kubectl create -f k8s-deploy-rollout.yaml`
3) Now, make a simple change on the page to say Version 2(Views/Home/Index.chtml), rebuild the 2.0 image and push it ACR\
`az acr build --registry yourACRName --image helloworld-mvc:2.0 .` 
4) Update the existing deployment and update it with version 2.0 of the image\
`kubectl set image deployment helloworld-mvc-deployment helloworld-mvc=yourACRName.azurecr.io/helloworld-mvc:2.0 --record`
5) Review rollout status and ensure the upgrade is successful by accessing the site\
`kubectl rollout status deployment helloworld-mvc-deployment`
6) Rollback the deployment to the previous version\
`kubectl rollout undo deployment helloworld-mvc-deployment --record`
7) Review deployment history\
`kubectl rollout history deployment helloworld-mvc-deployment`

## Lab 18: Event Driven Autoscaling with KEDA

1) Install KEDA into your cluster in a namespace called *keda*

```
helm repo add kedacore https://kedacore.github.io/charts
helm repo update
kubectl create namespace keda
helm install keda kedacore/keda --namespace keda
```

2) Creating a new Azure Service Bus namespace & queue\
`az servicebus namespace create --name yourNamespace --resource-group aks-training --sku basic`
3) Create a queue called *orders*.\
`az servicebus queue create --namespace-name yourNamespace --name orders --resource-group aks-training`
4) Create a new authorization rule with Management permissions which KEDA requires\
`az servicebus queue authorization-rule create --resource-group aks-training --namespace-name yourNamespace --queue-name orders --name order-consumer --rights Manage Send Listen`
5) Get the connection string to connect to the Service Bus queue\
`az servicebus queue authorization-rule keys list --resource-group aks-training --namespace-name yourNamespace --queue-name orders --name order-consumer`
6) Create a secret with the connection string obtained from step 5\
`kubectl create secret generic order-secrets --from-literal=SERVICEBUS_QUEUE_CONNECTIONSTRING=yourConnectionString`
7) Navigate to the KEDA folder. Deploy your Order processor\
`kubectl create -f deploy-queue-processor.yaml`
8) Navigate to the Order Generator folder and run the console app. Make sure you update the connection string from step 5. Create a large number of orders, say 1000\
`dotnet run`
9) Review the pods and deployment and see the scale out happening. The number of pods should go up to 20 and then get back to 0 when all order messages are processed. Also, review the metrics on the service bus namespace\
`kubectl get deploy order-processor`\
`kubectl get pods -l app=order-processor -w`

Note: This lab is based on the sample provided by [TomKherkov](https://github.com/tomkerkhove/sample-dotnet-worker-servicebus-queue)

## Lab 19: eShopOnContainers Microservice Application deployment on AKS (optional)

1) In your Azure shell, run the following command to deploy the eShoponContainers reference implementation for Microservices on Kubernetes. This deploys all resources into a resource group called eshop-learn-rg. \
`. <(wget -q -O - https://aka.ms/microservices-aspnet-core-setup)`
2) Once the command executes note the endpoint URLs and access the sites
2) For more information, checkout the [MS Learn module on Microservices](https://docs.microsoft.com/en-gb/learn/modules/microservices-aspnet-core/2-deploy-application)