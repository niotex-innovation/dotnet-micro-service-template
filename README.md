# dotnet-micro-service-template

```bash
dotnet ef dbcontext scaffold \
--project NxSuite.SignUp.csproj \
--startup-project NxSuite.SignUp.csproj \
--configuration Debug \
--no-build \
"Name=NxDb" \
Microsoft.EntityFrameworkCore.SqlServer \
--context NxDbContext \
--context-dir Database \
--force \
--output-dir Database \
--table Invitations
```bash
