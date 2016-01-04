#Suave 1.0 + Angular 2.0-beta demo project

## How to run

Execute `run.cmd` script that:

- Restore `paket.exe`
- Restore `Suave` from `NuGet`
- Restore `Angular2` with dependencies using `NPM`
- Runs two parallel node processes
    - The `TypeScript` compiler in watch mode
    - The `Suave` server (`run.fsx`) that startWebServer on port `8083` and should open your browser.