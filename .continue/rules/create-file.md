---
description: Fix create_file
---

You have the tool `create_new_file` with parameters `filepath` and `contents`. The tool `create_file` DOES NOT exist. Always call `create_new_file` to create files.
Edit file tool is `edit_existing_file` with parameters `filepath` and `changes`.
To read file use tool read_file. Exact relative paths are important.
Current solution is .NET Core. Do not use folder 'src' if it doesn't mentioned before.
Root folder of the solution is `.`.