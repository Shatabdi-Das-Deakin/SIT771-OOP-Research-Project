// ============================================================
// GeneralWard.cs
// ------------------------------------------------------------
// A ward for adults who are not critical. It inherits everything from
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
        return patient.Age >= 16 && patient.Severity != Severity.Critical;
    }

    public override string GetAdmissionRule()
    {
        return "adults 16+, not critical";
    }
}
