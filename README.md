# SIT771 Task 7.4 (HD): Object Oriented Design Research Project

Shatabdi Das (s226086986)
SIT771 Object-Oriented Development, Deakin University

This repository holds the code, tests and results for my High Distinction research project.

## What the project is about

Patient Management is a hospital program I wrote earlier in the unit. When a patient arrives, the program chooses a ward for them: Intensive Care takes critical patients, the Children's Ward takes children, and the General Wards take adults who are not critical.

There are two common ways to write the code that makes this choice:

* **The conditional design:** one concrete `Ward` class stores a type code (a `WardType` enumeration value), and `CanAdmit` uses if/else statements on it to select the rule.
* **The inheritance-based design:** an abstract `Ward` class declares `CanAdmit` and `GetAdmissionRule` as abstract methods, and each kind of ward is a subclass that overrides them. The `Hospital` works only with the abstract type, so each call runs the rule of the actual subclass (polymorphism).

**Thesis:** applying inheritance, through an abstract base class with one subclass for each kind of ward, makes the program easier and safer to extend than a conditional design.

**Research question:** How does applying inheritance and polymorphism affect how easily and safely a program can be extended and tested, compared with a conditional design based on an enumeration and if/else statements?

## The two designs

The same admission rules, written both ways (shortened):

```csharp
// Conditional design: one Ward class
bool CanAdmit(Patient p)
{
  if (_type == WardType.ICU)
    return p.Severity == Severity.Critical;
  else if (_type == WardType.Children)
    return p.Age < 16 && ...;
  else if (_type == WardType.General)
    return p.Age >= 16 && ...;
  return false;
}
```

```csharp
// Inheritance-based design: one class for each kind of ward
class ICUWard : Ward
{
  override bool CanAdmit(Patient p)
  {
    return p.Severity == Severity.Critical;
  }
}
// ChildrensWard and GeneralWard are also subclasses of Ward with their own rule
```

**The inheritance-based design** (folder `version-B-polymorphic`) is the original program. `Ward` is an abstract class that holds what every ward shares, such as the beds, and says every kind of ward must provide its own `CanAdmit` rule. The `Hospital` asks each ward "can you take this patient?" without checking what kind of ward it is.

![Class diagram of the inheritance-based design](docs/class-diagram-version-B.png)

**The conditional design** (folder `version-A-flat`) does exactly the same job with one ordinary `Ward` class that stores its type in a `WardType` enum. Everything else is the same, so the only difference is where the rules live. Before any change, both designs printed identical output on every test.

![Class diagram of the conditional design](docs/class-diagram-version-A.png)

