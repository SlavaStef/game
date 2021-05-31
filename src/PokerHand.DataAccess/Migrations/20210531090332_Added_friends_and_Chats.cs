using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PokerHand.DataAccess.Migrations
{
    public partial class Added_friends_and_Chats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExternalLogins_PlayerId",
                table: "ExternalLogins");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:country_code", "none,us,ca,br,jp,cn,in,kr,ru,ir,iq,il,tr,au,za,ae,es,it,pt,fr,gb,de,pl,cz,ua,se,fi,by,ee,ch,no,is")
                .Annotation("Npgsql:Enum:external_provider_name", "none,google,facebook")
                .Annotation("Npgsql:Enum:gender", "none,male,female")
                .Annotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .Annotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman")
                .OldAnnotation("Npgsql:Enum:country_code", "none,us,ca,br,jp,cn,in,kr,ru,ir,iq,il,tr,au,za,ae,es,it,pt,fr,gb,de,pl,cz,ua,se,fi,by,ee,ch,no,is")
                .OldAnnotation("Npgsql:Enum:external_provider_name", "google,facebook")
                .OldAnnotation("Npgsql:Enum:gender", "none,male,female")
                .OldAnnotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .OldAnnotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman");

            migrationBuilder.AddColumn<string>(
                name: "Friends",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalCode",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SecondPlayerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_FirstPlayerId",
                        column: x => x.FirstPlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Conversations_AspNetUsers_SecondPlayerId",
                        column: x => x.SecondPlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Text = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TimeCreated = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_AspNetUsers_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_PlayerId",
                table: "ExternalLogins",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_FirstPlayerId",
                table: "Conversations",
                column: "FirstPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_SecondPlayerId",
                table: "Conversations",
                column: "SecondPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_PlayerId",
                table: "Messages",
                column: "PlayerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropIndex(
                name: "IX_ExternalLogins_PlayerId",
                table: "ExternalLogins");

            migrationBuilder.DropColumn(
                name: "Friends",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PersonalCode",
                table: "AspNetUsers");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:country_code", "none,us,ca,br,jp,cn,in,kr,ru,ir,iq,il,tr,au,za,ae,es,it,pt,fr,gb,de,pl,cz,ua,se,fi,by,ee,ch,no,is")
                .Annotation("Npgsql:Enum:external_provider_name", "google,facebook")
                .Annotation("Npgsql:Enum:gender", "none,male,female")
                .Annotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .Annotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman")
                .OldAnnotation("Npgsql:Enum:country_code", "none,us,ca,br,jp,cn,in,kr,ru,ir,iq,il,tr,au,za,ae,es,it,pt,fr,gb,de,pl,cz,ua,se,fi,by,ee,ch,no,is")
                .OldAnnotation("Npgsql:Enum:external_provider_name", "none,google,facebook")
                .OldAnnotation("Npgsql:Enum:gender", "none,male,female")
                .OldAnnotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .OldAnnotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman");

            migrationBuilder.CreateIndex(
                name: "IX_ExternalLogins_PlayerId",
                table: "ExternalLogins",
                column: "PlayerId");
        }
    }
}
