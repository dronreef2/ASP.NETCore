param location string = 'brazilsouth'
param storageAccountName string = 'netcorestorage'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_RAGRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    networkAcls: {
      bypass: 'AzureServices'
      defaultAction: 'Deny'
      virtualNetworkRules: [
        {
          id: '/subscriptions/265d1de6-a897-4111-a166-d205c7ca4467/resourceGroups/netcor_group/providers/Microsoft.Network/virtualNetworks/netcore-vnet/subnets/default'
        }
      ]
      ipRules: []
    }
  }
}

resource privateEndpoint 'Microsoft.Network/privateEndpoints@2023-05-01' = {
  name: '${storageAccountName}-pe'
  location: location
  properties: {
    subnet: {
      id: '/subscriptions/265d1de6-a897-4111-a166-d205c7ca4467/resourceGroups/netcor_group/providers/Microsoft.Network/virtualNetworks/netcore-vnet/subnets/default'
    }
    privateLinkServiceConnections: [
      {
        name: '${storageAccountName}-plsc'
        properties: {
          privateLinkServiceId: storageAccount.id
          groupIds: [ 'blob' ]
        }
      }
    ]
  }
}

output storageAccountId string = storageAccount.id
output privateEndpointId string = privateEndpoint.id
