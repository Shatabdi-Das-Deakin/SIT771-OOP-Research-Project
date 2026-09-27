# SIT771 Task 7.4 (HD): Object Oriented Design Research Project

Shatabdi Das (s226086986)
SIT771 Object-Oriented Development, Deakin University

This repository holds the code, tests and results for my High Distinction research project. I took my Patient Management program, which I wrote earlier in the unit, built a second version of it with a different design, and made the same changes to both to see which design was easier to extend and test.

## Research question

How does designing with abstraction, inheritance and polymorphism affect how easily a program can be extended and tested, compared with a design built on an enum and if/else statements?

## The two versions

**Version B (polymorphic)** is the original design of the program. `Ward` is an abstract class with two abstract methods, `CanAdmit` and `GetAdmissionRule`. Each kind of ward is a subclass that overrides them. The `Hospital` class keeps every ward in one `List<Ward>` and never checks what kind of ward it is talking to.

![Version B class diagram](docs/class-diagram-version-B.png)

**Version A (flat)** does exactly the same job with one ordinary `Ward` class. It stores a `WardType` enum value, and `CanAdmit` and `GetAdmissionRule` use if/else statements on that value to choose the rule. Everything else is the same as Version B, so the only difference is how the ward rules are organised.

![Version A class diagram](docs/class-diagram-version-A.png)

The diagrams can also be opened in [Lucidchart](https://lucid.app/lucidchart/c43ebfbf-2a3f-425a-a625-0f350c25f852/edit?invitationId=inv_e427f615-a624-44fc-bdd2-e396cd70c8ab) (view only).

## The changes I made to both versions

| Change | What it tests |
|---|---|
| C1: add an Elderly Ward (75 and over, not critical) | A new kind of ward that only needs data the program already has |
| C2: add a Maternity Ward | A new kind of ward that needs new information about the patient |
| C3: change the Children's Ward from under 16 to under 18 | Editing a rule that already exists |
| C4: add a new ward but forget to write its rules | Which design notices the mistake |

## Results

After every change both versions passed all ten tests and printed identical output. This is what happened in each change:

| Change | Version A: one Ward class with if/else | Version B: one class for each kind of ward | Easier |
|---|---|---|:---:|
| **C1: add an Elderly Ward** | Had to edit the existing Ward class and the WardType enum. | Wrote one new class and one line to register it. No existing code was touched. | **B** |
| **C1: first attempt** | An 80 year old was sent to a General Ward. Only a test caught it. | The same mistake, caught the same way. | **Neither** |
| **C2: add a Maternity Ward (needs new patient data)** | Adding the new data took the same work in both versions. The new rules all went into one file, Ward.cs. | Adding the new data took the same work in both versions. The new rules touched four ward classes, because three other wards had to exclude pregnant patients. | **A** |
| **C3: raise the Children's Ward limit to 18** | One file. All the rules are in one method. | Two ward classes had to change together. | **A** |
| **C4: add a ward but forget its rule** | Built with no warnings. The new ward quietly admitted nobody. | Would not build. The compiler named both missing methods. | **B** |

In short, giving each ward its own class was better when something new was added, the single class was better when related rules had to change together, and neither design stopped a rule from being wrong.

### Detailed numbers

Files with code changes, and lines of code added and removed (comments and blank lines are not counted):

| Step | Version A | Version B |
|---|---|---|
| C1 Add Elderly Ward | 3 files, +7 / -1 | 2 files, +14 / -0 |
| C1 Fix overlap with General Ward | 1 file, +2 / -2 | 1 file, +2 / -2 |
| C1 Ward map layout | 1 file, +12 / -3 | 1 file, +12 / -3 |
| C2 Pregnancy data | 3 files, +30 / -6 | 3 files, +30 / -6 |
| C2 Maternity rules | 3 files, +10 / -4 | 5 files, +17 / -3 |
| C3 Children under 18 | 1 file, +4 / -4 | 2 files, +4 / -4 |

These numbers are small, so they are best read together with the table above. They are also in [`results/measurements.csv`](results/measurements.csv), and the C4 compiler output is in [`results/C4_forgotten_rule_output.txt`](results/C4_forgotten_rule_output.txt). The [results README](results/README.md) explains how to recount them.

## Repository layout

| Folder or file | What it holds |
|---|---|
| `version-B-polymorphic/` | Version B, the abstract class design |
| `version-A-flat/` | Version A, the enum and if/else design |
| `tests/inputs/` | Ten scripted test runs. Each line is one answer typed into the menu |
| `tests/expected/` | The output each test should print, with clock times replaced by `<TIME>` |
| `results/` | The measurements and the C4 compiler output |
| `docs/` | Class diagrams of both versions |
| `run_tests.sh` | Runs every test on Mac or Linux |
| `run_tests.ps1` | Runs every test on Windows |

The `tests`, `results` and both version folders each have their own README with more detail.

## How to follow the work

The commit history shows the steps in the order I did them. Start from the oldest commit:

1. Step 0: the program as it was before the research started
2. Step 1: three small fixes, then the scripted tests and their expected output
3. Step 2: Version A built, with identical output to Version B on every test
4. C1: the Elderly Ward. The first attempt failed test T7 in both versions, and the next commits show the fix
5. C2: the Maternity Ward
6. C3: the Children's Ward age limit
7. C4: on the separate branch `c4-forgotten-rule`, so the broken code does not stay in the main program

Each change has one commit for Version B and one for Version A, so `git show <commit>` shows exactly what each design needed.

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

This is one small program and one developer, and lines of code are only a rough measure of effort. I built Version A myself, so I kept its rules inside the `Ward` class to make it as fair a comparison as I could. The ward map was checked by hand because the scripted tests cannot see a window.
