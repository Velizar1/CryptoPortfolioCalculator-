pipeline {
    agent {
        docker {
            image 'dotnet-docker-agent:8.0'
            args '--user root -v /var/run/docker.sock:/var/run/docker.sock'
            reuseNode true
        }
    }

    stages {
        stage('Checkout') {
            steps {
                git branch: 'main',
                    url: 'https://github.com/Velizar1/CryptoPortfolioCalculator-.git'
            }
        }

        stage('Restore') {
            steps {
                sh 'dotnet restore'
            }
        }

        stage('Build') {
            steps {
                sh 'dotnet build --configuration Release'
            }
        }

        stage('Test') {
            steps {
                sh 'dotnet test --no-build --configuration Release'
            }
        }

        stage('Publish') {
            steps {
                sh 'dotnet publish -c Release -o out'
            }
        }

        stage('Docker Build & Deploy') {
            steps {
                script {
                    sh 'docker build -t myapp:latest .'
                    sh 'docker stop myapp || true'
                    sh 'docker rm myapp || true'
                    sh 'docker run -d --name myapp -p 5000:80 myapp:latest'
                }
            }
        }
    }
}
