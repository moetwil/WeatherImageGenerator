# Define variables
$projectName = "StartGeneratorFunction"
$solutionName = "WeatherImageGenerator"
$resourceGroupName = "weather-image-generator"
$functionAppName = "lucStartGeneratorFunction" # Name of the Azure Function App
$publishDir = "publish"

# Change directory to the function project
cd "$PSScriptRoot/$solutionName/$projectName"

# Publish the project
dotnet publish -c Release -o $publishDir

# Check if the publish was successful
if (Test-Path "$publishDir/*.dll") {
    # Create a zip file of the published output
    $zipFilePath = "$publishDir/$projectName.zip"
    Compress-Archive -Path "$publishDir/*" -DestinationPath $zipFilePath

    # Deploy the function app using Azure CLI
    az functionapp deployment source config-zip --name $functionAppName --resource-group $resourceGroupName --src $zipFilePath
} else {
    Write-Host "Publishing failed. Please check your project build."
}
