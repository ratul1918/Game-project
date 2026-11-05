using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TheLightHouse
{
    public sealed class AxeCombat : MonoBehaviour
    {
        [Header("Weapon Stats")]
        [SerializeField] private float lightDamage = 25f;
        [SerializeField] private float heavyDamage = 55f;
        [SerializeField] private float shoveDamage = 10f;
        [SerializeField] private float attackRange = 1.85f;
        [SerializeField] private float attackRadius = 0.35f;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Stamina Costs")]
        [SerializeField] private float lightStaminaCost = 15f;
        [SerializeField] private float heavyStaminaCost = 35f;
        [SerializeField] private float shoveStaminaCost = 20f;
        [SerializeField] private float blockStaminaCostPerHit = 15f;

        [Header("Durability")]
        [SerializeField] private WeaponDurability durability;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private FirstPersonArms arms;
        [SerializeField] private PlayerStamina stamina;
        [SerializeField] private NoiseEmitter noiseEmitter;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip swingSound;
        [SerializeField] private AudioClip heavySwingSound;
        [SerializeField] private AudioClip fleshImpactSound;
        [SerializeField] private AudioClip woodImpactSound;
        [SerializeField] private AudioClip blockImpactSound;

        private AudioSource audioSource;
        private GameObject axeModel;
        private bool isAttacking;
        private bool isBlocking;
        private float attackHoldTimer;
        private bool isHoldingAttack;
        private readonly RaycastHit[] hitResults = new RaycastHit[8];

        public bool IsBlocking => isBlocking;
        public bool IsEquipped { get; private set; } = true;
        public WeaponDurability Durability => durability;

        private void Awake()
        {
            if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
            if (!arms) arms = GetComponentInChildren<FirstPersonArms>();
            if (!stamina) stamina = GetComponent<PlayerStamina>();
            if (!noiseEmitter) noiseEmitter = GetComponent<NoiseEmitter>();
            if (!durability) durability = GetComponent<WeaponDurability>() ?? gameObject.AddComponent<WeaponDurability>();

            audioSource = GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0.5f;

            BuildAxeModel();
        }

        private void BuildAxeModel()
        {
            if (axeModel) return;

            Transform mount = arms && arms.RightHandSocket ? arms.RightHandSocket : (cameraTransform ? cameraTransform : transform);
            axeModel = new GameObject("FireAxe_Model");
            axeModel.transform.SetParent(mount, false);
            axeModel.transform.localPosition = new Vector3(0.04f, 0.05f, 0.12f);
            axeModel.transform.localRotation = Quaternion.Euler(15f, -10f, 45f);

            // Handle (Hickory Wood)
            var handle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            handle.name = "Axe_Handle";
            handle.transform.SetParent(axeModel.transform, false);
            handle.transform.localPosition = new Vector3(0f, -0.1f, 0f);
            handle.transform.localScale = new Vector3(0.035f, 0.42f, 0.035f);
            DestroyImmediate(handle.GetComponent<Collider>());
            var woodMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            woodMat.color = new Color(0.42f, 0.28f, 0.18f);
            handle.GetComponent<Renderer>().sharedMaterial = woodMat;

            // Axe Head (Red Iron)
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.name = "Axe_Head";
            head.transform.SetParent(axeModel.transform, false);
            head.transform.localPosition = new Vector3(0.055f, 0.24f, 0f);
            head.transform.localScale = new Vector3(0.12f, 0.12f, 0.032f);
            DestroyImmediate(head.GetComponent<Collider>());
            var headMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            headMat.color = new Color(0.65f, 0.15f, 0.12f); // Red maritime fire axe
            head.GetComponent<Renderer>().sharedMaterial = headMat;

            // Cutting Edge
            var blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.name = "Axe_BladeEdge";
            blade.transform.SetParent(head.transform, false);
            blade.transform.localPosition = new Vector3(0.55f, 0f, 0f);
            blade.transform.localScale = new Vector3(0.2f, 1.15f, 0.5f);
            DestroyImmediate(blade.GetComponent<Collider>());
            var bladeMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            bladeMat.color = new Color(0.85f, 0.88f, 0.9f); // Steel edge
            blade.GetComponent<Renderer>().sharedMaterial = bladeMat;
        }

        private void Update()
        {
            if (!IsEquipped || isAttacking) return;

            var mouse = Mouse.current;
            var keyboard = Keyboard.current;
            if (mouse == null) return;

            // Block handling (Right click)
            if (mouse.rightButton.isPressed)
            {
                if (!isBlocking)
                {
                    isBlocking = true;
                    if (arms) arms.AnimateBlock(true);
                }
                return;
            }
            else if (isBlocking)
            {
                isBlocking = false;
                if (arms) arms.AnimateBlock(false);
            }

            // Shove handling ('F' key)
            if (keyboard != null && keyboard.fKey.wasPressedThisFrame)
            {
                if (CanAffordStamina(shoveStaminaCost))
                {
                    StartCoroutine(ExecuteShove());
                    return;
                }
            }

            // Attack handling (Left click - tap for light, hold for heavy)
            if (mouse.leftButton.wasPressedThisFrame)
            {
                isHoldingAttack = true;
                attackHoldTimer = 0f;
            }

            if (isHoldingAttack)
            {
                attackHoldTimer += Time.deltaTime;
                if (mouse.leftButton.wasReleasedThisFrame)
                {
                    isHoldingAttack = false;
                    if (attackHoldTimer >= 0.35f)
                    {
                        if (CanAffordStamina(heavyStaminaCost))
                            StartCoroutine(ExecuteHeavySwing());
                    }
                    else
                    {
                        if (CanAffordStamina(lightStaminaCost))
                            StartCoroutine(ExecuteLightSwing());
                    }
                }
            }
        }

        private bool CanAffordStamina(float cost)
        {
            return stamina == null || stamina.Current >= cost;
        }

        private IEnumerator ExecuteLightSwing()
        {
            isAttacking = true;
            ConsumeStamina(lightStaminaCost);
            EmitNoise(0.4f);

            if (swingSound && audioSource) audioSource.PlayOneShot(swingSound, 0.8f);

            float duration = 0.38f;
            float elapsed = 0f;
            bool hitDetected = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                if (arms) arms.AnimateLightSwing(progress);

                if (!hitDetected && progress >= 0.42f)
                {
                    hitDetected = true;
                    PerformHitCheck(lightDamage * (durability ? durability.DamageMultiplier : 1f), false);
                }
                yield return null;
            }

            if (arms) arms.AnimateLightSwing(0f);
            isAttacking = false;
        }

        private IEnumerator ExecuteHeavySwing()
        {
            isAttacking = true;
            ConsumeStamina(heavyStaminaCost);
            EmitNoise(0.7f);

            if (heavySwingSound && audioSource) audioSource.PlayOneShot(heavySwingSound, 1f);

            float duration = 0.72f;
            float elapsed = 0f;
            bool hitDetected = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                if (arms) arms.AnimateHeavySwing(progress);

                if (!hitDetected && progress >= 0.48f)
                {
                    hitDetected = true;
                    PerformHitCheck(heavyDamage * (durability ? durability.DamageMultiplier : 1f), true);
                }
                yield return null;
            }

            if (arms) arms.AnimateHeavySwing(0f);
            isAttacking = false;
        }

        private IEnumerator ExecuteShove()
        {
            isAttacking = true;
            ConsumeStamina(shoveStaminaCost);
            EmitNoise(0.3f);

            float duration = 0.32f;
            float elapsed = 0f;
            bool hitDetected = false;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                if (arms) arms.AnimateShove(progress);

                if (!hitDetected && progress >= 0.35f)
                {
                    hitDetected = true;
                    PerformHitCheck(shoveDamage, true); // Shove staggers
                }
                yield return null;
            }

            if (arms) arms.AnimateShove(0f);
            isAttacking = false;
        }

        private void PerformHitCheck(float damage, bool isHeavy)
        {
            if (!cameraTransform) return;

            Vector3 origin = cameraTransform.position;
            Vector3 direction = cameraTransform.forward;
            int count = Physics.SphereCastNonAlloc(origin, attackRadius, direction, hitResults, attackRange, hitLayers, QueryTriggerInteraction.Ignore);

            for (int i = 0; i < count; i++)
            {
                var hit = hitResults[i];
                if (hit.collider.transform.IsChildOf(transform)) continue;

                // IDamageable targets
                var damageable = hit.collider.GetComponentInParent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeHit(damage, hit.point, direction, isHeavy);
                    if (durability) durability.Consume(1);
                    PlayImpactAudio(fleshImpactSound);
                    EmitNoise(0.9f); // Impact makes loud noise
                    return;
                }

                // Stalker AI reaction
                var stalker = hit.collider.GetComponentInParent<StalkerAI>();
                if (stalker != null)
                {
                    // Stalker stagger & retreat
                    if (durability) durability.Consume(1);
                    PlayImpactAudio(fleshImpactSound);
                    EmitNoise(1.0f);
                    return;
                }

                // Drowned Figure reaction
                var drowned = hit.collider.GetComponentInParent<DrownedFigure>();
                if (drowned != null)
                {
                    drowned.StepBack();
                    if (durability) durability.Consume(1);
                    PlayImpactAudio(fleshImpactSound);
                    return;
                }

                // Environment impact
                if (!hit.collider.isTrigger)
                {
                    PlayImpactAudio(woodImpactSound);
                    EmitNoise(0.6f);
                    return;
                }
            }
        }

        private void PlayImpactAudio(AudioClip clip)
        {
            if (clip && audioSource)
                audioSource.PlayOneShot(clip, 0.9f);
        }

        private void ConsumeStamina(float cost)
        {
            if (stamina) stamina.Spend(cost, 1.0f);
        }

        private void EmitNoise(float intensity)
        {
            if (noiseEmitter) noiseEmitter.Emit(intensity);
        }

        public bool TryBlock(float incomingDamage, out float reducedDamage)
        {
            if (isBlocking && CanAffordStamina(blockStaminaCostPerHit))
            {
                ConsumeStamina(blockStaminaCostPerHit);
                PlayImpactAudio(blockImpactSound);
                reducedDamage = incomingDamage * 0.3f; // 70% damage reduction
                return true;
            }

            reducedDamage = incomingDamage;
            return false;
        }
    }
}
