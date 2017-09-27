using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HurtFlash : MonoBehaviour {

	[Range(0f, 1f)]
	public float flashSpeed;
	[Range(0f, 1f)]
	public float fadeSpeed;
	public float waitTime = 3f;

	private Image image;

	private float alpha = 0;

	private IEnumerator fadeCouroutine;
	// Use this for initialization
	void Start () {
		image = GetComponent<Image> ();
		fadeCouroutine = Fade (waitTime);
	}
	
	// Update is called once per frame
	void Update () {
		Color c = image.color;
		if (alpha > 1f) {
			alpha = 1f;
		} else if (alpha < 0f) {
			alpha = 0f;
		}
		c.a = alpha;
		image.color = c;	

		if (Input.GetKeyDown (KeyCode.D)) {
			StopAllCoroutines ();
			StartCoroutine ("Flash");
		}
	}

	IEnumerator Flash() {
		Debug.Log ("Flashing!");
		for (; alpha < 1f; alpha += flashSpeed) {
			yield return null;
		}

		fadeCouroutine = Fade (waitTime);
		StartCoroutine (fadeCouroutine);
	}

	IEnumerator Fade(float waitTime) {
		Debug.Log ("Waiting!");
		yield return new WaitForSeconds (waitTime);

		Debug.Log ("Fading!");
		for (; alpha > 0f; alpha -= fadeSpeed) {
			yield return null;
		}
		Debug.Log ("Done!");

	}
}