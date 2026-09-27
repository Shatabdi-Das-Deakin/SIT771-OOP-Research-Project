// ============================================================
// GeneralWard.cs
// ------------------------------------------------------------
// A ward for adults aged 16 to 74 who are not critical. (Changed in C1
// so it no longer overlaps ElderlyWard.) It inherits everything from
// Ward (beds, Admit, Discharge and so on) and only adds its own rule.
// ============================================================

public class GeneralWard : Ward   // ": Ward" means GeneralWard inherits from Ward
{
    // Pass the code, name and number of beds up to the Ward constructor.
    public GeneralWard(string code, string name, int bedCount)
        : base(code, name, bedCount) { }

    // "override" replaces the abstract CanAdmit from Ward with this ward's rule.
    public override bool CanAdmit(Patient patient)
    {
        return patient.Age >= 16 && patient.Age < 75 && patient.Severity != Severity.Critical;
    }

    public override string GetAdmissionRule()
    {
        return "adults 16 to 74, not critical";
    }
}
