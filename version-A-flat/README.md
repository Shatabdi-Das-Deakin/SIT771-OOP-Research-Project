# The conditional design

This version does exactly the same job as the inheritance-based design and prints exactly the same output on every test. I built it for the research so the two designs could be compared fairly.

There are no ward subclasses. There is one `Ward` class, and each ward object stores a `WardType` value:

```
ICU, Children, General, Elderly, Maternity
```

`CanAdmit` and `GetAdmissionRule` use if/else statements on that value to choose the rule. Everything else, including `Hospital`, the menu and the ward map, is the same as the inheritance-based design.

The main differences I found:

* All the rules are in one method, so changing rules that depend on each other only touches `Ward.cs`.
* Adding a new kind of ward means editing the existing `Ward` class and the `WardType` enum.
* If a new ward type is added without its rules, the program still builds with no warning, and that ward accepts nobody.

## Running it

```
dotnet run
```

The menu is identical to the inheritance-based design. Option 9 loads demo patients and option 8 opens the SplashKit ward map.
