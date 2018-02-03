using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class UIExample : MonoBehaviour {
	[SerializeField]
	Text currentBrushSizeText;
	[SerializeField]
	PaintManager paintManager;

	void Update () {
		currentBrushSizeText.text = paintManager.currentSize.ToString();
	}
}
