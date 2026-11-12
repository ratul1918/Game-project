using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace TheLightHouse.Editor
{
    [InitializeOnLoad]
    public static class StairInspection
    {
        static StairInspection()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("StairInspection", false)) return;
                SessionState.SetBool("StairInspection", false);
                Begin(false);
            };
        }

        [MenuItem("TheLightHouse/Validation/Third Person Stair Inspection")]
        public static void Open()
        {
            if (EditorApplication.isPlaying) { Begin(false); return; }
            SessionState.SetBool("StairInspection", true);
            EditorApplication.isPlaying = true;
        }

        public static void Begin(bool automatic)
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play Mode first.");
            foreach (var script in Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None))
                if (script.GetType().Namespace == "TheLightHouse") script.enabled = false;
            foreach (var player in Object.FindObjectsByType<FirstPersonController>(FindObjectsSortMode.None)) player.gameObject.SetActive(false);
            foreach (var camera in Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) camera.enabled = false;
            foreach (var agent in Object.FindObjectsByType<UnityEngine.AI.NavMeshAgent>(FindObjectsSortMode.None)) agent.enabled = false;
            Time.timeScale = 1f;
            var go = new GameObject("Third Person Stair Inspection");
            go.transform.position = Point(3.925f, 210f, .17f);
            var controller = go.AddComponent<StairInspectionController>();
            if (automatic) go.AddComponent<StairTraversalRun>().StartRun(controller);
        }

        public static Vector3 Point(float radius, float angle, float y)
            => new(Mathf.Sin(angle * Mathf.Deg2Rad) * radius, y, Mathf.Cos(angle * Mathf.Deg2Rad) * radius);
    }

    public sealed class StairTraversalRun : MonoBehaviour
    {
        private StairInspectionController motor;
        private readonly List<string> failures = new();
        private int moves, pauses, wallChecks;
        public string Status { get; private set; } = "Not started";
        const float Dt = 1f / 60f;
        public void StartRun(StairInspectionController controller)
        {
            motor = controller;
            motor.Automated = true;
            StartCoroutine(Run());
        }

        private void Place(Vector3 position)
        {
            motor.Capsule.enabled = false;
            transform.position = position + Vector3.up * .03f;
            motor.Capsule.enabled = true;
            Physics.SyncTransforms();
            for (int i = 0; i < 30; i++) motor.Simulate(Vector3.zero, Dt);
        }

        private IEnumerator Walk(Vector3 destination, string label)
        {
            int frames = 0;
            while (true)
            {
                Vector3 delta = destination - transform.position;
                delta.y = 0;
                if (delta.magnitude < .025f) break;
                motor.Simulate(delta.normalized * Mathf.Min(2f, delta.magnitude / Dt), Dt);
                moves++;
                if (++frames > 180)
                {
                    failures.Add(label + " blocked at " + transform.position + ", target " + destination);
                    yield break;
                }
                if (frames % 8 == 0) yield return null;
            }
            if (Mathf.Abs(transform.position.y - destination.y) > .16f)
                failures.Add(label + " wrong height " + transform.position.y + ", expected " + destination.y);
            if (!motor.Grounded) failures.Add(label + " ungrounded");
        }

        private IEnumerator Pause(string label)
        {
            var before = transform.position;
            for (int i = 0; i < 120; i++)
            {
                motor.Simulate(Vector3.zero, Dt);
                if (i % 20 == 0) yield return null;
            }
            pauses++;
            if (Vector3.Distance(before, transform.position) > .08f || !motor.Grounded)
                failures.Add(label + " unstable stop " + Vector3.Distance(before, transform.position));
        }

        private IEnumerator Run()
        {
            yield return null;
            foreach (float radius in new[] {3.65f, 3.925f, 4.25f})
            {
                var route = new List<Vector3>();
                for (int angle = 210; angle <= 900; angle += 2)
                {
                    float y = angle <= 225 ? .15f : angle < 435 ? Mathf.Lerp(.15f, 3.35f, (angle - 225f) / 210f)
                        : angle <= 465 ? 3.35f : angle < 675 ? Mathf.Lerp(3.35f, 6.55f, (angle - 465f) / 210f)
                        : angle <= 705 ? 6.55f : angle < 885 ? Mathf.Lerp(6.55f, 9.75f, (angle - 705f) / 180f) : 9.75f;
                    route.Add(StairInspection.Point(radius, angle, y));
                }
                foreach (bool down in new[] {false, true})
                {
                    if (down) route.Reverse();
                    Status = (down ? "Descending" : "Ascending") + " r=" + radius;
                    Place(route[0]);
                    int failureCount = failures.Count;
                    for (int i = 1; i < route.Count; i++)
                    {
                        yield return Walk(route[i], Status + " sample " + i);
                        if (failures.Count > failureCount) break;
                        if (i % 50 == 0) yield return Pause(Status + " pause " + i);
                    }
                }
            }
            for (int level = 0; level < 3; level++)
            {
                float y = ArchitectureCollisionOverhaul.Levels[level];
                float angle = new[] {130f, 160f, 20f}[level];
                Place(StairInspection.Point(4.1f, angle, y));
                var direction = StairInspection.Point(1, angle, 0);
                for (int i = 0; i < 90; i++)
                {
                    motor.Simulate(direction * 2f, Dt);
                    if (i % 10 == 0) yield return null;
                }
                wallChecks++;
                if (new Vector2(transform.position.x, transform.position.z).magnitude > 4.85f || !motor.Grounded)
                    failures.Add("Wall/floor perimeter failed on Floor " + level + " at " + transform.position);
            }
            Status = $"{SceneManager.GetActiveScene().name}: {(failures.Count == 0 ? "PASS" : "FAIL")}; {moves} Move calls; {pauses} stops; {wallChecks} wall pushes; {failures.Count} failures";
            Directory.CreateDirectory("docs/stair-validation");
            File.WriteAllText("docs/stair-validation/" + SceneManager.GetActiveScene().name + ".txt", Status + "\n" + string.Join("\n", failures));
            Debug.Log(Status);
            motor.Automated = false;
        }
    }
}
