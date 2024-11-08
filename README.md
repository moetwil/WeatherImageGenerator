# WeatherImageGenerator

## Usage

To deploy the Weather Image Generator solution, you can run the PowerShell deployment script with the required parameters for resource group, location, and prefix.

### Parameters

- `resourceGroup`: The name of the Azure resource group where all resources will be deployed.
- `location`: The Azure region for deploying resources (e.g., `westeurope`).
- `prefix`: A unique prefix for naming your resources to avoid naming conflicts.

### Example Usage

To execute the script, open a terminal, navigate to the directory containing the script, and use the following command:

```powershell
.\deploy.ps1 -resourceGroup "MyResourceGroup" -location "westeurope" -prefix "weatherapp"
```

### Testing the Azure Functions

You can test the functionality of the Azure Functions (StartGeneratorFunction and GetJobFunction) using the weatherapp.http file. This file allows you to:
• Start Image Generation Job: Initiates the job by sending a POST request to the StartGeneratorFunction, and returns a unique jobId to track the process.
• Get Job Status: Retrieves the status of the job by sending a GET request to the GetJobFunction, using the jobId from the previous request.

## Azure functions

### StartGeneratorFunction

Creates a job by adding a message to the jobsqueue, initiating the image generation process for weather data. It also inserts a new entry in the jobstatus table in Table Storage, which records the job’s ID, status, and creation time. This function returns a jobId to track the job’s progress.

### JobProcessorFunction

Triggered by messages on the jobsqueue, this function processes each job by retrieving the relevant weather data from the Buienrader API. It enqueues tasks in imagequeue to initiate image generation for each weather station in the dataset, ensuring that each station’s data will be processed separately and in parallel by the ImageProcessorFunction.

### ImageProcessorFunction

Listens to imagequeue and generates images based on weather data for individual weather stations. The function uses a base image from a public API (e.g., Unsplash), overlays the weather information, and then saves the completed image to Blob Storage. Once the image is saved, it checks wether the job is finished if all images are generated, and updates the table.

### GetJobFunction

Retrieves the status of a specified job from the jobstatus table in Table Storage. It accepts a jobId as a parameter and returns details such as the job’s current status, and links to any generated images if the job has completed.

```

```
