# Job Orchestrator

This solution is intended as a starting point for a job orchestrator, and to demonstrate the relationship of the various components. It is not intended to be a complete solution, and is not production ready.

## Prerequisites

You will need to have docker installed on your machine. You can download it from [here](https://www.docker.com/products/docker-desktop).

You will need to build the solution, including DHI.Workflow.Engine.Example.

## Running the solution

Two Startup projects should be set in the solution properties. These are: Docker-Compose and DHI.Workflow.Host.Example

Docker-Compose will start the following containers:
 - postgres
 - adminer
 - seq

DHI.JobOrchestrator.Example and DHI.Workflow.Host.Example will start as a console application.

DHI.JobOrchestrator.Example will start a web server on port 5001.

The containers will bind to the following ports:
 - postgres: 32432
 - adminer: 32401
 - seq: 32404
	
Should these already be in use on your machine, you can change them in the docker-compose.yml file, and in the associated references in appsettings.

## Running the workflow

The solution contains a simple workflow in the test.json and test folders. This workflow can be run by calling the following endpoint:

[https://localhost:5001/job/queue/MyFirstCodeWorkflow](https://localhost:5001/job/queue/DHI.Workflow.Engine.Example.MyFirstCodeWorkflow)

The job execution can be seen in the Host console window, and in seq.

[https://localhost:5001/job/cancel](https://localhost:5001/job/cancel)

## Viewing the logs

[http://localhost:32404/#/events](http://localhost:32404/#/events)


## Viewing the database

[http://localhost:32401/?pgsql=db%3A10032&username=postgres&db=postgres&ns=public&password=Solutions!](http://localhost:32401/?pgsql=db%3A10032&username=postgres&db=postgres&ns=public)

## Limitations

The demonstration is limited to a static release, there is no ReleaseFolderManagger implementation. 
The demonstration uses some spoof endpoints on the JobOrchestrator to fill in for an authentication service and a job API service.
