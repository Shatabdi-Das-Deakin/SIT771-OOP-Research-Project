// ============================================================
// WardType.cs  (Version A, the flat design)
// ------------------------------------------------------------
// In this version there are no ward subclasses. Instead every Ward
// object stores one of these values, and the Ward class checks it
// with if/else statements to decide which rule to use.
// ============================================================

public enum WardType
{
    ICU,
    Children,
    General,
    Elderly,
    Maternity,
    Rehab
}
