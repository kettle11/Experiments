// AlanZucconi.com: http://www.alanzucconi.com/?p=4539
using UnityEngine;
using System.Collections;
 
//[ExecuteInEditMode]
public class SimulationShader : MonoBehaviour
{
    public Material waterSimulationMaterial;
	public Material finalCompositeMaterial;
	
	public Material waterFlowMaterial;

	public Material waterPaintMaterial;

	public RenderTexture heightMap;
    public RenderTexture water;
	public RenderTexture flowMap;

    private RenderTexture buffer;
	private RenderTexture finalOutput;

    public Texture initialTexture; // first texture

	public int size = 512;

    public Texture stampTexture;
	public Material singlePixelMaterial;


    void Start ()
    {
        water = new RenderTexture(size, size, 0, RenderTextureFormat.ARGBFloat);

       // Graphics.Blit(initialTexture, texture);

        buffer = new RenderTexture(water.width, water.height, water.depth, water.format);
		heightMap = new RenderTexture(water.width, water.height, water.depth, water.format);
		flowMap = new RenderTexture(water.width, water.height, water.depth, water.format);
		finalOutput = new RenderTexture(water.width, water.height, water.depth, water.format);

		//water.filterMode = FilterMode.Point;
		buffer.filterMode = FilterMode.Point;
		//flowMap.filterMode = FilterMode.Point;
		finalOutput.filterMode = FilterMode.Point;
		
		waterSimulationMaterial.SetTexture("_MainTex", water);
		waterSimulationMaterial.SetTexture("_FlowMap", flowMap);
		waterSimulationMaterial.SetTexture("_HeightMap", heightMap);
		waterSimulationMaterial.SetInt("_Pixels", size);
		waterSimulationMaterial.SetFloat("_TimeStep", Time.fixedDeltaTime);//.1f);//Time.fixedDeltaTime * .5f);

		waterFlowMaterial.SetTexture("_Water", water);
		waterFlowMaterial.SetTexture("_HeightMap", heightMap);
		waterFlowMaterial.SetTexture("_FlowMap", flowMap);
		waterFlowMaterial.SetFloat("_TimeStep", Time.fixedDeltaTime);//.1f);//Time.fixedDeltaTime * .5f);

		finalCompositeMaterial.SetTexture("_Water", water);
		finalCompositeMaterial.SetTexture("_HeightMap", heightMap);

		waterPaintMaterial.SetTexture("_Water", water);
		
		CreateHeightmap();
		GetComponent<MeshRenderer>().material.SetTexture("_MainTex", finalOutput);

		RandomValues(true);

		CreateMeshForTerrain();
		//EditManually(50,50);
    }
	
	public GameObject terrainMesh;

	public void CreateMeshForTerrain()
	{
		Mesh mesh = new Mesh();

		Vector3[] vertices = new Vector3[size * size];
		int[] triangles = new int[(size-1)*(size-1)*6];
		Vector2[] uvs = new Vector2[size * size];
		Vector3[] normals = new Vector3[size*size];
		for (int i = 0; i < size; i++)
		{
			for (int j = 0; j < size; j++)
			{
				vertices[i*size + j] = new Vector3(i, 0, j);
				uvs[i*size + j] = new Vector2((float)i / (float)size, (float)j / (float)size);
				normals[i*size + j] = Vector3.up;
			}
		}

		int triangleCount = 0;

		for (int i = 0; i < size - 1; i++)
		{
			for (int j = 0; j < size - 1; j++)
			{
				triangles[triangleCount] = i * size + j;
				triangles[triangleCount+1] = i * size + j + 1;
				triangles[triangleCount+2] = (i + 1) * size + j + 1;

				triangles[triangleCount+3] = i * size + j;
				triangles[triangleCount+4] = (i + 1) * size + j + 1;
				triangles[triangleCount+5] = (i + 1) * size + j;

				triangleCount += 6;
			}
		}

		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;

		terrainMesh.GetComponent<MeshFilter>().mesh = mesh;
		terrainMesh.GetComponent<MeshRenderer>().material.SetTexture("_HeightMap", heightMap);
		terrainMesh.GetComponent<MeshRenderer>().material.SetTexture("_Water", water);
		terrainMesh.GetComponent<MeshRenderer>().material.SetTexture("_Flow", flowMap);
	}

	public float[] octaveWeights;
	public float baseOctaveValue;
	public float scale;

