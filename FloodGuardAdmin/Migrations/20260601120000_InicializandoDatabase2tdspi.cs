using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FloodGuardAdmin.Migrations
{
    /// <inheritdoc />
    public partial class InicializandoDatabase2tdspi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TBL_OCORRENCIAS_2TDSPI",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TITULO = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    LOCALIZACAO = table.Column<string>(type: "NVARCHAR2(300)", maxLength: 300, nullable: false),
                    NIVEL_SEVERIDADE = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    STATUS = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    DATA_OCORRENCIA = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_OCORRENCIAS_2TDSPI", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TBL_EQUIPES_RESPOSTA_2TDSPI",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    NOME = table.Column<string>(type: "NVARCHAR2(150)", maxLength: 150, nullable: false),
                    ESPECIALIDADE = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    CAPACIDADE_MAX = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    DISPONIVEL = table.Column<bool>(type: "NUMBER(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_EQUIPES_RESPOSTA_2TDSPI", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TBL_ACOES_EMERGENCIAIS_2TDSPI",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    DESCRICAO = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: false),
                    TIPO_ACAO = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    DATA_INICIO = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: false),
                    DATA_FIM = table.Column<DateTime>(type: "TIMESTAMP(7)", nullable: true),
                    STATUS_ACAO = table.Column<string>(type: "NVARCHAR2(50)", maxLength: 50, nullable: false),
                    OCORRENCIA_ID = table.Column<int>(type: "NUMBER(10)", nullable: false),
                    EQUIPE_RESPOSTA_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TBL_ACOES_EMERGENCIAIS_2TDSPI", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ACOES_OCORRENCIA_ID",
                        column: x => x.OCORRENCIA_ID,
                        principalTable: "TBL_OCORRENCIAS_2TDSPI",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ACOES_EQUIPE_RESPOSTA_ID",
                        column: x => x.EQUIPE_RESPOSTA_ID,
                        principalTable: "TBL_EQUIPES_RESPOSTA_2TDSPI",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ACOES_EMERGENCIAIS_OCORRENCIA_ID",
                table: "TBL_ACOES_EMERGENCIAIS_2TDSPI",
                column: "OCORRENCIA_ID");

            migrationBuilder.CreateIndex(
                name: "IX_TBL_ACOES_EMERGENCIAIS_EQUIPE_RESPOSTA_ID",
                table: "TBL_ACOES_EMERGENCIAIS_2TDSPI",
                column: "EQUIPE_RESPOSTA_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TBL_ACOES_EMERGENCIAIS_2TDSPI");

            migrationBuilder.DropTable(
                name: "TBL_EQUIPES_RESPOSTA_2TDSPI");

            migrationBuilder.DropTable(
                name: "TBL_OCORRENCIAS_2TDSPI");
        }
    }
}
