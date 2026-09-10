pipeline {
  agent { label 'windows && dotnet' }
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
		powershell 'dotnet restore'
	  }
	}
	stage('Build') {
	  steps {
		powershell 'dotnet build --no-restore -c Release'
	  }
	}
	stage('Install Playwright Browsers (for Playwright for .NET)') {
	  steps {
		// If using Playwright for .NET, ensure browsers are installed before tests.
		// This runs the Playwright install via dotnet CLI if you included the CLI tool.
		powershell 'dotnet tool restore -v minimal'
		powershell 'pwsh -Command "dotnet tool run playwright install" || echo "playwright install step may vary for your setup"'
	  }
	}
	stage('Test') {
	  steps {
		powershell 'dotnet test --no-build -c Release --logger:trx'
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
