# Jenkins + GitHub CI for .NET (Playwright) — Jenkinsfile and Setup

This document provides a ready-to-use Declarative Jenkinsfile for a .NET repository that runs tests (including Playwright based tests) on every GitHub push/PR, plus concise setup steps for Jenkins and GitHub integration.

Prerequisites
- Jenkins server installed and reachable from the internet (or reachable by GitHub webhooks). If behind firewall, you can use a reverse proxy or GitHub App + webhook tunnel.
- Jenkins plugins: Git, Pipeline, GitHub Branch Source, GitHub, Credentials Binding (install via Manage Plugins).
- A build agent capable of running .NET tests. Recommended: use Docker agent with a dotnet SDK image that contains or can install Playwright browsers.
- A GitHub repository containing your .NET solution and tests and a Jenkinsfile at the repo root (we will use this approach).

Recommended approach
- Use a Multibranch Pipeline job in Jenkins so branches and PRs are automatically discovered and jobs are triggered by GitHub webhooks (via GitHub App integration).
- Put a Jenkinsfile at repo root (versioned with code). This makes CI reproducible and easy to maintain.

Sample Jenkinsfile (Declarative) — Linux Docker agent
```groovy
pipeline {
  agent {
	docker {
	  image 'mcr.microsoft.com/dotnet/sdk:7.0'
	  args '--cap-add=SYS_ADMIN --shm-size=1g' // adjust if Playwright needs shared memory
	}
  }
  environment {
	DOTNET_CLI_TELEMETRY_OPTOUT = '1'
  }
  stages {
	stage('Checkout') {
	  steps {
		checkout scm
	  }
	}
	stage('Restore') {
	  steps {
		sh 'dotnet restore'
	  }
	}
	stage('Build') {
	  steps {
		sh 'dotnet build --no-restore -c Release'
	  }
	}
	stage('Install Playwright Browsers (if needed)') {
	  steps {
		// If using Playwright for .NET, ensure browsers are installed prior to running tests.
		// Adjust command as needed for your test setup.
		sh 'dotnet tool restore || true'
		sh 'npx playwright install --with-deps || true'
	  }
	}
	stage('Test') {
	  steps {
		sh 'dotnet test --no-build -c Release --logger:trx'
	  }
	  post {
		always {
		  junit '**/TestResults/*.trx'
		}
	  }
	}
  }
  post {
	always {
	  echo 'Pipeline finished'
	}
  }
}
```
Notes:
- The Docker image and commands assume a Linux agent. If your Jenkins runs on Windows nodes, convert `sh` to `bat` and choose a Windows dotnet image.
- Playwright: the `npx playwright install --with-deps` command is for Node-based Playwright. For Playwright for .NET, ensure the browsers are installed per the official docs (you can call the Microsoft.Playwright CLI or include a startup step in your tests that calls Playwright.InstallAsync()).

Jenkins setup steps (high level)
1. Install required plugins: Git, Pipeline, GitHub Branch Source, GitHub, Credentials Binding.
2. Create a Jenkins credential for GitHub access: prefer installing the Jenkins GitHub App or create a Personal Access Token with repo:webhook permissions and add it under Jenkins credentials.
3. Create a new Multibranch Pipeline job in Jenkins. Configure the GitHub repository URL and credentials.
4. Configure webhooks: When using GitHub App integration, install the App on your org/repo. If using PAT/webhook, add a webhook in repo settings pointing to `https://<jenkins-host>/github-webhook/` and select push and PR events.
5. Push the Jenkinsfile to the repo root. Jenkins will detect branches and run pipelines on push/PR.

Security and agents
- Prefer GitHub App over PAT for more secure webhook management.
- Use dedicated build agents (Docker or ephemeral) for tests that require browsers.
- If tests need browsers with GUI, consider running headless or using xvfb in the container.

What I will deliver
- A ready-to-download ZIP containing:
  - Jenkinsfile (Declarative) at repo root
  - README with the setup steps above and any adjustments for Windows agents
  - Optional Jenkins job configuration snippet (if you want job XML)

Next steps
- I will create the ZIP package now. Tell me if you need a Windows-friendly Jenkinsfile instead of Docker/linux, or if I should include specific Playwright install commands for Playwright for .NET.
