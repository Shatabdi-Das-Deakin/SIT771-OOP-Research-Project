// ============================================================
// EventType.cs
// ------------------------------------------------------------
// Every time something happens in the hospital we write an entry
// to the activity log (see LogEntry.cs). This enum labels what kind
// of event it was, so the log can be filtered later, for example
// "show me only the transfers".
// ============================================================

public enum EventType
{
    Admitted,          // patient given a bed
    Waitlisted,        // no suitable bed, patient added to the waiting list
    Discharged,        // patient left a bed or left the waiting list
    Transferred,       // patient moved from one ward to another
    ConditionChanged,  // patient's severity was updated
    TransferPending    // patient needs a different ward but none has a free bed yet
}
