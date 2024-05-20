using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Image imageComponent;
    public string parentTag;

    public bool isSelf = false;

    private Transform cameraTransform;
    public Slider HealthBarSlider;
    public GameObject damagePopupPrefab;
    private List<GameObject> activePopups = new List<GameObject>();

    public TextMeshProUGUI LevelText;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        HealthBarSlider.interactable = false;
        StartCoroutine(WaitForReCheck());
    }

    public void SetMaxSlider(float value)
    {
        HealthBarSlider.maxValue = value;
    }

    public void UpdateSlider(float value)
    {
        float damage = HealthBarSlider.value - value;
        if (damage > 0)
        {
            ShowDamagePopup((int)damage);
        }
        HealthBarSlider.value = value;
    }

    public void UpdateLevel(int value)
    {
        LevelText.text = value.ToString();
    }

    private void ShowDamagePopup(int damageAmount)
    {
        GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity, transform);
        activePopups.Add(popup);

        TMP_Text popupText = popup.GetComponent<TMP_Text>();
        popupText.text = damageAmount.ToString();

        StartCoroutine(AnimatePopup(popup));
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
        activePopups.Remove(popup);
    }

    public IEnumerator WaitForReCheck()
    {
        while (MiniMapMark.RecheckFinish == false)
        {
            yield return null;
        }
        parentTag = transform.parent.gameObject.tag;
        if (isSelf)
        {
            imageComponent.color = Color.yellow;
        }
        else
        {
            imageComponent.color = parentTag == MiniMapMark.localPlayerTag ? Color.green : Color.red;
        }
    }

    private void OnDisable()
    {
        foreach (GameObject popup in activePopups)
        {
            if (popup != null)
            {
                Destroy(popup);
            }
        }
        activePopups.Clear();
    }

    private void Update()
    {
        if (cameraTransform == null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            gameObject.transform.LookAt(gameObject.transform.position + cameraTransform.rotation * Vector3.forward, cameraTransform.rotation * Vector3.up);
        }
    }
}