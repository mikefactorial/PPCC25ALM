/* eslint-disable*/
import { WebApiExecuteRequest } from "dataverse-ify";
import { StructuralProperty } from "dataverse-ify";
import { OperationType } from "dataverse-ify";

// Action ppcc_ProcessServiceBusMessages
export const ppcc_ProcessServiceBusMessagesMetadata = {
  parameterTypes: {
    "BatchSize": {
      typeName: "Edm.Int32",
      structuralProperty: StructuralProperty.PrimitiveType
      },		
  
  },
  operationType: OperationType.Action,
  operationName: "ppcc_ProcessServiceBusMessages"
};

export interface ppcc_ProcessServiceBusMessagesRequest extends WebApiExecuteRequest {
  BatchSize?: number;
}