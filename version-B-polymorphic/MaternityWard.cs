// ============================================================
// MaternityWard.cs
// ------------------------------------------------------------
// A ward for pregnant patients who are not critical, any age.
// Added for the research experiment (change C2).
// ============================================================

public class MaternityWard : Ward
{
    public MaternityWard(string code, string name, int bedCount)
        : base(code, name, bedCount) { }

    public override bool CanAdmit(Patient patient)
    {
        return patient.IsPregnant && patient.Severity != Severity.Critical;
    }

    public override string GetAdmissionRule()
    {
        return "pregnant, not critical";
    }
}
