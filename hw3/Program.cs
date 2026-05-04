using System;
using System.Collections.Generic;

namespace Hw3App
{
    public interface IEntity
    {
        int Id { get; }
    }

    public class Repository<T> where T : IEntity
    {
        private readonly Dictionary<int, T> _items = new();

        public int Count => _items.Count;

        public void Add(T item)
        {
            if (_items.ContainsKey(item.Id))
            {
                throw new InvalidOperationException($"Item with Id {item.Id} already exists.");
            }

            _items[item.Id] = item;
        }

        public bool Remove(int id)
        {
            return _items.Remove(id);
        }

        public T? GetById(int id)
        {
            return _items.TryGetValue(id, out var item) ? item : default;
        }

        public IReadOnlyList<T> GetAll()
        {
            return new List<T>(_items.Values);
        }

        public IReadOnlyList<T> Find(Predicate<T> predicate)
        {
            var result = new List<T>();
            foreach (var item in _items.Values)
            {
                if (predicate(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }
    }

    public static class CollectionUtils
    {
        public static List<T> Distinct<T>(List<T> source)
        {
            var seen = new HashSet<T>();
            var result = new List<T>();

            foreach (var item in source)
            {
                if (seen.Add(item))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        public static Dictionary<TKey, List<TValue>> GroupBy<TValue, TKey>(
            List<TValue> source,
            Func<TValue, TKey> keySelector)
            where TKey : notnull
        {
            var result = new Dictionary<TKey, List<TValue>>();

            foreach (var item in source)
            {
                var key = keySelector(item);
                if (!result.TryGetValue(key, out var list))
                {
                    list = new List<TValue>();
                    result[key] = list;
                }

                list.Add(item);
            }

            return result;
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
            Dictionary<TKey, TValue> first,
            Dictionary<TKey, TValue> second,
            Func<TValue, TValue, TValue> conflictResolver)
            where TKey : notnull
        {
            var result = new Dictionary<TKey, TValue>(first);

            foreach (var pair in second)
            {
                if (result.TryGetValue(pair.Key, out var existingValue))
                {
                    result[pair.Key] = conflictResolver(existingValue, pair.Value);
                }
                else
                {
                    result[pair.Key] = pair.Value;
                }
            }

            return result;
        }

        public static T MaxBy<T, TKey>(List<T> source, Func<T, TKey> selector)
            where TKey : IComparable<TKey>
        {
            if (source.Count == 0)
            {
                throw new InvalidOperationException("Source collection is empty.");
            }

            var bestItem = source[0];
            var bestKey = selector(bestItem);

            for (int i = 1; i < source.Count; i++)
            {
                var currentItem = source[i];
                var currentKey = selector(currentItem);
                if (currentKey.CompareTo(bestKey) > 0)
                {
                    bestKey = currentKey;
                    bestItem = currentItem;
                }
            }

            return bestItem;
        }
    }

    public class Product : IEntity
    {
        public int Id { get; }
        public string Name { get; }
        public decimal Price { get; }

        public Product(int id, string name, decimal price)
        {
            Id = id;
            Name = name;
            Price = price;
        }

        public override string ToString()
        {
            return $"Product(Id={Id}, Name={Name}, Price={Price})";
        }
    }

    public class User : IEntity
    {
        public int Id { get; }
        public string Name { get; }
        public string Email { get; }

        public User(int id, string name, string email)
        {
            Id = id;
            Name = name;
            Email = email;
        }

        public override string ToString()
        {
            return $"User(Id={Id}, Name={Name}, Email={Email})";
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("hw3 task 1 testing");
            var productRepository = new Repository<Product>();
            var userRepository = new Repository<User>();

            productRepository.Add(new Product(1, "Laptop", 1500m));
            productRepository.Add(new Product(2, "Headphones", 200m));
            productRepository.Add(new Product(3, "Smartphone", 1200m));

            userRepository.Add(new User(1, "Sigma", "sigma@example.com"));
            userRepository.Add(new User(2, "Fredj", "fredj@example.com"));

            Console.WriteLine("Product by id 3: " + productRepository.GetById(3));
            Console.WriteLine("User by id 2: " + userRepository.GetById(2));

            var expensiveProducts = productRepository.Find(p => p.Price > 1000m);
            Console.WriteLine("Products with price > 1000:");
            foreach (var product in expensiveProducts)
            {
                Console.WriteLine("  " + product);
            }

            try
            {
                productRepository.Add(new Product(1, "Tablet", 600m));
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Duplicate product add failed: " + ex.Message);
            }

            Console.WriteLine("Total products: " + productRepository.Count);
            Console.WriteLine("Total users: " + userRepository.Count);

            Console.WriteLine();
            Console.WriteLine("hw3 task 2 testings");

            var ints = new List<int> { 1, 2, 3, 2, 1, 4 };
            Console.WriteLine("Distinct ints: " + string.Join(", ", CollectionUtils.Distinct(ints)));

            var strings = new List<string> { "apple", "banana", "apple", "cherry", "banana" };
            Console.WriteLine("Distinct strings: " + string.Join(", ", CollectionUtils.Distinct(strings)));

            var words = new List<string> { "one", "two", "three", "four", "five", "six" };
            var grouped = CollectionUtils.GroupBy(words, word => word.Length);
            Console.WriteLine("Words grouped by length:");
            foreach (var pair in grouped)
            {
                Console.WriteLine($"  {pair.Key}: {string.Join(", ", pair.Value)}");
            }

            var firstCounts = new Dictionary<string, int>
            {
                ["apple"] = 2,
                ["banana"] = 1
            };
            var secondCounts = new Dictionary<string, int>
            {
                ["banana"] = 3,
                ["cherry"] = 1
            };
            var mergedCounts = CollectionUtils.Merge(firstCounts, secondCounts, (x, y) => x + y);
            Console.WriteLine("Merged word counts:");
            foreach (var pair in mergedCounts)
            {
                Console.WriteLine($"  {pair.Key}: {pair.Value}");
            }

            var productList = new List<Product>
            {
                new Product(4, "Monitor", 300m),
                new Product(5, "Gaming PC", 2200m),
                new Product(6, "Keyboard", 120m)
            };
            var mostExpensive = CollectionUtils.MaxBy(productList, product => product.Price);
            Console.WriteLine("Most expensive product: " + mostExpensive);
        }
    }
}
