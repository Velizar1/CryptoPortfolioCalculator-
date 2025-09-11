pipeline {
    agent {
        docker {
            image 'dotnet-build-agent:8.0'
            args '--user root -v /var/run/docker.sock:/var/run/docker.sock --entrypoint=""'
        }
    }
    stages {
        stage('Check User') {
            steps {
                sh 'whoami'
                sh 'id'
                sh 'docker ps'
            }
        }

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
        stage('AWS') {
        
            agent{
                docker{
                    image 'amazon/aws-cli'
                    args "--entrypoint=''"
                   
                }
            }
            steps{
                 withCredentials([usernamePassword(credentialsId: 'my-aws', passwordVariable: 'AWS_SECRET_ACCESS_KEY', usernameVariable: 'AWS_ACCESS_KEY_ID')]) {
                    sh '''
                        aws --version
                        aws s3 ls
                    '''
                 }
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