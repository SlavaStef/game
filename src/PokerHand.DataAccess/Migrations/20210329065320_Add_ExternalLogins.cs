using System;
using Microsoft.EntityFrameworkCore.Migrations;
using PokerHand.Common.Helpers.Authorization;
using PokerHand.Common.Helpers.CardEvaluation;
using PokerHand.Common.Helpers.Player;

namespace PokerHand.DataAccess.Migrations
{
    public partial class Add_ExternalLogins : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:country_name", "none,united_states_of_america,canada,brazil,japan,china,india,korea,russia,iran,iraq,israel,turkey,australia,south_africa,united_arab_emirates,spain,italy,portugal,france,great_britain,germany,poland,czech_republic,ukraine,sweden,finland,belarus,estonia,switzerland,norway,iceland")
                .Annotation("Npgsql:Enum:external_provider_name", "google,facebook")
                .Annotation("Npgsql:Enum:gender", "none,male,female")
                .Annotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .Annotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman");

            migrationBuilder.AlterColumn<CountryName>(
                name: "Country",
                table: "AspNetUsers",
                type: "country_name",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<HandType>(
                name: "BestHandType",
                table: "AspNetUsers",
                type: "hand_type",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<Gender>(
                name: "Gender",
                table: "AspNetUsers",
                type: "gender",
                nullable: false,
                defaultValue: Gender.None);

            migrationBuilder.AddColumn<HandsSpriteType>(
                name: "HandsSprite",
                table: "AspNetUsers",
                type: "hands_sprite_type",
                nullable: false,
                defaultValue: HandsSpriteType.None);

            migrationBuilder.CreateTable(
                name: "ExternalLogins",
                columns: table => new
                {
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderName = table.Column<ExternalProviderName>(type: "external_provider_name", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExternalLogins", x => new { x.ProviderKey, x.ProviderName });
                    table.ForeignKey(
                        name: "FK_ExternalLogins_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_PlayerId",
                table: "ExternalLogins",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_ProviderKey",
                table: "ExternalLogins",
                column: "ProviderKey");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExternalLogins");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "HandsSprite",
                table: "AspNetUsers");

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:Enum:country_name", "none,united_states_of_america,canada,brazil,japan,china,india,korea,russia,iran,iraq,israel,turkey,australia,south_africa,united_arab_emirates,spain,italy,portugal,france,great_britain,germany,poland,czech_republic,ukraine,sweden,finland,belarus,estonia,switzerland,norway,iceland")
                .OldAnnotation("Npgsql:Enum:external_provider_name", "google,facebook")
                .OldAnnotation("Npgsql:Enum:gender", "none,male,female")
                .OldAnnotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .OldAnnotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "AspNetUsers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(CountryName),
                oldType: "country_name");

            migrationBuilder.AlterColumn<int>(
                name: "BestHandType",
                table: "AspNetUsers",
                type: "integer",
                nullable: false,
                oldClrType: typeof(HandType),
                oldType: "hand_type");
        }
    }
}