The diagrams can also be opened in [Lucidchart](https://lucid.app/lucidchart/c43ebfbf-2a3f-425a-a625-0f350c25f852/edit?invitationId=inv_e427f615-a624-44fc-bdd2-e396cd70c8ab) (view only).

## The four changes

I made the same four changes to both designs, then refined the inheritance-based design. In the commit messages they are called C1 to C4.

| Change | What it tests |
|---|---|
| **1. Add an Elderly Ward** (75 and over) | A new ward that only needs information the program already has |
| **2. Add a Maternity Ward** | A new ward that needs new information: whether the patient is pregnant |
| **3. Raise the Children's Ward age limit** from under 16 to under 18 | Editing a rule that already exists |
| **4. Add a Rehab Ward but forget to write its rule** | Which version notices the mistake |

## Results

After every change both designs passed all ten tests and printed identical output. This is what happened in each change:

| Change | Conditional design | Inheritance-based design | Handled better by |
|---|---|---|:---:|
| **1. Add an Elderly Ward** | The existing Ward class had to be modified: a new branch in two methods and a new enumeration value. | Added as a new subclass plus one line to register it. No existing class was modified. | **Inheritance** |
| **1. (first attempt)** | An 80 year old was sent to a General Ward. The compiler did not detect it; a test did. | The same error, detected the same way. | **Neither** |
| **2. Add a Maternity Ward** | Storing the new patient information took the same work in both designs. The new rule was added in one class. | Storing the new patient information took the same work in both designs. Four ward subclasses changed, because three existing wards had to exclude pregnant patients. | **Conditional** |
| **3. Raise the Children's Ward age limit to 18** | All rules are in one method, so one class changed. | The limit was repeated in two subclasses, so both had to change together. | **Conditional** |
| **3. (after refinement)** | Moving the adult age limit needed 4 lines: two rules and two descriptions. | The limit is a protected constant in the abstract Ward class, so the same move needed 1 line. | **Inheritance** |
| **4. Add a ward but omit its rule** | Compiled with no warning. The new ward silently admitted no patients. | Did not compile. The compiler reported that the subclass does not implement the abstract methods. | **Inheritance** |

The inheritance-based design handled three of the six outcomes better, the conditional design two, and one affected both equally.

What this means:

* **Inheritance supports extension without modifying existing classes.** A new ward is a new subclass, so code that already works stays unchanged.
* **Abstract methods let the compiler enforce the design.** A subclass that leaves out its rule does not compile, so the error is found before the program runs. The conditional design gives no warning.
* **The advantage of the conditional design was caused by repeated data.** The inheritance-based design first repeated the shared age limits in several subclasses. After they were moved into the abstract `Ward` class as protected constants (`AdultAge`, `ElderlyAge`), moving a limit took 1 line against 4 in the conditional design. Named constants would also help the conditional design, so this shows where the extra edits came from.
* **New data is independent of the class design.** Storing whether a patient is pregnant took the same work in both.
* **Inheritance cannot confirm that a rule is correct.** Neither compiler noticed that the first Elderly Ward rule overlapped with the General Ward. Only a test did.

**Conclusion:** the results support the thesis. Inheritance is most effective when the base class holds everything the subclasses share and each subclass holds only what makes it different.

### Detailed numbers

Files with code changes, and lines of code added and removed (comments and blank lines are not counted):

| Step | Conditional design | Inheritance-based design |
|---|---|---|
| 1. Add Elderly Ward | 3 files, +7 / -1 | 2 files, +14 / -0 |
| 1. Fix overlap with General Ward | 1 file, +2 / -2 | 1 file, +2 / -2 |
| 1. Ward map layout | 1 file, +12 / -3 | 1 file, +12 / -3 |
| 2. Pregnancy information | 3 files, +30 / -6 | 3 files, +30 / -6 |
| 2. Maternity rules | 3 files, +10 / -4 | 5 files, +17 / -3 |
| 3. Children under 18 | 1 file, +4 / -4 | 2 files, +4 / -4 |
| Refinement: shared age limits | not applied | 4 files, +8 / -6 |
| Moving the adult age limit afterwards | 1 file, +4 / -4 | 1 file, +1 / -1 |

These numbers are small, so they are best read together with the table above. They are also in [`results/measurements.csv`](results/measurements.csv), the refinement check is in [`results/refinement_check.txt`](results/refinement_check.txt), and the compiler output from Change 4 is in [`results/C4_forgotten_rule_output.txt`](results/C4_forgotten_rule_output.txt). The [results README](results/README.md) explains how to recount them.

## Repository layout

| Folder or file | What it holds |
|---|---|
| `version-B-polymorphic/` | The inheritance-based design |
| `version-A-flat/` | The conditional design |
| `tests/inputs/` | Ten scripted test runs. Each line is one answer typed into the menu |
| `tests/expected/` | The output each test should print, with clock times replaced by `<TIME>` |
| `results/` | The measurements and the compiler output from Change 4 |
| `docs/` | Class diagrams of both designs |
| `run_tests.sh` | Runs every test on Mac or Linux |
| `run_tests.ps1` | Runs every test on Windows |

The `tests`, `results` and both version folders each have their own README with more detail.

## How to follow the work

The commit history shows the steps in the order I did them. Start from the oldest commit:

1. Step 0: the program as it was before the research started
2. Step 1: three small fixes, then the scripted tests and their expected output
3. Step 2: the conditional design built, with identical output to the inheritance-based design on every test
4. C1: the Elderly Ward. The first attempt failed a test in both designs, and the next commits show the fix
5. C2: the Maternity Ward
6. C3: the Children's Ward age limit, then the refinement that moves the shared age limits into the abstract Ward class
7. C4: on the separate branch `c4-forgotten-rule`, so the mistake does not stay in the main program

In the commit messages, "Version A" is the conditional design and "Version B" is the inheritance-based design. Each change has one commit for each design, so `git show <commit>` shows exactly what each design needed.

## Running the tests

You need the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0). From the top folder of this repository:

On Windows (PowerShell):

```
.\run_tests.ps1 version-B-polymorphic
.\run_tests.ps1 version-A-flat
```

On Mac or Linux:

```
./run_tests.sh version-B-polymorphic
./run_tests.sh version-A-flat
```

Each one should finish with `10 passed, 0 failed`.

## Running the program

```
cd version-B-polymorphic
dotnet run
```

Choose option 9 to load the demo patients, then option 3 to discharge P001. A patient from the waiting list is given the free bed straight away. Option 8 opens the SplashKit ward map.

## Limitations

This is one small program and one developer, and lines of code are only a rough measure of effort. I built the conditional design myself, so I kept its rules inside the `Ward` class to make it as fair a comparison as I could. The ward map was checked by hand because the scripted tests cannot see a window.
