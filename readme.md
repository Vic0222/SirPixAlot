### Deployment Scripts
Download artifact
`curl -o "deployment.zip" -H "Authorization: token $SOME_TOKEN_WITHOUT_PERMISSIONS" https://github.com/Vic0222/SirPixAlot/actions/runs/9430089341/artifacts/1581966037`

unzip and deploy to podman
`unzip deployment.zip -d deployment  && cd deployment && kustomize build . | podman play kube -`
