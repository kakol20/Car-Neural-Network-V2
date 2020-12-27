using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Car Population")]
    [SerializeField] private GameObject carPrefab;
    [SerializeField] private int populationSize = 25;

    private List<GameObject> carPopulation = new List<GameObject>();

    [Header("Training")]
    [SerializeField] [Range(0, 1)] private float mutationRate = 0.1f;

    [Header("Follow")]
    [SerializeField] private Transform lookTarget;

    private int currentFollow = 0;
    private float timeElapsed = 0;
    private int generation = 1;

    public void DrawRays()
    {
        //carPopulation[currentFollow].GetComponent<NeuralNetworkControl>().DrawRays();
    }

    private bool AllFinished()
    {
        bool temp = false;

        foreach (GameObject item in carPopulation)
        {
            if (!item.GetComponent<NeuralNetworkControl>().IsFinished) temp = true;
        }

        return !temp;
    }

    private void Awake()
    {
        Own.Random.Init();

        DebugGUI.SetGraphProperties("throttle", "Throttle", 0, 1, 0, Color.green, true);
        DebugGUI.SetGraphProperties("brake", "Brake", 0, 1, 0, Color.red, true);
        DebugGUI.SetGraphProperties("steering", "Steering", -1, 1, 0, Color.blue, true);

        DebugGUI.SetGraphProperties("topFitness", "Top Fitness", 0, 0, 1, Color.red, true);
    }

    private void CopyBestCars()
    {
        int half = carPopulation.Count / 2;
        for (int i = 0; i < half; i++)
        {
            // ----- COPY BEST CARS -----
            carPopulation[i + half].GetComponent<NeuralNetworkControl>().NeuralNetwork.CopyNetwork(carPopulation[i].GetComponent<NeuralNetworkControl>().NeuralNetwork);

            float hue = carPopulation[i].GetComponent<NeuralNetworkControl>().GetHue;
            //hue /= 2f;

            // ----- MUTATE -----
            float chance = Own.Random.Range(0, 1);

            if (chance <= mutationRate)
            {
                carPopulation[i + half].GetComponent<NeuralNetworkControl>().NeuralNetwork.Mutate(-1, 1, mutationRate);

                float random = Own.Random.Range();
                hue = random <= 0.1f ? hue + Own.Random.Range(-.5f, .5f) : hue;
                hue = Mathf.Clamp(hue % 1, 0, 1);
            }

            carPopulation[i + half].GetComponent<NeuralNetworkControl>().SetHue(hue);
        }
    }

    private bool CopyOutlier()
    {
        float limit = (populationSize / 2f) - 1f;

        // ----- MEDIAN -----
        //float medianIndex = (2 / 3f) * limit;
        //float median = GetFitness(medianIndex);

        // ----- QUARTILES -----

        float Q3Index = (1 / 3f) * limit;
        float Q3 = GetFitness(Q3Index);

        float Q1 = GetFitness(limit);

        float IQR = Q3 - Q1;

        // ----- CHECK OUTLIER -----
        float outlier = Q3 + (1.5f * IQR);

        if (GetFitness(0) > outlier)
        {
            // ----- COPY OUTLIER TO WORST CARS -----
            for (int i = carPopulation.Count / 2; i < carPopulation.Count; i++)
            {
                carPopulation[i].GetComponent<NeuralNetworkControl>().NeuralNetwork.CopyNetwork(carPopulation[0].GetComponent<NeuralNetworkControl>().NeuralNetwork);
                carPopulation[i].GetComponent<NeuralNetworkControl>().NeuralNetwork.Mutate(-1, 1, mutationRate);

                float hue = carPopulation[0].GetComponent<NeuralNetworkControl>().GetHue;
                //hue /= 2f;
                float random = Own.Random.Range();
                hue = random <= 0.1f ? hue + Own.Random.Range(-.1f, .1f) : hue;
                hue = Mathf.Clamp(hue % 1, 0, 1);
                carPopulation[i].GetComponent<NeuralNetworkControl>().SetHue(hue);
            }

            return true;
        }

        return false;
    }

    private float GetFitness(float index)
    {
        if (index % 1 != 0)
        {
            int tmp1 = Mathf.FloorToInt(index);
            int tmp2 = Mathf.CeilToInt(index);

            return (carPopulation[tmp1].GetComponent<NeuralNetworkControl>().Fitness + carPopulation[tmp2].GetComponent<NeuralNetworkControl>().Fitness) / 2f;
        }
        else
        {
            return carPopulation[(int)index].GetComponent<NeuralNetworkControl>().Fitness;
        }
    }

    private void SpawnCars()
    {
        for (int i = 0; i < populationSize; i++)
        {
            carPopulation.Add(Instantiate(carPrefab, transform.position, transform.rotation, transform));

            carPopulation[i].GetComponent<NeuralNetworkControl>().SetHue((float)i / (float)populationSize);
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        Own.Random.Init();

        populationSize = (int)Mathf.Round(populationSize / 2.0f) * 2;

        SpawnCars();

        generation = 1;
    }
    // Update is called once per frame
    private void Update()
    {
        //DebugGUI.Graph("throttle", car.NNThrottle);
        //DebugGUI.Graph("brake", car.NNBrake);

        if (!AllFinished())
        {
            for (int i = 0; i < carPopulation.Count; i++)
            {
                if (!carPopulation[i].GetComponent<NeuralNetworkControl>().IsFinished)
                {
                    currentFollow = i;

                    break;
                }
            }

            lookTarget.position = carPopulation[currentFollow].transform.position;
            lookTarget.rotation = carPopulation[currentFollow].transform.rotation;

            carPopulation[currentFollow].GetComponent<NeuralNetworkControl>().Graph("throttle", "brake", "steering");

            timeElapsed += Time.deltaTime;

            // Debugging
            //carPopulation[currentFollow].GetComponent<CarController>().DebugControls();
        }
        else // 1 generation finished
        {
            carPopulation = carPopulation.OrderByDescending(e => e.GetComponent<NeuralNetworkControl>().Fitness).ToList();

            DebugGUI.Graph("topFitness", carPopulation[0].GetComponent<NeuralNetworkControl>().Fitness);

            if (!CopyOutlier()) CopyBestCars();

            // ----- RESET CARS -----
            foreach (GameObject item in carPopulation)
            {
                item.GetComponent<NeuralNetworkControl>().ResetCar();
            }

            Debug.Log("Next Generation");
            timeElapsed = 0;
            generation++;
        }

        DebugGUI.LogPersistent("timeElapsed", "Time Elapsed: " + timeElapsed.ToString("F2"));
        DebugGUI.LogPersistent("fps", "FPS: " + (1.0f / Time.deltaTime).ToString("F0"));
        DebugGUI.LogPersistent("generarion", "Generation: " + generation.ToString());
    }
}