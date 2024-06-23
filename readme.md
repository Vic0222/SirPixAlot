### Project Goals
- Play around with .net orleans by creating a reddit r/place clone.
- Use Event Sourcing and CQRS with Orleans.
- Try to optimize as much as possible to cut on server cost.
  - We are using Kubernetes her becuase .net Orleans can't be deployed to serverless.

### TODO's
- Apply the CQRS patter
  - This would optimize read operations
- Enable multi silo.
  - Orleans essential feature. This would allow the app to scale.



### Deployment Scripts
Rebuild Secrets
`aspirate generate --skip-build --replace-secrets`
