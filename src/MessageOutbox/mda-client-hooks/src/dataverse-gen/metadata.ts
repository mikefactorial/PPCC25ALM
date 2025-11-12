/* eslint-disable*/
import { ppcc_messageoutboxMetadata } from "./entities/ppcc_MessageOutbox";
import { ppcc_ProcessServiceBusMessagesMetadata } from "./actions/ppcc_ProcessServiceBusMessages";
import { ppcc_SendServiceBusMessagesMetadata } from "./actions/ppcc_SendServiceBusMessages";

export const Entities = {
  ppcc_MessageOutbox: "ppcc_messageoutbox",
};

// Setup Metadata
// Usage: setMetadataCache(metadataCache);
export const metadataCache = {
  entities: {
    ppcc_messageoutbox: ppcc_messageoutboxMetadata,
  },
  actions: {
    ppcc_ProcessServiceBusMessages: ppcc_ProcessServiceBusMessagesMetadata,
    ppcc_SendServiceBusMessages: ppcc_SendServiceBusMessagesMetadata,
  }
};