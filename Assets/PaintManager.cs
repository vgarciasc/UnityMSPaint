using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;

public class PaintManager : MonoBehaviour {
	public enum Tools { NONE, BRUSH, ERASER };

	[SerializeField]
	Renderer canvasRenderer;

	Texture2D texture;
	int height = 100;
	int width = 100;

	public Tools currentTool { get; internal set; }
	public int currentSize { get; internal set; }

	int minSize = 1;
	int maxSize = 5;

	Vector2 last_point;
	bool should_use_last_point = false;

	void Start () {
		currentSize = 3;
		currentTool = Tools.NONE;

		texture = new Texture2D(100, 100);
		texture.filterMode = FilterMode.Point;
		canvasRenderer.sharedMaterial.mainTexture = texture;

		ClearTexture();
	}

	void Update () {
		if (texture != null) {
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			RaycastHit hit;

			Color currentColor = Color.clear;
			switch (currentTool) {
				case Tools.BRUSH:
					currentColor = Color.black;
					break;
				case Tools.ERASER:
					currentColor = Color.white;
					break;
			}

			if (Input.GetButton("Fire1")) {
				if (Physics.Raycast (ray, out hit, Mathf.Infinity))	{
					Vector2 uv;
					uv.x = (hit.point.x - hit.collider.bounds.min.x) / hit.collider.bounds.size.x;
					uv.y = - (hit.point.y - hit.collider.bounds.min.y) / hit.collider.bounds.size.y;

					if (should_use_last_point) {
						var between = new List<Vector2>();
						if (last_point.x < uv.x) {
							between = PointsBetween(texture, uv, last_point);
						}
						else {
							between = PointsBetween(texture, last_point, uv);
						}

						foreach (Vector2 pt in between) {
							PaintPixel(texture, pt.x, pt.y, currentColor);
						}
					}

					last_point = uv;
				}

				should_use_last_point = true;
			}

			if (Input.GetButtonUp("Fire1")) {
				should_use_last_point = false;
			}

			if (Input.GetKey(KeyCode.C)) {
				ClearTexture();
			}
		}
	}

	void PaintPixel(Texture2D texture, float x, float y, Color color) {
		switch (currentSize) {
			case 1:
				SafePaint(texture, (int) x, (int) y, color);
				break;
			case 2:
				SafePaint(texture, (int) x, (int) y, color);
				SafePaint(texture, (int) x, (int) y + 1, color);
				SafePaint(texture, (int) x - 1, (int) y, color);
				SafePaint(texture, (int) x - 1, (int) y + 1, color);
				break;
			case 3:
				SafePaint(texture, (int) x, (int) y, color);
				SafePaint(texture, (int) x, (int) y + 1, color);
				SafePaint(texture, (int) x, (int) y - 1, color);
				SafePaint(texture, (int) x + 1, (int) y, color);
				SafePaint(texture, (int) x - 1, (int) y, color);
				break;
			case 4:
				SafePaint(texture, (int) x, (int) y - 1, color);
				SafePaint(texture, (int) x, (int) y, color);
				SafePaint(texture, (int) x, (int) y + 1, color);
				SafePaint(texture, (int) x - 1, (int) y - 1, color);
				SafePaint(texture, (int) x - 1, (int) y, color);
				SafePaint(texture, (int) x - 1, (int) y + 1, color);
				SafePaint(texture, (int) x + 1, (int) y - 1, color);
				SafePaint(texture, (int) x + 1, (int) y, color);
				SafePaint(texture, (int) x + 1, (int) y + 1, color);
				break;
			case 5:
				SafePaint(texture, (int) x + 1, (int) y, color);
				SafePaint(texture, (int) x + 1, (int) y + 1, color);
				SafePaint(texture, (int) x, (int) y - 1, color);
				SafePaint(texture, (int) x, (int) y, color);
				SafePaint(texture, (int) x, (int) y + 1, color);
				SafePaint(texture, (int) x, (int) y + 2, color);
				SafePaint(texture, (int) x - 1, (int) y - 1, color);
				SafePaint(texture, (int) x - 1, (int) y, color);
				SafePaint(texture, (int) x - 1, (int) y + 1, color);
				SafePaint(texture, (int) x - 1, (int) y + 2, color);
				SafePaint(texture, (int) x - 2, (int) y, color);
				SafePaint(texture, (int) x - 2, (int) y + 1, color);
				break;
		}

		texture.Apply();
	}

	void SafePaint(Texture2D texture, int x, int y, Color color) {
		if (x < 0 && x > -width && y < 0 && y > -height) {
			texture.SetPixel(x, y, color);
		}	
	}

	List<Vector2> PointsBetween(Texture2D tex, Vector2 point_1, Vector2 point_2) {
		List<Vector2> output = new List<Vector2>();
		point_1 = new Vector2(- point_1.x * tex.width, point_1.y * tex.height);
		point_2 = new Vector2(- point_2.x * tex.width, point_2.y * tex.height);

		float delta_x = point_2.x - point_1.x;
		float delta_y = point_2.y - point_1.y;
		float delta_err = Mathf.Abs(delta_y / delta_x);
		float error = 0f;
		int y = (int) point_1.y;

		if ((int) point_1.x == (int) point_2.x) {
			int aux1, aux2;
			if (point_1.y < point_2.y) {
				aux1 = (int) point_1.y;
				aux2 = (int) point_2.y;
			}
			else {
				aux2 = (int) point_1.y;
				aux1 = (int) point_2.y;
			}

			for (int i = aux1; i < aux2; i++) { 
				Vector2 vec = new Vector2(point_1.x, i);
				output.Add(vec);
			}

			return output;
		}

		for (int x = (int) point_1.x; x < point_2.x; x++) {
			Vector2 vec = new Vector2(x, y);
			output.Add(vec);
			error += delta_err;
			if (error >= 0.5f) {
				if (point_1.y < point_2.y) {
					y = y + 1;
				}
				else {
					y = y - 1;
				}

				error -= 1;
			}
		}

		return output;
	}

	void SetTool(Tools tool) {
		currentTool = tool;
	}

	public void SetBrush() {
		SetTool(Tools.BRUSH);
	}

	public void SetEraser() {
		SetTool(Tools.ERASER);
	}

	public void ClearTexture() {
		should_use_last_point = false;

		for (int i = 0; i < width; i++) {
			for (int j = 0; j < height; j++) {
				texture.SetPixel(i, j, Color.white);
			}
		}

		texture.Apply();
	}

	public void AddBrushSize(int amount) {
		currentSize = Mathf.Clamp(currentSize + amount, minSize, maxSize);
	}
}