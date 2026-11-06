# Time Crunch - setup for this slice (accounts, REQ001-005)

1. `dotnet restore` (installs EF Core SQLite, Identity EF, MudBlazor)
2. `dotnet build`
3. `dotnet run` and open the URL it prints (https://localhost:7270 by default)

The SQLite file `timecrunch.db` is created automatically on first run.

## Switching to EF migrations (recommended once the build works)
Until a migration exists the app uses EnsureCreated. To move to migrations:

    dotnet tool install --global dotnet-ef      (once)
    del timecrunch.db*                          (delete the dev database)
    dotnet ef migrations add InitialCreate
    dotnet run                                  (applies the migration)

Add a new migration (`dotnet ef migrations add <Name>`) every time a model changes.

## Try it
- /register -> create an account (signs you in, lands on Overview)
- Profile -> Edit Profile -> change username / email / password
- Log out (bottom of the side menu), then /login

## Slice 2 - tasks and tags (REQ006-012)
- No schema change: the Tasks/Tags tables were already part of slice 1's model, so no new migration is needed.
- Tasks page: /tasks (side menu). Create/Edit use a dialog; status and priority are menus on each row;
  the + under Tags opens the tag popup; delete asks for confirmation.
- Theme: dark by default, with a sun/moon toggle in the top bar (choice is remembered in the browser).
  Colors live in Components/Layout/AppTheme.cs. Login and Create Account are static pages with a matching fixed dark style.

## Slice 3 - task ranking, Overview, and goals (REQ013-016)

### IMPORTANT: reset the dev database once
The Tasks table changed shape (new Difficulty column, Priority now optional, Due date now required),
and goals use existing tables. EnsureCreated does not alter an existing database, so delete the old one:

    del timecrunch.db*          (stop the app first; this removes your test accounts)
    dotnet run

If you already created EF migrations, delete the Migrations folder as well, then run
`dotnet ef migrations add InitialCreate` again before `dotnet run`.

### How tasks are ranked (one place: Services/TaskPrioritizer.cs)
1. Due date, soonest first (required on every task)
2. Priority, High then Medium then Low (a task with no priority ranks after tasks that have one)
3. Difficulty, Hard then Medium then Easy (no difficulty ranks last)
4. Title, only to keep the order stable

So with no priority or difficulty entered, tasks simply rank by due date. The Tasks page lists open tasks in
this order (completed tasks last) and the Overview shows the top 5 open tasks in the same order.

### Quick-edit dropdowns
Priority, Difficulty and Status on each task row (and Status on goals) are native <select> controls.
Choosing a value saves immediately; Priority and Difficulty have a "None" choice.

### Goals
- /goals: goal cards with type, status dropdown, due date, task progress and (quantitative) target progress.
- /goals/{id}: details, "Update progress" for quantitative goals, Associated Tasks, Attach Task, remove a task from the goal.
- Quantitative goals need a target greater than 0. Every goal shows "X of Y attached tasks completed".

## Running the tests

The `TimeCrunch.Tests` project uses NUnit 4 and NSubstitute. Unit tests (`Unit/`) use NSubstitute for the database factory. Integration tests (`Integration/`) repeat the same scenarios against a real in-memory SQLite database, plus the behaviour that needs a database, so nothing touches your real data. Every test is written Arrange / Act / Assert.

```
dotnet test                      # run everything
./run-tests.sh                   # tests + coverage report (Windows: .\run-tests.ps1)
```

Coverage is scoped to `TimeCrunch.Models.*` and `TimeCrunch.Services.*` by `coverlet.runsettings`. Install the report tool once with `dotnet tool install --global dotnet-reportgenerator-globaltool`. The report lands in `TestResults/coverage-report/index.html`.

`python3 tools/static_metrics.py` prints static counts (tests, lines of code). These are not coverage figures.
