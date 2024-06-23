### Project Goals
- Play around with .net orleans by creating a reddit r/place clone.
- Use Event Sourcing and CQRS with Orleans.
- Try to optimize as much as possible to cut on server cost.
- - We are using Kubernetes her becuase .net Orleans can't be deployed to serverless.

### TODO's
- Apply the CQRS patter
- Enable multi silo?



### Deployment Scripts
Rebuild Secrets
`aspirate generate --skip-build --replace-secrets`
