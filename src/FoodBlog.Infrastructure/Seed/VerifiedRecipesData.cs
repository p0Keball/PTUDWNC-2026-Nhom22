using System.Collections.Generic;
using FoodBlog.Domain.Enums;

namespace FoodBlog.Infrastructure.Seed;

public class RecipeSeedItem
{
    public string Title { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int Servings { get; set; }
    public Difficulty Difficulty { get; set; }
    public RecipeStatus Status { get; set; } = RecipeStatus.Published;
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fat { get; set; }
    public decimal Fiber { get; set; }
    public decimal Sodium { get; set; }
    public List<IngredientSeedItem> Ingredients { get; set; } = new();
    public List<StepSeedItem> Steps { get; set; } = new();
    public List<ImageSeedItem> Images { get; set; } = new();
}

public class IngredientSeedItem
{
    public string Name { get; set; } = string.Empty;
    public decimal? Quantity { get; set; }
    public string? Unit { get; set; }
    public string? Notes { get; set; }

    public IngredientSeedItem(string name, decimal? quantity, string? unit, string? notes = null)
    {
        Name = name;
        Quantity = quantity;
        Unit = unit;
        Notes = notes;
    }
}

public class StepSeedItem
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int? TimerMinutes { get; set; }

    public StepSeedItem(string title, string description, int? timerMinutes = null)
    {
        Title = title;
        Description = description;
        TimerMinutes = timerMinutes;
    }
}

