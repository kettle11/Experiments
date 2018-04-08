using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class world2 : MonoBehaviour {
	public float[,] heightMap;
	public Texture water;

	public int size = 512;
	public float[] octaveWeights;
	public float baseOctaveValue = 0.08f;
	public float scale = .5f;

	public float falloffRadius = 100;

	// Use this for initialization
	void Start () {
	}

	void Awake() {
		CreateHeightmap();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void CreateHeightmap()
	{
		water = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat);

	}

/* 
	void CreateCompositeTexture() {

		Texture newTexture = new Texture2D(size, size);

		float middle = size / 2f;

		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				float valueHere = 1f - (new Vector2(Mathf.Abs(i - middle), Mathf.Abs(j - middle)).magnitude / (falloffRadius / scale));

				valueHere = Mathf.Clamp(valueHere, 0, 1.0f);

				float value =0;
				float octave = baseOctaveValue;
				for (int k = 0; k < octaveWeights.Length; k++)
				{
					value += octaveWeights[k]*Mathf.PerlinNoise((i + 100) * octave * scale, (j + 100) * octave * scale);
					octave *= 2;
				}

				float finalValue = Mathf.Clamp(valueHere * value * 2f, 0, 1.0f);

				if (finalValue < .1f) {
					heightMap.SetPixel(i,j, Color.blue);
				} else {
					heightMap.SetPixel(i,j, new Color(0, finalValue,0, 1));
				}
			}
		}

		newTexture.Apply();
		Graphics.Blit(newTexture, heightMap);
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", heightMap);
	}
*/
}
