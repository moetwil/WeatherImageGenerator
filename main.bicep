param location string = resourceGroup().location

var prefix = 'luc'
var serverFarmName = '${prefix}sf'
var storageAccountName = '${prefix}sta'

var startGeneratorFunctionName = '${prefix}StartGeneratorFunction'
var jobProcessorFunctionName = '${prefix}JobProcessorFunction'
var imageProcessorFunctionName = '${prefix}ImageProcessorFunction'
var getJobFunctionName = '${prefix}GetJobFunction'
var blobActionFunctionName = '${prefix}BlobActionFunction'

resource serverFarm 'Microsoft.Web/serverfarms@2021-03-01' = {
  name: serverFarmName
  location: location
  tags: resourceGroup().tags
  sku: {
    tier: 'Consumption'
    name: 'Y1'
  }
  kind: 'elastic'
}

var storageAccountConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  tags: resourceGroup().tags
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    accessTier: 'Hot'
    publicNetworkAccess: 'Enabled'
  }
}

resource startGeneratorFunction 'Microsoft.Web/sites@2021-03-01' = {
  name: startGeneratorFunctionName
  location: location
  tags: resourceGroup().tags
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    enabled: true
    serverFarmId: serverFarm.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      http20Enabled: true
    }
    clientAffinityEnabled: false
    httpsOnly: true
    containerSize: 1536
    redundancyMode: 'None'
  }
}

resource startGeneratorFunctionConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${startGeneratorFunctionName}/appsettings'
  properties: {
    // function app settings
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
    WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    WEBSITE_CONTENTSHARE: toLower(startGeneratorFunctionName)
    JobStatusTableName: 'jobstatus'
    JobQueueName: 'jobqueue'
  }
}

resource jobProcessorFunction 'Microsoft.Web/sites@2021-03-01' = {
  name: jobProcessorFunctionName
  location: location
  tags: resourceGroup().tags
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    enabled: true
    serverFarmId: serverFarm.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      http20Enabled: true
    }
    clientAffinityEnabled: false
    httpsOnly: true
    containerSize: 1536
    redundancyMode: 'None'
  }
}

resource jobProcessorFunctionConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${jobProcessorFunctionName}/appsettings'
  properties: {
    // function app settings
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
    WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    WEBSITE_CONTENTSHARE: toLower(jobProcessorFunctionName)
    JobStatusTableName: 'jobstatus'
    JobQueueName: 'jobqueue'
    ImageQueueName: 'imagequeue'
    ImageContainerName: 'weather-images'
    WeatherEndpoint: 'https://data.buienradar.nl/2.0/feed/json'
    ImageEndpoint: 'https://picsum.photos/200'
  }
}

resource imageProcessorFunction 'Microsoft.Web/sites@2021-03-01' = {
  name: imageProcessorFunctionName
  location: location
  tags: resourceGroup().tags
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    enabled: true
    serverFarmId: serverFarm.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      http20Enabled: true
    }
    clientAffinityEnabled: false
    httpsOnly: true
    containerSize: 1536
    redundancyMode: 'None'
  }
}

resource imageProcessorFunctionConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${imageProcessorFunctionName}/appsettings'
  properties: {
    // function app settings
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
    WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    WEBSITE_CONTENTSHARE: toLower(imageProcessorFunctionName)
    ImageQueueName: 'imagequeue'
    ImageContainerName: 'weather-images'
  }
}

resource getJobFunction 'Microsoft.Web/sites@2021-03-01' = {
  name: getJobFunctionName
  location: location
  tags: resourceGroup().tags
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    enabled: true
    serverFarmId: serverFarm.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      http20Enabled: true
    }
    clientAffinityEnabled: false
    httpsOnly: true
    containerSize: 1536
    redundancyMode: 'None'
  }
}

resource getJobFunctionConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${getJobFunctionName}/appsettings'
  properties: {
    // function app settings
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
    WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    WEBSITE_CONTENTSHARE: toLower(getJobFunctionName)
    JobStatusTableName: 'jobstatus'          
  }
}

resource blobActionFunction 'Microsoft.Web/sites@2021-03-01' = {
  name: blobActionFunctionName
  location: location
  tags: resourceGroup().tags
  identity: {
    type: 'SystemAssigned'
  }
  kind: 'functionapp'
  properties: {
    enabled: true
    serverFarmId: serverFarm.id
    siteConfig: {
      netFrameworkVersion: 'v8.0'
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      http20Enabled: true
    }
    clientAffinityEnabled: false
    httpsOnly: true
    containerSize: 1536
    redundancyMode: 'None'
  }
}

resource blobActionFunctionConfig 'Microsoft.Web/sites/config@2021-03-01' = {
  name: '${blobActionFunctionName}/appsettings'
  properties: {
    // function app settings
    FUNCTIONS_EXTENSION_VERSION: '~4'
    FUNCTIONS_WORKER_RUNTIME: 'dotnet-isolated'
    WEBSITE_USE_PLACEHOLDER_DOTNETISOLATED: '1'
    AzureWebJobsStorage: storageAccountConnectionString
    WEBSITE_CONTENTAZUREFILECONNECTIONSTRING: storageAccountConnectionString
    WEBSITE_CONTENTSHARE: toLower(blobActionFunctionName)
    JobStatusTableName: 'jobstatus'
    JobQueueName: 'jobqueue'
    ImageQueueName: 'imagequeue'
    ImageContainerName: 'weather-images'
    WeatherEndpoint: 'https://data.buienradar.nl/2.0/feed/json'
    ImageEndpoint: 'https://picsum.photos/200'
    AccountName: storageAccount.name
    AccountKey: storageAccount.listKeys().keys[0].value
    BaseUrl: storageAccount.properties.primaryEndpoints.blob
  }
}
