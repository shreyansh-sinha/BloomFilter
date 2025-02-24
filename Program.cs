using Murmur;

class BloomFilter
{
    private byte[] filter;
    private UInt32 size;

    public BloomFilter(UInt32 size)
    {
        this.size = size;
        filter = new byte[size];
    }

    private UInt32 GetMurMurHash(string key, UInt32 size)
    {
        var murmur = MurmurHash.Create32(0); // Use a fixed seed value
        byte[] data = System.Text.Encoding.UTF8.GetBytes(key);
        byte[] hashBytes = murmur.ComputeHash(data);
        murmur.Clear();
        return BitConverter.ToUInt32(hashBytes, 0) % size;
    }

    public void Add(string key)
    {
        UInt32 idx = GetMurMurHash(key, size);
        int aIdx = (int)(idx / 8);
        int bIdx = (int)(idx % 8);
        filter[aIdx] |= (byte)(1 << bIdx);
    }

    public bool Contains(string key)
    {
        UInt32 idx = GetMurMurHash(key, size);
        int aIdx = (int)(idx / 8);
        int bIdx = (int)(idx % 8);
        return (filter[aIdx] & (byte)(1 << bIdx)) > 0;
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

        for(uint size = 100; size <= 10000; size += 100)
        {
            BloomFilter bloomFilter = new BloomFilter(size);
            foreach (string data in dataset_present)
            {
                bloomFilter.Add(data);
            }

            int falsePositives = 0;
            foreach (string data in dataset_absent)
            {
                if (bloomFilter.Contains(data))
                {
                    falsePositives++;
                }
            }

            double falsePositiveRate = (double)falsePositives / dataset.Count;
            Console.WriteLine("Size: " + size + " False Positive Rate: " + falsePositiveRate);
        }
    }
}
