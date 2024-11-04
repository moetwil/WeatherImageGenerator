# Set variables
$solutionDirectory = "WeatherImageGenerator"
$prefix = "luc"
$functionAppProjects = @(
    "BlobActionFunction",
    "JobProcessorFunction",
    "GetJobFunction",
    "StartGeneratorFunction",
    "ImageProcessorFunction"
)

$outputDirectory = "./publish" # Directory where published output will go

# Publish the function apps
foreach ($project in $functionAppProjects) {
    Write-Host "Publishing function app: $project"
    
    # Navigate to the project directory and publish
    $projectPath = Join-Path $solutionDirectory $project
    dotnet publish "$projectPath/$project.csproj" -c Release -o "$outputDirectory/$project"
    
    # Check if publish was successful
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to publish $project. Exiting."
        exit $LASTEXITCODE
    }

    # Create a zip package for deployment
    Write-Host "Creating zip package for: $project"
    Compress-Archive -Path "$outputDirectory/$project/*" -DestinationPath "$outputDirectory/$project.zip"
}

# Deploy each function app
foreach ($project in $functionAppProjects) {
    $functionAppName = "$prefix$project" # Define how your Azure function app names correspond to the projects
    Write-Host "Deploying function app: $functionAppName"
    
    # Deploy using the zip package created during the publish process
    az functionapp deployment source config-zip --name $functionAppName --resource-group weather-image-generator --src "$outputDirectory/$project.zip"

    # Check if the deployment was successful
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to deploy $functionAppName. Exiting."
        exit $LASTEXITCODE
    }
}

Write-Host "All function apps published and deployed successfully!"
