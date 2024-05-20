using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HillPointBar : MonoBehaviour
{
    private Transform cameraTransform;

    public Slider TopTeamSlider;
    public Slider BottomTeamSlider;

    public GameObject EXPPrefab;
    public GameObject GoldPrefab;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        BottomTeamSlider.interactable = false;
        TopTeamSlider.interactable = false;
    }

    public void SetMaxSlider(string team, float value)
    {
        if (team == "Top")
        {
            TopTeamSlider.maxValue = value;
        }
        else if (team == "Bottom")
        {
            BottomTeamSlider.maxValue = value;
        }
    }

    public void UpdateSlider(string team, float value)
    {
        if (team == "Top")
        {
            TopTeamSlider.value = value;
        }
        else if (team == "Bottom")
        {
            BottomTeamSlider.value = value;
        }
    }

    private void ShowPopup(GameObject prefab, int amount, string type)
    {
        GameObject popup = Instantiate(prefab, transform.position, Quaternion.identity, transform);
        popup.layer = gameObject.layer;

        TMP_Text popupText = popup.GetComponent<TMP_Text>();
        popupText.text = $"{type} + {amount}";

        StartCoroutine(AnimatePopup(popup));
    }

    public void ShowEXPPopup(int expAmount)
    {
        ShowPopup(EXPPrefab, expAmount, "EXP");
    }

    public void ShowGoldPopup(int goldAmount)
    {
        ShowPopup(GoldPrefab, goldAmount, "Gold");
    }

    private IEnumerator AnimatePopup(GameObject popup)
    {
        float duration = 1f;
        Vector3 startPosition = popup.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, 1f, 0);

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            popup.transform.position = Vector3.Lerp(startPosition, endPosition, t / duration);
            popup.transform.LookAt(popup.transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
            yield return null;
        }

        Destroy(popup);
    }

    private void Update()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            TopTeamSlider.transform.LookAt(TopTeamSlider.transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
            BottomTeamSlider.transform.LookAt(BottomTeamSlider.transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
        }
    }
}