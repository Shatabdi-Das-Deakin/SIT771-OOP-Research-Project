// ============================================================
// ChildrensWard.cs
// ------------------------------------------------------------
// A ward for children under 16 who are not critical.
// The shape is the same as GeneralWard. Only the rule is different.
// ============================================================

public class ChildrensWard : Ward
{
    public ChildrensWard(string code, string name, int bedCount)
        : base(code, name, bedCount) { }

    public override bool CanAdmit(Patient patient)
    {
        return patient.Age < 16 && patient.Severity != Severity.Critical;
    }

    public override string GetAdmissionRule()
    {
        return "children under 16, not critical";
    }
}
