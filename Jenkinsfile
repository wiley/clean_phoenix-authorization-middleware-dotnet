#!/usr/bin/env groovy

def call(Map args) {

  def credentialsId = 'babee6c1-14fe-4d90-9da0-ffa7068c69af'
  
  def ckLib = library(
    identifier: 'ck_ip_jenkins_library@v3.4.1',
    retriever: modernSCM([
      $class: 'GitSCMSource',
      remote: 'git@github.com:wiley/ck_ip_jenkins_library.git',
      credentialsId: credentialsId
    ])
  )

  def gitUtil = ckLib.ck.Git.new(this)
  def mailUtil = ckLib.ck.Mail.new(this)

  def String workerLabel = params.targetEnv == 'prod' ? 'docker-ckbase-ubuntu-prod-eks' : 'eks-nonprod-us'
  def String regionName = params.targetEnv != 'prod' ? 'us-east-1' : 'eu-central-1'
  def String accountId =  params.targetEnv != 'prod' ? '889859566884' : '988207228673'
  def List environments = ['qa', 'prod']
  def String artUrl = "https://crossknowledge-${accountId}.d.codeartifact.${regionName}.amazonaws.com/nuget/phoenix/v3/index.json"

  pipeline {
    agent {
      node {
        label workerLabel
      }
    }

    parameters {
       gitParameter branchFilter: '.*',
            description: 'Release to deploy',
            selectedValue: 'TOP',
            quickFilterEnabled: true,
            sortMode: 'DESCENDING_SMART',
            name: 'TAG',
            type: 'PT_TAG'

        choice (
            name: 'targetEnv',
            choices: environments,
            description: 'Where it should be deployed to? (Default: none - No deploy)'
        )
    }

    stages {
      stage ('Checkout') {
            steps {
              script {
                echo 'Cleaning workspace'
                cleanWs()
                echo "Checking out from Tag ${TAG}"
                gitUtil.branchCheckout('', 'babee6c1-14fe-4d90-9da0-ffa7068c69af', args.repoUrl, "refs/tags/${TAG}")
              }
            }
        }
      stage('Build and Publish') {
        steps {
          script {
            
            sh """
              export ART_PASS=`aws codeartifact get-authorization-token --domain crossknowledge --domain-owner ${accountId} --region ${regionName} --query authorizationToken --output text`
              docker build -t darwin-authorization-middleware:${TAG} --no-cache --build-arg VERSION=${TAG}  --build-arg ART_USER=aws --build-arg ART_URL=${artUrl} --build-arg ART_PASS=\$ART_PASS  .
            """
          }
        }
      }
    }
    post {
      always {
        script {
          gitUtil.updateGithubCommitStatus("${currentBuild.currentResult}", "${env.WORKSPACE}")
          mailUtil.sendConditionalEmail()
          wrap([$class: 'BuildUser']) {
              def String displayName = "#${currentBuild.number}_${TAG}_${BUILD_USER}"
              currentBuild.displayName = displayName
            }
        }
      }
    }
  }
}

def pipelineParams = [
  repoUrl: 'git@github.com:wiley/darwin-authorization-middleware.git'
  ]

call(pipelineParams)
