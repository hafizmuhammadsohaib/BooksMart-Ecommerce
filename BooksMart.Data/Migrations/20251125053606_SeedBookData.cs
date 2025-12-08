using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BooksMart.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedBookData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ISBN = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AuthorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ListPrice = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Price50 = table.Column<double>(type: "float", nullable: false),
                    Price100 = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Books",
                columns: new[] { "Id", "AuthorName", "Description", "ISBN", "ListPrice", "Price", "Price100", "Price50", "Title" },
                values: new object[,]
                {
                    { 1, "Marvin Cole", "A gripping tale of mystery and redemption set in a forgotten town where secrets slowly unravel. Follow the journey of a man trying to reclaim his past while confronting the shadows that haunt him.", "BK10001001", 110.0, 95.0, 85.0, 90.0, "Echoes of the Forgotten" },
                    { 2, "Diana Crest", "A heartwarming story about a young woman's return to her childhood valley, discovering love, loss, and the beauty hidden in everyday life. A gentle reminder that home is more than a place.", "BK20002001", 45.0, 38.0, 28.0, 33.0, "Whispers of the Valley" },
                    { 3, "Felix Hart", "A suspense novel that follows an investigator racing against time to uncover a conspiracy threatening the peace of a bustling city. Every clue unravels a deeper layer of danger.", "BK30003001", 60.0, 52.0, 40.0, 45.0, "Shadows Over Dawn" },
                    { 4, "Clara Monroe", "An enchanting romantic drama set under the glow of moonlit nights, where two unlikely people cross paths and discover the bittersweet nature of love and destiny.", "BK40004001", 75.0, 70.0, 60.0, 65.0, "The Sugar Moon" },
                    { 5, "Ernest Hale", "A thrilling ocean adventure following a marine explorer who uncovers a hidden world beneath the waves. Courage, mystery, and danger collide deep in the unknown.", "BK50005001", 35.0, 30.0, 22.0, 27.0, "Waves of Midnight" },
                    { 6, "Amelia Thorn", "A beautifully written fantasy novel about a young girl's discovery of a magical realm blooming beneath an ancient garden. Wonder, imagination, and bravery shape her unforgettable journey.", "BK60006001", 28.0, 25.0, 21.0, 23.0, "Garden of Starlight" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);
        }
    }
}
