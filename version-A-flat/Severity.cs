// ============================================================
// Severity.cs
// ------------------------------------------------------------
// An enum is a fixed list of named values. A patient's condition
// can only ever be one of these three, so an enum is safer than
// using a string like "critical" that could be misspelt.
//
// The order matters. C# gives each value a number behind the
// scenes (Low = 0, Moderate = 1, Critical = 2), which means we can
// sort patients by severity and Critical will come out on top.
// Hospital.FillFreeBeds relies on this.
// ============================================================

public enum Severity
{
    Low,
    Moderate,
    Critical
}
