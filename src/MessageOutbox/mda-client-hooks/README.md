# Message Outbox Client Hooks

This project contains TypeScript client-side functions for managing message outbox records in Dataverse.

## Build

```bash
npm install
npm run build
```

The build output will be in `dist/ClientHooks.js`.

## Ribbon Button Configuration

### Send Selected Messages Button

Sends selected message outbox records to Azure Service Bus.

**Command Definition:**
```xml
<CommandDefinition Id="ppcc.messageoutbox.SendMessages">
  <EnableRules>
    <EnableRule Id="ppcc.messageoutbox.SendMessages.EnableRule" />
  </EnableRules>
  <DisplayRules />
  <Actions>
    <JavaScriptFunction FunctionName="sendSelectedMessages" Library="$webresource:ppcc_ClientHooks">
      <CrmParameter Value="SelectedControlSelectedItemIds" />
    </JavaScriptFunction>
  </Actions>
</CommandDefinition>
```

**Enable Rule:**
```xml
<EnableRule Id="ppcc.messageoutbox.SendMessages.EnableRule">
  <CustomRule FunctionName="enableSendMessagesRule" Library="$webresource:ppcc_ClientHooks">
    <CrmParameter Value="SelectedControlSelectedItemIds" />
  </CustomRule>
</EnableRule>
```

**Button:**
```xml
<Button Id="ppcc.messageoutbox.SendMessagesButton"
        Command="ppcc.messageoutbox.SendMessages"
        Sequence="10"
        LabelText="Send to Service Bus"
        ToolTipTitle="Send to Service Bus"
        ToolTipDescription="Send selected messages to Azure Service Bus"
        Image16by16="/_imgs/ico_16_4210.png" />
```

### Process Messages from Queue Button

Processes messages from the Azure Service Bus queue (doesn't require selection).

**Command Definition:**
```xml
<CommandDefinition Id="ppcc.messageoutbox.ProcessMessages">
  <EnableRules />
  <DisplayRules />
  <Actions>
    <JavaScriptFunction FunctionName="processMessagesFromQueue" Library="$webresource:ppcc_ClientHooks">
      <IntParameter Value="10" />
    </JavaScriptFunction>
  </Actions>
</CommandDefinition>
```

**Button:**
```xml
<Button Id="ppcc.messageoutbox.ProcessMessagesButton"
        Command="ppcc.messageoutbox.ProcessMessages"
        Sequence="20"
        LabelText="Process from Queue"
        ToolTipTitle="Process from Queue"
        ToolTipDescription="Process messages from Azure Service Bus queue"
        Image16by16="/_imgs/ico_16_4210.png" />
```

## Functions

### sendSelectedMessages(selectedIds: string[])

Sends selected message outbox records to Azure Service Bus.

- **Parameters:**
  - `selectedIds`: Array of selected record IDs from the grid
- **Validates:**
  - At least one record is selected
  - Maximum 50 records can be sent at once
- **Calls:** `ppcc_SendServiceBusMessages` Custom API
- **Updates:** Sets records to "Sent" status after successful send

### enableSendMessagesRule(selectedIds: string[])

Enable rule that ensures 1-50 records are selected.

- **Parameters:**
  - `selectedIds`: Array of selected record IDs
- **Returns:** `true` if button should be enabled

### processMessagesFromQueue(batchSize?: number)

Processes messages from the Service Bus queue.

- **Parameters:**
  - `batchSize`: Optional batch size (defaults to 10)
- **Calls:** `ppcc_ProcessMessages` Custom API
- **Updates:** Marks processed messages with "Processed" status

## Deployment

1. Build the project: `npm run build`
2. Upload `dist/ClientHooks.js` as a web resource named `ppcc_ClientHooks`
3. Configure ribbon buttons using the XML definitions above
4. Publish customizations

## Custom APIs Required

This solution requires the following Custom APIs to be deployed:

- `ppcc_SendMessages` - Sends message outbox records to Service Bus
- `ppcc_ProcessMessages` - Processes messages from Service Bus queue

See the plugin documentation for more details.
