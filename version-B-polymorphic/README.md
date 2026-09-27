# The class-per-ward version

This is my Patient Management program in its original design, with the three small fixes and Changes 1 to 3 from the research.

`Ward` is an abstract class. It holds everything every ward shares, such as the beds and admitting and discharging patients. It declares two abstract methods that every kind of ward must write for itself:

* `CanAdmit(Patient)`: whether this kind of ward accepts the patient
* `GetAdmissionRule()`: a short description of the rule

| Ward class | Accepts |
|---|---|
| `ICUWard` | Critical patients of any age |
| `MaternityWard` | Pregnant patients who are not critical |
| `ChildrensWard` | Under 18, not pregnant, not critical |
| `GeneralWard` | 18 to 74, not pregnant, not critical |
| `ElderlyWard` | 75 and over, not pregnant, not critical |

`Hospital` keeps all the wards in one `List<Ward>` and calls `CanAdmit` on each one without checking which kind it is. Adding a new kind of ward means writing one new class and adding one line in `Program.cs`. If a new ward class leaves out either method, the program does not build.

## Running it

```
dotnet run
```

Option 9 loads demo patients, option 3 discharges a patient, and option 8 opens the SplashKit ward map. It needs the SplashKit package, which `dotnet run` downloads the first time.
