Jenkins + GitHub CI for .NET (Playwright) — Windows Multibranch Pipeline

Overview
This guide configures a Jenkins Multibranch Pipeline that triggers on GitHub push/PR events using the GitHub App, runs your .NET (Playwright) tests on Windows build agents, and reports results.

Prerequisites
- Jenkins installed and reachable by GitHub webhooks (public URL or via tunneling).
- Jenkins plugins: Git, Pipeline, GitHub Branch Source, GitHub App, Credentials Binding.
- Windows build agent(s) registered with label `windows && dotnet`, with .NET SDK installed and Playwright prerequisites.
- A GitHub App installed on your org or repo and connected to Jenkins.
- Repository containing your .NET solution and tests.

Steps
1. Install GitHub App and connect to Jenkins
   - In Jenkins: Manage Jenkins -> Configure Global Security -> GitHub -> Add GitHub Server -> Connect with GitHub App.
   - Follow Jenkins documentation to install the GitHub App and configure credentials. Use the GitHub Branch Source plugin to supply the App.

2. Ensure Windows build agents
   - Register one or more Windows agents with label `windows && dotnet`.
   - Verify `dotnet --info` works on those agents and test-run a simple `dotnet test`.

3. Add the Jenkinsfile to repository
   - I will commit a Jenkinsfile to your repo at the root. The provided file is `jenkins-setup/Jenkinsfile.windows.ps1`.
   - The Jenkinsfile assumes an agent with label `windows && dotnet`.

4. Create Multibranch Pipeline job
   - New Item -> Multibranch Pipeline -> Enter job name
   - Under Branch Sources add your GitHub repository using the GitHub App credential
   - Configure behavior (discover branches and PRs)
   - Save; Jenkins will scan the repository and create jobs per branch/PR.

5. Verify webhooks and trigger
   - The GitHub App handles webhooks; ensure Jenkins receives them (Manage Jenkins -> System Log -> GitHub App logs).
   - Push a commit to repo; job should be discovered and run automatically.

Notes on Playwright for .NET
- Playwright for .NET requires browser binaries. In Windows pipeline you can add a step to call Playwright.InstallAsync() from a small helper or use the dotnet tool if configured.
- If tests require interactive UI, run headless or ensure the agent has necessary display support.

I will commit the Jenkinsfile now if you provide a repo URL and grant push access via a temporary write token or add me as a collaborator. Alternatively, I can provide the Jenkinsfile content for you to commit.
