using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class WeatherWarningUI : MonoBehaviour
{
	[Header("UI References")]
	public GameObject panel;
	public TextMeshProUGUI text;
	public float displayDuration = 2.5f;
	public float animationSpeed = 3f;

	Coroutine _routine;

	void Start()
	{
		if (panel != null) panel.SetActive(false);
	}

	public void Show(string message)
	{
		if (panel == null || text == null) return;
		if (_routine != null) StopCoroutine(_routine);
		text.text = message;
		panel.SetActive(true);
		_routine = StartCoroutine(ShowRoutine());
	}

	IEnumerator ShowRoutine()
	{
		CanvasGroup cg = panel.GetComponent<CanvasGroup>();
		if (cg == null) cg = panel.AddComponent<CanvasGroup>();

		float a = 0f;
		while (a < 1f)
		{
			a += Time.deltaTime * animationSpeed;
			cg.alpha = a;
			yield return null;
		}
		cg.alpha = 1f;

		yield return new WaitForSeconds(displayDuration);

		while (a > 0f)
		{
			a -= Time.deltaTime * animationSpeed;
			cg.alpha = a;
			yield return null;
		}
		cg.alpha = 0f;
		panel.SetActive(false);
		_routine = null;
	}
}



