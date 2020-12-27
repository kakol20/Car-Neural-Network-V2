using System.Collections.Generic;

public class NeuralNetwork
{
    public NeuralNetwork(int[] layers)
    {
        if (layers.Length >= 2)
        {
            for (int i = 0; i < layers.Length; i++)
            {
                // ----- CREATE NEURONS -----
                Matrix neuronLayer = new Matrix(1, layers[i]);

                Neurons.Add(neuronLayer);

                if (i != 0)
                {
                    // ----- CREATE BIASES -----
                    Matrix biasLayer = new Matrix(1, layers[i]);
                    biasLayer.Randomise(-0.5f, 0.5f);

                    Biases.Add(biasLayer);
                }

                if (i + 1 != layers.Length)
                {
                    // ----- CREATE WEIGHTS -----
                    Matrix weightLayer = new Matrix(layers[i], layers[i + 1]);
                    weightLayer.Randomise(-1, 1);

                    Weights.Add(weightLayer);
                }

                //Matrix neuronLayer = new Matrix();
            }
        }
    }

    public List<Matrix> Biases { get; private set; } = new List<Matrix>();
    public List<Matrix> Neurons { get; private set; } = new List<Matrix>();

    public void CopyNetwork(NeuralNetwork network)
    {
        for (int i = 0; i < Biases.Count; i++)
        {
            Biases[i].Copy(network.Biases[i]);
        }

        for (int i = 0; i < Weights.Count; i++)
        {
            Weights[i].Copy(network.Weights[i]);
        }
    }

    /// <summary>
    /// Mutate Neural Network and clamp it between min and max
    /// </summary>
    /// <param name="mutationRate"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public void Mutate(float min, float max, float chance)
    {
        foreach (Matrix item in Biases)
        {
            item.Mutate(min, max, chance);

            item.Clamp(-1, 1);
        }

        foreach (Matrix item in Weights)
        {
            item.Mutate(min, max, chance);

            item.Clamp(-1, 1);
        }
    }
    public List<float> Output
    {
        get
        {
            return Neurons[Neurons.Count - 1].Array[0];
        }
    }

    public List<Matrix> Weights { get; private set; } = new List<Matrix>();

    public void FeedForward(float[] input, ActivationFunction activation = ActivationFunction.Tanh)
    {
        //Matrix inputMatrix = new Matrix();

        List<List<float>> inputLayer = new List<List<float>>();
        List<float> col = new List<float>();

        for (int i = 0; i < input.Length; i++)
        {
            col.Add(input[i]);
        }
        inputLayer.Add(col);

        Neurons[0].Copy(inputLayer);

        for (int i = 0; i < Neurons.Count - 1; i++)
        {
            //Matrix output = new Matrix();
            //output.Copy(Weights[i]);
            //output.Multiply(Neurons[i]);
            //output.Add(Biases[i]);

            Neurons[i + 1].Copy(Weights[i]);
            Neurons[i + 1].Multiply(Neurons[i]);
            Neurons[i + 1].Add(Biases[i]);

            Neurons[i + 1].Activation(activation);
            Neurons[i + 1].Clamp(-1, 1);
        }
    }

    public void Randomise(float min = -1f, float max = 1f)
    {
        foreach (Matrix item in Weights)
        {
            item.Randomise(min, max);
        }

        foreach (Matrix item in Biases)
        {
            item.Randomise(min, max);
        }
    }
}

public class PrintableNN
{
    public List<List<List<float>>> Weights = new List<List<List<float>>>();
    public List<List<List<float>>> Biases = new List<List<List<float>>>();
    //public List<List<List<float>>> Neurons = new List<List<List<float>>>();


    public PrintableNN()
    {
    }

    public PrintableNN(NeuralNetwork copyNetwork)
    {
        for (int i = 0; i < copyNetwork.Biases.Count; i++)
        {
            Biases.Add(copyNetwork.Biases[i].Array);
        }

        for (int i = 0; i < copyNetwork.Weights.Count; i++)
        {
            Weights.Add(copyNetwork.Weights[i].Array);
        }

        //for (int i = 0; i < copyNetwork.Neurons.Count; i++)
        //{
        //    Neurons.Add(copyNetwork.Neurons[i].Array);
        //}
    }
}