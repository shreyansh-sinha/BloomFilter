using System.Drawing;
using Murmur;

class BloomFilter
{
    private byte[] filter;
    private UInt32 size;
    private List<Murmur32> hashFunctions;

    public BloomFilter(UInt32 size)
    {
        this.size = size;
        filter = new byte[size];
        hashFunctions = GenerateHashFunctions(100);
    }

    private List<Murmur32> GenerateHashFunctions(int count)
    {
        var hashFunctions = new List<Murmur32>();
        var random = new Random();
        for (int i = 0; i < count; i++)
        {
            uint seed = (uint)random.Next();
            hashFunctions.Add(MurmurHash.Create32(seed));
        }
        return hashFunctions;
    }

    public List<Murmur32> GetHashFunctions()
    {
        return hashFunctions;
    }

    public void Add(string key, int hashFunctionCount)
    {
        for(int i = 0; i < hashFunctionCount; i++)
        {
            var murmur = hashFunctions[i]; // Use a fixed seed value
            byte[] data = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] hashBytes = murmur.ComputeHash(data);
            UInt32 idx = BitConverter.ToUInt32(hashBytes, 0) % size;

            int aIdx = (int)(idx / 8);
            int bIdx = (int)(idx % 8);
            filter[aIdx] |= (byte)(1 << bIdx);
        }
    }

    public bool Contains(string key, int hashFunctionCount)
    {
        for (int i = 0; i < hashFunctionCount; i++)
        {
            var murmur = hashFunctions[i]; // Use a fixed seed value
            byte[] data = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] hashBytes = murmur.ComputeHash(data);
            UInt32 idx = BitConverter.ToUInt32(hashBytes, 0) % size;

            int aIdx = (int)(idx / 8);
            int bIdx = (int)(idx % 8);
            var isPresent = (filter[aIdx] & (byte)(1 << bIdx)) > 0;
            if (!isPresent)
            {
                return false;
            }
        }
        return true;
    }
}

public class Program
{
    public static void Main()
    {
        // create a dataset of 1000 elements (strings).
        // divide the dataset into two parts => training data and testing data of 500 elements each.
        // training data => insert into bloom filters.
        // check if the elements of dataset is present in the bloom filter.
        // calculate the false positive rate = (number of false positives) / (number of elements in dataset).
        // check the false positive rate for different sizes of bloom filters.
        // how to detect false positive ? => check if element is not present in the training data but present in the bloom filter.
        // add the space optimization to use byte array instead of bool array.
        // check the false positive rate for different hash functions with same size of bloom filter.

        List<string> dataset = new List<string>();
        List<string> dataset_present = new List<string>();
        List<string> dataset_absent = new List<string>();

        for (int i = 0; i < 1000; i++)
        {
            string data = Guid.NewGuid().ToString();
            dataset.Add(data);
            if (i < 500)
            {
                dataset_present.Add(data);
            }
            else
            {
                dataset_absent.Add(data);
            }
        }

        // check the false positive rate for different sizes of bloom filters.
        for (uint size = 100; size <= 10000; size += 100)
        {
            BloomFilter bloomFilter = new BloomFilter(size);
            foreach (string data in dataset_present)
            {
                bloomFilter.Add(data, 1);
            }

            int falsePositives = 0;
            foreach (string data in dataset_absent)
            {
                if (bloomFilter.Contains(data, 1))
                {
                    falsePositives++;
                }
            }

            double falsePositiveRate = (double)falsePositives / dataset.Count;
            Console.WriteLine("Size: " + size + " False Positive Rate: " + falsePositiveRate);
        }

        // check the false positive rate for different hash functions with same size of bloom filter.

        uint boolFilterSize = 10000;
        BloomFilter secondBloomFilter = new BloomFilter(boolFilterSize);

        var hashFunctions = secondBloomFilter.GetHashFunctions();

        for (int index = 1; index <= hashFunctions.Count; index++)
        {
            foreach (string data in dataset_present)
            {
                secondBloomFilter.Add(data, index);
            }

            int falsePositives = 0;
            foreach (string data in dataset_absent)
            {
                if (secondBloomFilter.Contains(data, index))
                {
                    falsePositives++;
                }
            }

            double falsePositiveRate = (double)falsePositives / dataset.Count;
            Console.WriteLine(" False Positive Rate: " + falsePositiveRate);
        }
    }
}
