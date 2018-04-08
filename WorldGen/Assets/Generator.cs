using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : MonoBehaviour {


	public int size = 512;

	float[,] heights;

	public int numberOfPlates = 4;

	int[,] plates;

	Texture2D texture;

	public int seed = 0;
	public bool randomSeed = false;

	public float worldScale = 1.0f;
	
	public Renderer neighborSameTexture;

	void Start()
    {
		if (!randomSeed)
		{
			Random.seed = seed;
		}

		heights = new float[size,size];
		plates = new int[size,size];

        texture = new Texture2D(size, size);
		GetComponent<Renderer>().material.mainTexture = texture;

		if (neighborSameTexture != null)
		{
			neighborSameTexture.material.mainTexture = texture;
		}
		
        texture.Apply();

		CreateHeat();
		RenderEarthHeat();
		
		//CreatePlates();
		//RenderPlates();

    }
	
	public Gradient earthHeatGradient;
	public Gradient terrainGradient;

	// Hotter heats mean closer to the surface is molten
	float[,] earthHeat;
	float[,] material;
	float[,] materialTransfer;

	Vector2[,] earthHeatVelocity;
	float[,] earthHeatTransfer;
	Vector2 [,] earthHeatVelocityTransfer;

	void CreateHeat()
	{
		earthHeat = new float[size, size];
		earthHeatVelocity = new Vector2[size,size];
		earthHeatTransfer = new float[size,size];
		earthHeatVelocityTransfer = new Vector2[size,size];

		material = new float[size, size];
		materialTransfer = new float[size, size];

		for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
				earthHeat[x,y] = Random.value + .2f;
				totalHeat += earthHeat[x,y];
				material[x,y] = Random.value;
			}
		}
	}

	public float totalHeat = 0;

	public float densityImportance = 1.0f;
	public float velocityImportance = 1.0f;
	public float materialDensityImportance = 1.0f;

	int[] xOffsets = {0,-1,0,1, -1,-1,1,1};
	int[] yOffsets = {-1,0,1,0, -1,1,1,-1};
	void StepEarthHeat()
	{
		for (int x = 0; x < texture.width; x++)
		{
			for (int y = 0; y < texture.height; y++)
			{

				float earthHeatHere = earthHeat[x, y];
				// Check neighbors
				float total = earthHeatHere;
				float averageOfNeighbors = 0;

				float[] neighborValues = new float[8];
				float[] transfer = new float[8];
				Vector2 velocityHere = earthHeatVelocity[x,y];

				for (int k = 0; k < 8; k++)
				{
					int i = xOffsets[k];
					int j = yOffsets[k];
					float transferValue = 0;

					int nextX = x + i;
					int nextY = y + j;

					if (nextX == -1)
					{
						nextX = size - 1;
					}

					if (nextX == size)
					{
						nextX = 0;
					}

					if (nextY == -1)
					{
						nextY = size -1;
					}

					if (nextY == size)
					{
						nextY = 0;
					}

					Vector2 difPos = new Vector2(i, j);
					float dif = earthHeatHere - earthHeat[nextX, nextY];
					
					float difAlongVelocity = Vector2.Dot(velocityHere, difPos.normalized);

					float materialDif = material[x,y] - material[nextX, nextY];

					transferValue =
					     ( difAlongVelocity * velocityImportance)
						+ (dif * densityImportance )
						+ (materialDif * materialDensityImportance);

					if (transferValue > 0)
					{
						total += transferValue;
						transfer[k] = transferValue;
					}
				}

				float friction = .8f;
				total += friction * earthHeatHere;

				if (total > 0)
				{
					// Apply transfer
					for (int k = 0; k < 8; k++)
					{
						int i = xOffsets[k];
						int j = yOffsets[k];
						float transferValue = transfer[k];

						int nextX = x + i;
						int nextY = y + j;

						if (nextX == -1)
						{
							nextX = size - 1;
						}

						if (nextX == size)
						{
							nextX = 0;
						}

						if (nextY == -1)
						{
							nextY = size -1;
						}

						if (nextY == size)
						{
							nextY = 0;
						}

						float heatHere = earthHeat[nextX, nextY];
						float transferAmount = (transferValue / total) * earthHeatHere;
						
						earthHeatTransfer[x,y] -= transferAmount;

						float materialTransferAmount = (transferValue / total) * material[x,y] * .1f;
						materialTransfer[x,y] -= materialTransferAmount;

						materialTransfer[nextX,nextY] += materialTransferAmount;

						earthHeatTransfer[nextX, nextY] += transferAmount;
						earthHeatVelocityTransfer[x, y] += new Vector2(i,j) * transferAmount;
						earthHeatVelocityTransfer[nextX, nextY] += new Vector2(i,j) * transferAmount;
						
					}
				}
			}
		}


		totalHeat = 0;
		for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
				earthHeat[x,y] += earthHeatTransfer[x,y];
				earthHeatTransfer[x,y] = 0;

				earthHeatVelocity[x,y] += earthHeatVelocityTransfer[x,y];
				
				if (earthHeatVelocity[x,y].magnitude > 2.0f)
				{
					earthHeatVelocity[x,y] = earthHeatVelocity[x,y].normalized * 3.0f;
				}

				earthHeatVelocity[x,y] *= friction;


				earthHeatVelocityTransfer[x,y] = Vector2.zero;

				material[x,y] += materialTransfer[x,y];
				materialTransfer[x,y] = 0;

				if (earthHeat[x,y] < .1f)
				{
					//earthHeat[x,y] *= .2f;
				}

				totalHeat += earthHeat[x,y];
			}
		}
	}

	public float friction = .9f;

	void RenderEarthHeat()
	{
		for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
				Color color = earthHeatGradient.Evaluate(Mathf.Clamp(earthHeat[x,y] / 2.0f, 0, 1.0f));
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
	}

	void RenderEarthHeatVelocity()
	{
		for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
				Color color = earthHeatGradient.Evaluate(Mathf.Clamp(earthHeatVelocity[x,y].magnitude / 10f, 0, 1.0f));
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
	}

	void RenderEarthMaterial()
	{
		for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
				Color color = terrainGradient.Evaluate(Mathf.Clamp(material[x,y] / 2.0f, 0, 1.0f));
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
	}

	Color[] plateColors;

	public float perlinScale = 10.0f;
	public float maxPerlinMove = 100.0f;
	void CreatePlates()
	{
		float xOffset = Random.value * 10000;
		float yOffset = Random.value * 10000;

		Vector2[] plateSourcePositions = new Vector2[numberOfPlates];
		plateColors = new Color[numberOfPlates+1];
		plateColors[0] = Color.white;
		for (int i = 0; i < numberOfPlates; i++)
		{
			plateSourcePositions[i] = new Vector2(Random.value * size * worldScale, Random.value * size * worldScale);
			plateColors[i+1] = new Color(Random.value, Random.value, Random.value);
		}

		for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
				int secondClosestIndex = 0;
				float secondClosestDistance = float.MinValue;
				
				int closestPlateIndex = 0;
				float closestPlateDistance = float.MaxValue;

				for (int i = 0; i < numberOfPlates; i++)
				{
					float perlinX = Mathf.PerlinNoise((x + xOffset) * perlinScale * worldScale, (y + xOffset) * perlinScale * worldScale) * maxPerlinMove;
					float perlinY = Mathf.PerlinNoise((x + yOffset) * perlinScale * worldScale, (y + yOffset) * perlinScale * worldScale) * maxPerlinMove;

					float baseX = x + perlinX;
					float baseY = y + perlinY;

					Vector2 dif = new Vector2(plateSourcePositions[i].x - baseX, plateSourcePositions[i].y - baseY);
					float distance = dif.sqrMagnitude;

					if (distance < closestPlateDistance)
					{
						closestPlateIndex = i;
						closestPlateDistance = distance;
					}
				}

				plates[x,y] = closestPlateIndex + 1;
			}
		}
	}

	void CreatePlatesFill()
	{
		plateColors = new Color[numberOfPlates+1];
		plateColors[0] = Color.white;

		bool[,] traversed = new bool[size, size];
		int untraversedCount = size * size;

		for (int i = 1; i < numberOfPlates+1; i++)
		{
			Vector2 next = new Vector2(Random.value, Random.value) * size;
			if (untraversedCount > 0)
			{
				int randomIndex = (int)(untraversedCount * Random.value);
				int count = 0;

				for (int x = 0; x < size; x++)
				{
					for (int y = 0; y < size; y++)
					{
						if (!traversed[x,y])
						{
							if (randomIndex == count)
							{
								next = new Vector2(x, y);
								goto end_of_loop;
							}
							count += 1;
						}
					}
				}
			}

			end_of_loop:

			Debug.Log("Start X: " + next.x);
			Debug.Log("Start Y: " + next.y);

			FillRegion((int)(next.x), (int)(next.y), i, 5000, traversed, ref untraversedCount);
			plateColors[i] = new Color(Random.value, Random.value, Random.value);
		}

	}
	
	void FillRegion (int x, int y, int plateId, int amountToFill, bool[,] traversed, ref int untraversedCount)
	{
		List<Vector2> potentialNeighbors = new List<Vector2>();
		
		while (amountToFill > 0)
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					if (x + i >= 0 && x + i < size && y + j >= 0 && y + j < size && (i != 0 || j != 0)
					 && (i == 0 || j == 0))
					{
						if (plates[x + i, y + j] == 0)
						{
							potentialNeighbors.Add(new Vector2(x + i, y + j));
						}
					}
				}
			}

			if (potentialNeighbors.Count == 0)
			{
				Debug.Log("Hello");
				return;
			}
			
			int randomNeighbor = (int)(Random.value * potentialNeighbors.Count);
			
			x = (int)potentialNeighbors[randomNeighbor].x;
			y = (int)potentialNeighbors[randomNeighbor].y;
			traversed[x,y] = true;
			untraversedCount -= 1;
			potentialNeighbors.RemoveAt(randomNeighbor);
			plates[x,y] = plateId;
			amountToFill -= 1;
		}
	}

	int[,] newValues;

	void MovePlates()
	{
		if (newValues == null)
		{
			newValues = new int[size, size];
		}

		Vector3[] plateMovementVector = new Vector3[numberOfPlates+1];
		plateMovementVector[0] = Vector3.zero;
		
		for (int i = 0; i < numberOfPlates; i++)
		{
			plateMovementVector[i] = Random.insideUnitCircle.normalized * 2.0f * worldScale;
		}
		
		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				Vector3 movementVector = plateMovementVector[plates[x,y]];
				int xVal = (int)(x + movementVector.x);
				int yVal = (int)(y + movementVector.y);

				//Debug.Log(xVal);
				if (xVal >= 0 && xVal < size && yVal >= 0 && yVal < size)
				{
					newValues[xVal, yVal] = plates[x, y];
					plates[x,y] = 0;
				}
			}
		}

		for (int x = 0; x < size; x++)
		{
			for (int y = 0; y < size; y++)
			{
				plates[x,y] = newValues[x,y];
			}
		}
	}

	void RenderPlates()
	{
		for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
				Color color = plateColors[plates[x,y]];
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
	}

	bool run = false;
	bool visualize = true;
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			run = !run;
		}

		if (Input.GetKeyDown(KeyCode.I))
		{
			visualize = !visualize;
		}

		if (run)
		{
			StepEarthHeat();

			if (visualize)
			{
				RenderEarthHeat();
			// RenderEarthMaterial();
			}

		}

		if (Input.GetKeyDown(KeyCode.S))
		{
			StepEarthHeat();
			RenderEarthHeat();
			//StepPlates();
		}

		if (Input.GetKeyDown(KeyCode.V))
		{
			RenderEarthHeatVelocity();
		}

		if (Input.GetKeyDown(KeyCode.M))
		{
			RenderEarthMaterial();
		}
	}

	void StepPlates()
	{
		//MovePlates();
		RenderPlates();
	}
}
