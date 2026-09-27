# SIT771 Task 7.4 (HD): a small research project

**Question:** How does designing with abstraction, inheritance and polymorphism affect how easily a program can be extended and tested, compared with a design built on an enum and if/else statements?

**Student:** Shatabdi Das (s226086986), SIT771 Object-Oriented Development, Deakin University

## What is in this repository

| Folder or file | What it is |
|---|---|
| `version-B-polymorphic/` | My Patient Management program from Task 7.3D: an abstract `Ward` class with one subclass per kind of ward |
| `version-A-flat/` | The same program rebuilt with one `Ward` class, a `WardType` enum and if/else rules |
| `tests/inputs/` | Ten scripted test runs. Each line is something typed into the menu |
| `tests/expected/` | The output each test should produce (clock times replaced with `<TIME>`) |
| `results/measurements.csv` | Files and lines of code changed by each change, for both versions |
| `results/C4_forgotten_rule_output.txt` | Compiler and program output from the "forgotten rule" experiment |
| `docs/` | Class diagrams of both versions (also in [Lucidchart](https://lucid.app/lucidchart/c43ebfbf-2a3f-425a-a625-0f350c25f852/edit?invitationId=inv_e427f615-a624-44fc-bdd2-e396cd70c8ab), view only) |
| `run_tests.sh`, `run_tests.ps1` | Build one version and run every test (Mac/Linux and Windows) |

## How the research was done

The commit history is the record of the process. Read it from the oldest commit:

1. **Step 0:** the program exactly as submitted for Task 7.3D
2. **Step 1:** three small fixes, then ten scripted tests with their expected output
3. **Step 2:** Version A built; both versions give identical output on every test
4. **C1:** add an Elderly Ward. The first attempt failed test T7 in both versions (an 80 year old went to a General Ward), which led to a fix
5. **C2:** add a Maternity Ward, which needed new patient data
6. **C3:** change the Children's Ward age limit from under 16 to under 18
7. **C4:** add a ward type and "forget" its rules. This is on the branch `c4-forgotten-rule` so it does not break the main code

Every change was made to Version B and Version A in separate commits, so `git show <commit>` shows exactly what each design needed.

## Running it

You need the .NET 8 SDK. From this folder:

```
./run_tests.sh version-B-polymorphic      # Mac or Linux
./run_tests.sh version-A-flat

.\run_tests.ps1 version-B-polymorphic     # Windows PowerShell
.\run_tests.ps1 version-A-flat
```

Both versions should print `10 passed, 0 failed`.

To use the program itself: `cd version-B-polymorphic` then `dotnet run`. Option 9 loads demo patients and option 8 opens the SplashKit ward map.
