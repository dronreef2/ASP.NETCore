param location string = resourceGroup().location
param nsgName string = 'netcore-nsg'

param vnetName string = 'netcore-vnet'
param subnetName string = 'default'

resource nsg 'Microsoft.Network/networkSecurityGroups@2023-04-01' = {
  name: nsgName
  location: location
  properties: {
    securityRules: [
      {
        name: 'Allow-HTTP'
        properties: {
          priority: 100
          direction: 'Inbound'
          access: 'Allow'
          protocol: 'Tcp'
          sourcePortRange: '*'
          destinationPortRange: '80'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
        }
      }
      {
        name: 'Allow-HTTPS'
        properties: {
          priority: 110
          direction: 'Inbound'
          access: 'Allow'
          protocol: 'Tcp'
          sourcePortRange: '*'
          destinationPortRange: '443'
          sourceAddressPrefix: '*'
          destinationAddressPrefix: '*'
        }
      }
    ]
  }
}

resource vnet 'Microsoft.Network/virtualNetworks@2024-01-01' existing = {
  name: vnetName
}

resource subnet 'Microsoft.Network/virtualNetworks/subnets@2024-01-01' existing = {
  parent: vnet
  name: subnetName
}

resource subnetUpdate 'Microsoft.Network/virtualNetworks/subnets@2024-01-01' = {
  parent: vnet
  name: subnetName
  properties: {
    addressPrefix: subnet.properties.addressPrefix
    networkSecurityGroup: {
      id: nsg.id
    }
    privateEndpointNetworkPolicies: subnet.properties.privateEndpointNetworkPolicies
    privateLinkServiceNetworkPolicies: subnet.properties.privateLinkServiceNetworkPolicies
  }
}

output nsgId string = nsg.id
