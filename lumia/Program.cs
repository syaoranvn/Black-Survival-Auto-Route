using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

class Program
{
    public class GameData
    {
        public Dictionary<string, Dictionary<string, int>> Equipment { get; set; }
        public Dictionary<string, List<string>> Locations { get; set; }
    }

    static void Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;

        string filePath = "data.json";

        // Tạo file data.json mẫu nếu chưa tồn tại
        if (!File.Exists(filePath))
        {
            Console.WriteLine("File dữ liệu chưa tồn tại. Đang tạo file data.json mẫu...");

            var sampleData = new GameData
            {
                Equipment = new Dictionary<string, Dictionary<string, int>>
                {
                    { "sword_cross", new Dictionary<string, int> { { "long_sword", 1 }, { "iron_dust", 2 }, { "scrap_iron", 3 }, { "holy_cross", 1 } } },
                    { "helmet", new Dictionary<string, int> { { "iron_dust", 1 }, { "scrap_iron", 2 } } },
                    { "armor", new Dictionary<string, int> { { "iron_plate", 2 }, { "cloth", 1 } } },
                    { "shield", new Dictionary<string, int> { { "wood", 3 }, { "iron_dust", 2 }, { "leather", 1 } } }
                },
                Locations = new Dictionary<string, List<string>>
                {
                    { "dock", new List<string> { "iron_dust", "tree", "pen", "holy_cross" } },
                    { "church", new List<string> { "long_sword", "scrap_iron", "windbreaker", "cloth" } },
                    { "pond", new List<string> { "scrap_iron", "tv", "paper", "robe" } },
                    { "alley", new List<string> { "long_sword", "ballpoint_pen", "dice" } },
                    { "forest", new List<string> { "wood", "iron_dust", "leather" } }
                }
            };

            string jsonData = JsonConvert.SerializeObject(sampleData, Formatting.Indented);
            File.WriteAllText(filePath, jsonData, System.Text.Encoding.UTF8);

            Console.WriteLine("Đã tạo file data.json mẫu thành công!");
        }
        else
        {
            Console.WriteLine("File data.json đã tồn tại. Đang tải dữ liệu...");
        }

        // Tải dữ liệu từ file data.json
        var data = LoadData(filePath);

        // Nhập tên các trang bị
        Console.Write("Nhập tên các trang bị bạn muốn chế tạo (cách nhau bằng dấu phẩy, ví dụ: sword_cross,shield): ");
        string inputItems = Console.ReadLine()?.Trim();

        var itemNames = inputItems?.Split(',').Select(item => item.Trim()).ToList();

        if (itemNames == null || itemNames.Count == 0 || !itemNames.All(name => data.Equipment.ContainsKey(name)))
        {
            Console.WriteLine("Tên trang bị không hợp lệ hoặc không có trong dữ liệu.");
            return;
        }

        // Tính tổng số lượng nguyên liệu cần thiết cho tất cả các trang bị đã chọn
        Dictionary<string, int> totalMaterialsNeeded = new Dictionary<string, int>();
        foreach (var itemName in itemNames)
        {
            foreach (var material in data.Equipment[itemName])
            {
                if (totalMaterialsNeeded.ContainsKey(material.Key))
                {
                    totalMaterialsNeeded[material.Key] += material.Value;
                }
                else
                {
                    totalMaterialsNeeded[material.Key] = material.Value;
                }
            }
        }

        // Tìm các khu vực cần đi để thu thập đủ nguyên liệu và không lặp lại nguyên liệu
        var materialsCollected = new HashSet<string>();
        var uniqueLocationsToVisit = new Dictionary<string, List<string>>();

        foreach (var location in data.Locations)
        {
            var neededMaterialsInLocation = location.Value
                .Where(material => totalMaterialsNeeded.ContainsKey(material) && !materialsCollected.Contains(material))
                .ToList();

            if (neededMaterialsInLocation.Any())
            {
                uniqueLocationsToVisit[location.Key] = neededMaterialsInLocation;

                // Đánh dấu các nguyên liệu trong khu vực này là đã thu thập
                foreach (var material in neededMaterialsInLocation)
                {
                    materialsCollected.Add(material);
                }
            }
        }

        // Hiển thị kết quả
        Console.WriteLine("\nTổng số lượng nguyên liệu cần thiết:");
        foreach (var material in totalMaterialsNeeded)
        {
            Console.WriteLine($"{material.Key}: {material.Value}");
        }

        Console.WriteLine("\nCác khu vực tối ưu để thu thập nguyên liệu (không bao gồm nguyên liệu bị trùng):");
        foreach (var location in uniqueLocationsToVisit)
        {
            Console.WriteLine($"- {location.Key}: {string.Join(", ", location.Value)}");
        }
    }

    static GameData LoadData(string filePath)
    {
        string jsonData = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
        return JsonConvert.DeserializeObject<GameData>(jsonData);
    }
}
