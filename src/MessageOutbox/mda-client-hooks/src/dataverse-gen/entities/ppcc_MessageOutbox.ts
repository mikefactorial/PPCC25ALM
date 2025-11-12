/* eslint-disable*/

// Entity ppcc_MessageOutbox FormContext
export interface ppcc_MessageOutboxFormContext extends Xrm.FormContext {
    getAttribute(): Xrm.Attributes.Attribute[];
    getAttribute<T extends Xrm.Attributes.Attribute>(attributeName: string): T;
    getAttribute(attributeName: string): Xrm.Attributes.Attribute;
    getAttribute(index: number): Xrm.Attributes.Attribute;

    getControl(): Xrm.Controls.Control[];
    getControl<T extends Xrm.Controls.Control>(controlName: string): T;
    getControl(controlName: string): Xrm.Controls.Control;
    getControl(index: number): Xrm.Controls.Control;

    /*
    Date and time when the record was created.
    */
    getAttribute(name: 'createdon'): Xrm.Attributes.DateAttribute;
    /*
    Date and time when the record was created.
    */
    getControl(name: 'createdon'): Xrm.Controls.DateControl;
    /*
    Sequence number of the import that created this record.
    */
    getAttribute(name: 'importsequencenumber'): Xrm.Attributes.NumberAttribute;
    /*
    Sequence number of the import that created this record.
    */
    getControl(name: 'importsequencenumber'): Xrm.Controls.NumberControl;
    /*
    Date and time when the record was modified.
    */
    getAttribute(name: 'modifiedon'): Xrm.Attributes.DateAttribute;
    /*
    Date and time when the record was modified.
    */
    getControl(name: 'modifiedon'): Xrm.Controls.DateControl;
    /*
    Date and time that the record was migrated.
    */
    getAttribute(name: 'overriddencreatedon'): Xrm.Attributes.DateAttribute;
    /*
    Date and time that the record was migrated.
    */
    getControl(name: 'overriddencreatedon'): Xrm.Controls.DateControl;
    /*
    
    */
    getAttribute(name: 'ppcc_entity'): Xrm.Attributes.StringAttribute;
    /*
    
    */
    getControl(name: 'ppcc_entity'): Xrm.Controls.StringControl;
    /*
    Service Bus Message Id
    */
    getAttribute(name: 'ppcc_messageid'): Xrm.Attributes.StringAttribute;
    /*
    Service Bus Message Id
    */
    getControl(name: 'ppcc_messageid'): Xrm.Controls.StringControl;
    /*
    
    */
    getAttribute(name: 'ppcc_messageprocessedon'): Xrm.Attributes.DateAttribute;
    /*
    
    */
    getControl(name: 'ppcc_messageprocessedon'): Xrm.Controls.DateControl;
    /*
    
    */
    getAttribute(name: 'ppcc_messagesenton'): Xrm.Attributes.DateAttribute;
    /*
    
    */
    getControl(name: 'ppcc_messagesenton'): Xrm.Controls.DateControl;
    /*
    
    */
    getAttribute(name: 'ppcc_name'): Xrm.Attributes.StringAttribute;
    /*
    
    */
    getControl(name: 'ppcc_name'): Xrm.Controls.StringControl;
    /*
    
    */
    getAttribute(name: 'ppcc_recordid'): Xrm.Attributes.StringAttribute;
    /*
    
    */
    getControl(name: 'ppcc_recordid'): Xrm.Controls.StringControl;
    /*
    For internal use only.
    */
    getAttribute(name: 'timezoneruleversionnumber'): Xrm.Attributes.NumberAttribute;
    /*
    For internal use only.
    */
    getControl(name: 'timezoneruleversionnumber'): Xrm.Controls.NumberControl;
    /*
    Time zone code that was in use when the record was created.
    */
    getAttribute(name: 'utcconversiontimezonecode'): Xrm.Attributes.NumberAttribute;
    /*
    Time zone code that was in use when the record was created.
    */
    getControl(name: 'utcconversiontimezonecode'): Xrm.Controls.NumberControl;
}
// Entity ppcc_MessageOutbox
export const ppcc_messageoutboxMetadata = {
  typeName: "mscrm.ppcc_messageoutbox",
  logicalName: "ppcc_messageoutbox",
  collectionName: "ppcc_messageoutboxes",
  primaryIdAttribute: "ppcc_messageoutboxid",
  attributeTypes: {
    // Numeric Types
    importsequencenumber: "Integer",
    timezoneruleversionnumber: "Integer",
    utcconversiontimezonecode: "Integer",
    versionnumber: "BigInt",
    // Optionsets
    statecode: "Optionset",
    statuscode: "Optionset",
    // Date Formats
    createdon: "DateAndTime:UserLocal",
    modifiedon: "DateAndTime:UserLocal",
    overriddencreatedon: "DateOnly:UserLocal",
    ppcc_messageprocessedon: "DateAndTime:UserLocal",
    ppcc_messagesenton: "DateAndTime:UserLocal",
  },
  navigation: {
    createdby: ["mscrm.systemuser"],
    createdonbehalfby: ["mscrm.systemuser"],
    modifiedby: ["mscrm.systemuser"],
    modifiedonbehalfby: ["mscrm.systemuser"],
    ownerid: ["mscrm.principal"],
    owningbusinessunit: ["mscrm.businessunit"],
    owningteam: ["mscrm.team"],
    owninguser: ["mscrm.systemuser"],
  },
};

// Attribute constants
export const enum ppcc_MessageOutboxAttributes {
  CreatedBy = "createdby",
  CreatedByName = "createdbyname",
  CreatedByYomiName = "createdbyyominame",
  CreatedOn = "createdon",
  CreatedOnBehalfBy = "createdonbehalfby",
  CreatedOnBehalfByName = "createdonbehalfbyname",
  CreatedOnBehalfByYomiName = "createdonbehalfbyyominame",
  ImportSequenceNumber = "importsequencenumber",
  ModifiedBy = "modifiedby",
  ModifiedByName = "modifiedbyname",
  ModifiedByYomiName = "modifiedbyyominame",
  ModifiedOn = "modifiedon",
  ModifiedOnBehalfBy = "modifiedonbehalfby",
  ModifiedOnBehalfByName = "modifiedonbehalfbyname",
  ModifiedOnBehalfByYomiName = "modifiedonbehalfbyyominame",
  OverriddenCreatedOn = "overriddencreatedon",
  OwnerId = "ownerid",
  OwnerIdName = "owneridname",
  OwnerIdType = "owneridtype",
  OwnerIdYomiName = "owneridyominame",
  OwningBusinessUnit = "owningbusinessunit",
  OwningBusinessUnitName = "owningbusinessunitname",
  OwningTeam = "owningteam",
  OwningUser = "owninguser",
  ppcc_Entity = "ppcc_entity",
  ppcc_Message = "ppcc_message",
  ppcc_MessageId = "ppcc_messageid",
  ppcc_MessageOutboxId = "ppcc_messageoutboxid",
  ppcc_MessageProcessedOn = "ppcc_messageprocessedon",
  ppcc_MessageSentOn = "ppcc_messagesenton",
  ppcc_Name = "ppcc_name",
  ppcc_RecordId = "ppcc_recordid",
  statecode = "statecode",
  statuscode = "statuscode",
  TimeZoneRuleVersionNumber = "timezoneruleversionnumber",
  UTCConversionTimeZoneCode = "utcconversiontimezonecode",
  VersionNumber = "versionnumber",
}
