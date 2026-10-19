using System.Collections;
using EmberBlade.Network;
using TMPro;
using UnityEngine;

namespace EmberBlade.UI
{
    public class HudController : MonoBehaviour
    {
        [SerializeField] private TMP_Text threatText;
        [SerializeField] private GameObject alertBanner;
        [SerializeField] private TMP_Text alertText;

        private Coroutine hideAlertCoroutine;

        private void OnEnable()
        {
            if (NetworkManager.Instance == null)
            {
                Debug.LogWarning("HudController requires an active NetworkManager.");
                return;
            }

            NetworkManager.Instance.WarStateDeltaReceived += UpdateThreat;
            NetworkManager.Instance.RaidAlertReceived += ShowRaidAlert;
            alertBanner.SetActive(false);
        }

        private void OnDisable()
        {
            if (NetworkManager.Instance == null)
            {
                return;
            }

            NetworkManager.Instance.WarStateDeltaReceived -= UpdateThreat;
            NetworkManager.Instance.RaidAlertReceived -= ShowRaidAlert;
        }

        private void UpdateThreat(NetworkManager.WarStateDelta state)
        {
            threatText.text = $"Emberfield threat: {state.value}";
        }

        private void ShowRaidAlert(NetworkManager.RaidAlert alert)
        {
            alertText.text = alert.message;
            alertBanner.SetActive(true);
            if (hideAlertCoroutine != null)
            {
                StopCoroutine(hideAlertCoroutine);
            }

            hideAlertCoroutine = StartCoroutine(HideAlertAfterDelay());
        }

        private IEnumerator HideAlertAfterDelay()
        {
            yield return new WaitForSeconds(5f);
            alertBanner.SetActive(false);
            hideAlertCoroutine = null;
        }
    }
}