public class ImageSeedItem
{
    public string Url { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;

    public ImageSeedItem(string url, string altText)
    {
        Url = url;
        AltText = altText;
    }
}

public static class VerifiedRecipesData
{
    public static List<RecipeSeedItem> GetRecipes()
    {
        return new List<RecipeSeedItem>
        {
            // 1. Phở Bò Tái Nạm Hà Nội
            new()
            {
                Title = "Phở Bò Tái Nạm Hà Nội",
                CategoryName = "Món bò",
                Description = "Món quốc hồn quốc túy của ẩm thực Việt Nam với nước dùng xương bò ninh trong vắt, thơm nức hương hoa hồi quế gừng nướng, thịt nạm mềm ngậy và thăn bò tái ngọt thanh.",
                Instructions = "Ninh xương ống bò trong nhiều giờ lấy nước dùng thanh ngọt, nướng xém hành gừng và thảo mộc, chần bánh phở tươi, xếp thịt nạm thái mỏng cùng thăn bò tái, chan nước dùng sôi sùng sục và rắc ngập hành hoa rau mùi.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 180,
                Servings = 4,
                Difficulty = Difficulty.Hard,
                Status = RecipeStatus.Published,
                Calories = 480,
                Protein = 32,
                Carbohydrates = 58,
                Fat = 14,
                Fiber = 3,
                Sodium = 1150,
                Ingredients = new()
                {
                    new("Xương ống bò", 1.5m, "kg", "Chặt khúc, ngâm muối"),
                    new("Nạm bò", 400, "g", "Thịt nạc pha mỡ gân"),
                    new("Thịt thăn bò", 250, "g", "Dùng ăn tái, thái mỏng"),
                    new("Bánh phở tươi", 600, "g"),
                    new("Quế thanh", 2, "thanh"),
                    new("Hoa hồi", 4, "bông"),
                    new("Thảo quả", 2, "quả"),
                    new("Gừng già", 1, "củ", "Nướng cháy xém"),
                    new("Hành tây", 2, "củ", "Nướng xém vỏ"),
                    new("Hành tím", 5, "củ", "Nướng thơm"),
                    new("Hành lá và ngò gai", 100, "g", "Rửa sạch, cắt nhỏ"),
                    new("Nước mắm truyền thống", 4, "thìa canh"),
                    new("Đường phèn", 20, "g"),
                    new("Muối hạt", 1, "thìa canh")
                },
                Steps = new()
                {
                    new("Sơ chế và chần sạch xương bò", "Ngâm xương bò trong nước muối loãng 30 phút, chần qua nồi nước sôi trong 5 phút để khử sạch mùi hôi và bọt bẩn, vớt ra xả sạch dưới vòi nước lạnh.", 10),
                    new("Ninh xương lấy nước dùng trong", "Cho xương vào nồi lớn với 4 lít nước, đun sôi rồi hạ nhỏ lửa ninh liu riu liên tục, thường xuyên hớt sạch bọt nổi lên để nước dùng phở giữ được độ trong vắt tuyệt đối.", 180),
                    new("Nướng gia vị thảo mộc và hành gừng", "Nướng hoa hồi, quế, thảo quả, gừng và hành tây trên lửa than hoặc chảo gang đến khi dậy mùi thơm nức, cạo sạch muội than rồi cho vào túi vải buộc chặt thả vào nồi nước dùng.", 15),
                    new("Luộc chín nạm bò và thái thịt tái", "Thả nạm bò vào nồi nước dùng luộc chín mềm khoảng 60 phút, vớt ra ngâm nước đá lạnh rồi thái lát mỏng. Thịt thăn bò tái dùng dao sắc thái thật mỏng ngang thớ.", 60),
                    new("Nêm nếm nước dùng phở", "Nêm nước mắm cốt cá cơm ngon, đường phèn và muối hạt vào nồi nước dùng sao cho vị mặn ngọt thanh thoát hài hòa tự nhiên.", 10),
                    new("Chần bánh phở và trình bày hoàn chỉnh", "Chần bánh phở tươi qua nước sôi rồi chia đều vào các tô lớn. Xếp thịt nạm bò, thịt thăn bò tái, hành tây bào mỏng, rắc hành hoa ngò gai, chan ngập nước dùng sôi sùng sục và thưởng thức cùng chanh ớt tươi.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1582878826629-29b7ad1cdc43?auto=format&fit=crop&w=1200&q=80", "Tô phở bò tái nạm thơm ngon bốc khói nghi ngút"),
                    new("https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=1200&q=80", "Bánh phở tươi và các loại thảo mộc")
                }
            },

            // 2. Bún Bò Huế Cố Đô Chuẩn Vị
            new()
            {
                Title = "Bún Bò Huế Cố Đô Chuẩn Vị",
                CategoryName = "Món bò",
                Description = "Hương vị cay nồng đặc trưng đất cố đô với sợi bún to, nước dùng thơm nức sả ớt quyện cùng mắm ruốc Huế đậm đà, thịt bắp bò hoa giòn sần sật và chả cua béo ngậy.",
                Instructions = "Hầm xương bò và giò heo lấy nước ngọt, hòa mắm ruốc Huế lắng cặn chắt nước trong, phi thơm sả băm ớt màu, nấu sôi nêm nếm gia vị đậm đà và chan lên bát bún đầy ắp rau sống tươi ngon.",
                PrepTimeMinutes = 40,
                CookTimeMinutes = 120,
                Servings = 5,
                Difficulty = Difficulty.Hard,
                Status = RecipeStatus.Published,
                Calories = 560,
                Protein = 38,
                Carbohydrates = 62,
                Fat = 18,
                Fiber = 4,
                Sodium = 1350,
                Ingredients = new()
                {
                    new("Bắp bò hoa", 600, "g", "Thịt bắp hoa giòn ngọt"),
                    new("Nạm bò", 300, "g"),
                    new("Giò heo", 500, "g", "Chặt khoanh tròn"),
                    new("Bún sợi to Huế", 700, "g"),
                    new("Mắm ruốc Huế nguyên chất", 3, "thìa canh"),
                    new("Sả cây", 8, "cây", "Đập dập"),
                    new("Ớt bột sa tế màu điều", 3, "thìa canh"),
                    new("Tiết heo luộc", 200, "g", "Cắt miếng vuông"),
                    new("Chả cua hoặc chả lụa Huế", 200, "g"),
                    new("Rau muống chẻ và hoa chuối bào", 300, "g"),
                    new("Hành tây, chanh tươi, ớt hiểm", 100, "g")
                },
                Steps = new()
                {
                    new("Sơ chế bắp bò và giò heo", "Chần giò heo, nạm bò và bắp bò qua nước sôi có gừng và muối, xả sạch lại dưới vòi nước chảy để thịt săn chắc và sạch tạp chất.", 10),
                    new("Hầm thịt bò và giò heo với sả cây", "Cho giò heo và bắp bò vào nồi hầm cùng 6 cây sả đập dập. Hầm khoảng 45 phút cho thịt mềm tới, vớt bắp bò ra ngâm nước nguội rồi thái lát mỏng.", 60),
                    new("Pha và lắng mắm ruốc Huế", "Khuấy đều mắm ruốc Huế với 1 chén nước lạnh, để lắng cặn rồi gạn lấy phần nước trong thơm mùi ruốc cho vào nồi nước dùng.", 15),
                    new("Làm sa tế sả ớt màu điều", "Phi thơm sả băm nhỏ, ớt băm cùng dầu màu điều để tạo màu đỏ cam óng ánh và hương cay nồng đặc trưng cho nước lèo bún bò.", 10),
                    new("Hoàn thiện nước dùng và nêm gia vị", "Trút dầu màu điều sa tế vào nồi nước dùng, thả tiết heo, nêm muối, hạt nêm, đường phèn sao cho vị mặn ngọt cay hòa quyện cân bằng.", 20),
                    new("Trình bày bát bún bò Huế", "Trần bún sợi to cho vào tô, xếp bắp bò, nạm bò, giò heo, chả cua, rắc hành tây thái lát mỏng, chan nước dùng nóng hổi và dùng kèm đĩa rau sống bắp chuối bào.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=1200&q=80", "Tô bún bò Huế đỏ cam đậm đà quyến rũ")
                }
            },

            // 3. Cơm Tấm Sườn Bì Chả Sài Gòn
            new()
            {
                Title = "Cơm Tấm Sườn Bì Chả Sài Gòn",
                CategoryName = "Món heo",
                Description = "Món ăn đường phố biểu tượng của Sài Gòn với hạt gạo tấm dẻo thơm, miếng sườn cốt lết nướng mật ong vàng óng đượm khói than, bì giòn dai thính gạo và chả trứng hấp béo ngậy.",
                Instructions = "Ướp sườn cốt lết với sốt sả tỏi mật ong qua đêm, nướng trên than hoa chín tới, hấp chả trứng thịt mộc nhĩ vàng óng mặt lòng đỏ, trộn bì heo với thính gạo thơm, nấu cơm tấm và pha nước mắm tỏi ớt chua ngọt kẹo quánh.",
                PrepTimeMinutes = 45,
                CookTimeMinutes = 40,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 680,
                Protein = 42,
                Carbohydrates = 75,
                Fat = 24,
                Fiber = 2,
                Sodium = 980,
                Ingredients = new()
                {
                    new("Gạo tấm thơm", 400, "g"),
                    new("Sườn cốt lết heo có viền mỡ", 4, "miếng", "Khoảng 600g"),
                    new("Bì heo luộc thái sợi", 150, "g"),
                    new("Thính gạo rang vàng", 40, "g"),
                    new("Thịt nạc vai xay", 200, "g"),
                    new("Trứng gà tươi", 4, "quả"),
                    new("Mộc nhĩ ngâm nở", 30, "g", "Thái chỉ"),
                    new("Miến dong", 30, "g", "Ngâm mềm cắt ngắn"),
                    new("Mật ong nguyên chất", 2, "thìa canh"),
                    new("Nước mắm ngon", 3, "thìa canh"),
                    new("Mỡ hành lá và đồ chua cà rốt", 100, "g")
                },
                Steps = new()
                {
                    new("Ướp sườn cốt lết thấm đượm gia vị", "Dùng búa dần mềm thịt, ướp sườn với mật ong, nước mắm, dầu hào, hành tỏi băm nhuyễn, tiêu và một thìa sữa đặc trong ít nhất 2 tiếng.", 120),
                    new("Làm chả trứng hấp chuẩn vị", "Trộn đều thịt xay, mộc nhĩ, miến, 3 quả trứng gà và gia vị. Cho vào khuôn hấp chín trong 25 phút, sau đó quét 1 lòng đỏ trứng lên mặt và hấp mở nắp thêm 5 phút cho vàng đẹp.", 30),
                    new("Trộn bì heo thính gạo giòn thơm", "Bì heo luộc chín bóp muối xả sạch, thái sợi thật mỏng, để ráo rồi trộn đều cùng thính gạo thơm, tỏi phi vàng và chút bột nêm.", 15),
                    new("Nấu cơm tấm và làm mỡ hành", "Vo sạch gạo tấm, nấu với lượng nước vừa phải cho cơm chín xốp tơi. Phi hành lá cắt nhỏ với mỡ heo nóng sôi để có chén mỡ hành xanh mướt.", 35),
                    new("Nướng sườn trên than hoa vàng thơm", "Đặt sườn lên vỉ than hoa, nướng trở mặt liên tục và phết nước ướp thường xuyên để sườn chín mọng nước, xém cạnh thơm phức.", 20),
                    new("Trình bày đĩa cơm tấm Sài Gòn", "Xới cơm tấm ra đĩa, đặt miếng sườn nướng mọng nước, lát chả trứng vàng ươm, nhúm bì heo, rưới mỡ hành, ăn kèm đồ chua dưa leo và chén nước mắm ớt chua ngọt.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1565299585323-38d6b0865b47?auto=format&fit=crop&w=1200&q=80", "Đĩa cơm tấm sườn bì chả Sài Gòn thơm nức")
                }
            },

            // 4. Bánh Mì Thịt Nướng Giòn Rụm
            new()
            {
                Title = "Bánh Mì Thịt Nướng Giòn Rụm",
                CategoryName = "Món ăn sáng",
                Description = "Chiếc bánh mì kẹp giòn tan trứ danh với nhân thịt heo nướng xiên thơm nức mũi, pate gan mịn béo, sốt bơ trứng vàng ngậy cùng rau mùi và đồ chua thanh mát chống ngấy.",
                Instructions = "Thịt nạc vai heo ướp sả ớt mật ong nướng than xém vàng, nướng bánh mì nóng giòn, quết đẫm pate và sốt bơ, kẹp thịt nướng nóng hổi, dưa leo, rau răm, đồ chua và chan chút nước sốt đậm đà.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 25,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 450,
                Protein = 26,
                Carbohydrates = 48,
                Fat = 17,
                Fiber = 3,
                Sodium = 720,
                Ingredients = new()
                {
                    new("Bánh mì vỏ giòn", 4, "ổ"),
                    new("Thịt nạc dăm heo", 400, "g", "Thái mỏng"),
                    new("Sả băm nhuyễn", 3, "thìa canh"),
                    new("Tỏi và hành tím băm", 2, "thìa canh"),
                    new("Mật ong nguyên chất", 1.5m, "thìa canh"),
                    new("Nước tương và dầu hào", 2, "thìa canh"),
                    new("Pate gan heo truyền thống", 100, "g"),
                    new("Bơ trứng gà sốt béo", 50, "g"),
                    new("Dưa leo, rau mùi, ớt sừng", 100, "g"),
                    new("Đồ chua củ cải cà rốt ngâm giấm", 100, "g")
                },
                Steps = new()
                {
                    new("Thái mỏng thịt heo và ướp gia vị", "Thịt nạc dăm rửa sạch, thái lát mỏng vừa ăn. Ướp thịt với sả băm, hành tỏi, dầu hào, mật ong, nước tương và tiêu trong 45 phút.", 45),
                    new("Xiên que và nướng thịt chín vàng", "Xiên thịt vào que tre, nướng trên bếp than hoặc nồi chiên không dầu ở 180°C trong 15 phút, trở mặt cho thịt chín vàng đều óng ả.", 18),
                    new("Chuẩn bị bánh mì giòn nóng", "Bánh mì nướng qua lò nướng hoặc áp chảo 2-3 phút cho vỏ bánh đạt độ giòn rụm hoàn hảo.", 5),
                    new("Kẹp nhân bánh mì hoàn chỉnh", "Rạch dọc ổ bánh mì, quét lớp pate gan béo ngậy và sốt bơ trứng, xếp thịt nướng xiên nóng hổi, kẹp dưa leo thái lát, đồ chua, ngò rí và vài lát ớt cay.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1599488615731-7e5c2823ff28?auto=format&fit=crop&w=1200&q=80", "Ổ bánh mì thịt nướng giòn rụm đầy ắp nhân")
                }
            },

            // 5. Bánh Xèo Miền Tây Tôm Thịt Giòn Rụm
            new()
            {
                Title = "Bánh Xèo Miền Tây Tôm Thịt Giòn Rụm",
                CategoryName = "Món chính",
                Description = "Chiếc bánh xèo vàng ươm nghệ tươi, mỏng giòn rụm đặc trưng miền Tây Nam Bộ, cuộn cùng tôm đất tươi ngọt, thịt ba chỉ xém cạnh, giá đỗ và cả rổ rau rừng tươi ngon.",
                Instructions = "Pha bột gạo với nước cốt dừa và bột nghệ, tráng mỏng trên chảo gang lửa lớn kêu xèo xèo vui tai, cho tôm thịt giá đỗ đậy nắp cho chín giòn, gập đôi bánh và ăn kèm rau cải bẹ xanh, nước mắm chua ngọt.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 30,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 520,
                Protein = 28,
                Carbohydrates = 52,
                Fat = 22,
                Fiber = 4,
                Sodium = 850,
                Ingredients = new()
                {
                    new("Bột bánh xèo pha sẵn", 400, "g"),
                    new("Nước cốt dừa béo", 250, "ml"),
                    new("Bột nghệ vàng", 1, "thìa cà phê"),
                    new("Hành lá cắt nhỏ", 50, "g"),
                    new("Tôm đất tươi", 300, "g", "Cắt râu"),
                    new("Thịt ba chỉ heo thái mỏng", 300, "g"),
                    new("Giá đỗ tươi", 300, "g"),
                    new("Đậu xanh cà vỏ hấp chín", 100, "g"),
                    new("Rau cải bẹ xanh, xà lách, rau thơm", 300, "g"),
                    new("Nước mắm tỏi ớt chua ngọt miền Tây", 1, "bát")
                },
                Steps = new()
                {
                    new("Pha bột bánh xèo béo ngậy", "Khuấy đều bột bánh xèo, bột nghệ, nước cốt dừa và nước lọc theo tỉ lệ hướng dẫn, thêm hành lá cắt nhỏ, để bột nghỉ 30 phút.", 30),
                    new("Xào sơ tôm thịt", "Cho chút dầu vào chảo gang, xào tôm và thịt ba chỉ xém cạnh cho thơm, nêm chút hạt nêm và tiêu.", 8),
                    new("Đổ bánh xèo lửa lớn", "Làm nóng chảo với chút dầu ăn, múc một muôi bột tráng đều quanh chảo thật mỏng tạo tiếng xèo lớn, thêm tôm thịt, đậu xanh hấp và giá đỗ vào giữa.", 3),
                    new("Đậy nắp và chiên bánh giòn tan", "Đậy nắp chảo 2 phút cho giá chín hơi, sau đó mở nắp rưới chút dầu quanh vành bánh, chiên lửa nhỏ đến khi vành bánh cong lên và giòn rụm thì gập đôi lại.", 6),
                    new("Thưởng thức cùng rau rừng và nước mắm", "Dùng kéo cắt bánh xèo thành miếng vừa ăn, cuốn cùng lá cải bẹ xanh, rau thơm rồi chấm ngập trong nước mắm tỏi ớt cà rốt chua ngọt.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1626777552726-4a6b54c97e46?auto=format&fit=crop&w=1200&q=80", "Bánh xèo miền Tây vàng giòn óng ả")
                }
            },

            // 6. Bún Chả Hà Nội Nướng Than Hoa
            new()
            {
                Title = "Bún Chả Hà Nội Nướng Than Hoa",
                CategoryName = "Món nướng",
                Description = "Hương vị nức tiếng đất Thăng Long với từng miếng chả băm và chả miếng nạc dăm nướng than hoa thơm lừng cháy cạnh, thả trong bát nước chấm đu đủ cà rốt ấm nóng ngọt thanh.",
                Instructions = "Ướp thịt ba chỉ thái lát mỏng và thịt nạc vai băm viên với nước màu, nước mắm, hành tỏi băm. Kẹp vỉ nướng trên than hoa đượm lửa, pha nước mắm ấm chua ngọt dịu, ăn kèm bún tươi và rau sống kinh giới tía tô.",
                PrepTimeMinutes = 40,
                CookTimeMinutes = 30,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 580,
                Protein = 34,
                Carbohydrates = 55,
                Fat = 24,
                Fiber = 3,
                Sodium = 920,
                Ingredients = new()
                {
                    new("Thịt ba chỉ heo", 400, "g", "Thái lát mỏng"),
                    new("Thịt nạc vai băm nhuyễn", 400, "g"),
                    new("Bún tươi sợi nhỏ", 600, "g"),
                    new("Đu đủ xanh và cà rốt tỉa hoa", 200, "g"),
                    new("Hành khô và tỏi băm", 50, "g"),
                    new("Nước hàng caramen", 2, "thìa canh"),
                    new("Nước mắm ngon và dầu hào", 4, "thìa canh"),
                    new("Giấm gạo và đường cát", 3, "thìa canh"),
                    new("Rau sống: tía tô, kinh giới, xà lách", 200, "g")
                },
                Steps = new()
                {
                    new("Ướp chả miếng và chả viên riêng biệt", "Thịt ba chỉ thái mỏng ướp hành tỏi, nước màu, nước mắm, dầu hào. Thịt nạc vai băm ướp gia vị tương tự rồi nặn thành từng viên tròn dẹt vừa ăn.", 60),
                    new("Làm dưa góp đu đủ cà rốt giòn ngọt", "Đu đủ xanh và cà rốt thái lát mỏng bóp muối cho giòn, xả sạch rồi ngâm với giấm đường và tỏi ớt tạo vị chua ngọt thanh mát.", 20),
                    new("Nướng chả trên than hoa rực hồng", "Kẹp chả vào vỉ sắt nướng trên than hoa, lật đều tay liên tục đến khi chả chín vàng ươm, xém cạnh và chảy mỡ xèo xèo thơm ngát.", 25),
                    new("Pha nước chấm bún chả ấm nóng", "Đun sôi hỗn hợp nước lọc, nước mắm, đường, giấm theo tỷ lệ vàng (5 nước : 1 mắm : 1 đường : 1 giấm), giữ ấm, khi ăn múc ra bát thả chả nướng và dưa góp vào.", 10),
                    new("Trình bày và thưởng thức", "Bày bún tươi ra đĩa cùng rổ rau kinh giới tía tô xanh non, gắp bún chấm vào bát nước mắm chả nóng hổi thơm phức.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=1200&q=80", "Bún chả Hà Nội nướng than hoa thơm lừng")
                }
            },

            // 7. Gỏi Cuốn Tôm Thịt Nước Tương Đậu Phộng
            new()
            {
                Title = "Gỏi Cuốn Tôm Thịt Nước Tương Đậu Phộng",
                CategoryName = "Món khai vị",
                Description = "Món ăn thanh đạm, lành mạnh xếp hàng đầu ẩm thực thế giới với tôm đỏ au, thịt ba chỉ luộc ngọt nước, bún tươi và hẹ xanh cuộn chặt trong bánh tráng dẻo dai.",
                Instructions = "Luộc chín tôm và thịt heo thái mỏng, làm ướt bánh tráng, xếp lần lượt xà lách, rau thơm, bún, thịt heo và tôm úp mặt đỏ ra ngoài, cuộn chặt tay kèm nhánh hẹ xanh, chấm sốt tương đen bơ đậu phộng béo bùi.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 20,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 320,
                Protein = 22,
                Carbohydrates = 38,
                Fat = 8,
                Fiber = 3,
                Sodium = 540,
                Ingredients = new()
                {
                    new("Bánh tráng dẻo", 20, "cái"),
                    new("Tôm sú tươi sống", 300, "g"),
                    new("Thịt ba chỉ heo", 300, "g"),
                    new("Bún tươi sợi nhỏ", 300, "g"),
                    new("Hẹ lá tươi", 100, "g"),
                    new("Xà lách, húng quế, ngò rí", 200, "g"),
                    new("Tương đen hoisin", 4, "thìa canh"),
                    new("Bơ đậu phộng mịn", 2, "thìa canh"),
                    new("Đậu phộng rang giã dập", 30, "g")
                },
                Steps = new()
                {
                    new("Luộc tôm và thịt ba chỉ giòn ngọt", "Luộc tôm chín đỏ với chút muối, vớt ra lột vỏ chẻ đôi sống lưng. Thịt ba chỉ luộc chín mềm vớt ngâm nước đá rồi thái lát mỏng.", 20),
                    new("Nấu sốt chấm tương đậu phộng béo bùi", "Phi thơm tỏi băm, cho tương đen, bơ đậu phộng và chút nước lọc đun sôi liu riu cho sốt sánh mịn, múc ra chén rắc đậu phộng rang giã dập.", 8),
                    new("Cuộn gỏi cuốn khéo léo", "Thoa nhẹ nước ấm lên bánh tráng cho mềm, xếp xà lách, rau thơm, bún, lát thịt heo. Gấp một mép bánh tráng, đặt 2-3 con tôm lên rồi cuộn chặt, chèn cọng hẹ thò đầu ra ngoài.", 15),
                    new("Trình bày và thưởng thức", "Xếp các cuốn gỏi đều tăm tắp lên đĩa, chấm ngập chén tương đậu phộng béo ngậy thanh mát.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1541544741938-0af808871cc0?auto=format&fit=crop&w=1200&q=80", "Đĩa gỏi cuốn tôm thịt thanh đạm đẹp mắt")
                }
            },

            // 8. Bò Kho Nước Dừa Chấm Bánh Mì
            new()
            {
                Title = "Bò Kho Nước Dừa Chấm Bánh Mì",
                CategoryName = "Món bò",
                Description = "Món ăn ấm lòng với từng khúc thịt bò nạm gân hầm mềm rục ngấm vị sốt cà chua, sả cây, hoa hồi và nước dừa xiêm ngọt thanh, nước sốt sánh mịn chấm bánh mì giòn rụm.",
                Instructions = "Ướp thịt bò với bột bò kho và gia vị, xào săn thơm với sả gừng, châm nước dừa xiêm hầm lửa nhỏ cho bò mềm nhừ, thêm cà rốt ninh chín tới, hòa chút bột năng cho nước sốt sánh sánh óng ả.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 75,
                Servings = 5,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 540,
                Protein = 38,
                Carbohydrates = 28,
                Fat = 26,
                Fiber = 4,
                Sodium = 860,
                Ingredients = new()
                {
                    new("Nạm bò pha gân giòn", 800, "g", "Cắt miếng vuông 3cm"),
                    new("Nước dừa tươi xiêm", 700, "ml"),
                    new("Cà rốt tỉa hoa", 2, "củ", "Cắt khúc dày"),
                    new("Gói gia vị bột bò kho", 1, "gói", "25g"),
                    new("Sả cây đập dập", 5, "cây"),
                    new("Gừng, tỏi, hành tím băm", 40, "g"),
                    new("Dầu màu điều", 2, "thìa canh"),
                    new("Nước mắm và đường phèn", 3, "thìa canh"),
                    new("Bột năng", 1, "thìa canh", "Hòa tan với nước"),
                    new("Bánh mì giòn ăn kèm", 5, "ổ")
                },
                Steps = new()
                {
                    new("Sơ chế và ướp thịt bò đậm vị", "Thịt bò khử mùi bằng rượu gừng, thái khối vuông 3x3 cm. Ướp thịt với bột bò kho, dầu màu điều, hành tỏi gừng băm, nước mắm, đường trong 40 phút.", 40),
                    new("Xào săn thịt bò dậy hương thơm", "Làm nóng nồi với chút dầu, phi thơm sả cây đập dập rồi trút thịt bò vào đảo lửa lớn đến khi thịt săn chắc và dậy mùi thơm ngào ngạt.", 10),
                    new("Hầm thịt bò với nước dừa tươi", "Châm nước dừa tươi ngập thịt, đun sôi rồi hạ nhỏ lửa đậy nắp hầm liu riu khoảng 50 phút đến khi thịt bò mềm gân dẻo dai.", 50),
                    new("Nấu cà rốt và tạo độ sánh cho nước sốt", "Thả cà rốt vào ninh cùng thịt thêm 15 phút cho chín mềm ngọt. Hòa tan chút bột năng với nước rót từ từ vào khuấy đều cho sốt đạt độ sánh sệt sánh mịn.", 15),
                    new("Trình bày món bò kho", "Múc bò kho ra tô sâu lòng, rắc rau húng quế, ngò gai thái nhỏ và tiêu hạt, thưởng thức cùng bánh mì nóng giòn hoặc hủ tiếu dai.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1534939561126-855b8675edd7?auto=format&fit=crop&w=1200&q=80", "Tô bò kho nước dừa sánh sệt bốc khói")
                }
            },

            // 9. Gà Nướng Muối Ớt Da Giòn
            new()
            {
                Title = "Gà Nướng Muối Ớt Da Giòn",
                CategoryName = "Món gà",
                Description = "Thịt gà thả vườn chắc ngọt được phết sốt muối ớt cay xè thơm nức, nướng chín vàng ươm da giòn rụm bên ngoài nhưng thịt bên trong mọng nước ngọt đậm đà.",
                Instructions = "Mổ phanh gà hình cánh bướm, ướp gà với hỗn hợp muối ớt sả tỏi chắt nước cốt, nướng trên than hoa hoặc nồi chiên không dầu ở nhiệt độ chuẩn, quét dầu màu điều cho màu da vàng óng.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 50,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 490,
                Protein = 45,
                Carbohydrates = 4,
                Fat = 28,
                Fiber = 1,
                Sodium = 820,
                Ingredients = new()
                {
                    new("Gà ta nguyên con", 1.5m, "kg", "Mổ phanh cánh bướm"),
                    new("Ớt hiểm đỏ", 5, "quả", "Băm nhuyễn"),
                    new("Muối hạt giã dập", 2, "thìa cà phê"),
                    new("Sả băm nhuyễn và tỏi băm", 3, "thìa canh"),
                    new("Dầu màu điều", 2, "thìa canh"),
                    new("Mật ong nguyên chất", 1, "thìa canh"),
                    new("Nước mắm và hạt nêm", 2, "thìa canh"),
                    new("Rau răm và muối tiêu chanh", 100, "g")
                },
                Steps = new()
                {
                    new("Sơ chế gà sạch sẽ mổ phanh", "Rửa sạch gà với muối và gừng khử mùi hôi. Dùng dao rạch dọc ức gà mổ phanh ép phẳng xương để gà chín đều khi nướng.", 15),
                    new("Pha sốt ướp muối ớt cay nồng", "Giã nhuyễn ớt hiểm, muối hạt, sả, tỏi, nước mắm, mật ong và dầu màu điều tạo thành hỗn hợp sốt đỏ cam óng ánh.", 10),
                    new("Thoa sốt và ướp gà", "Thoa đều sốt muối ớt khắp mình gà từ trong ra ngoài, mát-xa nhẹ nhàng cho gia vị ngấm sâu vào từng thớ thịt trong 1 giờ.", 60),
                    new("Nướng gà chín da giòn vàng rộm", "Nướng gà trên bếp than hoa hoặc lò nướng 190°C trong 40 phút, thỉnh thoảng phết thêm dầu điều cho da gà giòn căng bóng không bị khô.", 40),
                    new("Chặt gà và thưởng thức", "Chặt gà thành miếng vừa ăn xếp ra đĩa, trang trí rau răm tươi, chấm cùng muối ớt chanh chua cay mặn ngọt.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1598103442097-8b74394b95c6?auto=format&fit=crop&w=1200&q=80", "Gà nướng muối ớt da giòn vàng óng mọng nước")
                }
            },

            // 10. Cá Kho Tộ Đậm Đà Chuẩn Vị Nam Bộ
            new()
            {
                Title = "Cá Kho Tộ Đậm Đà Chuẩn Vị Nam Bộ",
                CategoryName = "Món hải sản",
                Description = "Khoanh cá lóc tươi ngon được kho trong nồi đất đun liu riu với nước màu dừa, thịt ba chỉ béo ngậy, tiêu sọ cay nồng và ớt hiểm, nước sốt sánh kẹo ăn cùng cơm nóng.",
                Instructions = "Cá rửa sạch cắt khoanh ướp nước mắm nhĩ và nước màu dừa, phi thơm hành tỏi rán sơ thịt ba chỉ, xếp cá vào tộ đất đun lửa lớn rồi hạ nhỏ lửa kho đến khi nước cá keo quánh lại.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 45,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 380,
                Protein = 32,
                Carbohydrates = 8,
                Fat = 22,
                Fiber = 0,
                Sodium = 980,
                Ingredients = new()
                {
                    new("Cá lóc tươi", 700, "g", "Cắt khoanh dày 2.5cm"),
                    new("Thịt ba chỉ heo", 150, "g", "Thái con chì"),
                    new("Nước màu dừa Bến Tre", 1.5m, "thìa canh"),
                    new("Nước mắm cá cơm truyền thống", 4, "thìa canh"),
                    new("Hành tím và tỏi băm", 40, "g"),
                    new("Ớt hiểm nguyên trái", 5, "quả"),
                    new("Tiêu sọ đập dập", 1, "thìa cà phê"),
                    new("Đường cát và hạt nêm", 2, "thìa canh"),
                    new("Nước dừa tươi", 150, "ml")
                },
                Steps = new()
                {
                    new("Sơ chế cá khử sạch nhớt", "Đánh sạch vảy cá, dùng muối hột và chanh tươi chà xát khử nhớt và mùi tanh, cắt khoanh dày khoảng 2.5 cm để ráo nước.", 15),
                    new("Ướp cá thấm đượm gia vị", "Ướp cá với nước màu dừa, 3 thìa nước mắm ngon, đường, hành tỏi băm và hạt nêm trong 30 phút cho cá cứng thịt và lên màu đẹp.", 30),
                    new("Xào thịt ba chỉ tiết mỡ", "Cho thịt ba chỉ vào tộ đất đảo đều đến khi thịt xém cạnh và tiết mỡ thơm lừng, vớt bớt mỡ thừa ra chén.", 7),
                    new("Kho cá lửa liu riu", "Xếp các khoanh cá vào tộ đất cùng thịt ba chỉ, rưới toàn bộ nước ướp và nước dừa tươi, đun sôi bùng rồi hạ lửa nhỏ nhất đậy hờ nắp đun liu riu.", 35),
                    new("Hoàn thiện tộ cá kho", "Khi nước kho sánh kẹo lại màu cánh gián bóng mượt, thả ớt hiểm nguyên trái, đầu hành lá chẻ và tiêu sọ đập dập vào, đun thêm 2 phút rồi tắt bếp.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1519708227418-c8fd9a32b7a2?auto=format&fit=crop&w=1200&q=80", "Niêu cá kho tộ đậm đà óng ánh bốc khói")
                }
            },

            // 11. Canh Chua Cá Lóc Nam Bộ
            new()
            {
                Title = "Canh Chua Cá Lóc Nam Bộ",
                CategoryName = "Món canh",
                Description = "Bát canh chua giải nhiệt đậm đà phong vị miền Tây với vị chua thanh của me chín, vị ngọt tự nhiên của thịt cá lóc đồng, dứa thơm, bạc hà (dọc mùng), đậu bắp và ngò ôm thơm nức.",
                Instructions = "Nấu nước sôi dầm me chắt lấy nước cốt chua thanh, thả cá lóc vào nấu chín vớt ra, cho dứa, cà chua, đậu bắp, bạc hà vào đun sôi chín tới, nêm nước mắm nhĩ và rắc ngò ôm ngò gai, tỏi phi thơm phức.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 20,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 240,
                Protein = 26,
                Carbohydrates = 18,
                Fat = 5,
                Fiber = 4,
                Sodium = 680,
                Ingredients = new()
                {
                    new("Cá lóc đồng", 600, "g", "Cắt khúc vừa ăn"),
                    new("Me vắt chín", 50, "g"),
                    new("Dứa chín thái lát", 0.25m, "quả"),
                    new("Cà chua thái múi cau", 2, "quả"),
                    new("Đậu bắp thái vát", 6, "quả"),
                    new("Bạc hà (dọc mùng)", 1, "cây", "Tước vỏ, bóp muối"),
                    new("Giá đỗ tươi", 100, "g"),
                    new("Ngò ôm và ngò gai cắt nhỏ", 30, "g"),
                    new("Tỏi phi vàng giòn", 2, "thìa canh"),
                    new("Nước mắm ngon và đường phèn", 3, "thìa canh")
                },
                Steps = new()
                {
                    new("Sơ chế cá lóc và rau củ", "Cá lóc xát muối chanh rửa sạch, cắt khúc. Bạc hà tước vỏ bóp nhẹ muối rồi xả sạch vắt ráo. Dứa, cà chua, đậu bắp thái miếng vừa ăn.", 15),
                    new("Nấu nước dùng me chua thanh", "Đun sôi 1.2 lít nước, múc nước nóng dầm me vắt trong rây lọc lấy nước cốt chua đổ vào nồi nước canh.", 7),
                    new("Luộc cá lóc chín ngọt", "Thả cá lóc vào nồi nước sôi nấu khoảng 7 phút cho cá chín tới rồi vớt nhẹ ra đĩa để cá không bị nát vụn.", 7),
                    new("Nấu các loại rau canh chua", "Lần lượt cho dứa, cà chua vào đun sôi lại, tiếp tục cho đậu bắp, bạc hà và giá đỗ vào, nêm nước mắm cốt và đường phèn cho vị chua ngọt hài hòa.", 5),
                    new("Trình bày tô canh chua cá lóc", "Múc rau và nước canh nóng hổi ra tô lớn, đặt khúc cá lóc lên trên, rắc nhiều ngò ôm, ngò gai, ớt tươi và rưới muỗng tỏi phi vàng ươm lên mặt.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=1200&q=80", "Tô canh chua cá lóc nóng hổi thanh mát")
                }
            },

            // 12. Lẩu Thái Hải Sản Chua Cay Thập Cẩm
            new()
            {
                Title = "Lẩu Thái Hải Sản Chua Cay Thập Cẩm",
                CategoryName = "Món lẩu",
                Description = "Nồi lẩu bốc khói nghi ngút với nước dùng Tom Yum chua cay đậm đà dậy mùi sả, lá chanh chúc kaffir, ớt hiểm và nước cốt dừa béo ngậy, nhúng ngập tràn tôm sú, mực tươi, nghêu và nấm.",
                Instructions = "Xào thơm sả ớt riềng lá chanh cùng gói sốt Tom Yum, ninh nước xương gà làm ngọt, châm nước cốt dừa và nước cốt me, nêm nếm gia vị vừa miệng và bày nồi lẩu nhúng hải sản, rau muống, nấm tươi.",
                PrepTimeMinutes = 35,
                CookTimeMinutes = 30,
                Servings = 6,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 420,
                Protein = 36,
                Carbohydrates = 24,
                Fat = 16,
                Fiber = 5,
                Sodium = 1200,
                Ingredients = new()
                {
                    new("Tôm sú tươi sống", 400, "g"),
                    new("Mực ống khía vảy rồng", 300, "g"),
                    new("Nghêu sống sạch cát", 500, "g"),
                    new("Thịt ba chỉ bò Mỹ nhúng lẩu", 300, "g"),
                    new("Nấm rơm, nấm kim châm, nấm đùi gà", 300, "g"),
                    new("Cà chua và dứa chín", 200, "g"),
                    new("Sả cây, riềng củ, lá chanh", 60, "g"),
                    new("Nước cốt dừa", 100, "ml"),
                    new("Gia vị lẩu Thái Tom Yum", 1, "hũ"),
                    new("Rau muống cọng và mì tôm ăn kèm", 400, "g")
                },
                Steps = new()
                {
                    new("Sơ chế hải sản và nấm tươi", "Tôm cắt râu rửa sạch, mực khía vảy rồng thái khoanh, nghêu rửa sạch vỏ. Nấm cắt gốc ngâm nước muối loãng vớt ráo.", 20),
                    new("Nấu nước lẩu Thái thơm nồng", "Phi thơm sả đập dập, riềng lát, lá chanh và ớt hiểm với dầu ăn. Cho sốt Tom Yum, cà chua và dứa vào xào thơm lên màu đỏ rực rỡ.", 8),
                    new("Ninh nước lèo và nêm nếm chua cay", "Châm 2.5 lít nước hầm xương vào đun sôi, rót nước cốt me và nước cốt dừa béo ngậy, nêm nước mắm cốt, đường phèn cho tròn vị chua cay béo ngọt.", 15),
                    new("Chuẩn bị bếp lẩu và thưởng thức", "Đặt nồi nước lẩu lên bếp gas mini đun sôi sùng sục, nhúng tôm mực nghêu bò cùng nấm và các loại rau xanh tươi giòn, ăn kèm bún tươi hoặc mì.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1546069901-ba9599a7e63c?auto=format&fit=crop&w=1200&q=80", "Nồi lẩu Thái hải sản chua cay nóng hổi")
                }
            },

            // 13. Chả Giò Tôm Thịt Rế Giòn Rụm
            new()
            {
                Title = "Chả Giò Tôm Thịt Rế Giòn Rụm",
                CategoryName = "Món khai vị",
                Description = "Cuốn chả giò bọc bánh tráng rế vàng ươm giòn tan rôm rốp, nhân tôm thịt tươi ngọt quyện cùng khoai môn bùi bùi, mộc nhĩ giòn sần sật, chấm nước mắm ớt chua ngọt khó cưỡng.",
                Instructions = "Trộn nhân tôm thịt xay cùng khoai môn bào sợi, miến, mộc nhĩ và lòng đỏ trứng. Cuộn bánh tráng rế đều tay, chiên ngập dầu lửa vừa đến khi vỏ giòn tan vàng rộm, vớt ráo dầu.",
                PrepTimeMinutes = 35,
                CookTimeMinutes = 25,
                Servings = 5,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 360,
                Protein = 18,
                Carbohydrates = 26,
                Fat = 21,
                Fiber = 2,
                Sodium = 620,
                Ingredients = new()
                {
                    new("Bánh tráng rế", 30, "cái"),
                    new("Thịt nạc vai heo xay nhuyễn", 350, "g"),
                    new("Tôm sú băm hạt lựu", 200, "g"),
                    new("Khoai môn bào sợi vắt ráo", 150, "g"),
                    new("Mộc nhĩ ngâm nở thái chỉ", 40, "g"),
                    new("Miến dong ngâm mềm cắt ngắn", 40, "g"),
                    new("Lòng đỏ trứng gà", 2, "quả"),
                    new("Hành tím băm và tiêu xay", 30, "g"),
                    new("Dầu ăn chiên chả", 500, "ml")
                },
                Steps = new()
                {
                    new("Chuẩn bị và trộn nhân chả giò", "Trộn đều thịt xay, tôm băm, khoai môn bào sợi, miến dong, mộc nhĩ, lòng đỏ trứng gà, hành tím và 1 thìa cà phê tiêu cho nhân quyện dẻo.", 15),
                    new("Cuộn chả giò bánh tráng rế", "Đặt bánh tráng rế lên thớt phẳng, múc lượng nhân vừa phải đặt vào 1/3 mép bánh, gấp hai bên mép lại rồi cuộn tròn chặt vừa tay.", 20),
                    new("Bí quyết chiên chả giò giòn lâu", "Đun nóng dầu ăn lửa vừa, thả chả giò vào chiên ngập dầu. Chiên 2 lần: lần 1 chiên sơ se mặt, lần 2 chiên lửa lớn cho chả vàng rộm và giòn tan.", 15),
                    new("Vớt ráo dầu và dọn đĩa", "Vớt chả giò ra rổ có lót giấy thấm dầu, xếp lên đĩa kèm xà lách rau thơm và bát nước mắm tỏi ớt cà rốt chua ngọt.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1563379091339-03b21ab4a4f8?auto=format&fit=crop&w=1200&q=80", "Đĩa chả giò chiên vàng giòn rụm bắt mắt")
                }
            },

            // 14. Mì Quảng Tôm Thịt Trứng Cút
            new()
            {
                Title = "Mì Quảng Tôm Thịt Trứng Cút",
                CategoryName = "Món chính",
                Description = "Đặc sản xứ Quảng trứ danh với sợi mì gạo vàng nghệ mềm dai, tôm thịt rim đậm đà ánh màu điều, nước nhưn xâm xấp ngọt sắc vị xương, rắc đậu phộng rang giã dập và bánh tráng mè nướng giòn.",
                Instructions = "Sợi mì Quảng ươm nghệ, tôm thịt ba chỉ ướp củ nén đập dập rim thấm vị, nấu nước nhưn thơm nức, chan nước lèo xâm xấp đáy tô, rắc đậu phộng và bẻ bánh tráng mè giòn rụm.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 35,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 510,
                Protein = 32,
                Carbohydrates = 60,
                Fat = 16,
                Fiber = 3,
                Sodium = 890,
                Ingredients = new()
                {
                    new("Mì Quảng sợi vàng tươi", 600, "g"),
                    new("Tôm đất tươi", 300, "g"),
                    new("Thịt ba chỉ heo thái mỏng", 300, "g"),
                    new("Trứng cút luộc bóc vỏ", 12, "quả"),
                    new("Củ nén giã nát", 50, "g"),
                    new("Dầu phộng nguyên chất", 3, "thìa canh"),
                    new("Dầu màu điều", 2, "thìa canh"),
                    new("Nước mắm nhĩ và hạt nêm", 3, "thìa canh"),
                    new("Đậu phộng rang vàng giã dập", 50, "g"),
                    new("Bánh tráng mè nướng giòn", 2, "cái"),
                    new("Rau bắp chuối bào, cải con, húng lủi", 250, "g")
                },
                Steps = new()
                {
                    new("Ướp tôm thịt với củ nén dậy mùi", "Tôm cắt râu giữ vỏ, thịt ba chỉ thái mỏng. Ướp cùng củ nén giã dập, ớt bột, nước mắm, tiêu trong 30 phút cho thấm gia vị.", 30),
                    new("Phi củ nén với dầu phụng và rim nhân", "Đun nóng dầu phụng trong nồi, phi thơm củ nén băm cho dậy mùi nức mũi, trút tôm thịt và trứng cút vào xào săn kẹo lại.", 12),
                    new("Nấu nước nhưn đậm đà", "Châm 700ml nước hầm xương vào nồi tôm thịt, đun sôi rồi hạ nhỏ lửa ninh 20 phút cho nước nhưn sắc ngọt và sánh đậm đặc trưng.", 20),
                    new("Trình bày tô Mì Quảng chuẩn xứ Quảng", "Trần sợi mì vàng xếp vào tô, gắp tôm thịt trứng cút lên trên, chan nước nhưn xâm xấp chỉ ngập 1/3 sợi mì, rắc đậu phộng rang, hành ngò và bẻ bánh tráng mè giòn rụm ăn kèm.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1612927601601-6638404737ce?auto=format&fit=crop&w=1200&q=80", "Tô Mì Quảng đậm đà đặc sắc miền Trung")
                }
            },

            // 15. Hủ Tiếu Nam Vang Thập Cẩm
            new()
            {
                Title = "Hủ Tiếu Nam Vang Thập Cẩm",
                CategoryName = "Món ăn sáng",
                Description = "Món hủ tiếu vang danh phương Nam với nước lèo trong vắt nấu từ xương ống và mực khô thơm lừng, sợi hủ tiếu dai dai, phủ ngập tôm, thịt băm, gan heo, tim heo và trứng cút.",
                Instructions = "Hầm xương heo cùng tôm khô mực khô lấy nước dùng thanh ngọt, trụng sợi hủ tiếu dai, xếp tôm luộc, gan heo, thịt nạc và xốt thịt băm tỏi phi thơm lừng, dùng kèm cần tàu và hẹ lá.",
                PrepTimeMinutes = 35,
                CookTimeMinutes = 90,
                Servings = 5,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 490,
                Protein = 35,
                Carbohydrates = 58,
                Fat = 14,
                Fiber = 2,
                Sodium = 980,
                Ingredients = new()
                {
                    new("Hủ tiếu dai Sa Đéc", 500, "g"),
                    new("Xương ống heo hầm", 1, "kg"),
                    new("Thịt nạc xay xào sốt", 200, "g"),
                    new("Tôm tươi lột vỏ chừa đuôi", 250, "g"),
                    new("Gan heo và tim heo luộc chín", 200, "g"),
                    new("Trứng cút luộc bóc vỏ", 10, "quả"),
                    new("Mực khô nướng và tôm khô", 50, "g"),
                    new("Củ cải trắng và củ cải muối", 100, "g"),
                    new("Tỏi băm phi vàng giòn", 4, "thìa canh"),
                    new("Cần tàu, hẹ lá, giá tươi", 200, "g")
                },
                Steps = new()
                {
                    new("Ninh nước lèo trong vắt", "Hầm xương ống heo với củ cải trắng, củ cải muối, mực khô nướng và tôm khô trong 90 phút lửa nhỏ, vớt bọt liên tục để nước lèo ngọt thanh tự nhiên.", 90),
                    new("Làm xốt thịt băm tỏi phi", "Phi thơm thật nhiều tỏi băm cho vàng giòn vớt ra chén. Giữ lại chút dầu trong chảo, xào chín thịt nạc xay với chút nước tương và tiêu thơm.", 10),
                    new("Luộc tim gan heo và tôm", "Luộc chín gan heo, tim heo và tôm, vớt ra ngâm nước đá lạnh cho giòn, sau đó thái lát mỏng vừa ăn.", 15),
                    new("Trụng hủ tiếu và chan nước lèo", "Trụng sợi hủ tiếu qua nước sôi cho chín dai, trộn với một thìa tỏi phi cho sợi không dính. Xếp thịt băm, tôm, tim gan, trứng cút, cần tàu, chan ngập nước dùng sôi sùng sục.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=1200&q=80", "Tô hủ tiếu Nam Vang thập cẩm thơm phức")
                }
            },

            // 16. Bánh Flan Caramel Mềm Mịn Không Bị Rỗ
            new()
            {
                Title = "Bánh Flan Caramel Mềm Mịn Không Bị Rỗ",
                CategoryName = "Món tráng miệng",
                Description = "Món tráng miệng thần thánh mềm mịn như lụa, tan chảy trong miệng với hương thơm béo ngậy của sữa tươi và trứng gà quyện cùng lớp sốt caramel đắng nhẹ ngọt ngào quyến rũ.",
                Instructions = "Thắng đường caramen hổ phách tráng đáy khuôn, khuấy nhẹ trứng gà cùng sữa tươi và sữa đặc ấm không tạo bọt, lọc qua rây 2 lần cho mịn mượt, hấp cách thủy lửa nhỏ liu riu.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 35,
                Servings = 6,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 210,
                Protein = 7,
                Carbohydrates = 26,
                Fat = 9,
                Fiber = 0,
                Sodium = 110,
                Ingredients = new()
                {
                    new("Trứng gà tươi", 5, "quả", "3 quả nguyên + 2 lòng đỏ"),
                    new("Sữa tươi không đường", 500, "ml"),
                    new("Sữa đặc có đường", 150, "ml"),
                    new("Đường cát trắng thắng caramel", 100, "g"),
                    new("Nước cốt chanh tươi", 1, "thìa cà phê"),
                    new("Chiết xuất vani", 1, "thìa cà phê")
                },
                Steps = new()
                {
                    new("Thắng đường caramel màu cánh gián", "Cho đường và chút nước vào chảo nhỏ đun không khuấy đến khi đường chuyển màu hổ phách đẹp mắt, vắt vài giọt chanh rồi tráng nhanh một lớp mỏng đáy từng khuôn.", 8),
                    new("Đun ấm hỗn hợp sữa", "Khuấy đều sữa tươi và sữa đặc, đun trên lửa nhỏ đến khi hỗn hợp ấm khoảng 60°C (hơi bốc khói nhẹ, không đun sôi sùng sục) rồi tắt bếp.", 5),
                    new("Khuấy trứng và sữa nhẹ tay", "Đánh tan trứng gà cùng vani thật nhẹ tay theo một chiều để tránh tạo bọt khí. Rót từ từ hỗn hợp sữa ấm vào bát trứng, vừa rót vừa khuấy đều.", 5),
                    new("Lọc qua rây mịn mượt", "Rót hỗn hợp sữa trứng qua rây lọc 2-3 lần để loại bỏ hoàn toàn lợn cợn lòng trắng, giúp bánh flan khi chín láng mịn tuyệt đối.", 5),
                    new("Hấp bánh flan lửa liu riu", "Rót hỗn hợp vào khuôn caramel, đậy nắp hoặc bọc giấy bạc từng khuôn. Hấp cách thủy lửa nhỏ nhất trong 30-35 phút đến khi cắm tăm vào rút ra khô ráo là bánh chín.", 35),
                    new("Làm lạnh và thưởng thức", "Để nguội rồi cho vào ngăn mát tủ lạnh ít nhất 4 tiếng. Khi ăn úp ngược khuôn ra đĩa, chan thêm chút cà phê đen và đá bào mát lạnh.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1528975604071-b4dc52a2d18c?auto=format&fit=crop&w=1200&q=80", "Bánh flan caramel mềm mịn mướt mắt")
                }
            },

            // 17. Cà Phê Trứng Hà Nội Bồng Bềnh Béo Ngậy
            new()
            {
                Title = "Cà Phê Trứng Hà Nội Bồng Bềnh Béo Ngậy",
                CategoryName = "Đồ uống",
                Description = "Thức uống sáng tạo kinh điển của người Hà Nội với lớp bọt trứng đánh bông mịn như mây béo ngậy vị sữa ngự trên cốt cà phê phin đậm đặc nóng hổi thơm nồng.",
                Instructions = "Đánh bông lòng đỏ trứng gà cùng sữa đặc và chút mật ong đến khi thành kem xốp mịn màng không mùi tanh, rót nhẹ nhàng lên tách cà phê phin nguyên chất đang nóng rực rỡ.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 5,
                Servings = 2,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 180,
                Protein = 6,
                Carbohydrates = 18,
                Fat = 10,
                Fiber = 0,
                Sodium = 60,
                Ingredients = new()
                {
                    new("Bột cà phê Robusta nguyên chất", 25, "g"),
                    new("Lòng đỏ trứng gà tươi", 2, "quả"),
                    new("Sữa đặc có đường", 25, "ml"),
                    new("Mật ong nguyên chất", 1, "thìa cà phê"),
                    new("Chiết xuất vani", 3, "giọt"),
                    new("Nước sôi 95°C", 100, "ml")
                },
                Steps = new()
                {
                    new("Pha cà phê phin truyền thống", "Cho bột cà phê vào phin nhôm, ủ nước sôi 2 phút rồi châm thêm nước sôi chiết xuất lấy cốt cà phê đen đậm đặc nóng hổi.", 5),
                    new("Đánh bông kem trứng béo ngậy", "Tách lấy 2 lòng đỏ trứng gà tươi vào âu sâu lòng, thêm sữa đặc, mật ong và vani. Dùng máy đánh trứng đánh ở tốc độ cao trong 4-5 phút đến khi kem trứng chuyển màu vàng nhạt, bông xốp mịn màng.", 5),
                    new("Rót và thưởng thức ly cà phê trứng", "Đun nóng tách cà phê, nhẹ nhàng múc lớp kem trứng bồng bềnh phủ đầy mặt tách. Khi uống nghiêng cốc 45 độ để thưởng thức cùng lúc vị đắng của cà phê và vị ngọt béo ngậy của kem trứng.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1514432324607-a09d9b4aefdd?auto=format&fit=crop&w=1200&q=80", "Tách cà phê trứng Hà Nội bồng bềnh béo ngậy")
                }
            },

            // 18. Sinh Tố Bơ Nước Cốt Dừa Béo Bùi
            new()
            {
                Title = "Sinh Tố Bơ Nước Cốt Dừa Béo Bùi",
                CategoryName = "Đồ uống",
                Description = "Món sinh tố giải nhiệt quốc dân với trái bơ sáp chín dẻo béo ngậy xay mịn cùng nước cốt dừa thơm phức, sữa tươi và đá bào mát lạnh sảng khoái.",
                Instructions = "Nạo thịt bơ sáp chín, cho vào máy xay cùng sữa tươi, sữa đặc, nước cốt dừa và đá viên xay nhuyễn mịn, rót ra ly và trang trí vài lát bơ tươi hấp dẫn.",
                PrepTimeMinutes = 10,
                CookTimeMinutes = 5,
                Servings = 2,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 280,
                Protein = 4,
                Carbohydrates = 32,
                Fat = 16,
                Fiber = 7,
                Sodium = 50,
                Ingredients = new()
                {
                    new("Bơ sáp chín dẻo", 2, "quả"),
                    new("Sữa tươi có đường", 120, "ml"),
                    new("Sữa đặc có đường", 4, "thìa canh"),
                    new("Nước cốt dừa", 50, "ml"),
                    new("Đá viên nhỏ", 1, "bát")
                },
                Steps = new()
                {
                    new("Sơ chế bơ sáp", "Dùng dao bổ đôi quả bơ, bỏ hạt, dùng thìa nhẹ nhàng nạo lấy toàn bộ phần thịt bơ sáp vàng béo.", 5),
                    new("Xay sinh tố nhuyễn mịn", "Cho thịt bơ, sữa tươi, sữa đặc, nước cốt dừa và đá viên vào máy xay sinh tố. Bật máy xay ở tốc độ cao trong 1-2 phút cho đến khi sinh tố sánh mịn đặc dẻo.", 3),
                    new("Rót ra ly và thưởng thức", "Rót sinh tố bơ ra ly thủy tinh, rưới thêm chút sữa đặc hoặc rắc dừa khô sấy giòn lên trên và thưởng thức ngay khi còn lạnh mát.", 2)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1553530666-ba11a7da3888?auto=format&fit=crop&w=1200&q=80", "Ly sinh tố bơ sánh mịn xanh mát mắt")
                }
            },

            // 19. Bánh Chuối Nướng Nước Cốt Dừa Thơm Lừng
            new()
            {
                Title = "Bánh Chuối Nướng Nước Cốt Dừa Thơm Lừng",
                CategoryName = "Bánh ngọt",
                Description = "Bánh chuối nướng Nam Bộ nồng nàn hương chuối sứ chín muồi ngâm rượu rum, hòa quyện trong cốt bánh mì sandwich ngập nước cốt dừa béo ngậy nướng chín vàng ươm mặt bánh.",
                Instructions = "Chuối sứ thái lát ướp đường và rượu rum, bánh mì xé nhỏ ngâm ngập sữa và nước cốt dừa bơ tan chảy, xếp lớp chuối bánh xen kẽ trong khuôn và nướng chín vàng thơm lừng.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 60,
                Servings = 8,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 290,
                Protein = 5,
                Carbohydrates = 46,
                Fat = 11,
                Fiber = 3,
                Sodium = 120,
                Ingredients = new()
                {
                    new("Chuối sứ chín muồi", 1, "kg", "Khoảng 10-12 quả"),
                    new("Bánh mì sandwich", 200, "g", "Xé nhỏ"),
                    new("Nước cốt dừa đậm đặc", 400, "ml"),
                    new("Sữa đặc có đường", 100, "ml"),
                    new("Bơ lạt đun chảy", 50, "g"),
                    new("Đường cát trắng", 100, "g"),
                    new("Rượu rum thơm", 2, "thìa canh"),
                    new("Bột mì đa dụng", 50, "g")
                },
                Steps = new()
                {
                    new("Ướp chuối sứ lên màu đỏ đẹp", "Lột vỏ chuối, thái lát xéo dày 0.5 cm. Ướp chuối với đường cát và 2 thìa rượu rum trong ít nhất 1 tiếng cho chuối tiết mật thơm lừng.", 60),
                    new("Pha hỗn hợp cốt bánh mì dừa", "Xé nhỏ bánh mì, trộn cùng nước cốt dừa, sữa đặc, bơ đun chảy, bột mì và một chút vani, để bánh mì ngậm no nước sốt nở mềm.", 15),
                    new("Xếp bánh vào khuôn nướng", "Lót giấy nến vào khuôn, đổ 1/2 hỗn hợp bánh mì cốt dừa làm đáy, xếp một lớp chuối ướp mật, rồi phủ tiếp phần bánh mì còn lại, trên cùng xếp kín các lát chuối tạo hình hoa bắt mắt.", 10),
                    new("Nướng bánh vàng ươm quyến rũ", "Làm nóng lò 170°C, nướng bánh trong 60 phút đến khi mặt chuối chuyển sang màu đỏ nâu óng ánh và thơm ngát mùi dừa nướng.", 60),
                    new("Để nguội và cắt lát thưởng thức", "Lấy bánh ra để nguội hoàn toàn cho kết cấu bánh dẻo mịn chắc chắn, cắt miếng tam giác thưởng thức cùng trà nóng.", 10)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1509440159596-0249088772ff?auto=format&fit=crop&w=1200&q=80", "Khuôn bánh chuối nướng cốt dừa thơm nức mũi")
                }
            },

            // 20. Sườn Xào Chua Ngọt Miền Bắc
            new()
            {
                Title = "Sườn Xào Chua Ngọt Miền Bắc",
                CategoryName = "Món heo",
                Description = "Từng dẻ sườn non chặt khúc vàng xém cạnh, áo đều lớp sốt chua ngọt đỏ bóng óng ả làm từ cà chua, giấm bỗng và tỏi ớt, thịt mềm róc xương ăn hao cơm vô cùng.",
                Instructions = "Chần sườn non rán sơ vàng xém mặt, phi thơm hành tỏi pha sốt giấm đường nước mắm cà chua xào sánh sệt, đảo sườn ngấm sốt lửa nhỏ đến khi sốt keo bám chặt từng miếng sườn.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 25,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 430,
                Protein = 28,
                Carbohydrates = 16,
                Fat = 29,
                Fiber = 1,
                Sodium = 690,
                Ingredients = new()
                {
                    new("Sườn non heo", 600, "g", "Chặt khúc 3-4cm"),
                    new("Cà chua chín băm nhuyễn", 2, "quả"),
                    new("Giấm táo hoặc giấm gạo", 3, "thìa canh"),
                    new("Đường cát trắng", 2.5m, "thìa canh"),
                    new("Nước mắm truyền thống", 2.5m, "thìa canh"),
                    new("Tỏi và hành tím băm", 30, "g"),
                    new("Hành lá cắt khúc", 30, "g")
                },
                Steps = new()
                {
                    new("Sơ chế và chần sườn non", "Sườn non chặt khúc 3-4 cm, chần qua nước sôi pha chút muối trong 3 phút khử bọt bẩn, rửa sạch để ráo nước.", 10),
                    new("Rán sườn xém cạnh", "Làm nóng chảo với chút dầu ăn, cho sườn vào rán lửa vừa đến khi các mặt sườn chín vàng xém cạnh thơm phức thì gắp ra đĩa.", 10),
                    new("Pha nước sốt chua ngọt chuẩn tỉ lệ", "Hòa tan giấm, đường, nước mắm ngon và 3 thìa nước lọc trong bát nhỏ tạo vị chua ngọt hài hòa cân đối.", 5),
                    new("Xào sườn quyện sốt kẹo quánh", "Phi thơm hành tỏi băm, cho cà chua vào đảo nhừ thành sốt mịn, trút sườn và nước sốt chua ngọt vào đun sôi rồi hạ nhỏ lửa đảo đều tay đến khi sốt keo quánh lại áo bóng loáng quanh miếng sườn.", 12),
                    new("Trình bày món sườn xào", "Gắp sườn ra đĩa, rắc hành lá ngò rí và chút tiêu xay lên trên, dùng nóng cùng cơm trắng thơm dẻo.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=1200&q=80", "Đĩa sườn xào chua ngọt óng ả thơm ngon")
                }
            },

            // 21. Thịt Kho Tàu Trứng Cút Nước Dừa
            new()
            {
                Title = "Thịt Kho Tàu Trứng Cút Nước Dừa",
                CategoryName = "Món Tết",
                Description = "Món ăn truyền thống không thể thiếu trong mâm cơm Tết Việt Nam với thịt ba chỉ mềm tan béo ngậy, trứng cút ngấm sốt nâu bóng và nước dừa xiêm ngọt lịm đậm đà.",
                Instructions = "Thịt ba chỉ cắt vuông lớn ướp mắm tỏi, thắng nước màu dừa phi thơm thịt, châm nước dừa tươi hầm lửa nhỏ liu riu cho thịt trong veo, thả trứng cút luộc kho đến khi nước sánh màu hổ phách.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 60,
                Servings = 5,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 520,
                Protein = 30,
                Carbohydrates = 12,
                Fat = 38,
                Fiber = 0,
                Sodium = 850,
                Ingredients = new()
                {
                    new("Thịt ba chỉ rút sườn", 700, "g", "Cắt khối vuông 4x4cm"),
                    new("Trứng cút luộc bóc vỏ", 20, "quả"),
                    new("Nước dừa tươi xiêm", 600, "ml"),
                    new("Nước mắm cá cơm", 4, "thìa canh"),
                    new("Nước màu dừa Bến Tre", 1.5m, "thìa canh"),
                    new("Hành tỏi giã nhuyễn và ớt hiểm", 40, "g"),
                    new("Đường thốt nốt", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Cắt thịt và ướp gia vị", "Thịt ba chỉ cạo sạch bì, cắt miếng vuông lớn 4x4 cm. Ướp với nước mắm, hành tỏi giã nhuyễn, đường thốt nốt, nước màu dừa trong 45 phút.", 45),
                    new("Xào săn thịt ba chỉ", "Đặt nồi lên bếp, trút thịt vào đảo lửa vừa cho thịt săn chắc và lớp mỡ bắt đầu trong lại dậy mùi thơm nức.", 10),
                    new("Kho thịt với nước dừa tươi", "Châm nước dừa tươi ngập mặt thịt, đun sôi rồi hớt sạch bọt, hạ lửa nhỏ liu riu không đậy nắp nồi để nước kho giữ được màu trong đẹp.", 45),
                    new("Thả trứng cút kho ngấm vị", "Thả trứng cút luộc bóc vỏ vào nồi thịt kho tiếp tục đun nhỏ lửa thêm 15-20 phút đến khi thịt mềm rục, mỡ trong veo và trứng chuyển màu vàng cánh gián óng ả.", 20),
                    new("Thưởng thức", "Múc thịt và trứng cút ra đĩa sâu lòng, chan nước sốt sánh ngọt, ăn kèm dưa cải chua hoặc củ kiệu giòn tan ngày Tết.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1504674900247-0877df9cc836?auto=format&fit=crop&w=1200&q=80", "Tô thịt kho tàu trứng cút nước dừa óng ả")
                }
            },

            // 22. Salad Ức Gà Sốt Mè Rang Giảm Cân
            new()
            {
                Title = "Salad Ức Gà Sốt Mè Rang Giảm Cân",
                CategoryName = "Salad",
                Description = "Bữa ăn lành mạnh giàu protein cho người tập gym và ăn kiêng với ức gà áp chảo mềm mọng nước, rau củ quả tươi giòn và sốt mè rang béo bùi thơm ngậy.",
                Instructions = "Ức gà ướp muối tiêu áp chảo chín mọng nước thái lát mỏng, chuẩn bị xà lách giòn, cà chua bi, dưa leo bắp ngọt, xếp ra đĩa và rưới đẫm sốt mè rang thơm phức.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 12,
                Servings = 2,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 290,
                Protein = 34,
                Carbohydrates = 14,
                Fat = 11,
                Fiber = 4,
                Sodium = 450,
                Ingredients = new()
                {
                    new("Ức gà phi lê tươi", 300, "g"),
                    new("Xà lách xoăn tươi", 150, "g"),
                    new("Cà chua bi bổ đôi", 100, "g"),
                    new("Dưa leo thái lát", 1, "quả"),
                    new("Bắp ngọt luộc tách hạt", 50, "g"),
                    new("Sốt mè rang Nhật Bản", 3, "thìa canh"),
                    new("Dầu oliu và tiêu đen", 1, "thìa canh")
                },
                Steps = new()
                {
                    new("Ướp ức gà mềm mọng", "Ức gà khía nhẹ mặt, ướp với chút dầu oliu, muối và tiêu đen trong 10 phút.", 10),
                    new("Áp chảo ức gà không bị khô", "Làm nóng chảo chống dính, áp chảo ức gà lửa vừa mỗi mặt khoảng 5-6 phút đến khi chín vàng tới, đậy nắp 2 phút cho thịt giữ trọn độ mọng nước.", 12),
                    new("Sơ chế rau củ giòn mát", "Rau xà lách rửa sạch ngâm nước đá cho giòn tan rồi vẩy ráo nước. Cà chua bi bổ đôi, dưa leo thái lát mỏng.", 8),
                    new("Trình bày đĩa salad hấp dẫn", "Xếp rau xà lách, dưa leo, cà chua bi và bắp ngọt ra đĩa lớn. Thái ức gà thành từng lát mỏng xếp lên trên, khi ăn rưới sốt mè rang béo ngậy trộn đều.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1512621776951-a57141f2eefd?auto=format&fit=crop&w=1200&q=80", "Đĩa salad ức gà sốt mè rang tươi mát lành mạnh")
                }
            },

            // 23. Lẩu Nấm Chay Thanh Đạm Thanh Lọc
            new()
            {
                Title = "Lẩu Nấm Chay Thanh Đạm Thanh Lọc",
                CategoryName = "Món chay",
                Description = "Nồi lẩu chay thanh mát, bổ dưỡng với nước dùng ngọt lịm từ củ quả ninh chậm như bắp ngọt, táo đỏ, cà rốt kết hợp với hơn 6 loại nấm tươi giòn ngọt tự nhiên.",
                Instructions = "Hầm nước dùng từ bắp ngọt, củ cải trắng, mía và táo đỏ trong 45 phút, nêm muối hồng và hạt nêm nấm chay, bày các loại nấm kim châm, nấm đùi gà, đậu hũ non và rau xanh nhúng lẩu.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 45,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 220,
                Protein = 14,
                Carbohydrates = 32,
                Fat = 4,
                Fiber = 8,
                Sodium = 560,
                Ingredients = new()
                {
                    new("Nấm đùi gà, nấm kim châm, nấm đông cô", 400, "g"),
                    new("Đậu hũ non cắt khối", 2, "bìa"),
                    new("Bắp ngọt cắt khúc", 1, "trái"),
                    new("Củ cải trắng và cà rốt", 200, "g"),
                    new("Táo đỏ và kỷ tử", 30, "g"),
                    new("Mía lau chẻ nhỏ", 2, "khúc"),
                    new("Rau cải thìa và cải cúc", 300, "g"),
                    new("Hạt nêm nấm chay và muối hồng", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Nấu nước dùng rau củ ngọt thanh", "Cho bắp ngọt, mía lau, củ cải, cà rốt và táo đỏ vào nồi với 2 lít nước ninh nhỏ lửa trong 45 phút để chiết xuất trọn vẹn vị ngọt tinh khiết tự nhiên.", 45),
                    new("Sơ chế các loại nấm tươi", "Nấm cắt bỏ gốc, ngâm nước muối loãng 5 phút rồi xả sạch, để ráo nước. Đậu hũ non cắt miếng vuông vừa ăn.", 15),
                    new("Nêm nếm nước lẩu", "Vớt bỏ phần xác mía, nêm nước dùng với hạt nêm từ nấm, chút muối hồng và kỷ tử cho nước dùng thanh dịu ngọt ngào.", 10),
                    new("Thưởng thức lẩu nấm", "Bắc nồi lẩu ra bếp bàn ăn đun sôi, lần lượt nhúng các loại nấm giòn ngọt, đậu hũ non béo bùi và rau xanh tươi non, chấm nước tương ớt cay nhẹ.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=1200&q=80", "Nồi lẩu nấm chay thanh tịnh thơm ngon")
                }
            },

            // 24. Bò Lúc Lắc Khoai Tây Chiên
            new()
            {
                Title = "Bò Lúc Lắc Khoai Tây Chiên",
                CategoryName = "Món xào",
                Description = "Món bò xào kiểu Pháp giao thoa ẩm thực Việt với thịt thăn bò cắt quân cờ mềm ngọt mọng nước lắc chảo lửa lớn cùng ớt chuông ba màu, hành tây và khoai tây chiên vàng giòn.",
                Instructions = "Thăn bò thái quân cờ ướp dầu hào, tỏi băm và bơ lạt. Lắc chảo lửa cực lớn cho bò xém cạnh mọng nước, xào nhanh ớt chuông hành tây giòn ngọt, dọn kèm muối tiêu chanh.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 15,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 480,
                Protein = 38,
                Carbohydrates = 26,
                Fat = 24,
                Fiber = 3,
                Sodium = 680,
                Ingredients = new()
                {
                    new("Thịt thăn bò mềm", 500, "g", "Thái quân cờ 2x2cm"),
                    new("Ớt chuông đỏ, xanh, vàng", 1.5m, "quả"),
                    new("Hành tây thái vuông", 1, "củ"),
                    new("Khoai tây chiên giòn", 200, "g"),
                    new("Bơ lạt", 20, "g"),
                    new("Tỏi băm nhuyễn", 2, "thìa canh"),
                    new("Dầu hào và xì dầu", 3, "thìa canh"),
                    new("Tiêu đen xay và muối tiêu chanh", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Thái bò quân cờ và ướp thịt", "Thịt bò thái khối vuông 2x2 cm. Ướp thịt với dầu hào, xì dầu, tỏi băm, đường và 1 thìa dầu ăn trong 20 phút cho thịt mềm mọng.", 20),
                    new("Cắt ớt chuông và hành tây", "Ớt chuông và hành tây rửa sạch, cắt miếng vuông vừa ăn bằng kích cỡ thịt bò.", 10),
                    new("Lắc bò trên chảo lửa lớn", "Làm chảo thật nóng với chút dầu ăn, trút thịt bò vào lắc chảo liên tục ở lửa lớn trong 2-3 phút cho các mặt thịt xém vàng mọng nước rồi trút ra đĩa.", 3),
                    new("Xào rau củ với bơ", "Cho bơ lạt vào chảo phi thơm tỏi, cho ớt chuông và hành tây vào xào nhanh 2 phút giữ độ giòn ngọt, trút bò trở lại đảo đều 30 giây rồi tắt bếp.", 3),
                    new("Dọn đĩa cùng khoai tây chiên", "Bày bò lúc lắc ra đĩa cùng khoai tây chiên vàng giòn và xà lách cà chua, chấm cùng muối tiêu chanh tươi.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1546964124-0cce460f38ef?auto=format&fit=crop&w=1200&q=80", "Đĩa bò lúc lắc khoai tây chiên thơm nức")
                }
            },

            // 25. Trà Đào Cam Sả Tươi Mát
            new()
            {
                Title = "Trà Đào Cam Sả Tươi Mát",
                CategoryName = "Đồ uống",
                Description = "Ly trà giải khát đỉnh cao ngày hè với hương thơm ngát của sả tươi, vị chua dịu thanh khiết của cam vàng, cốt trà đào đậm đà và những miếng đào ngâm giòn sần sật ngọt lịm.",
                Instructions = "Đun sôi nước sả đập dập ủ trà túi lọc, hòa nước cốt cam vàng cùng siro đào và đường phèn, thêm đá viên đầy ly và trang trí lát cam vàng, nhánh sả cùng miếng đào ngâm giòn.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 10,
                Servings = 2,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 130,
                Protein = 1,
                Carbohydrates = 31,
                Fat = 0,
                Fiber = 1,
                Sodium = 10,
                Ingredients = new()
                {
                    new("Trà đào túi lọc", 2, "gói"),
                    new("Sả tươi đập dập", 3, "cây"),
                    new("Cam vàng tươi", 1, "quả"),
                    new("Đào ngâm giòn đóng hộp", 4, "miếng"),
                    new("Nước đường phèn siro đào", 40, "ml"),
                    new("Đá viên mát lạnh", 1, "tô")
                },
                Steps = new()
                {
                    new("Nấu nước sả và ủ trà", "Đun sôi 300ml nước với sả đập dập trong 5 phút cho tinh dầu sả tiết ra thơm phức, tắt bếp thả 2 gói trà đào vào ủ 7 phút rồi vớt túi lọc.", 12),
                    new("Pha chế ly trà đào cam sả", "Cho nước cốt cam tươi, siro đào và nước trà sả vào bình lắc (shaker), khuấy đều cho hòa quyện.", 3),
                    new("Trang trí và thưởng thức", "Cho đá viên đầy ly thủy tinh, rót hỗn hợp trà đào cam sả vào, xếp 2 miếng đào ngâm giòn lên mặt cùng lát cam vàng và cắm nhánh sả tươi trang trí.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1556881286-fc6915169721?auto=format&fit=crop&w=1200&q=80", "Ly trà đào cam sả tươi mát thơm ngát")
                }
            },

            // 26. Mực Xào Sa Tế Giòn Ngọt Cay Nồng
            new()
            {
                Title = "Mực Xào Sa Tế Giòn Ngọt Cay Nồng",
                CategoryName = "Món xào",
                Description = "Từng miếng mực ống dày mình khía vảy rồng giòn sần sật, áo đều lớp sốt sa tế tỏi ớt cay nồng đượm khói, xào cùng hành tây, cần tây giòn ngọt thơm lừng góc bếp.",
                Instructions = "Mực sơ chế sạch khía vảy rồng chần sơ nước sôi gừng, phi thơm tỏi đảo mực lửa lớn cho săn giòn, cho hành tây cần tây sốt sa tế xào nhanh lửa to rồi trút ra đĩa.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 10,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 280,
                Protein = 32,
                Carbohydrates = 12,
                Fat = 11,
                Fiber = 2,
                Sodium = 720,
                Ingredients = new()
                {
                    new("Mực ống tươi dày mình", 500, "g"),
                    new("Ớt sa tế tôm cay", 2, "thìa canh"),
                    new("Hành tây thái múi cau", 1, "củ"),
                    new("Cần tây cắt khúc", 100, "g"),
                    new("Tỏi băm và gừng tươi", 30, "g"),
                    new("Dầu hào và nước mắm", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Sơ chế và khía vảy rồng mực", "Mực làm sạch túi mực bóc màng, khía vảy rồng rồi cắt miếng vừa ăn, chần nhanh qua nước sôi gừng 15 giây vớt ráo.", 10),
                    new("Pha sốt sa tế xào cay", "Hòa tan sa tế tôm, dầu hào, nước mắm ngon và tiêu trong chén nhỏ.", 3),
                    new("Xào mực lửa lớn giòn ngọt", "Phi thơm tỏi băm trút mực vào đảo lửa lớn trong 2 phút cho mực săn giòn, cho hành tây cần tây và sốt sa tế vào đảo nhanh thêm 2 phút là hoàn thành.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1599084993091-1cb5c0721cc6?auto=format&fit=crop&w=1200&q=80", "Đĩa mực xào sa tế cay nồng giòn sần sật")
                }
            },

            // 27. Tôm Rim Thịt Ba Chỉ Đậm Đà Đưa Cơm
            new()
            {
                Title = "Tôm Rim Thịt Ba Chỉ Đậm Đà Đưa Cơm",
                CategoryName = "Món chính",
                Description = "Món ăn kinh điển của mâm cơm gia đình Việt với tôm tươi vỏ giòn đanh ánh màu hổ phách quyện cùng thịt ba chỉ béo ngậy ngấm trọn nước mắm đường hành tiêu kẹo quánh.",
                Instructions = "Tôm cắt râu rửa sạch, thịt ba chỉ xào tiết bớt mỡ xém cạnh, phi thơm hành tỏi trút tôm vào đảo đỏ au, nêm mắm đường đun liu riu đến khi sốt keo bám bóng.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 20,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 420,
                Protein = 29,
                Carbohydrates = 10,
                Fat = 28,
                Fiber = 0,
                Sodium = 820,
                Ingredients = new()
                {
                    new("Tôm đất tươi sống", 350, "g"),
                    new("Thịt ba chỉ heo thái con chì", 300, "g"),
                    new("Nước mắm nhĩ ngon", 3, "thìa canh"),
                    new("Đường cát vàng", 2, "thìa canh"),
                    new("Hành tím và tỏi băm", 30, "g"),
                    new("Hành lá và tiêu đen xay", 20, "g")
                },
                Steps = new()
                {
                    new("Sơ chế tôm và thịt", "Tôm cắt đầu đuôi rút chỉ lưng xát muối rửa sạch. Thịt ba chỉ thái miếng con chì vừa ăn.", 10),
                    new("Rán xém cạnh thịt ba chỉ", "Cho thịt vào chảo đảo đều cho tiết mỡ thơm và xém cạnh vàng óng.", 8),
                    new("Rim tôm thịt kẹo bóng", "Cho hành tỏi vào phi thơm cùng mỡ thịt, trút tôm vào đảo lửa lớn đến khi tôm cong đỏ, nêm nước mắm và đường đun liu riu đến khi nước rim sánh keo bóng loáng, rắc hành tiêu thơm nức.", 10)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1559847844-5315695dadae?auto=format&fit=crop&w=1200&q=80", "Tôm rim thịt ba chỉ óng ả đậm đà")
                }
            },

            // 28. Đậu Hũ Nhồi Thịt Sốt Cà Chua
            new()
            {
                Title = "Đậu Hũ Nhồi Thịt Sốt Cà Chua",
                CategoryName = "Món chính",
                Description = "Miếng đậu hũ chiên vàng ươm nhồi đầy ắp nhân thịt heo mộc nhĩ nấm hương béo ngọt, om mềm trong nước sốt cà chua đỏ tươi sánh mịn ngọt ngào.",
                Instructions = "Đậu hũ khoét rãnh nhồi nhân thịt mộc nhĩ nấm hương băm, rán sơ mặt thịt cho định hình, om trong nước sốt cà chua đun nhừ sánh quyện đến khi ngấm gia vị.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 25,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 320,
                Protein = 22,
                Carbohydrates = 14,
                Fat = 20,
                Fiber = 3,
                Sodium = 620,
                Ingredients = new()
                {
                    new("Đậu hũ trắng miếng vuông", 4, "miếng"),
                    new("Thịt nạc vai heo xay nhuyễn", 250, "g"),
                    new("Mộc nhĩ nấm hương băm nhỏ", 30, "g"),
                    new("Cà chua chín mọng băm nhỏ", 3, "quả"),
                    new("Hành tím băm và hành lá", 30, "g"),
                    new("Nước mắm và hạt nêm", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Trộn nhân thịt nấm", "Trộn đều thịt xay, mộc nhĩ nấm hương băm, hành tím, tiêu và 1 thìa hạt nêm cho nhân dẻo quánh.", 10),
                    new("Nhồi thịt vào đậu hũ", "Rạch một đường giữa miếng đậu, khéo léo nhồi nhân thịt vào đầy đặn, rán sơ mặt thịt cho cố định.", 10),
                    new("Nấu sốt cà chua om đậu", "Phi thơm hành, xào nhừ cà chua tạo sốt đỏ mịn, xếp đậu hũ nhồi vào om nhỏ lửa 15 phút cho ngấm sốt, rắc hành lá cắt nhỏ.", 15)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1546069901-d72b3857e4e1?auto=format&fit=crop&w=1200&q=80", "Đậu hũ nhồi thịt sốt cà chua đỏ mọng hấp dẫn")
                }
            },

            // 29. Bánh Bông Lan Trứng Muối Sốt Phô Mai
            new()
            {
                Title = "Bánh Bông Lan Trứng Muối Sốt Phô Mai",
                CategoryName = "Bánh ngọt",
                Description = "Cốt bánh bông lan chiffon mềm xốp như mây, phủ đẫm sốt phô mai bơ sữa hoàng kim béo ngậy, trứng muối bùi béo nướng thơm lừng và chà bông (ruốc) heo đậm đà.",
                Instructions = "Nướng cốt bánh chiffon xốp mềm, nấu sốt phô mai bơ sữa sánh mịn phết đẫm mặt bánh, rải đều trứng muối nướng chín vàng và chà bông sợi tơi xốp.",
                PrepTimeMinutes = 35,
                CookTimeMinutes = 40,
                Servings = 6,
                Difficulty = Difficulty.Hard,
                Status = RecipeStatus.Published,
                Calories = 380,
                Protein = 12,
                Carbohydrates = 42,
                Fat = 18,
                Fiber = 1,
                Sodium = 410,
                Ingredients = new()
                {
                    new("Trứng gà tươi", 4, "quả", "Tách lòng đỏ lòng trắng"),
                    new("Bột mì làm bánh cake flour", 70, "g"),
                    new("Sữa tươi không đường và dầu ăn", 80, "ml"),
                    new("Đường xay mịn", 70, "g"),
                    new("Lòng đỏ trứng muối nướng chín", 6, "quả"),
                    new("Phô mai miếng béo", 4, "miếng"),
                    new("Chà bông (ruốc) heo thơm ngon", 60, "g")
                },
                Steps = new()
                {
                    new("Đánh bông lòng trắng trứng", "Đánh bông lòng trắng trứng với đường đến chóp mềm dẻo mịn.", 10),
                    new("Trộn bột bánh chiffon", "Khuấy tan lòng đỏ với sữa dầu ăn, rây bột mì vào trộn đều, fold nhẹ tay với lòng trắng bông thành hỗn hợp đồng nhất.", 10),
                    new("Nướng bánh bông lan", "Đổ vào khuôn tròn, nướng ở 150°C trong 40 phút cho bánh nở phồng vàng đều, lấy ra úp ngược để nguội.", 40),
                    new("Làm sốt phô mai và trang trí", "Đun chảy phô mai cùng sữa tươi và bơ thành sốt mịn sánh, phết đều lên mặt bánh rồi xếp trứng muối nướng và chà bông lên.", 15)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1578985545062-69928b1d9587?auto=format&fit=crop&w=1200&q=80", "Bánh bông lan trứng muối sốt phô mai thơm ngon")
                }
            },

            // 30. Trà Sữa Trân Châu Đường Đen
            new()
            {
                Title = "Trà Sữa Trân Châu Đường Đen",
                CategoryName = "Đồ uống",
                Description = "Cơn sốt đồ uống giới trẻ với từng viên trân châu đen dai giòn dẻo quánh nấu mật mía đường thốt nốt thơm lừng, hòa quyện cùng dòng sữa tươi thanh trùng béo ngậy mát lạnh.",
                Instructions = "Luộc trân châu dẻo dai rồi ngâm mật đường đen nóng kẹo, xoay ly tạo vân đường đen bắt mắt, đổ đá viên và rót ngập dòng sữa tươi thanh trùng béo ngậy.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 30,
                Servings = 2,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 310,
                Protein = 5,
                Carbohydrates = 58,
                Fat = 7,
                Fiber = 1,
                Sodium = 80,
                Ingredients = new()
                {
                    new("Trân châu đen bột năng", 150, "g"),
                    new("Đường đen Hàn Quốc (hoặc đường thốt nốt)", 100, "g"),
                    new("Sữa tươi thanh trùng không đường", 350, "ml"),
                    new("Đá viên nhỏ", 1, "tô")
                },
                Steps = new()
                {
                    new("Luộc trân châu dẻo dai", "Đun sôi nước thả trân châu vào luộc 20 phút lửa vừa, tắt bếp đậy nắp ủ thêm 20 phút rồi vớt xả nước lạnh.", 40),
                    new("Nấu siro đường đen kẹo quánh", "Nấu tan đường đen với 50ml nước lọc đến khi sủi bọt sánh mịn, trút trân châu vào đảo nhỏ lửa 5 phút cho ngấm mật óng ánh.", 8),
                    new("Rót ly tạo vệt đường đen hổ phách", "Múc trân châu đường đen vào đáy ly xoay nghiêng tạo vệt đường bám thành cốc, thêm đá viên rồi rót sữa tươi lạnh lên trên và thưởng thức.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1558857563-b371033873b8?auto=format&fit=crop&w=1200&q=80", "Ly trà sữa trân châu đường đen hấp dẫn")
                }
            },

            // 31. Bún Riêu Cua Đồng Đậm Đà
            new()
            {
                Title = "Bún Riêu Cua Đồng Đậm Đà",
                CategoryName = "Món canh",
                Description = "Bát bún riêu cua thơm lừng góc phố với từng mảng riêu cua đồng nổi dày béo bùi, nước dùng cà chua thanh mát điểm vị giấm bỗng chua dịu và đậu hũ chiên vàng giòn.",
                Instructions = "Giã lọc cua đồng nấu lửa nhỏ cho gạch cua kết mảng nổi lên, xào cà chua phi thơm hành tỏi đổ vào nồi nước cua, thả đậu rán và nêm giấm bỗng thanh ngọt, ăn kèm bún và rau chuối.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 35,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 430,
                Protein = 26,
                Carbohydrates = 54,
                Fat = 13,
                Fiber = 3,
                Sodium = 980,
                Ingredients = new()
                {
                    new("Cua đồng tươi giã lọc", 500, "g"),
                    new("Bún tươi", 600, "g"),
                    new("Đậu hũ chiên vàng cắt miếng", 3, "miếng"),
                    new("Cà chua chín thái múi cau", 4, "quả"),
                    new("Giấm bỗng nếp chua thanh", 4, "thìa canh"),
                    new("Hành tím và tỏi phi", 30, "g"),
                    new("Rau muống chẻ, hoa chuối, kinh giới", 250, "g")
                },
                Steps = new()
                {
                    new("Nấu mảng riêu cua đồng", "Hòa nước lọc cua với chút muối, đun lửa vừa khuấy nhẹ theo một chiều đến khi riêu cua kết tụ nổi lên mặt nước thì hạ nhỏ lửa, vớt riêu cua ra bát riêng.", 15),
                    new("Xào cà chua tạo màu nước lèo", "Phi thơm hành tím, xào chín cà chua cho lên màu đỏ tự nhiên, trút vào nồi nước cua đun sôi lại cùng đậu hũ chiên.", 10),
                    new("Nêm giấm bỗng và gia vị", "Rót giấm bỗng vào nồi nước dùng, nêm nước mắm cốt và hạt nêm cho vừa miệng chua thanh ngọt đậm đà.", 8),
                    new("Trình bày tô bún riêu cua", "Trụng bún xếp vào bát, múc riêu cua, đậu rán, chan ngập nước dùng cà chua đỏ mọng, rắc hành ngò và dùng kèm rau sống.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=1200&q=80", "Tô bún riêu cua đồng đỏ mọng thơm phức")
                }
            },

            // 32. Gỏi Ngó Sen Tôm Thịt Chua Ngọt
            new()
            {
                Title = "Gỏi Ngó Sen Tôm Thịt Chua Ngọt",
                CategoryName = "Salad",
                Description = "Món gỏi khai vị tiệc tùng sang trọng với ngó sen trắng muốt giòn sần sật, tôm sú luộc đỏ au, thịt ba chỉ luộc thái mỏng quyện đều nước sốt chua ngọt và đậu phộng rang bùi ngậy.",
                Instructions = "Ngó sen chẻ sợi ngâm nước chanh đường cho trắng giòn, luộc chín tôm và thịt heo thái mỏng, trộn đều cùng cà rốt ngò rí rau răm và nước sốt chua ngọt tỏi ớt.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 15,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 280,
                Protein = 24,
                Carbohydrates = 22,
                Fat = 11,
                Fiber = 4,
                Sodium = 610,
                Ingredients = new()
                {
                    new("Ngó sen tươi", 300, "g", "Chẻ đôi ngâm nước chanh"),
                    new("Tôm sú tươi luộc bóc vỏ", 250, "g"),
                    new("Thịt ba chỉ luộc thái mỏng", 200, "g"),
                    new("Cà rốt bào sợi", 1, "củ"),
                    new("Rau răm và cần tây cắt nhỏ", 50, "g"),
                    new("Nước mắm tỏi ớt chua ngọt", 4, "thìa canh"),
                    new("Đậu phộng rang giòn", 40, "g")
                },
                Steps = new()
                {
                    new("Sơ chế ngó sen giòn trắng", "Ngó sen bẻ khúc chẻ đôi, ngâm nước đá pha nước cốt chanh và đường 15 phút cho trắng muốt và giòn tan, vớt ra vắt ráo.", 15),
                    new("Trộn gỏi thấm vị", "Cho ngó sen, cà rốt, thịt ba chỉ, tôm luộc vào âu lớn, rưới nước mắm tỏi ớt chua ngọt vào trộn nhẹ tay cho ngấm đều trong 5 phút.", 5),
                    new("Trình bày đĩa gỏi ngó sen", "Bày gỏi ra đĩa, rắc rau răm, ớt sợi và đậu phộng rang giã dập lên trên, ăn kèm bánh phồng tôm giòn tan.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1541544741938-0af808871cc0?auto=format&fit=crop&w=1200&q=80", "Đĩa gỏi ngó sen tôm thịt thanh mát bắt mắt")
                }
            },

            // 33. Cơm Chiên Dương Châu Chuẩn Nhà Hàng
            new()
            {
                Title = "Cơm Chiên Dương Châu Chuẩn Nhà Hàng",
                CategoryName = "Món chính",
                Description = "Từng hạt cơm vàng óng tơi xốp không khô cứng, đan xen rực rỡ sắc màu của lạp xưởng ngọt bùi, tôm tươi, đậu Hà Lan, cà rốt và trứng gà xào thơm nức mũi.",
                Instructions = "Cơm nguội trộn đều lòng đỏ trứng gà cho bám vàng hạt cơm, xào thơm lạp xưởng tôm đậu hạt lựu, chiên cơm lửa lớn đảo đều tay cho săn tơi, rắc hành lá tiêu thơm.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 15,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 510,
                Protein = 22,
                Carbohydrates = 68,
                Fat = 17,
                Fiber = 3,
                Sodium = 750,
                Ingredients = new()
                {
                    new("Cơm nguội để tủ lạnh", 4, "chén"),
                    new("Lạp xưởng Mai Quế Lộ", 2, "cây", "Cắt hạt lựu"),
                    new("Tôm sú tươi lột vỏ cắt hạt lựu", 150, "g"),
                    new("Trứng gà tươi", 3, "quả"),
                    new("Đậu Hà Lan và cà rốt hạt lựu", 100, "g"),
                    new("Tỏi băm và hành lá", 30, "g"),
                    new("Nước tương, dầu hào, dầu mè", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Trộn cơm với lòng đỏ trứng", "Bóp tơi cơm nguội, trộn đều với 2 lòng đỏ trứng gà và 1 thìa dầu ăn để từng hạt cơm được áo một lớp vàng óng ả.", 10),
                    new("Xào nhân tôm lạp xưởng", "Phi thơm tỏi, cho lạp xưởng và tôm vào xào săn thơm, tiếp tục cho cà rốt và đậu Hà Lan chần sơ vào đảo chín tới rồi trút ra đĩa.", 8),
                    new("Chiên cơm lửa lớn tơi xốp", "Làm nóng chảo lớn với chút dầu ăn, trút cơm vào đảo liên tục trên lửa lớn đến khi hạt cơm săn lại và nảy tanh tách trên chảo.", 8),
                    new("Hòa quyện và hoàn thiện", "Trút phần nhân tôm lạp xưởng vào chảo cơm, nêm nước tương, dầu hào và dầu mè, đảo nhanh tay 2 phút, rắc hành lá cắt nhỏ rồi tắt bếp.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1565299585323-38d6b0865b47?auto=format&fit=crop&w=1200&q=80", "Đĩa cơm chiên Dương Châu vàng óng đầy ắp nhân")
                }
            },

            // 34. Canh Cua Rau Đay Mướp Hương Cà Pháo
            new()
            {
                Title = "Canh Cua Rau Đay Mướp Hương Cà Pháo",
                CategoryName = "Món canh",
                Description = "Món canh giải nhiệt bất hủ mùa hè Bắc Bộ với nước canh cua ngọt lịm mát ruột, rau đay mồng tơi trơn mềm, mướp hương thơm ngát ăn kèm bát cà pháo giòn tan.",
                Instructions = "Lọc nước cua đồng nấu sôi cho thịt cua nổi mảng, thả mướp hương và rau đay mồng tơi thái nhỏ đun chín tới, nêm mắm tôm hoặc nước mắm cốt ngọt thơm.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 15,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 150,
                Protein = 16,
                Carbohydrates = 12,
                Fat = 4,
                Fiber = 5,
                Sodium = 540,
                Ingredients = new()
                {
                    new("Cua đồng tươi giã lọc", 400, "g"),
                    new("Rau đay và mồng tơi", 200, "g", "Rửa sạch, thái nhỏ"),
                    new("Mướp hương", 1, "quả", "Gọt vỏ, thái vát"),
                    new("Nước mắm cá cơm", 2, "thìa canh"),
                    new("Mắm tôm Bắc ngon (tùy chọn)", 1, "thìa cà phê"),
                    new("Cà pháo muối giòn ăn kèm", 1, "bát")
                },
                Steps = new()
                {
                    new("Nấu nước cua nổi gạch", "Cho nước lọc cua vào nồi cùng 1 thìa cà phê muối, đun lửa vừa khuấy nhẹ để thịt cua không bén đáy nồi, khi thịt cua đóng tảng nổi lên thì ngừng khuấy.", 10),
                    new("Thả mướp hương và rau", "Gạt nhẹ mảng thịt cua sang một bên, thả mướp hương vào đun 2 phút, tiếp tục cho rau đay và mồng tơi vào nấu chín tới.", 5),
                    new("Nêm gia vị và múc canh", "Nêm chút nước mắm ngon hoặc mắm tôm cho dậy mùi thơm nồng nàn, múc canh ra bát lớn ăn cùng cơm trắng và cà pháo giòn tan.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=1200&q=80", "Bát canh cua rau đay mướp hương thanh mát")
                }
            },

            // 35. Nấm Bào Ngư Chiên Giòn Lắc Muối Ớt
            new()
            {
                Title = "Nấm Bào Ngư Chiên Giòn Lắc Muối Ớt",
                CategoryName = "Món chay",
                Description = "Món ăn vặt chay giòn tan thơm lừng với nấm bào ngư dai ngọt bọc lớp bột chiên giòn rụm bên ngoài, lắc cùng muối ớt cay xè kích thích vị giác.",
                Instructions = "Nấm bào ngư rửa sạch xé sợi vắt ráo nước, nhúng qua lớp bột chiên giòn pha sệt, chiên ngập dầu lửa vừa cho vàng rộm, vớt ráo dầu rồi lắc đều muối ớt.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 15,
                Servings = 3,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 220,
                Protein = 8,
                Carbohydrates = 28,
                Fat = 9,
                Fiber = 4,
                Sodium = 380,
                Ingredients = new()
                {
                    new("Nấm bào ngư xám tươi", 350, "g"),
                    new("Bột chiên giòn", 150, "g"),
                    new("Muối ớt Tây Ninh hoặc muối hồng", 1.5m, "thìa cà phê"),
                    new("Dầu ăn chiên ngập nấm", 400, "ml")
                },
                Steps = new()
                {
                    new("Sơ chế nấm bào ngư", "Nấm ngâm nước muối loãng 5 phút, xả sạch vắt thật ráo nước, xé nấm thành các sợi vừa ăn.", 10),
                    new("Pha bột chiên giòn", "Khuấy bột chiên giòn với nước lọc lạnh tạo thành hỗn hợp sền sệt bám đều đũa.", 5),
                    new("Chiên nấm vàng giòn tan", "Đun nóng dầu ăn, nhúng từng cụm nấm qua bột thả vào chiên lửa vừa đến khi nấm nở vàng giòn rụm thì vớt ra giấy thấm dầu.", 10),
                    new("Lắc muối ớt và thưởng thức", "Cho nấm chiên vào hộp, rắc muối ớt rồi đậy nắp lắc đều tay, dùng nóng chấm kèm tương ớt xí muội.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=1200&q=80", "Đĩa nấm bào ngư chiên giòn lắc muối ớt vàng rộm")
                }
            },

            // 36. Chè Bưởi An Giang Giòn Sần Sật
            new()
            {
                Title = "Chè Bưởi An Giang Giòn Sần Sật",
                CategoryName = "Món tráng miệng",
                Description = "Món chè trứ danh miền Tây với cùi bưởi trong veo giòn sần sật không hề bị đắng, quyện trong đậu xanh bùi ngậy nấu bột năng sánh mịn và nước cốt dừa thơm béo.",
                Instructions = "Cùi bưởi thái hạt lựu bóp muối xả sạch vị đắng, ướp đường sên ngậm no bột năng luộc chín trong veo, nấu đậu xanh nở mềm cùng nước đường hoa bưởi sánh mịn, chan cốt dừa béo ngậy.",
                PrepTimeMinutes = 40,
                CookTimeMinutes = 30,
                Servings = 5,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 260,
                Protein = 6,
                Carbohydrates = 48,
                Fat = 7,
                Fiber = 3,
                Sodium = 60,
                Ingredients = new()
                {
                    new("Cùi bưởi da xanh hoặc năm roi", 200, "g", "Thái hạt lựu"),
                    new("Đậu xanh cà vỏ hấp chín mềm", 150, "g"),
                    new("Bột năng", 150, "g"),
                    new("Đường phèn hoặc đường thốt nốt", 180, "g"),
                    new("Nước cốt dừa béo", 200, "ml"),
                    new("Tinh dầu hoa bưởi", 3, "giọt")
                },
                Steps = new()
                {
                    new("Xử lý cùi bưởi không bị đắng", "Cùi bưởi bóp muối hột nhiều lần xả nước lạnh đến khi cắn thử hết sạch vị đắng cay, vắt ráo.", 25),
                    new("Sên cùi bưởi áo bột năng giòn sần sật", "Ướp cùi bưởi với đường cát cho tan, sên lửa nhỏ cho ngấm rồi đổ bột năng vào xốc đều áo kín từng hạt cùi bưởi, thả vào nồi nước sôi luộc đến khi trong veo thì vớt ngâm nước đá.", 15),
                    new("Nấu chè đậu xanh sánh mịn", "Nấu tan đường phèn trong 1 lít nước, hòa bột năng rót vào khuấy đều cho sánh mịn trong veo, thả đậu xanh hấp và cùi bưởi giòn vào khuấy đều, thêm vài giọt hoa bưởi thơm ngát.", 15),
                    new("Thưởng thức", "Múc chè ra bát, chan nước cốt dừa đậm đặc béo ngậy lên trên, ăn ấm hoặc thêm đá bào mát lạnh.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=1200&q=80", "Bát chè bưởi An Giang sánh mịn thơm mát")
                }
            },

            // 37. Bánh Tráng Trộn Sài Gòn
            new()
            {
                Title = "Bánh Tráng Trộn Sài Gòn",
                CategoryName = "Ăn vặt",
                Description = "Món ăn vặt quốc dân gắn liền với tuổi học trò với bánh tráng dẻo mềm ngấm trọn vị chua của tắc, vị mặn của muối tôm, béo của trứng cút, bùi của đậu phộng và thơm nồng rau răm bò khô.",
                Instructions = "Cắt sợi bánh tráng, trộn cùng nước sốt bò khô, muối tôm, nước cốt tắc, xoài xanh bào sợi, rau răm, bò khô cay, hành phi và trứng cút luộc.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 5,
                Servings = 2,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 310,
                Protein = 12,
                Carbohydrates = 45,
                Fat = 10,
                Fiber = 2,
                Sodium = 680,
                Ingredients = new()
                {
                    new("Bánh tráng dẻo cắt sợi", 150, "g"),
                    new("Trứng cút luộc bóc vỏ", 6, "quả"),
                    new("Thịt bò khô xé sợi", 40, "g"),
                    new("Xoài xanh bào sợi", 50, "g"),
                    new("Muối tôm Tây Ninh", 1, "thìa canh"),
                    new("Quả tắc (quất) tươi vắt nước", 3, "quả"),
                    new("Hành phi vàng và đậu phộng rang", 30, "g"),
                    new("Rau răm cắt nhỏ và sa tế tôm", 30, "g")
                },
                Steps = new()
                {
                    new("Chuẩn bị nguyên liệu trộn", "Xoài bào sợi, rau răm rửa sạch cắt nhỏ, trứng cút bổ đôi.", 5),
                    new("Pha sốt trộn", "Khuấy tan nước cốt tắc, muối tôm, chút đường, sa tế và nước sốt bò tạo vị chua cay mặn ngọt đậm đà.", 5),
                    new("Trộn bánh tráng đều tay", "Cho bánh tráng sợi vào thau, rưới sốt bóp đều tay cho bánh tráng mềm dịu, thêm xoài xanh, bò khô, rau răm, hành phi và trứng cút trộn đều thưởng thức ngay.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=1200&q=80", "Đĩa bánh tráng trộn Sài Gòn đầy ắp topping")
                }
            },

            // 38. Bánh Cuốn Nóng Tráng Chảo Nhân Thịt Mộc Nhĩ
            new()
            {
                Title = "Bánh Cuốn Nóng Tráng Chảo Nhân Thịt Mộc Nhĩ",
                CategoryName = "Món hấp",
                Description = "Đĩa bánh cuốn mỏng tang mềm mướt tráng bằng chảo chống dính tại nhà, nhân thịt nạc vai băm thơm nức mộc nhĩ nấm hương, rắc hành phi giòn rụm chấm nước mắm cà cuống ấm nóng.",
                Instructions = "Pha bột gạo và bột năng để nghỉ, xào nhân thịt nạc băm nấm hương mộc nhĩ, tráng bánh trên chảo chống dính thoa dầu đậy nắp chín trong veo, cuộn nhân xếp ra đĩa rắc hành phi.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 20,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 360,
                Protein = 18,
                Carbohydrates = 50,
                Fat = 10,
                Fiber = 2,
                Sodium = 710,
                Ingredients = new()
                {
                    new("Bột gạo tẻ", 200, "g"),
                    new("Bột năng", 100, "g"),
                    new("Thịt nạc vai heo xay", 300, "g"),
                    new("Mộc nhĩ nấm hương ngâm nở băm nhỏ", 40, "g"),
                    new("Hành phi vàng giòn rụm", 50, "g"),
                    new("Chả lụa thái lát ăn kèm", 150, "g"),
                    new("Nước mắm tỏi ớt ấm chua ngọt", 1, "bát")
                },
                Steps = new()
                {
                    new("Pha bột bánh cuốn mịn màng", "Hòa tan bột gạo, bột năng, 600ml nước lọc và chút muối, để bột nghỉ 1 tiếng cho bột lắng trong.", 60),
                    new("Xào nhân thịt mộc nhĩ", "Phi thơm hành tím, trút thịt xay và nấm mộc nhĩ vào xào chín săn thơm, nêm nước mắm hạt nêm và hạt tiêu thơm lừng.", 10),
                    new("Tráng bánh cuốn bằng chảo chống dính", "Làm nóng chảo quết lớp dầu mỏng, múc 1 muôi bột tráng đều quanh chảo thật mỏng, đậy nắp 30 giây đến khi bánh trong veo thì úp ra mâm thoa dầu.", 15),
                    new("Cuộn nhân và thưởng thức", "Múc 1 thìa nhân rải đều lên bánh rồi cuộn tròn lại, cắt đôi xếp ra đĩa rắc nhiều hành phi, ăn kèm chả lụa và nước mắm ấm.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1541544741938-0af808871cc0?auto=format&fit=crop&w=1200&q=80", "Đĩa bánh cuốn nóng tráng chảo bóng mịn rắc hành phi")
                }
            },

            // 39. Chè Đậu Đỏ Nước Cốt Dừa
            new()
            {
                Title = "Chè Đậu Đỏ Nước Cốt Dừa",
                CategoryName = "Món tráng miệng",
                Description = "Bát chè đậu đỏ nấu mềm nhừ bùi béo ngọt thanh vị đường phèn, hòa quyện dòng nước cốt dừa trắng ngần béo ngậy thơm nức mùi vani mang lại may mắn và bình an.",
                Instructions = "Ngâm hạt đậu đỏ nở mềm ninh nhừ hạt còn nguyên vẹn, sên đường phèn cho ngấm vị ngọt đậm đà, nấu sôi nước cốt dừa bột bắp béo ngậy chan lên mặt bát chè.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 45,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 270,
                Protein = 9,
                Carbohydrates = 46,
                Fat = 6,
                Fiber = 6,
                Sodium = 40,
                Ingredients = new()
                {
                    new("Đậu đỏ hạt lớn", 250, "g"),
                    new("Đường phèn kết tinh", 150, "g"),
                    new("Nước cốt dừa đậm đặc", 150, "ml"),
                    new("Bột năng hoặc bột bắp", 1, "thìa canh"),
                    new("Muối tinh và vani", 1, "thìa cà phê")
                },
                Steps = new()
                {
                    new("Ngâm và ninh đậu đỏ", "Ngâm đậu đỏ 4 tiếng, cho vào nồi ngập nước đun sôi hớt bọt rồi hạ nhỏ lửa ninh khoảng 40 phút cho hạt đậu chín mềm bùi mà không bị nát.", 40),
                    new("Sên đường phèn ngấm vị", "Cho đường phèn vào nồi đậu đun nhỏ lửa thêm 10 phút cho hạt đậu ngậm no vị ngọt thanh.", 10),
                    new("Nấu nước cốt dừa sánh béo", "Đun ấm nước cốt dừa với chút muối và bột năng hòa tan cho sốt dừa sánh mịn thơm ngát.", 5),
                    new("Múc chè và thưởng thức", "Múc chè đậu đỏ ra bát, rưới nước cốt dừa sánh béo lên trên, rắc chút dừa nạo sợi thưởng thức nóng hoặc lạnh.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1563805042-7684c019e1cb?auto=format&fit=crop&w=1200&q=80", "Bát chè đậu đỏ nước cốt dừa béo bùi ngọt lịm")
                }
            },

            // 40. Gà Hấp Lá Chanh Vàng Óng Da Giòn
            new()
            {
                Title = "Gà Hấp Lá Chanh Vàng Óng Da Giòn",
                CategoryName = "Món hấp",
                Description = "Gà ta thả vườn hấp cách thủy giữ trọn vẹn vị ngọt tinh túy nguyên bản của thịt, lớp da gà vàng ruộm căng bóng giòn sần sật thoang thoảng hương thơm ngát của lá chanh tươi.",
                Instructions = "Xoa nghệ tươi khắp mình gà cho màu vàng ươm, lót sả cây và lá chanh dưới đáy nồi hấp cách thủy trong 35 phút, chặt miếng đều tay và rắc lá chanh thái chỉ chấm muối tiêu chanh.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 35,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 410,
                Protein = 42,
                Carbohydrates = 2,
                Fat = 26,
                Fiber = 1,
                Sodium = 650,
                Ingredients = new()
                {
                    new("Gà ta thả vườn nguyên con", 1.4m, "kg"),
                    new("Lá chanh tươi bánh tẻ", 30, "lá", "Một nửa lót đáy, một nửa thái chỉ"),
                    new("Sả cây đập dập", 5, "cây"),
                    new("Củ nghệ tươi giã vắt lấy nước cốt", 1, "củ"),
                    new("Gừng tươi thái lát", 1, "củ"),
                    new("Muối tiêu chanh chấm gà", 1, "chén")
                },
                Steps = new()
                {
                    new("Sơ chế và ướp gà vàng óng", "Gà xát muối gừng rửa sạch khử mùi hôi. Thoa đều nước cốt nghệ tươi khắp mình gà để khi hấp gà lên màu vàng bóng bẩy đẹp mắt.", 15),
                    new("Chuẩn bị xửng hấp hương lá chanh", "Rải sả đập dập, gừng lát và 15 lá chanh xuống đáy xửng hấp để hơi nước bốc lên thơm ngát.", 5),
                    new("Hấp gà cách thủy chín tới", "Đặt gà vào xửng đậy kín nắp, hấp lửa vừa trong 30-35 phút. Dùng tăm xăm vào đùi gà nước chảy ra trong vắt là gà đã chín tới mọng nước.", 35),
                    new("Chặt gà và trình bày đĩa tiệc", "Để gà nguội bớt cho da săn giòn rồi chặt miếng vuông vắn xếp ra đĩa, rắc nhiều lá chanh non thái chỉ lên mặt, chấm muối tiêu chanh ớt truyền thống.", 10)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1598103442097-8b74394b95c6?auto=format&fit=crop&w=1200&q=80", "Đĩa gà hấp lá chanh vàng ruộm căng bóng thơm nức")
                }
            },

            // 41. Bánh Bột Lọc Huế Nhân Tôm Thịt
            new()
            {
                Title = "Bánh Bột Lọc Huế Nhân Tôm Thịt",
                CategoryName = "Món khai vị",
                Description = "Đặc sản xứ Huế với lớp vỏ bánh trong suốt dai giòn sần sật nhìn thấu con tôm đất đỏ au và miếng thịt ba chỉ kho đậm đà bên trong, chấm nước mắm ớt cay xé lưỡi.",
                Instructions = "Nhào bột năng với nước sôi lấy trùng, rim tôm thịt ba chỉ với dầu điều đậm đà, gói bánh trong lá chuối hấp chín trong veo, ăn cùng nước mắm ớt nguyên chất.",
                PrepTimeMinutes = 35,
                CookTimeMinutes = 25,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 290,
                Protein = 18,
                Carbohydrates = 42,
                Fat = 7,
                Fiber = 1,
                Sodium = 620,
                Ingredients = new()
                {
                    new("Bột năng tinh khiết", 300, "g"),
                    new("Tôm đất tươi rim", 200, "g"),
                    new("Thịt ba chỉ heo thái hạt lựu", 150, "g"),
                    new("Dầu màu điều", 2, "thìa canh"),
                    new("Nước mắm nhĩ và tiêu xay", 2, "thìa canh"),
                    new("Lá chuối tươi gói bánh", 1, "xấp")
                },
                Steps = new()
                {
                    new("Rim tôm thịt đậm vị", "Xào tôm và thịt ba chỉ với dầu màu điều, nước mắm, đường và tiêu cho săn chắc và lên màu đỏ cam óng ánh.", 12),
                    new("Nhào bột năng lấy trùng", "Đổ nước sôi từ từ vào âu bột năng nhào đều tay đến khi bột dẻo mịn không dính tay.", 10),
                    new("Gói bánh và hấp chín", "Vắt từng viên bột nhỏ bọc nhân tôm thịt gói trong lá chuối, hấp cách thủy trong 20 phút cho bánh trong suốt.", 20),
                    new("Thưởng thức", "Bóc bánh chấm nước mắm cốt pha ớt cay xé lưỡi chuẩn vị cố đô.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1541544741938-0af808871cc0?auto=format&fit=crop&w=1200&q=80", "Đĩa bánh bột lọc Huế trong suốt lộ nhân tôm đỏ au")
                }
            },

            // 42. Cơm Gà Hội An Xé Phay
            new()
            {
                Title = "Cơm Gà Hội An Xé Phay",
                CategoryName = "Món gà",
                Description = "Cơm nấu nước luộc gà vàng ươm nghệ tươi thơm nức mỡ gà, gà ta thả vườn xé phay bóp gỏi hành tây chua ngọt, rau răm cay nồng cùng bát súp lòng gà đậm đà.",
                Instructions = "Luộc gà ta với nghệ tươi và gừng, lấy nước luộc gà nấu cơm cho hạt vàng bóng tơi xốp, gà xé phay bóp muối tiêu chanh hành tây và rau răm phố Hội.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 40,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 540,
                Protein = 38,
                Carbohydrates = 64,
                Fat = 16,
                Fiber = 2,
                Sodium = 780,
                Ingredients = new()
                {
                    new("Gà ta thả vườn", 1.3m, "kg"),
                    new("Gạo tẻ thơm", 350, "g"),
                    new("Nghệ tươi giã vắt nước", 1, "củ"),
                    new("Hành tây thái lát mỏng ngâm đá", 1, "củ"),
                    new("Rau răm phố Hội", 50, "g"),
                    new("Muối tiêu chanh ớt", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Luộc gà vàng ươm", "Thoa nước nghệ lên gà, luộc cùng gừng hành tím trong 25 phút cho gà chín tới, vớt ra ngâm nước đá lạnh rồi xé miếng vừa ăn.", 25),
                    new("Nấu cơm gà dẻo thơm", "Phi thơm tỏi mỡ gà, rang gạo cho săn rồi đổ nước luộc gà vào nấu cơm chín vàng tơi xốp.", 30),
                    new("Bóp gỏi gà xé phay", "Trộn thịt gà xé với hành tây bào mỏng, rau răm, nước cốt chanh, đường và tiêu cho thấm vị chua cay mặn ngọt.", 5),
                    new("Trình bày đĩa cơm gà Hội An", "Xới cơm ra đĩa, đặt gỏi gà lên trên, ăn kèm tương ớt rim Hội An và chén nước súp lòng gà nóng hổi.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1565299585323-38d6b0865b47?auto=format&fit=crop&w=1200&q=80", "Đĩa cơm gà Hội An vàng ươm bắt mắt")
                }
            },

            // 43. Bún Cá Nha Trang Chả Cá Dầm
            new()
            {
                Title = "Bún Cá Nha Trang Chả Cá Dầm",
                CategoryName = "Món hải sản",
                Description = "Hương vị biển cả thanh ngọt tinh tế với nước dùng trong veo nấu từ xương cá thu cá cờ, chả cá chiên và hấp dai giòn sần sật, sứa biển tươi giòn sừn sựt.",
                Instructions = "Ninh xương cá biển lấy nước dùng thanh trong nêm dứa và cà chua, chả cá quết nhuyễn chiên vàng hấp chín, xếp bún tô kèm cá dầm sứa tươi và rau sống.",
                PrepTimeMinutes = 25,
                CookTimeMinutes = 45,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 420,
                Protein = 32,
                Carbohydrates = 56,
                Fat = 9,
                Fiber = 3,
                Sodium = 890,
                Ingredients = new()
                {
                    new("Chả cá thu Nha Trang chiên và hấp", 400, "g"),
                    new("Thịt cá cờ hoặc cá ngừ dầm", 250, "g"),
                    new("Bún tươi", 600, "g"),
                    new("Cà chua và dứa chín", 200, "g"),
                    new("Hành ngò, giá đỗ, xà lách", 200, "g"),
                    new("Mắm ruốc hoặc mắm nêm Nha Trang", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Nấu nước dùng cá ngọt thanh", "Ninh xương cá với đầu hành và gừng, thêm cà chua dứa đun sôi hạ nhỏ lửa hớt bọt cho nước dùng trong veo ngọt lịm.", 40),
                    new("Sơ chế chả cá và bún", "Thái lát chả cá chiên và chả cá hấp, trụng bún qua nước sôi chia đều ra tô.", 10),
                    new("Trình bày tô bún cá Nha Trang", "Xếp chả cá, thịt cá dầm lên tô bún, rắc hành ngò chan ngập nước dùng ngọt lịm, dùng kèm đĩa rau sống thái sợi.", 5)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1569718212165-3a8278d5f624?auto=format&fit=crop&w=1200&q=80", "Tô bún cá Nha Trang trong veo ngọt mát")
                }
            },

            // 44. Súp Cua Gà Xé Nấm Tuyết
            new()
            {
                Title = "Súp Cua Gà Xé Nấm Tuyết",
                CategoryName = "Món khai vị",
                Description = "Món súp khai vị thanh tao giàu dinh dưỡng với thịt cua biển tươi ngọt, ức gà xé sợi mềm, nấm tuyết giòn sần sật và vân trứng bồng bềnh trong nước súp sánh mịn.",
                Instructions = "Ninh nước dùng gà ngọt thanh, cho thịt cua gà xé bắp ngọt và nấm tuyết vào nấu sôi, khuấy tan bột năng tạo độ sánh và đánh vân trứng hoa đẹp mắt.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 25,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 210,
                Protein = 18,
                Carbohydrates = 20,
                Fat = 6,
                Fiber = 2,
                Sodium = 580,
                Ingredients = new()
                {
                    new("Thịt cua biển hấp chín gỡ miếng", 200, "g"),
                    new("Ức gà luộc xé sợi", 150, "g"),
                    new("Nấm tuyết ngâm nở cắt nhỏ", 30, "g"),
                    new("Bắp ngọt tách hạt", 50, "g"),
                    new("Trứng gà tươi đánh tan", 2, "quả"),
                    new("Bột năng hòa tan", 3, "thìa canh"),
                    new("Dầu mè thơm và tiêu trắng", 1, "thìa canh")
                },
                Steps = new()
                {
                    new("Nấu nước súp cua ngọt ngào", "Đun sôi 1 lít nước dùng gà, thả bắp ngọt, nấm tuyết, thịt gà xé và thịt cua vào đun sôi 5 phút.", 10),
                    new("Tạo độ sánh và vân trứng hoa", "Rót từ từ bột năng vào khuấy đều cho súp sánh mượt, tiếp tục rót trứng qua rây khuấy vòng tròn tạo vân hoa tuyệt đẹp.", 8),
                    new("Múc chén và thưởng thức", "Múc súp ra chén nóng, rưới vài giọt dầu mè thơm, rắc ngò rí và tiêu trắng xay mịn.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=1200&q=80", "Chén súp cua gà xé nóng hổi sóng sánh")
                }
            },

            // 45. Bò Cuộn Nấm Kim Châm Nướng Sốt Tiêu
            new()
            {
                Title = "Bò Cuộn Nấm Kim Châm Nướng Sốt Tiêu",
                CategoryName = "Món nướng",
                Description = "Từng cuộn ba chỉ bò Mỹ mềm ngọt béo ngậy ôm trọn nấm kim châm tươi giòn mọng nước, nướng xém vàng óng ánh quyện cùng sốt tiêu đen cay nồng đậm vị.",
                Instructions = "Trải từng lát ba chỉ bò cuộn chặt bó nấm kim châm, phết sốt ướp nướng trên bếp than hoặc áp chảo xém cạnh vàng ruộm, rưới sốt tiêu thơm lừng.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 12,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 360,
                Protein = 26,
                Carbohydrates = 8,
                Fat = 25,
                Fiber = 3,
                Sodium = 610,
                Ingredients = new()
                {
                    new("Ba chỉ bò Mỹ thái lát mỏng", 400, "g"),
                    new("Nấm kim châm cắt gốc rửa sạch", 300, "g"),
                    new("Sốt tiêu đen hoặc dầu hào", 3, "thìa canh"),
                    new("Tỏi băm và mè rang vàng", 20, "g")
                },
                Steps = new()
                {
                    new("Cuộn bò với nấm kim châm", "Trải lát bò ba chỉ, đặt cụm nấm kim châm vào giữa rồi cuộn tròn chặt tay, dùng tăm ghim nhẹ cố định.", 10),
                    new("Nướng bò xém cạnh", "Đặt từng cuộn bò lên vỉ nướng hoặc chảo gang, nướng mỗi mặt 2-3 phút cho bò chín vàng xém mọng nước.", 8),
                    new("Phết sốt và dọn đĩa", "Phết đẫm sốt tiêu đen lên cuộn bò, rắc mè rang thơm lừng và thưởng thức ngay khi còn nóng giòn.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1544025162-d76694265947?auto=format&fit=crop&w=1200&q=80", "Đĩa bò cuộn nấm kim châm nướng thơm lừng")
                }
            },

            // 46. Ốc Bươu Nhồi Thịt Hấp Sả Gừng
            new()
            {
                Title = "Ốc Bươu Nhồi Thịt Hấp Sả Gừng",
                CategoryName = "Món hấp",
                Description = "Món ăn đặc sản Đà Lạt với thịt ốc bươu giòn sần sật trộn giò sống và nấm hương nhồi vào vỏ ốc lót lá sả tươi, hấp nóng nghi ngút khói chấm nước mắm gừng cay ấm bụng.",
                Instructions = "Luộc ốc băm nhỏ trộn giò sống mộc nhĩ nấm hương, đặt lá sả vào vỏ ốc nhồi đầy ắp nhân thịt, hấp cách thủy trong 20 phút chấm nước mắm gừng ớt chua cay.",
                PrepTimeMinutes = 30,
                CookTimeMinutes = 20,
                Servings = 4,
                Difficulty = Difficulty.Medium,
                Status = RecipeStatus.Published,
                Calories = 270,
                Protein = 22,
                Carbohydrates = 12,
                Fat = 14,
                Fiber = 2,
                Sodium = 690,
                Ingredients = new()
                {
                    new("Thịt ốc bươu băm hạt lựu", 300, "g"),
                    new("Giò sống thịt heo", 250, "g"),
                    new("Vỏ ốc bươu luộc sạch", 20, "vỏ"),
                    new("Lá sả tươi và sả cây", 10, "cây"),
                    new("Gừng tươi giã nhuyễn", 30, "g"),
                    new("Nước mắm gừng ớt chua ngọt", 1, "bát")
                },
                Steps = new()
                {
                    new("Trộn nhân ốc dẻo quánh", "Trộn thịt ốc bươu băm giòn, giò sống, mộc nhĩ, tiêu sọ và hành gừng băm cho dẻo quánh.", 10),
                    new("Nhồi ốc lá sả khéo léo", "Gấp đôi lá sả đặt vào trong vỏ ốc, nhồi nhân thịt ốc đầy ắp miệng vỏ để khi ăn cầm hai đầu lá sả kéo thịt ra dễ dàng.", 15),
                    new("Hấp ốc thơm lừng", "Xếp ốc vào xửng hấp cùng sả gừng trong 20 phút cho nhân thịt chín căng mọng.", 20),
                    new("Thưởng thức", "Kéo cọng lá sả rút thịt ốc nóng hổi chấm ngập chén nước mắm gừng cay nồng ấm bụng.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1559847844-5315695dadae?auto=format&fit=crop&w=1200&q=80", "Xửng ốc bươu nhồi thịt hấp sả nghi ngút khói")
                }
            },

            // 47. Canh Rong Biển Đậu Hũ Thịt Băm
            new()
            {
                Title = "Canh Rong Biển Đậu Hũ Thịt Băm",
                CategoryName = "Món canh",
                Description = "Bát canh thanh mát giải nhiệt mùa hè bổ sung nhiều khoáng chất với rong biển nâu thanh ngọt, đậu hũ non mềm tan béo ngậy và thịt nạc băm đậm đà thoang thoảng dầu mè.",
                Instructions = "Rong biển khô ngâm nở xả sạch bóp chút gừng, xào thơm thịt nạc băm đổ nước sôi đun sôi thả đậu hũ non và rong biển nấu chín tới, rưới vài giọt dầu mè thơm.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 12,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 170,
                Protein = 16,
                Carbohydrates = 8,
                Fat = 7,
                Fiber = 3,
                Sodium = 510,
                Ingredients = new()
                {
                    new("Rong biển khô nấu canh", 30, "g"),
                    new("Đậu hũ non cắt khối vuông", 1, "hộp"),
                    new("Thịt nạc vai heo băm nhỏ", 150, "g"),
                    new("Gừng tươi đập dập", 1, "nhánh"),
                    new("Dầu mè thơm và hạt nêm", 1, "thìa canh")
                },
                Steps = new()
                {
                    new("Sơ chế rong biển", "Ngâm rong biển trong nước ấm 10 phút cho nở đều, bóp nhẹ với chút gừng khử tanh rồi xả sạch để ráo.", 10),
                    new("Xào thịt và nấu canh", "Phi thơm hành, xào săn thịt băm rồi đổ 1 lít nước đun sôi hớt bọt.", 6),
                    new("Thả đậu hũ và rong biển", "Cho đậu hũ non và rong biển vào nấu sôi thêm 2 phút, nêm hạt nêm và rưới dầu mè thơm rồi tắt bếp.", 4)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1547592166-23ac45744acd?auto=format&fit=crop&w=1200&q=80", "Bát canh rong biển đậu hũ non thanh mát")
                }
            },

            // 48. Rau Muống Xào Tỏi Xanh Giòn
            new()
            {
                Title = "Rau Muống Xào Tỏi Xanh Giòn",
                CategoryName = "Món xào",
                Description = "Món xào kinh điển trong mọi bữa cơm gia đình Việt với ngọn rau muống xanh mướt giòn sần sật, đượm khói chảo lửa lớn thơm lừng mùi tỏi đập dập phi vàng óng.",
                Instructions = "Rau muống nhặt ngọn non chần sơ nước sôi ngâm nước đá cho xanh mướt, phi thơm nhiều tỏi đập dập xào rau trên lửa cực lớn trong 2 phút nêm mắm thơm.",
                PrepTimeMinutes = 10,
                CookTimeMinutes = 5,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 110,
                Protein = 4,
                Carbohydrates = 9,
                Fat = 6,
                Fiber = 3,
                Sodium = 420,
                Ingredients = new()
                {
                    new("Rau muống non", 500, "g"),
                    new("Tỏi ta đập dập", 2, "củ lớn"),
                    new("Nước mắm ngon và dầu hào", 2, "thìa canh"),
                    new("Dầu ăn xào rau", 2, "thìa canh")
                },
                Steps = new()
                {
                    new("Chần rau giữ màu xanh mướt", "Đun sôi nước với chút muối, thả rau muống chần nhanh 30 giây rồi vớt ngay vào thau nước đá lạnh để rau giữ màu xanh mướt và độ giòn tuyệt đối.", 5),
                    new("Xào rau lửa cực lớn", "Làm nóng chảo phi thơm 1/2 lượng tỏi cho vàng thơm, trút rau vào đảo thật nhanh tay trên lửa lớn, nêm nước mắm dầu hào và cho nốt phần tỏi còn lại vào đảo đều 1 phút rồi tắt bếp.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1540420773420-3366772f4999?auto=format&fit=crop&w=1200&q=80", "Đĩa rau muống xào tỏi xanh mướt giòn rụm")
                }
            },

            // 49. Mì Ý Sốt Bò Băm Bolognese
            new()
            {
                Title = "Mì Ý Sốt Bò Băm Bolognese",
                CategoryName = "Món chay Âu", // hoặc "Món bò", xếp vào món Âu
                Description = "Đĩa mì Ý spaghetti chuẩn hương vị Địa Trung Hải với sợi mì luộc đạt độ al dente hoàn hảo, đẫm sốt bò băm cà chua ninh chậm cùng lá xạ hương thơm nồng và phô mai Parmesan bào mịn.",
                Instructions = "Luộc mì spaghetti chín tới giữ độ dai, sốt thịt bò băm cùng cà chua nghiền cần tây cà rốt hầm nhỏ lửa cho sánh ngọt, trộn mì với sốt và rắc phô mai bào.",
                PrepTimeMinutes = 20,
                CookTimeMinutes = 35,
                Servings = 4,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 520,
                Protein = 28,
                Carbohydrates = 68,
                Fat = 15,
                Fiber = 4,
                Sodium = 670,
                Ingredients = new()
                {
                    new("Mì sợi Spaghetti", 350, "g"),
                    new("Thịt bò xay", 350, "g"),
                    new("Cà chua xay nhuyễn hộp", 400, "g"),
                    new("Cần tây và cà rốt hạt lựu", 100, "g"),
                    new("Hành tây băm nhỏ", 1, "củ"),
                    new("Lá nguyệt quế và cỏ xạ hương oregano", 1, "thìa cà phê"),
                    new("Phô mai Parmesan bào mịn", 40, "g")
                },
                Steps = new()
                {
                    new("Hầm sốt bò băm cà chua", "Phi thơm tỏi hành tây, cho cà rốt cần tây và thịt bò vào xào săn thơm, đổ cà chua nghiền cùng lá thơm vào đun liu riu trong 30 phút cho sốt sánh đậm đà.", 30),
                    new("Luộc mì Spaghetti al dente", "Luộc mì trong nước sôi pha muối khoảng 8-9 phút cho sợi mì chín tới dai ngon, vớt ra trộn chút dầu oliu.", 10),
                    new("Trộn sốt và thưởng thức", "Cho mì ra đĩa, múc sốt bò băm nóng hổi phủ lên mặt, rắc nhiều phô mai Parmesan bào mịn thơm ngậy.", 3)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1546964124-0cce460f38ef?auto=format&fit=crop&w=1200&q=80", "Đĩa mì Ý sốt bò băm thơm lừng phô mai")
                }
            },

            // 50. Rau Câu Dừa Hoa Đậu Biếc Thanh Mát
            new()
            {
                Title = "Rau Câu Dừa Hoa Đậu Biếc Thanh Mát",
                CategoryName = "Món tráng miệng",
                Description = "Món thạch tráng miệng lung linh sắc xanh biếc huyền ảo của hoa đậu biếc thiên nhiên, xen kẽ lớp cốt dừa béo ngậy thơm lừng nước dừa tươi ngọt lịm tan chảy mát lạnh.",
                Instructions = "Nấu nước dừa tươi bột rau câu giòn chia 2 phần: một phần pha nước cốt hoa đậu biếc biếc xanh, một phần pha nước cốt dừa trắng béo, đổ xen kẽ từng lớp tạo hình đẹp mắt.",
                PrepTimeMinutes = 15,
                CookTimeMinutes = 20,
                Servings = 6,
                Difficulty = Difficulty.Easy,
                Status = RecipeStatus.Published,
                Calories = 150,
                Protein = 2,
                Carbohydrates = 28,
                Fat = 4,
                Fiber = 2,
                Sodium = 30,
                Ingredients = new()
                {
                    new("Bột rau câu giòn Agar", 1, "gói", "25g"),
                    new("Nước dừa tươi xiêm", 1, "lít"),
                    new("Hoa đậu biếc khô", 15, "bông", "Hãm lấy nước xanh"),
                    new("Nước cốt dừa đậm đặc", 150, "ml"),
                    new("Đường phèn", 120, "g")
                },
                Steps = new()
                {
                    new("Hãm hoa đậu biếc và ngâm thạch", "Hãm hoa đậu biếc với 100ml nước sôi lấy màu xanh biếc. Hòa bột rau câu với nước dừa tươi và đường phèn ngâm 15 phút.", 15),
                    new("Nấu rau câu trong suốt", "Đun sôi hỗn hợp rau câu khuấy đều cho tan hoàn toàn, chia làm 2 nồi: 1 nồi hòa nước cốt hoa đậu biếc, 1 nồi hòa nước cốt dừa béo.", 10),
                    new("Đổ lớp thạch lung linh", "Đổ một lớp thạch hoa đậu biếc vào khuôn chờ se mặt, đổ tiếp lớp thạch cốt dừa trắng, lặp lại cho đến hết rồi để vào ngăn mát tủ lạnh 2 tiếng trước khi cắt lát thưởng thức.", 20)
                },
                Images = new()
                {
                    new("https://images.unsplash.com/photo-1528975604071-b4dc52a2d18c?auto=format&fit=crop&w=1200&q=80", "Đĩa thạch rau câu hoa đậu biếc cốt dừa lung linh")
                }
            }
        };
    }
}
