// ============================================================
// ICUWard.cs
// ------------------------------------------------------------
// Intensive Care takes critical patients of any age, including
// children. It only looks at severity, not age.
//
// Between the three ward types, every possible patient fits exactly
// one kind of ward:
//   Critical, any age      -> ICUWard
//   Not critical, under 16 -> ChildrensWard
//   Not critical, 16+      -> GeneralWard
// ============================================================

public class ICUWard : Ward
{
    public ICUWard(string code, string name, int bedCount)
        : base(code, name, bedCount) { }

    public override bool CanAdmit(Patient patient)
    {
        return patient.Severity == Severity.Critical;
    }

    public override string GetAdmissionRule()
    {
        return "critical patients, any age";
    }
}