	public float falloffRadius;
	void CreateHeightmap()
	{
		Texture2D newTexture = new Texture2D(size, size);

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

				//finalValue = (float) i / 300.0f;
				newTexture.SetPixel(i,j, new Color(finalValue, 0,0, 1));
			}
		}

		newTexture.Apply();
		Graphics.Blit(newTexture, heightMap);
	}

	void RandomValues(bool initial)
	{
		//Random.seed = 2;

		Texture2D newTexture = new Texture2D(size, size);

		for (int x = 0; x < water.width; x++)
		{
			for (int y = 0; y < water.height; y++)
			{
			 	newTexture.SetPixel(x, y, new Color(0,0,0,0));
			}
		}

		for (int i = 200; i < 240 ; i++)
		{
			for (int j = 200; j < 400; j++)
			{
				  newTexture.SetPixel(i,size - j, new Color(0, 0, 0, .2f));
			}
		}

		
		for (int i = 0; i < 100; i++)
		{
			newTexture.SetPixel((int)(Random.value * (float)size),(int)(Random.value * (float)size), new Color(0,0,0,.01f));
			//newTexture.SetPixel((int)(Random.value * (float)size), (int)(Random.value * (float)size), new Color(0, 0, 1, 1));
		}

	   // newTexture.SetPixel(size / 2 + 10,size / 2, new Color(0, 0, 0, .1f));

		newTexture.Apply();

		if (initial)
		{
			Graphics.Blit(newTexture, water, waterPaintMaterial);
		}
		else
		{
			Graphics.Blit(newTexture, buffer, waterPaintMaterial);
			Graphics.Blit(buffer, water);
		}
	}

	/*
	void EditManually(int posX, int posY)
    {
        Texture2D editTexture = new Texture2D(texture.width, texture.height);

        GL.PushMatrix ();     
        GL.LoadPixelMatrix(0, size , size, 0);

        RenderTexture.active = texture; 
        
		Graphics.DrawTexture (new Rect (posX, posY, 20, 20), stampTexture, singlePixelMaterial);
        //Graphics.DrawTexture (new Rect (posX, posY, stampTexture.width, stampTexture.height), stampTexture, singlePixelMaterial);

        GL.PopMatrix (); 
        RenderTexture.active = null; //don't forget to set it back to null once you finished playing with it.
    }*/

	public void RunWaterShader()
	{
		Graphics.Blit(water, flowMap, waterSimulationMaterial);
		Graphics.Blit(flowMap, water, waterFlowMaterial);

		//float waterTotal = CheckWaterValues();
		//float flowTotal = CheckFlowValues();

		//Debug.Log("Next water total: " + waterTotal + flowTotal);
	}

	public void RenderFinalComposite()
	{
		Graphics.Blit(finalOutput, buffer, finalCompositeMaterial);
        Graphics.Blit(buffer, finalOutput);
	}
    // Postprocess the image
    public void UpdateTexture()
    {
		//RandomValues(false);
		RunWaterShader();
		RenderFinalComposite();
       // Graphics.Blit(texture, buffer, material);
       // Graphics.Blit(buffer, texture);
    }

	float CheckWaterValues()
	{
		// Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = water;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(water.width, water.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;
		
		Color[] color = tex.GetPixels();

		float totalValues = 0;

		for (int i = 0; i < color.Length; i++)
		{
			totalValues += color[i].a;// color[i].r + color[i].g + color[i].b + color[i].a;
		}

		Debug.Log("Total water : " + totalValues);
		return totalValues;
	}

	float CheckFlowValues()
	{
		// Remember currently active render texture
        RenderTexture currentActiveRT = RenderTexture.active;

        // Set the supplied RenderTexture as the active one
        RenderTexture.active = flowMap;

        // Create a new Texture2D and read the RenderTexture image into it
        Texture2D tex = new Texture2D(flowMap.width, flowMap.height);
        tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

        // Restorie previously active render texture
        RenderTexture.active = currentActiveRT;
		
		Color[] color = tex.GetPixels();

		float totalValues = 0;
	
		for (int j = 0; j < size; j++)
		{
			for (int i = 0; i < size; i++)
			{
				Color here = color[j*size + i];
				totalValues -= here.r + here.g + here.b + here.a;

				if (i != 0)
				{
					Color neighbor = color[j*size + i - 1];
					totalValues += neighbor.b;
				}

				if (i != size - 1)
				{
					Color neighbor = color[j*size + i + 1];
					totalValues += neighbor.r;
				}

				if (j != 0)
				{
					Color neighbor = color[(j-1)*size + i];
					totalValues += neighbor.a;
				}

				if (j != size - 1)
				{
					Color neighbor = color[(j+1)*size + i];
					totalValues += neighbor.g;
				}
			}
		}

		Debug.Log("Total flow : " + totalValues);
		return totalValues;

	}
    // Updates regularly
    private float lastUpdateTime = 0;
    public float updateInterval = 0.1f; // s
    public void Update ()
    {
        //if (Time.time > lastUpdateTime + updateInterval)
        {
           // UpdateTexture();
            lastUpdateTime = Time.time;
        }

		if (Input.GetKeyDown(KeyCode.S))
		{
			UpdateTexture();
		}
    }

	public void FixedUpdate()
	{
		UpdateTexture();
	}
}
