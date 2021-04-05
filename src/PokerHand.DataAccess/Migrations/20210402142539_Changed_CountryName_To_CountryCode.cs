using Microsoft.EntityFrameworkCore.Migrations;
using PokerHand.Common.Helpers.Player;

namespace PokerHand.DataAccess.Migrations
{
    public partial class Changed_CountryName_To_CountryCode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:country_code", "none,us,ca,br,jp,cn,in,kr,ru,ir,iq,il,tr,au,za,ae,es,it,pt,fr,gb,de,pl,cz,ua,se,fi,by,ee,ch,no,is")
                .Annotation("Npgsql:Enum:external_provider_name", "google,facebook")
                .Annotation("Npgsql:Enum:gender", "none,male,female")
                .Annotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .Annotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman")
                .OldAnnotation("Npgsql:Enum:country_name", "none,united_states_of_america,canada,brazil,japan,china,india,korea,russia,iran,iraq,israel,turkey,australia,south_africa,united_arab_emirates,spain,italy,portugal,france,great_britain,germany,poland,czech_republic,ukraine,sweden,finland,belarus,estonia,switzerland,norway,iceland")
                .OldAnnotation("Npgsql:Enum:external_provider_name", "google,facebook")
                .OldAnnotation("Npgsql:Enum:gender", "none,male,female")
                .OldAnnotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .OldAnnotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman");

            migrationBuilder.AlterColumn<CountryCode>(
                name: "Country",
                table: "AspNetUsers",
                type: "country_code",
                nullable: false,
                oldClrType: typeof(CountryCode),
                oldType: "country_name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:country_name", "none,united_states_of_america,canada,brazil,japan,china,india,korea,russia,iran,iraq,israel,turkey,australia,south_africa,united_arab_emirates,spain,italy,portugal,france,great_britain,germany,poland,czech_republic,ukraine,sweden,finland,belarus,estonia,switzerland,norway,iceland")
                .Annotation("Npgsql:Enum:external_provider_name", "google,facebook")
                .Annotation("Npgsql:Enum:gender", "none,male,female")
                .Annotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .Annotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman")
                .OldAnnotation("Npgsql:Enum:country_code", "none,us,ca,br,jp,cn,in,kr,ru,ir,iq,il,tr,au,za,ae,es,it,pt,fr,gb,de,pl,cz,ua,se,fi,by,ee,ch,no,is")
                .OldAnnotation("Npgsql:Enum:external_provider_name", "google,facebook")
                .OldAnnotation("Npgsql:Enum:gender", "none,male,female")
                .OldAnnotation("Npgsql:Enum:hand_type", "none,high_card,one_pair,two_pairs,three_of_a_kind,straight,flush,full_house,four_of_a_kind,straight_flush,royal_flush,five_of_a_kind")
                .OldAnnotation("Npgsql:Enum:hands_sprite_type", "none,white_man,white_woman,black_man,black_woman");

            migrationBuilder.AlterColumn<CountryCode>(
                name: "Country",
                table: "AspNetUsers",
                type: "country_name",
                nullable: false,
                oldClrType: typeof(CountryCode),
                oldType: "country_code");
        }
    }
}
