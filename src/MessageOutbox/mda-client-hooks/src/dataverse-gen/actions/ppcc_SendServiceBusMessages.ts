/* eslint-disable*/
import { WebApiExecuteRequest } from "dataverse-ify";
import { StructuralProperty } from "dataverse-ify";
import { OperationType } from "dataverse-ify";

// Action ppcc_SendServiceBusMessages
export const ppcc_SendServiceBusMessagesMetadata = {
  parameterTypes: {
    "Messages": {
      typeName: "Collection(mscrm.crmbaseentity)",
      structuralProperty: StructuralProperty.Collection
      },		
  
  },
  operationType: OperationType.Action,
  operationName: "ppcc_SendServiceBusMessages"
};

export interface ppcc_SendServiceBusMessagesRequest extends WebApiExecuteRequest {
  Messages?: any[];
}