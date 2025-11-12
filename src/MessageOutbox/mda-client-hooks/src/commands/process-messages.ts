/**
 * Ribbon command functions for processing message outbox records
 */

import {
  ppcc_SendServiceBusMessagesRequest,
  ppcc_SendServiceBusMessagesMetadata,
} from '../dataverse-gen/actions/ppcc_SendServiceBusMessages';
import {
  ppcc_ProcessServiceBusMessagesRequest,
  ppcc_ProcessServiceBusMessagesMetadata,
} from '../dataverse-gen/actions/ppcc_ProcessServiceBusMessages';

// Xrm is a global variable provided by Dataverse
declare const Xrm: Xrm.XrmStatic;

/**
 * Sends selected message outbox records to Service Bus
 * This function is called from a ribbon button on the ppcc_messageoutbox grid
 * @param selectedIds - Array of selected record IDs from the grid
 */
export async function sendSelectedMessages(
  selectedIds: string[],
): Promise<void> {
  try {
    // Validate selection
    if (!selectedIds || selectedIds.length === 0) {
      Xrm.Navigation.openAlertDialog({
        text: 'Please select at least one message to send.',
        title: 'No Selection',
      });
      return;
    }

    // Check batch size limit (max 50 as per plugin validation)
    if (selectedIds.length > 50) {
      Xrm.Navigation.openAlertDialog({
        text: `You can only send up to 50 messages at a time. You selected ${selectedIds.length} messages.`,
        title: 'Too Many Records',
      });
      return;
    }

    // Show progress dialog
    Xrm.Utility.showProgressIndicator(
      `Sending ${selectedIds.length} message(s) to Service Bus...`,
    );

    // Build entity references for the selected records
    const messages = selectedIds.map((id) => ({
      '@odata.type': 'Microsoft.Dynamics.CRM.ppcc_messageoutbox',
      ppcc_messageoutboxid: id,
    }));

    // Create the strongly-typed request
    const request: ppcc_SendServiceBusMessagesRequest = {
      logicalName: 'ppcc_SendServiceBusMessages',
      Messages: messages,
      getMetadata: () => ppcc_SendServiceBusMessagesMetadata,
    };

    // Execute the request using dataverse-ify
    await Xrm.WebApi.online.execute(request);

    // Close progress and show success
    Xrm.Utility.closeProgressIndicator();

    await Xrm.Navigation.openAlertDialog({
      text: `Successfully queued ${selectedIds.length} message(s) to be sent to Service Bus.`,
      title: 'Success',
    });

    // Refresh the grid to show updated statuses
    const formContext = Xrm.Page as Xrm.FormContext;
    if (formContext && formContext.data && formContext.data.refresh) {
      formContext.data.refresh(false);
    }
  } catch (error: unknown) {
    // Close progress indicator
    Xrm.Utility.closeProgressIndicator();

    // Show error message
    const errorMessage =
      error instanceof Error
        ? error.message
        : 'An unknown error occurred while sending messages.';
    const errorDetails = error instanceof Error ? error.stack || '' : '';
    await Xrm.Navigation.openErrorDialog({
      message: errorMessage,
      details: errorDetails,
    });

    console.error('Error sending messages:', error);
  }
}

/**
 * Enable rule for the Send to Service Bus button
 * The button should be enabled when one or more records are selected
 * @param selectedIds - Array of selected record IDs
 * @returns true if button should be enabled, false otherwise
 */
export function enableSendMessagesRule(selectedIds: string[]): boolean {
  return selectedIds && selectedIds.length > 0 && selectedIds.length <= 50;
}

/**
 * Processes messages from the Service Bus queue
 * This function is called from a ribbon button and doesn't require selected records
 * @param batchSize - Optional batch size (defaults to 10 if not provided)
 */
export async function processMessagesFromQueue(
  batchSize?: number,
): Promise<void> {
  try {
    const actualBatchSize = batchSize || 10;

    // Show progress dialog
    Xrm.Utility.showProgressIndicator(
      `Processing up to ${actualBatchSize} messages from Service Bus queue...`,
    );

    // Create the strongly-typed request
    const request: ppcc_ProcessServiceBusMessagesRequest = {
      logicalName: 'ppcc_ProcessServiceBusMessages',
      BatchSize: actualBatchSize,
      getMetadata: () => ppcc_ProcessServiceBusMessagesMetadata,
    };

    // Execute the request using dataverse-ify
    await Xrm.WebApi.online.execute(request);

    // Close progress and show success
    Xrm.Utility.closeProgressIndicator();

    await Xrm.Navigation.openAlertDialog({
      text: `Successfully processed messages from the Service Bus queue.`,
      title: 'Success',
    });

    // Refresh the grid to show updated statuses
    const formContext = Xrm.Page as Xrm.FormContext;
    if (formContext && formContext.data && formContext.data.refresh) {
      formContext.data.refresh(false);
    }
  } catch (error: unknown) {
    // Close progress indicator
    Xrm.Utility.closeProgressIndicator();

    // Show error message
    const errorMessage =
      error instanceof Error
        ? error.message
        : 'An unknown error occurred while processing messages.';
    const errorDetails = error instanceof Error ? error.stack || '' : '';
    await Xrm.Navigation.openErrorDialog({
      message: errorMessage,
      details: errorDetails,
    });

    console.error('Error processing messages:', error);
  }
}
