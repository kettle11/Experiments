using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator2 : MonoBehaviour {

	public Gradient gradient;
	Texture2D texture;
	public int size = 128;

	public float falloffRadius = 200.0f;
	float[,] heights;
	float[,] heightTransfer;

	// Use this for initialization
	void Start () {
		texture = new Texture2D(size, size);
		GetComponent<Renderer>().material.mainTexture = texture;
        texture.Apply();

		Create();
		CreateTexture();
	}
	
	public float[] octaveWeights;
	public float baseOctaveValue = 1.0f;

	
	public float scale = 1.0f;


	void Create()
	{
		heights = new float[size, size];
		water = new float[size,size];
		waterTransfer = new float[size,size];
		heightTransfer = new float[size,size];
		float middle = size / 2f;

		for (int i = 0; i < 100; i++)
		{
			//water[(int)(Random.value * size), (int)(Random.value * size)] = .4f;
		}

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
					value += octaveWeights[k]*Mathf.PerlinNoise(i * octave * scale, j * octave * scale);
					octave *= 2;
				}

				heights[i,j]  = valueHere * value;
			}
		}
	}


	float[,] water;
	float[,] waterTransfer;
	int[] xOffsets = {0,-1,0,1, -1,-1,1,1};
	int[] yOffsets = {-1,0,1,0, -1,1,1,-1};
	
	void Erode()
	{
		for (int i = 0; i < 100; i++)
		{
			heights[(int)(Random.value * size), (int)(Random.value * size)] = 0;
		}
		for (int i = 1; i < size; i++)
		{
			for (int j = 1; j < size; j++)
			{
				//float total = water[]
				for (int k = 0; k < 8; k++)
				{
					//if 
				}
			}
		}
	}

	public bool renderWater = true;

	void CreateTexture()
	{
		for (int x = 0; x < texture.width; x++)
		{
			for (int y = 0; y < texture.height; y++)
			{

				Color color = gradient.Evaluate(heights[x,y]);

				if (renderWater)
				{
					if (water[x,y] > .0001f)
					{
						color = Color.Lerp(color, Color.blue, water[x,y] / .001f);
					}
				}
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
	}

	public float soilHardness = .3f;
	public float soilTransferRate = .1f;

	public float waterFriction = .5f;

	void StepErosion()
	{
            for (int z = 0; z < 10; z++)
            {
                //water[(int)(Random.value * mapSizeX), (int)(Random.value * mapSizeY)] = .2f;
            }
            for (int x1 = 0; x1 < size; x1++)
            {
                for (int y1 = 0; y1 < size; y1++)
                {
                    int x = x1;
                    int y = y1;

                    if (water[x,y] <= 0)
                    {
                        continue;
                    }

					float valueHere = water[x,y] + heights[x,y];
					float waterHere = water[x,y];

					float total = valueHere;
					int averageCount = 1;

					float totalDifference = 0;

                    int leftX = Mathf.Max(0, x - 1);
                    int bottomY = Mathf.Max(0, y - 1);

                    int rightX = Mathf.Min(size-1, x + 1);
                    int maxY = Mathf.Min(size-1, y + 1);

                    float min = float.MaxValue;
                    int minX = 0;
                    int minY = 0;

					for (int i = 0; i < 8; i++)
					{
						int newX = x + xOffsets[i];
						int newY = y + yOffsets[i];

						if (newX >= 0 && newX < size && newY >= 0 && newY < size)
						{
							float heightOffset = heights[newX,newY];
							float waterOffset = water[newX,newY];
							float valueOffset = heightOffset + waterOffset;
							if (valueOffset < valueHere)
							{
								totalDifference += valueHere - valueOffset;

								total += valueOffset;
								averageCount += 1;
							}
						}
					}

					if (totalDifference > waterHere)
					{
						totalDifference += totalDifference - waterHere;
					}

					float totaltransfer = 0;

					for (int i = 0; i < 8; i++)
					{
						int newX = x + xOffsets[i];
						int newY = y + yOffsets[i];

						if (newX >= 0 && newX < size && newY >= 0 && newY < size)
						{
							float valueOffset = heights[newX,newY] + water[newX,newY];

							if (valueOffset < valueHere)
							{
								float waterTransferValue = waterHere * ((valueHere - valueOffset) / totalDifference) * waterFriction;

								waterTransfer[newX,newY] += waterTransferValue;
								waterTransfer[x,y] -= waterTransferValue;

								totaltransfer += waterTransferValue;

								float heightTransferValue = waterTransferValue * soilTransferRate * heights[x,y];
								
								heightTransfer[newX, newY] += heightTransferValue;
								heightTransfer[x,y] -= heightTransferValue;
							}
						}
					}

					waterTotal = 0;
                }
            }

			waterTotal = 0;
			for (int x1 = 0; x1 < size; x1++)
            {
                for (int y1 = 0; y1 < size; y1++)
                {
					water[x1, y1] += waterTransfer[x1,y1];
					heights[x1,y1] += heightTransfer[x1,y1];

					waterTotal += water[x1,y1];

					heightTransfer[x1,y1] = 0;
					waterTransfer[x1,y1] = 0;
				}
			}
			
	}

	public float waterTotal = 0;
	public float rainFallRate = .005f;

	public bool run = false;

	// Update is called once per frame
	void Update () {
		for (int i = 0; i < 20; i++)
		{
			water[(int)(Random.value * size), (int)(Random.value * size)] = rainFallRate;
		}

		if (Input.GetKeyDown(KeyCode.W))
		{
			renderWater = !renderWater;
		}

		if (Input.GetKeyDown(KeyCode.R))
		{
			run = !run;
		}

		if (Input.GetKeyDown(KeyCode.S) || run)
		{
			StepErosion();
			CreateTexture();
		}

	}
}
