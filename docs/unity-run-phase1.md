# Run Ember Blade Phase 1

Automated setup is complete. The generated scene is
`client/Assets/Scenes/Phase1.unity`, and it is already in the Unity Build
Settings.

1. Open the `client/` folder in Unity Hub with Unity `6000.6.6f1`.
2. Wait for Unity to finish importing packages and compiling scripts.
3. If the automated Unity run was unavailable on your machine, open the
   `Phase1` scene and choose **Ember Blade > Build Phase 1 Scene**.
4. Start PostgreSQL and the server from the repository root:

   ```bash
   docker compose -f server/docker-compose.yml up -d
   mvn -f server/pom.xml spring-boot:run
   ```

5. Register a test user:

   ```bash
   curl -X POST http://localhost:8080/api/auth/register \
     -H 'Content-Type: application/json' \
     -d '{"username":"player1","password":"emberblade"}'
   ```

   The request field names are `username` and `password`, matching the
   server's `AuthController.Credentials` record.

6. Open `Phase1.unity` if it is not already open and press **Play**.
7. Enter `player1` and `emberblade`, then click **Login**.
8. Confirm the login panel closes, the threat value updates, and the red raid
   banner appears when the server's threat crosses 70.
