using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Buffers;
using System.Threading.Tasks;

public class ConvolutionProcessor
{
    // Wariant 1: Bazowa konwolucja (double[,])
    public double[,] BasicConvolution(double[,] input, double[,] kernel)
    {
        int width = input.GetLength(0);
        int height = input.GetLength(1);
        double[,] output = new double[width, height];

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                double sum = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        sum += input[x + i, y + j] * kernel[i + 1, j + 1];
                    }
                }
                output[x, y] = sum;
            }
        }
        return output;
    }

    // Wariant 2: Równoległa konwolucja (Parallel.For)
    public double[,] ParallelConvolution(double[,] input, double[,] kernel)
    {
        int width = input.GetLength(0);
        int height = input.GetLength(1);
        double[,] output = new double[width, height];

        Parallel.For(1, width - 1, x =>
        {
            for (int y = 1; y < height - 1; y++)
            {
                double sum = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        sum += input[x + i, y + j] * kernel[i + 1, j + 1];
                    }
                }
                output[x, y] = sum;
            }
        });
        return output;
    }

    // Wariant 3: Konwolucja z SIMD (Vector<T>)
    public float[,] SimdConvolution(float[,] input, float[,] kernel)
    {
        int width = input.GetLength(0);
        int height = input.GetLength(1);
        float[,] output = new float[width, height];
        var kernelVec = new Vector<float>(kernel.Cast<float>().ToArray());

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                Vector<float> sum = Vector<float>.Zero;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        var inputVec = new Vector<float>(input[x + i, y + j]);
                        sum += inputVec * kernelVec;
                    }
                }
                output[x, y] = sum[0];
            }
        }
        return output;
    }

    // Wariant 4: Konwolucja z ArrayPool
    public double[,] ArrayPoolConvolution(double[,] input, double[,] kernel)
    {
        int width = input.GetLength(0);
        int height = input.GetLength(1);
        var output = ArrayPool<double>.Shared.Rent(width * height);

        try
        {
            Parallel.For(1, width - 1, x =>
            {
                for (int y = 1; y < height - 1; y++)
                {
                    double sum = 0;
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            sum += input[x + i, y + j] * kernel[i + 1, j + 1];
                        }
                    }
                    output[x * height + y] = sum;
                }
            });
            return ConvertTo2D(output, width, height);
        }
        finally
        {
            ArrayPool<double>.Shared.Return(output);
        }
    }

    // Wariant 5: Pamięć niezarządzana (unsafe + IntPtr)
    public unsafe void UnmanagedConvolution(double* input, double* kernel, double* output, int width, int height)
    {
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                double sum = 0;
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        sum += input[(x + i) * height + (y + j)] * kernel[(i + 1) * 3 + (j + 1)];
                    }
                }
                output[x * height + y] = sum;
            }
        }
    }

    private double[,] ConvertTo2D(double[] array, int width, int height)
    {
        double[,] result = new double[width, height];
        Buffer.BlockCopy(array, 0, result, 0, array.Length * sizeof(double));
        return result;
    }
}
