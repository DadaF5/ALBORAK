using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FRAProject.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AcCategories",
                columns: table => new
                {
                    AcCategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcCategories", x => x.AcCategoryId);
                });

            migrationBuilder.CreateTable(
                name: "AcStatusTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcStatusTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Bases",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BaseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    BaseNameLocal = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RankTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AcMainGroups",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    AcCategoryId = table.Column<int>(type: "int", nullable: false),
                    BaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcMainGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcMainGroups_AcCategories_AcCategoryId",
                        column: x => x.AcCategoryId,
                        principalTable: "AcCategories",
                        principalColumn: "AcCategoryId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AcMainGroups_Bases_BaseId",
                        column: x => x.BaseId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    BaseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Departments_Bases_BaseId",
                        column: x => x.BaseId,
                        principalTable: "Bases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ranks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    FullRank = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    RankTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ranks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ranks_RankTypes_RankTypeId",
                        column: x => x.RankTypeId,
                        principalTable: "RankTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AcTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    AcMainGroupId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AcTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AcTypes_AcMainGroups_AcMainGroupId",
                        column: x => x.AcMainGroupId,
                        principalTable: "AcMainGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DepartmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubDepartments_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Aircrafts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TailNo = table.Column<int>(type: "int", nullable: false),
                    Registration = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Model = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ManufactureDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Serviceable = table.Column<bool>(type: "bit", nullable: false),
                    IntCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Obs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AcTypeId = table.Column<int>(type: "int", nullable: false),
                    AcStatusTypeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Aircrafts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Aircrafts_AcStatusTypes_AcStatusTypeId",
                        column: x => x.AcStatusTypeId,
                        principalTable: "AcStatusTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Aircrafts_AcTypes_AcTypeId",
                        column: x => x.AcTypeId,
                        principalTable: "AcTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RankId = table.Column<int>(type: "int", nullable: false),
                    Matricule = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    SubDepartmentId = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NationalId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Speciality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Active = table.Column<bool>(type: "bit", nullable: false),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Persons_Ranks_RankId",
                        column: x => x.RankId,
                        principalTable: "Ranks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Persons_SubDepartments_SubDepartmentId",
                        column: x => x.SubDepartmentId,
                        principalTable: "SubDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AcMainGroups_AcCategoryId",
                table: "AcMainGroups",
                column: "AcCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_AcMainGroups_BaseId",
                table: "AcMainGroups",
                column: "BaseId");

            migrationBuilder.CreateIndex(
                name: "IX_AcTypes_AcMainGroupId",
                table: "AcTypes",
                column: "AcMainGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_Aircrafts_AcStatusTypeId",
                table: "Aircrafts",
                column: "AcStatusTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Aircrafts_AcTypeId",
                table: "Aircrafts",
                column: "AcTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_BaseId",
                table: "Departments",
                column: "BaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_RankId",
                table: "Persons",
                column: "RankId");

            migrationBuilder.CreateIndex(
                name: "IX_Persons_SubDepartmentId",
                table: "Persons",
                column: "SubDepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Ranks_RankTypeId",
                table: "Ranks",
                column: "RankTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_SubDepartments_DepartmentId",
                table: "SubDepartments",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Aircrafts");

            migrationBuilder.DropTable(
                name: "Persons");

            migrationBuilder.DropTable(
                name: "AcStatusTypes");

            migrationBuilder.DropTable(
                name: "AcTypes");

            migrationBuilder.DropTable(
                name: "Ranks");

            migrationBuilder.DropTable(
                name: "SubDepartments");

            migrationBuilder.DropTable(
                name: "AcMainGroups");

            migrationBuilder.DropTable(
                name: "RankTypes");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropTable(
                name: "AcCategories");

            migrationBuilder.DropTable(
                name: "Bases");
        }
    }
}
