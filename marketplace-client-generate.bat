if not exist "%appdata%\npm\node_modules\autorest" "cmd /c npm install --global autorest"

autorest ./marketplace-client-autorest-config.md --csharp
