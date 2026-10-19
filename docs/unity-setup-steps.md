# Ember Blade Unity 6.6 setup

These steps create the thin-slice client scene. The scripts are in
`client/Assets/Scripts/`; copy that `Assets/Scripts` folder into the Unity
project's `Assets/` folder before attaching scripts.

1. Open Unity Hub and create a new **3D (URP)** project using Unity **6.6**.
2. Open **Window > Package Manager**, choose **+ > Add package from git URL**,
   enter `https://github.com/endel/NativeWebSocket.git#upm`, and click Add.
3. In Package Manager, install/import **Starter Assets - ThirdPerson** from the
   Unity Registry or the provided package source. Import its input actions when
   prompted.
4. Copy `client/Assets/Scripts/Network` and `client/Assets/Scripts/UI` into the
   project's `Assets/Scripts` folder. Wait for Unity to finish compiling.
5. Open the sample scene or create a scene with **GameObject > 3D Object >
   Plane** and a camera. Starter Assets' Third Person Controller is optional
   scene movement only; it is not required by the network slice.
6. Create an empty GameObject named `NetworkManager`, add
   `NetworkManager.cs`, and leave `Websocket Url` as
   `ws://localhost:8080/ws`.
7. Create an empty GameObject named `LoginManager`, add `LoginManager.cs`, and
   leave `Login Url` as `http://localhost:8080/api/auth/login`.
8. Create a **Canvas** in **Screen Space - Overlay** mode. Under it create a
   `LoginPanel` with TMP text labels, two **TMP Input Field** objects named
   `UsernameInput` and `PasswordInput` (set password content type on the
   latter), a **Button** named `LoginButton`, and a TMP text named
   `LoginStatus`.
9. Add `LoginUI.cs` to `LoginPanel`. Assign its `Username Input`,
   `Password Input`, `Login Button`, `Status Text`, and `Login Panel` fields.
   Assign the `LoginManager` and `NetworkManager` scene objects to the last
   two fields.
10. Under the Canvas create an empty UI object named `Hud`. Add
    `HudController.cs`. Add a TMP text named `ThreatText` and a red UI Panel
    named `AlertBanner` with a child TMP text named `AlertText`.
11. Assign `ThreatText`, `AlertBanner`, and `AlertText` to the matching
    `HudController` fields. Disable `AlertBanner` in the Inspector so it starts
    hidden. Set its Image color to red and its text color to white.
12. Ensure both manager objects and the Canvas/HUD are active, then add the
    scene to **File > Build Profiles > Platforms > Scene List** if building.
13. Start the server from the repository root with `docker compose -f
    server/docker-compose.yml up -d`, then start the Spring Boot app with
    `mvn -f server/pom.xml spring-boot:run`.
14. Register a user using the REST endpoint (see the checklist in the final
    response), enter those credentials in Unity, and press Play.

At runtime, login hides `LoginPanel`, opens the authenticated WebSocket, updates
`Emberfield threat: <value>` every second, and shows the red raid banner for
five seconds when the server first crosses the raid threshold.
