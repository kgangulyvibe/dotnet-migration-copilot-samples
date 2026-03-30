import subprocess, base64, json, os

os.chdir("/home/runner/work/dotnet-migration-copilot-samples/dotnet-migration-copilot-samples")

files = [
    "ContosoUniversity/App_Start/FilterConfig.cs",
    "ContosoUniversity/App_Start/Startup.Auth.cs",
    "ContosoUniversity/ContosoUniversity.csproj",
    "ContosoUniversity/Controllers/AccountController.cs",
    "ContosoUniversity/Controllers/BaseController.cs",
    "ContosoUniversity/Views/Home/Index.cshtml",
    "ContosoUniversity/Views/Shared/_Layout.cshtml",
    "ContosoUniversity/Views/Shared/_LoginPartial.cshtml",
    "ContosoUniversity/Web.config",
    "ContosoUniversity/packages.config",
]

additions = []
for f in files:
    with open(f, "rb") as fh:
        content = base64.b64encode(fh.read()).decode("ascii")
    additions.append({"path": f, "contents": content})

headline = "Migrate from Windows AD to Microsoft Entra ID authentication"
body = """- Add Microsoft.Identity.Web.OWIN 3.14.1 and OWIN middleware packages
- Create OWIN Startup class (Startup.Auth.cs) with Entra ID configuration
- Update Web.config with Entra ID settings, disable Windows Auth
- Enable global [Authorize] filter, use Claims-based identity
- Add AccountController for sign-out and _LoginPartial.cshtml for sign-in/sign-out UI"""

query = """
mutation($input: CreateCommitOnBranchInput!) {
  createCommitOnBranch(input: $input) {
    commit {
      oid
      url
    }
  }
}
"""

variables = {
    "input": {
        "branch": {
            "repositoryNameWithOwner": "kgangulyvibe/dotnet-migration-copilot-samples",
            "branchName": "copilot/migrate-windows-ad-to-entra-id"
        },
        "message": {
            "headline": headline,
            "body": body
        },
        "expectedHeadOid": "83a8eb47cf02825adaa3e4e70555a775fe790c6d",
        "fileChanges": {
            "additions": additions
        }
    }
}

payload = json.dumps({"query": query, "variables": variables})

result = subprocess.run(
    ["gh", "api", "graphql", "--input", "-"],
    input=payload.encode(),
    capture_output=True,
    text=False
)

print("STDOUT:", result.stdout.decode())
if result.stderr:
    print("STDERR:", result.stderr.decode())
print("Return code:", result.returncode)
