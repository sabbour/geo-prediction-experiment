﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using System.Diagnostics; 
 


namespace GeoPrediction.Common
{
    public class EventHubManager
    {
        /// <summary>
        /// Creates an event hub
        /// </summary>
        /// <param name="eventHubName"></param>
        /// <param name="numberOfPartitions"></param>
        /// <param name="manager"></param>
        public static void CreateEventHub(string eventHubName, int numberOfPartitions, NamespaceManager manager)
        {
            try
            {
                // Create the Event Hub 
                Trace.TraceInformation("Creating Event Hub: " + eventHubName);
                EventHubDescription ehd = new EventHubDescription(eventHubName);
                ehd.PartitionCount = numberOfPartitions;
                manager.CreateEventHubIfNotExistsAsync(ehd).Wait();
            }
            catch (AggregateException agexp)
            {
                Trace.TraceError(agexp.Flatten().ToString());
                throw;
            }
        }

        /// <summary>
        /// Updates an event hub
        /// </summary>
        /// <param name="eventHubName"></param>
        /// <param name="namespaceManager"></param>
        /// <returns></returns>
        public static async Task<EventHubDescription> UpdateEventHub(string eventHubName, NamespaceManager namespaceManager)
        {
            // Add a consumer group 
            EventHubDescription ehd = await namespaceManager.GetEventHubAsync(eventHubName);
            await namespaceManager.CreateConsumerGroupIfNotExistsAsync(ehd.Path, "consumerGroupName");

            // Create a customer SAS rule with Manage permissions 
            ehd.UserMetadata = "Some updated info";
            string ruleName = "myeventhubmanagerule";
            string ruleKey = SharedAccessAuthorizationRule.GenerateRandomKey();
            ehd.Authorization.Add(new SharedAccessAuthorizationRule(ruleName, ruleKey, new AccessRights[] { AccessRights.Manage, AccessRights.Listen, AccessRights.Send }));

            EventHubDescription ehdUpdated = await namespaceManager.UpdateEventHubAsync(ehd);
            return ehd;
        } 
    }
}
