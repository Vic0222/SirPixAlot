### Deployment Scripts
Download artifact
`curl -o "deployment.zip" "https://<redirect_url>"`

unzip and deploy to podman
`unzip deployment.zip -d deployment  && cd deployment && kustomize build . | podman play kube -`
